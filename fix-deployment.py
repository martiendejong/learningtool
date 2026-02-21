#!/usr/bin/env python3
"""Fix the deployment - extract properly and configure IIS"""

import paramiko
import time

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"

def execute_cmd(ssh, command):
    """Execute command and return output"""
    stdin, stdout, stderr = ssh.exec_command(command, timeout=60)
    output = stdout.read().decode('utf-8', errors='ignore')
    error = stderr.read().decode('utf-8', errors='ignore')
    return output, error

print("Connecting to server...")
ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

print("\n[1/5] Extracting archive properly...")
cmd = """
cd C:\\Temp
if (Test-Path learningtool-deploy) { Remove-Item learningtool-deploy -Recurse -Force }
mkdir learningtool-deploy
cd learningtool-deploy
tar -xzf ..\\learningtool-deploy.tar.gz
Get-ChildItem | Select-Object Name
"""
out, err = execute_cmd(ssh, f'powershell -Command "{cmd}"')
print(out)

print("\n[2/5] Deploying to C:\\stores\\learningtool...")
cmd = """
if (Test-Path C:\\stores\\learningtool) { Remove-Item C:\\stores\\learningtool -Recurse -Force }
New-Item -ItemType Directory -Path C:\\stores\\learningtool -Force
New-Item -ItemType Directory -Path C:\\stores\\learningtool\\backend -Force
New-Item -ItemType Directory -Path C:\\stores\\learningtool\\www -Force
New-Item -ItemType Directory -Path C:\\stores\\learningtool\\data -Force

Copy-Item C:\\Temp\\learningtool-deploy\\backend\\* C:\\stores\\learningtool\\backend -Recurse -Force
Copy-Item C:\\Temp\\learningtool-deploy\\frontend\\* C:\\stores\\learningtool\\www -Recurse -Force

Write-Host 'Files deployed'
Test-Path C:\\stores\\learningtool\\www\\index.html
"""
out, err = execute_cmd(ssh, f'powershell -Command "{cmd}"')
print(out)

print("\n[3/5] Creating IIS application pool...")
cmd = """
Import-Module WebAdministration
$poolName = 'LearningToolPool'
if (Test-Path IIS:\\AppPools\\$poolName) {
    Remove-WebAppPool -Name $poolName
}
New-WebAppPool -Name $poolName
Set-ItemProperty IIS:\\AppPools\\$poolName -Name managedRuntimeVersion -Value ''
Write-Host 'Pool created'
"""
out, err = execute_cmd(ssh, f'powershell -Command "{cmd}"')
print(out)

print("\n[4/5] Creating IIS website...")
cmd = """
Import-Module WebAdministration
$siteName = 'LearningToolApp'
if (Test-Path IIS:\\Sites\\$siteName) {
    Remove-Website -Name $siteName
}
New-Website -Name $siteName -PhysicalPath C:\\stores\\learningtool\\www -ApplicationPool LearningToolPool -Port 5192
Start-Website -Name $siteName
Write-Host 'Website created on port 5192'
"""
out, err = execute_cmd(ssh, f'powershell -Command "{cmd}"')
print(out)

print("\n[5/5] Creating API application...")
cmd = """
Import-Module WebAdministration
New-WebApplication -Site LearningToolApp -Name api -PhysicalPath C:\\stores\\learningtool\\backend -ApplicationPool LearningToolPool
Write-Host 'API application created at /api'
Get-Website LearningToolApp
"""
out, err = execute_cmd(ssh, f'powershell -Command "{cmd}"')
print(out)

ssh.close()

print("\n=== Deployment Fixed! ===")
print(f"Application URL: http://{SSH_HOST}:5192")
print("\nTest in browser now!")
