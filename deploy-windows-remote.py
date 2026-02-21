#!/usr/bin/env python3
"""
Deploy LearningTool to Windows Server (85.215.217.154)
Uses SSH to execute PowerShell commands remotely
"""

import paramiko
import time
from pathlib import Path

# Configuration
SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"
ARCHIVE_LOCAL = r"C:\Projects\learningtool\learningtool-deploy.tar.gz"
ARCHIVE_REMOTE = r"C:\Temp\learningtool-deploy.tar.gz"

def print_step(step, total, msg):
    print(f"\n[{step}/{total}] {msg}")
    print("-" * 60)

def execute_powershell(ssh, command, timeout=300):
    """Execute PowerShell command via SSH"""
    ps_command = f'powershell -Command "{command}"'
    stdin, stdout, stderr = ssh.exec_command(ps_command, timeout=timeout)

    # Stream output
    while not stdout.channel.exit_status_ready():
        if stdout.channel.recv_ready():
            line = stdout.channel.recv(1024).decode('utf-8', errors='ignore')
            print(line, end='')
        time.sleep(0.1)

    output = stdout.read().decode('utf-8', errors='ignore')
    error = stderr.read().decode('utf-8', errors='ignore')

    return output, error, stdout.channel.recv_exit_status()

def main():
    print("\n" + "="*60)
    print("  LearningTool Windows Server Deployment")
    print("="*60)
    print(f"\nTarget: {SSH_HOST}")
    print(f"Time: {time.strftime('%Y-%m-%d %H:%M:%S')}\n")

    # Connect to server
    print_step(1, 7, "Connecting to Windows Server...")
    ssh = paramiko.SSHClient()
    ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())

    try:
        ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS, timeout=30)
        print("   Connected!")
    except Exception as e:
        print(f"   Connection failed: {e}")
        return False

    # Upload archive (already done, skip)
    print_step(2, 7, "Archive already uploaded to C:\\Temp")

    # Extract archive
    print_step(3, 7, "Extracting deployment package...")
    extract_cmd = """
    $tempPath = 'C:\\Temp\\learningtool-deploy'
    if (Test-Path $tempPath) { Remove-Item $tempPath -Recurse -Force }
    New-Item -ItemType Directory -Path $tempPath -Force | Out-Null
    tar -xzf 'C:\\Temp\\learningtool-deploy.tar.gz' -C $tempPath
    Write-Host 'Extracted to C:\\Temp\\learningtool-deploy'
    """
    output, error, code = execute_powershell(ssh, extract_cmd)
    if output: print(output)
    if code == 0:
        print("   Extraction complete!")

    # Create directories
    print_step(4, 7, "Creating application directories...")
    mkdir_cmd = """
    $appPath = 'C:\\inetpub\\wwwroot\\learningtool'
    New-Item -ItemType Directory -Path $appPath -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $appPath 'backend') -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $appPath 'frontend') -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $appPath 'data') -Force | Out-Null
    Write-Host 'Directories created at C:\\inetpub\\wwwroot\\learningtool'
    """
    output, error, code = execute_powershell(ssh, mkdir_cmd)
    if output: print(output)

    # Deploy backend
    print_step(5, 7, "Deploying backend...")
    deploy_backend_cmd = """
    $source = 'C:\\Temp\\learningtool-deploy\\backend\\*'
    $dest = 'C:\\inetpub\\wwwroot\\learningtool\\backend'
    Copy-Item -Path $source -Destination $dest -Recurse -Force
    $size = (Get-ChildItem $dest -Recurse | Measure-Object Length -Sum).Sum / 1MB
    Write-Host "Backend deployed: $($size.ToString('F2')) MB"
    """
    output, error, code = execute_powershell(ssh, deploy_backend_cmd)
    if output: print(output)

    # Deploy frontend
    print_step(6, 7, "Deploying frontend...")
    deploy_frontend_cmd = """
    $source = 'C:\\Temp\\learningtool-deploy\\frontend\\*'
    $dest = 'C:\\inetpub\\wwwroot\\learningtool\\frontend'
    Copy-Item -Path $source -Destination $dest -Recurse -Force
    $size = (Get-ChildItem $dest -Recurse | Measure-Object Length -Sum).Sum / 1MB
    Write-Host "Frontend deployed: $($size.ToString('F2')) MB"
    """
    output, error, code = execute_powershell(ssh, deploy_frontend_cmd)
    if output: print(output)

    # Configure IIS
    print_step(7, 7, "Configuring IIS...")
    iis_cmd = """
    Import-Module WebAdministration -ErrorAction SilentlyContinue

    # Create application pool
    $poolName = 'LearningToolPool'
    if (-not (Test-Path \"IIS:\\AppPools\\$poolName\")) {
        New-WebAppPool -Name $poolName
        Set-ItemProperty \"IIS:\\AppPools\\$poolName\" -Name managedRuntimeVersion -Value \"\"
        Write-Host \"Created application pool: $poolName\"
    }

    # Stop existing site if exists
    $siteName = 'LearningTool'
    if (Test-Path \"IIS:\\Sites\\$siteName\") {
        Stop-Website -Name $siteName -ErrorAction SilentlyContinue
        Remove-Website -Name $siteName -ErrorAction SilentlyContinue
        Write-Host \"Removed existing site\"
    }

    # Create new website
    $frontendPath = 'C:\\inetpub\\wwwroot\\learningtool\\frontend'
    New-Website -Name $siteName -PhysicalPath $frontendPath -ApplicationPool $poolName -Port 8080
    Write-Host \"Created website on port 8080\"

    # Create backend API application
    $backendPath = 'C:\\inetpub\\wwwroot\\learningtool\\backend'
    New-WebApplication -Site $siteName -Name 'api' -PhysicalPath $backendPath -ApplicationPool $poolName
    Write-Host \"Created API application at /api\"

    # Start website
    Start-Website -Name $siteName
    Write-Host \"Website started\"
    """
    output, error, code = execute_powershell(ssh, iis_cmd, timeout=60)
    if output: print(output)
    if error and "warning" not in error.lower():
        print(f"Errors: {error}")

    ssh.close()

    print("\n" + "="*60)
    print("  Deployment Complete!")
    print("="*60)
    print(f"\nApplication URL: http://{SSH_HOST}:8080")
    print("\nNext steps:")
    print("  1. Test in browser")
    print("  2. Configure SSL certificate in IIS")
    print("  3. Update DNS: learning.prospergenics.com → 85.215.217.154")
    print("  4. Change IIS binding from port 8080 to 443 (HTTPS)")
    print("")

    return True

if __name__ == "__main__":
    import sys
    success = main()
    sys.exit(0 if success else 1)
