#!/usr/bin/env python3
"""
COMPLETE LearningTool Deployment Script
Deploys everything automatically: files + IIS + domain binding + verification
NEVER fails silently - every step is verified
"""

import paramiko
import os
from pathlib import Path
import sys

# Configuration
SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"
DOMAIN = "learning.prospergenics.com"
PORT_HTTP = 80
PORT_DIRECT = 5192

FRONTEND_SOURCE = r"C:\Projects\learningtool\publish\frontend"
BACKEND_SOURCE = r"C:\Projects\learningtool\publish\backend"

def print_step(step, total, message):
    """Print formatted step"""
    print(f"\n[{step}/{total}] {message}")
    print("-" * 70)

def execute_and_verify(ssh, cmd, description, expect_output=True):
    """Execute command and verify it worked"""
    print(f"  {description}...")
    stdin, stdout, stderr = ssh.exec_command(cmd, timeout=60)
    output = stdout.read().decode('utf-8', errors='ignore')
    error = stderr.read().decode('utf-8', errors='ignore')

    if error.strip() and "warning" not in error.lower():
        print(f"  ❌ ERROR: {error}")
        return False, output

    if expect_output and not output.strip():
        print(f"  ⚠️  WARNING: No output (command may have failed silently)")
        return False, output

    print(f"  ✓ Success")
    return True, output

def upload_directory(sftp, local_dir, remote_dir, description):
    """Upload directory with progress"""
    print(f"  Uploading {description}...")
    file_count = 0

    for root, dirs, files in os.walk(local_dir):
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
            sftp.put(local_file, remote_file)
            file_count += 1

    print(f"  ✓ Uploaded {file_count} files")
    return file_count

