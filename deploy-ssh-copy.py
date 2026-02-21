#!/usr/bin/env python3
"""Deploy by uploading individual files via SSH"""

import paramiko
import os
from pathlib import Path

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"

def upload_directory(sftp, local_dir, remote_dir):
    """Upload directory recursively"""
    for root, dirs, files in os.walk(local_dir):
        # Calculate relative path
        rel_path = os.path.relpath(root, local_dir)
        if rel_path == ".":
            remote_path = remote_dir
        else:
            remote_path = f"{remote_dir}/{rel_path}".replace("\\", "/")

        # Create remote directory
        try:
            sftp.stat(remote_path)
        except:
            try:
                sftp.mkdir(remote_path)
            except:
                pass

        # Upload files
        for file in files:
            local_file = os.path.join(root, file)
            remote_file = f"{remote_path}/{file}".replace("\\", "/")
            print(f"   {remote_file}")
            sftp.put(local_file, remote_file)

print("=" * 60)
print("  SSH Direct File Upload Deployment")
print("=" * 60)

# Connect
print("\n[1/6] Connecting...")
ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS, timeout=30)
sftp = ssh.open_sftp()
print("   Connected!")

# Create directories
print("\n[2/6] Creating directories...")
for dir in ["C:/stores/learningtool/backend", "C:/stores/learningtool/www", "C:/stores/learningtool/data"]:
    stdin, stdout, stderr = ssh.exec_command(f'powershell -Command "New-Item -ItemType Directory -Path {dir} -Force | Out-Null"')
    stdout.channel.recv_exit_status()
print("   Directories created")

# Upload Frontend
print("\n[3/6] Uploading Frontend files...")
upload_directory(sftp, r"C:\Projects\learningtool\publish\frontend", "C:/stores/learningtool/www")
print("   Frontend uploaded")

# Upload Backend
print("\n[4/6] Uploading Backend files...")
upload_directory(sftp, r"C:\Projects\learningtool\publish\backend", "C:/stores/learningtool/backend")
print("   Backend uploaded")

# Configure IIS
print("\n[5/6] Configuring IIS...")
iis_cmd = """
Import-Module WebAdministration

$poolName = 'LearningToolPool'
if (Test-Path IIS:\\AppPools\\$poolName) {
    Remove-WebAppPool -Name $poolName -ErrorAction SilentlyContinue
}
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
print(stdout.read().decode())

# Verify
print("\n[6/6] Verifying deployment...")
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Website LearningToolApp | Select-Object Name, State"')
print(stdout.read().decode())

stdin, stdout, stderr = ssh.exec_command('powershell -Command "netstat -an | Select-String :5192"')
listening = stdout.read().decode().strip()
if listening:
    print("Port 5192: LISTENING")

sftp.close()
ssh.close()

print("\n" + "=" * 60)
print("  Deployment Complete!")
print("=" * 60)
print(f"\nApplication: http://{SSH_HOST}:5192")
print("API: http://{SSH_HOST}:5192/api")
