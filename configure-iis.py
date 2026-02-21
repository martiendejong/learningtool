#!/usr/bin/env python3
"""Configure IIS for LearningTool"""

import paramiko

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"

def execute_cmd(ssh, cmd, description):
    """Execute command and show output"""
    print(f"\n{description}")
    print("-" * 60)
    stdin, stdout, stderr = ssh.exec_command(cmd, timeout=60)
    output = stdout.read().decode('utf-8', errors='ignore')
    error = stderr.read().decode('utf-8', errors='ignore')

    if output.strip():
        print(output)
    if error.strip():
        print(f"STDERR: {error}")

    return output, error

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

print("=" * 60)
print("  IIS Configuration for LearningTool")
print("=" * 60)

# Step 1: Create Application Pool
cmd1 = """
Import-Module WebAdministration
$poolName = 'LearningToolPool'
if (Test-Path IIS:\\AppPools\\$poolName) {
    Write-Host "Removing existing pool..."
    Remove-WebAppPool -Name $poolName
}
Write-Host "Creating pool..."
New-WebAppPool -Name $poolName
Set-ItemProperty IIS:\\AppPools\\$poolName -Name managedRuntimeVersion -Value ''
Write-Host "Pool created: $poolName"
"""
execute_cmd(ssh, f'powershell -Command "{cmd1}"', "[1/4] Creating Application Pool")

# Step 2: Remove existing site if present
cmd2 = """
Import-Module WebAdministration
$siteName = 'LearningToolApp'
if (Test-Path IIS:\\Sites\\$siteName) {
    Write-Host "Stopping and removing existing site..."
    Stop-Website -Name $siteName -ErrorAction SilentlyContinue
    Remove-Website -Name $siteName
    Write-Host "Removed: $siteName"
} else {
    Write-Host "No existing site found"
}
"""
execute_cmd(ssh, f'powershell -Command "{cmd2}"', "[2/4] Removing Existing Site")

# Step 3: Create Website
cmd3 = """
Import-Module WebAdministration
Write-Host "Creating website..."
New-Website -Name LearningToolApp -PhysicalPath C:\\stores\\learningtool\\www -ApplicationPool LearningToolPool -Port 5192
Write-Host "Website created"
"""
execute_cmd(ssh, f'powershell -Command "{cmd3}"', "[3/4] Creating Website")

# Step 4: Create API Application
cmd4 = """
Import-Module WebAdministration
Write-Host "Creating API application..."
New-WebApplication -Site LearningToolApp -Name api -PhysicalPath C:\\stores\\learningtool\\backend -ApplicationPool LearningToolPool
Write-Host "API application created"
"""
execute_cmd(ssh, f'powershell -Command "{cmd4}"', "[4/4] Creating API Application")

# Verify
print("\n" + "=" * 60)
print("  Verification")
print("=" * 60)

execute_cmd(ssh, 'powershell -Command "Import-Module WebAdministration; Get-Website LearningToolApp | Select-Object Name, State, PhysicalPath | Format-List"', "Website Status")
execute_cmd(ssh, 'powershell -Command "Import-Module WebAdministration; Get-WebApplication -Site LearningToolApp | Select-Object Path, PhysicalPath | Format-List"', "Applications")
execute_cmd(ssh, 'powershell -Command "netstat -an | Select-String \':5192\'"', "Port 5192 Status")

ssh.close()

print("\n" + "=" * 60)
print("  Configuration Complete!")
print("=" * 60)
print("\nApplication: http://85.215.217.154:5192")
print("API: http://85.215.217.154:5192/api")
