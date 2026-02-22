#!/usr/bin/env python3
"""Find Certbot installation path on Windows Server"""

import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password='3WsXcFr$7YhNmKi*')

print("Searching for Certbot...")

# Method 1: Check if it's in PATH
print("\n[1] Checking if certbot is in PATH...")
stdin, stdout, stderr = ssh.exec_command('certbot --version')
output = stdout.read().decode()
error = stderr.read().decode()

if "certbot" in output.lower():
    print(f"  [OK] Found in PATH: {output}")
else:
    print(f"  Not in PATH")
    if error:
        print(f"  Error: {error[:200]}")

# Method 2: Check common install locations
print("\n[2] Checking common Windows install locations...")
locations = [
    r'C:\Program Files\Certbot\bin\certbot.exe',
    r'C:\Program Files (x86)\Certbot\bin\certbot.exe',
    r'C:\ProgramData\chocolatey\bin\certbot.exe',
    r'C:\Users\administrator\AppData\Local\Microsoft\WinGet\Packages\Certbot.Certbot_*\certbot.exe',
]

for loc in locations:
    cmd = f'powershell -Command "Test-Path \'{loc}\'"'
    stdin, stdout, stderr = ssh.exec_command(cmd)
    result = stdout.read().decode().strip()

    if result == 'True':
        print(f"  [OK] Found: {loc}")
        # Test it
        stdin, stdout, stderr = ssh.exec_command(f'"{loc}" --version')
        version = stdout.read().decode()
        print(f"       Version: {version.strip()}")
    else:
        print(f"  [ ] Not found: {loc}")

# Method 3: Search WinGet packages directory
print("\n[3] Searching WinGet packages directory...")
cmd = r'powershell -Command "Get-ChildItem -Path C:\Users\administrator\AppData\Local\Microsoft\WinGet\Packages -Recurse -Filter certbot.exe -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName"'
stdin, stdout, stderr = ssh.exec_command(cmd)
winget_path = stdout.read().decode().strip()

if winget_path:
    print(f"  [OK] Found via WinGet: {winget_path}")
    # Test it
    stdin, stdout, stderr = ssh.exec_command(f'"{winget_path}" --version')
    version = stdout.read().decode()
    print(f"       Version: {version.strip()}")

ssh.close()