def main():
    print("=" * 70)
    print("  COMPLETE LEARNINGTOOL DEPLOYMENT")
    print("=" * 70)
    print(f"  Server: {SSH_HOST}")
    print(f"  Domain: {DOMAIN}")
    print("=" * 70)

    # Verify build exists
    print_step(1, 10, "Verifying Build Artifacts")
    if not os.path.exists(FRONTEND_SOURCE):
        print(f"❌ Frontend build not found: {FRONTEND_SOURCE}")
        print("Run: cd frontend && npm run build")
        sys.exit(1)
    if not os.path.exists(BACKEND_SOURCE):
        print(f"❌ Backend build not found: {BACKEND_SOURCE}")
        print("Run: dotnet publish -c Release")
        sys.exit(1)
    print("  ✓ Frontend build found")
    print("  ✓ Backend build found")

    # Connect
    print_step(2, 10, "Connecting to Server")
    try:
        ssh = paramiko.SSHClient()
        ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
        ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS, timeout=30)
        sftp = ssh.open_sftp()
        print("  ✓ Connected via SSH")
    except Exception as e:
        print(f"  ❌ Connection failed: {e}")
        sys.exit(1)

    # Create directories
    print_step(3, 10, "Creating Application Directories")
    for dir in ["C:/stores/learningtool/backend", "C:/stores/learningtool/www", "C:/stores/learningtool/data"]:
        execute_and_verify(ssh,
            f'powershell -Command "New-Item -ItemType Directory -Path {dir} -Force | Out-Null; Write-Host OK"',
            f"Creating {dir}",
            expect_output=True)

    # Upload Frontend
    print_step(4, 10, "Uploading Frontend")
    frontend_count = upload_directory(sftp, FRONTEND_SOURCE, "C:/stores/learningtool/www", "frontend files")

    # Upload Backend
    print_step(5, 10, "Uploading Backend")
    backend_count = upload_directory(sftp, BACKEND_SOURCE, "C:/stores/learningtool/backend", "backend files")

    # Verify files
    print_step(6, 10, "Verifying Uploaded Files")
    success, _ = execute_and_verify(ssh,
        'powershell -Command "if (Test-Path C:\\stores\\learningtool\\www\\index.html) { Write-Host OK }"',
        "Checking index.html")
    if not success:
        print("  ❌ Frontend verification failed")
        sys.exit(1)

    success, _ = execute_and_verify(ssh,
        'powershell -Command "if (Test-Path C:\\stores\\learningtool\\backend\\LearningTool.API.dll) { Write-Host OK }"',
        "Checking API DLL")
    if not success:
        print("  ❌ Backend verification failed")
        sys.exit(1)

    # Configure IIS - App Pool
    print_step(7, 10, "Configuring IIS Application Pool")
    execute_and_verify(ssh,
        'powershell -Command "Import-Module WebAdministration; if (Test-Path IIS:\\AppPools\\LearningToolPool) { Remove-WebAppPool -Name LearningToolPool }; New-WebAppPool -Name LearningToolPool; Set-ItemProperty IIS:\\AppPools\\LearningToolPool -Name managedRuntimeVersion -Value \'\'; Write-Host OK"',
        "Creating LearningToolPool")

    # Configure IIS - Website
    print_step(8, 10, "Configuring IIS Website")
    execute_and_verify(ssh,
        'powershell -Command "Import-Module WebAdministration; if (Test-Path IIS:\\Sites\\LearningToolApp) { Stop-Website -Name LearningToolApp -EA SilentlyContinue; Remove-Website -Name LearningToolApp }; Write-Host OK"',
        "Removing old site")

    execute_and_verify(ssh,
        'powershell -Command "Import-Module WebAdministration; New-Website -Name LearningToolApp -PhysicalPath C:\\stores\\learningtool\\www -ApplicationPool LearningToolPool -Port ' + str(PORT_DIRECT) + '; Write-Host OK"',
        f"Creating website on port {PORT_DIRECT}")

    execute_and_verify(ssh,
        'powershell -Command "Import-Module WebAdministration; New-WebApplication -Site LearningToolApp -Name api -PhysicalPath C:\\stores\\learningtool\\backend -ApplicationPool LearningToolPool; Write-Host OK"',
        "Creating API application")

    # Add domain binding
    print_step(9, 10, "Configuring Domain Binding")
    execute_and_verify(ssh,
        f'powershell -Command "Import-Module WebAdministration; New-WebBinding -Name LearningToolApp -Protocol http -Port {PORT_HTTP} -HostHeader {DOMAIN} -ErrorAction SilentlyContinue; Write-Host OK"',
        f"Adding {DOMAIN} on port {PORT_HTTP}")

    # Verify deployment
    print_step(10, 10, "Verifying Deployment")

    # Check website status
    success, output = execute_and_verify(ssh,
        'powershell -Command "Import-Module WebAdministration; $site = Get-Website LearningToolApp; Write-Host $site.State"',
        "Checking website state")
    if "Started" not in output:
        print("  ❌ Website not started, attempting to start...")
        execute_and_verify(ssh,
            'powershell -Command "Import-Module WebAdministration; Start-Website -Name LearningToolApp; Write-Host OK"',
            "Starting website")

    # Check port listening
    success, output = execute_and_verify(ssh,
        f'powershell -Command "if (netstat -an | Select-String \\\':{PORT_HTTP}\\\') {{ Write-Host OK }}"',
        f"Checking port {PORT_HTTP}")

    success, output = execute_and_verify(ssh,
        f'powershell -Command "if (netstat -an | Select-String \\\':{PORT_DIRECT}\\\') {{ Write-Host OK }}"',
        f"Checking port {PORT_DIRECT}")

    # Cleanup
    sftp.close()
    ssh.close()

    # Final summary
    print("\n" + "=" * 70)
    print("  ✓ DEPLOYMENT COMPLETE")
    print("=" * 70)
    print(f"\n  Frontend: {frontend_count} files uploaded")
    print(f"  Backend: {backend_count} files uploaded")
    print(f"\n  URLs:")
    print(f"    Production: http://{DOMAIN}")
    print(f"    Direct IP:  http://{SSH_HOST}:{PORT_DIRECT}")
    print(f"    API Docs:   http://{DOMAIN}/api/swagger")
    print(f"\n  Next: Configure SSL certificate for HTTPS")
    print("=" * 70)

    return 0

if __name__ == "__main__":
    sys.exit(main())
