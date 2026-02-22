#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

# Create a PowerShell script to update web.config
ps_script = '''
$webconfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\LearningTool.API.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="outofprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="ASPNETCORE_URLS" value="http://localhost:5028" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
"@

Set-Content -Path "C:\stores\learningtool\backend\web.config" -Value $webconfig
Write-Host "web.config updated"
'''

print('=== 1. Execute PowerShell to update web.config ===')
stdin, stdout, stderr = ssh.exec_command(f'powershell -Command "{ps_script}"')
result = stdout.read().decode('utf-8', errors='ignore')
err = stderr.read().decode('utf-8', errors='ignore')
print(result if result else '(no output)')
if err:
    print('Errors:', err[:300])

print('\n=== 2. Verify web.config ===')
stdin, stdout, stderr = ssh.exec_command('type C:\stores\learningtool\backend\web.config')
config = stdout.read().decode('utf-8', errors='ignore')
if 'ASPNETCORE_URLS' in config:
    print('[OK] web.config updated')
else:
    print('[FAIL] web.config not updated')
    print(config[:500])

print('\n=== 3. Restart app pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool -Name LearningToolPool"')
stdout.read()
print('App pool restarted')

print('\n=== 4. Wait for startup ===')
import time
time.sleep(7)

print('\n=== 5. Test endpoints ===')
stdin, stdout, stderr = ssh.exec_command('curl -s http://localhost:5028/api/auth/health')
local = stdout.read().decode('utf-8', errors='ignore')
print('Local (5028):', repr(local))

stdin, stdout, stderr = ssh.exec_command('curl -s -k https://learning.prospergenics.com/api/auth/health')
ext = stdout.read().decode('utf-8', errors='ignore')
print('External:', repr(ext))

print('\n=== 6. Check process listening ===')
stdin, stdout, stderr = ssh.exec_command('netstat -ano | findstr ":5028"')
netstat = stdout.read().decode('utf-8', errors='ignore')
print('Listening:', netstat if netstat else 'NOT FOUND')

ssh.close()
