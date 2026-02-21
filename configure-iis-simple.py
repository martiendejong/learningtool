#!/usr/bin/env python3
"""Configure IIS for LearningTool - Simple version"""

import paramiko

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"

def run_cmd(ssh, cmd, desc):
    print(f"\n{desc}...")
    stdin, stdout, stderr = ssh.exec_command(cmd, timeout=60)
    out = stdout.read().decode('utf-8', errors='ignore')
    err = stderr.read().decode('utf-8', errors='ignore')
    if out.strip():
        print(out)
    if err.strip():
        print(f"ERROR: {err}")
    return out, err

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

print("=" * 60)
print("  IIS Configuration for LearningTool")
print("=" * 60)

# Step 1: Create or reset app pool
run_cmd(ssh, 'powershell -Command "Import-Module WebAdministration; if (Test-Path IIS:\\AppPools\\LearningToolPool) { Remove-WebAppPool -Name LearningToolPool }; New-WebAppPool -Name LearningToolPool; Set-ItemProperty IIS:\\AppPools\\LearningToolPool -Name managedRuntimeVersion -Value \'\'"',
        "[1/5] Creating Application Pool")

# Step 2: Remove existing site if any
run_cmd(ssh, 'powershell -Command "Import-Module WebAdministration; if (Test-Path IIS:\\Sites\\LearningToolApp) { Stop-Website -Name LearningToolApp -EA SilentlyContinue; Remove-Website -Name LearningToolApp }"',
        "[2/5] Removing Existing Site")

# Step 3: Create website
run_cmd(ssh, 'powershell -Command "Import-Module WebAdministration; New-Website -Name LearningToolApp -PhysicalPath C:\\stores\\learningtool\\www -ApplicationPool LearningToolPool -Port 5192"',
        "[3/5] Creating Website")

# Step 4: Create API app
run_cmd(ssh, 'powershell -Command "Import-Module WebAdministration; New-WebApplication -Site LearningToolApp -Name api -PhysicalPath C:\\stores\\learningtool\\backend -ApplicationPool LearningToolPool"',
        "[4/5] Creating API Application")

# Step 5: Start website
run_cmd(ssh, 'powershell -Command "Import-Module WebAdministration; Start-Website -Name LearningToolApp"',
        "[5/5] Starting Website")

print("\n" + "=" * 60)
print("  Verification")
print("=" * 60)

run_cmd(ssh, 'powershell -Command "Import-Module WebAdministration; Get-Website LearningToolApp"',
        "Website Status")

run_cmd(ssh, 'powershell -Command "netstat -an | Select-String :5192"',
        "Port 5192")

ssh.close()

print("\n" + "=" * 60)
print("  Done!")
print("=" * 60)
print("Application: http://85.215.217.154:5192")
