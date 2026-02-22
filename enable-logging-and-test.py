#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import paramiko
import sys
import time

# Set UTF-8 output
if sys.platform == "win32":
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Step 1: Enable stdout logging in web.config ===')
new_config = r'''<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\LearningTool.API.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
<!--ProjectGuid: 321FF5C8-C4A8-492F-A2B1-DB6C8C7C8184-->'''

# Write new web.config
stdin, stdout, stderr = ssh.exec_command(f'echo {new_config} > C:\\stores\\learningtool\\backend\\web.config')
stdout.read()  # Wait for command
print('✓ Enabled stdoutLogEnabled="true"')

print('\n=== Step 2: Recycle app pool to force reload ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool -Name LearningToolPool"')
result = stdout.read().decode('utf-8', errors='ignore')
print('✓ App pool recycled')

time.sleep(3)

print('\n=== Step 3: Trigger API request to generate logs ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -H "Host: learning.prospergenics.com" http://localhost/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
print(f'Response: {response if response else "Empty"}')

time.sleep(2)

print('\n=== Step 4: Check latest log file ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Select-Object Name,LastWriteTime"')
latest_log = stdout.read().decode('utf-8', errors='ignore')
print(latest_log)

print('\n=== Step 5: Show last 30 lines of latest log ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content -Tail 30"')
log_content = stdout.read().decode('utf-8', errors='replace')
print(log_content)

ssh.close()
