#!/usr/bin/env python3
"""Upload large file in chunks and deploy to Windows Server"""

import paramiko
import os
import time

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"
LOCAL_ARCHIVE = r"C:\Projects\learningtool\learningtool-deploy.tar.gz"
CHUNK_SIZE = 500 * 1024  # 500KB chunks

def upload_in_chunks(ssh, local_file, remote_file):
    """Upload file in small chunks via SSH"""
    file_size = os.path.getsize(local_file)
    num_chunks = (file_size + CHUNK_SIZE - 1) // CHUNK_SIZE

    print(f"   File size: {file_size / 1024 / 1024:.1f} MB")
    print(f"   Uploading in {num_chunks} chunks of {CHUNK_SIZE / 1024:.0f} KB each...")

    # Create empty file on remote
    ssh.exec_command(f'powershell -Command "New-Item -Path {remote_file} -ItemType File -Force | Out-Null"')
    time.sleep(0.5)

    with open(local_file, 'rb') as f:
        for i in range(num_chunks):
            chunk = f.read(CHUNK_SIZE)
            if not chunk:
                break

            # Encode chunk as base64
            import base64
            encoded = base64.b64encode(chunk).decode('ascii')

            # Append to remote file
            cmd = f'powershell -Command "[System.IO.File]::AppendAllBytes(\'{remote_file}\', [System.Convert]::FromBase64String(\'{encoded}\'))"'
            stdin, stdout, stderr = ssh.exec_command(cmd, timeout=30)
            stdout.channel.recv_exit_status()

            progress = (i + 1) / num_chunks * 100
            print(f"   Progress: {progress:.1f}% ({i+1}/{num_chunks})", end='\r')

    print(f"\n   Upload complete!")

print("="*60)
print("  Chunked Upload Deployment")
print("="*60)

# Connect
print("\n[1/8] Connecting to server...")
ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS, timeout=30)
print("   Connected!")

# Upload in chunks
print("\n[2/8] Uploading archive in chunks...")
upload_in_chunks(ssh, LOCAL_ARCHIVE, r"C:\Temp\learningtool-deploy.tar.gz")

# Verify
stdin, stdout, stderr = ssh.exec_command('powershell -Command "(Get-Item C:\\Temp\\learningtool-deploy.tar.gz).Length / 1MB"')
size = stdout.read().decode().strip()
print(f"   Verified: {size} MB on server")

# Extract
print("\n[3/8] Extracting archive...")
cmd = 'powershell -Command "cd C:\\Temp; Remove-Item learningtool-deploy -Recurse -Force -ErrorAction SilentlyContinue; mkdir learningtool-deploy; cd learningtool-deploy; tar -xzf ..\\learningtool-deploy.tar.gz"'
stdin, stdout, stderr = ssh.exec_command(cmd, timeout=120)
stdout.channel.recv_exit_status()
print("   Extracted!")

# Create directories
print("\n[4/8] Creating application directories...")
cmd = 'powershell -Command "New-Item -Path C:\\stores\\learningtool\\backend,C:\\stores\\learningtool\\www,C:\\stores\\learningtool\\data -ItemType Directory -Force | Out-Null; Write-Host Done"'
stdin, stdout, stderr = ssh.exec_command(cmd)
print("   " + stdout.read().decode().strip())

# Copy backend
print("\n[5/8] Deploying backend...")
cmd = 'powershell -Command "Copy-Item C:\\Temp\\learningtool-deploy\\backend\\* C:\\stores\\learningtool\\backend -Recurse -Force; Write-Host Done"'
stdin, stdout, stderr = ssh.exec_command(cmd, timeout=120)
stdout.channel.recv_exit_status()
print("   Backend deployed")

# Copy frontend
print("\n[6/8] Deploying frontend...")
cmd = 'powershell -Command "Copy-Item C:\\Temp\\learningtool-deploy\\frontend\\* C:\\stores\\learningtool\\www -Recurse -Force; Write-Host Done"'
stdin, stdout, stderr = ssh.exec_command(cmd, timeout=120)
stdout.channel.recv_exit_status()
print("   Frontend deployed")

# Verify files
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Test-Path C:\\stores\\learningtool\\www\\index.html"')
has_index = stdout.read().decode().strip()
print(f"   index.html exists: {has_index}")

# Configure IIS
print("\n[7/8] Configuring IIS...")
iis_cmd = """
Import-Module WebAdministration
$poolName = 'LearningToolPool'
if (Test-Path IIS:\\AppPools\\$poolName) { Remove-WebAppPool -Name $poolName -ErrorAction SilentlyContinue }
New-WebAppPool -Name $poolName | Out-Null
Set-ItemProperty IIS:\\AppPools\\$poolName -Name managedRuntimeVersion -Value ''

$siteName = 'LearningToolApp'
if (Test-Path IIS:\\Sites\\$siteName) {
    Stop-Website -Name $siteName -ErrorAction SilentlyContinue
    Remove-Website -Name $siteName -ErrorAction SilentlyContinue
}
New-Website -Name $siteName -PhysicalPath C:\\stores\\learningtool\\www -ApplicationPool $poolName -Port 5192 | Out-Null
New-WebApplication -Site $siteName -Name api -PhysicalPath C:\\stores\\learningtool\\backend -ApplicationPool $poolName | Out-Null
Start-Website -Name $siteName
Write-Host 'IIS configured'
"""
stdin, stdout, stderr = ssh.exec_command(f'powershell -Command "{iis_cmd}"', timeout=60)
stdout.channel.recv_exit_status()
print("   " + stdout.read().decode().strip())

# Verify
print("\n[8/8] Verifying deployment...")
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Website LearningToolApp | Select-Object Name, State, PhysicalPath"')
print(stdout.read().decode())

stdin, stdout, stderr = ssh.exec_command('powershell -Command "netstat -an | Select-String :5192"')
listening = stdout.read().decode().strip()
if listening:
    print(f"   Port 5192: LISTENING")
else:
    print(f"   Port 5192: Not listening (may need time to start)")

ssh.close()

print("\n" + "="*60)
print("  Deployment Complete!")
print("="*60)
print(f"\nApplication: http://{SSH_HOST}:5192")
print("\nWaiting 5 seconds for IIS to start...")
time.sleep(5)
print("\nReady to test!")
