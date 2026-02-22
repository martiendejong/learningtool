#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Checking App Pool Environment Variables ===')
stdin, stdout, stderr = ssh.exec_command(r'''powershell -Command "
Import-Module WebAdministration
$config = Get-WebConfiguration /system.applicationHost/applicationPools/add[@name='LearningToolPool']/environmentVariables
$config.Collection | Format-Table name,value
"''')
print(stdout.read().decode('utf-8', errors='ignore'))
err = stderr.read().decode('utf-8', errors='ignore')
if err:
    print('ERROR:', err)

print('\n=== Setting via web.config instead ===')
stdin, stdout, stderr = ssh.exec_command(r'''powershell -Command "
$webConfigPath = 'C:\stores\learningtool\backend\web.config'
$webConfig = [xml](Get-Content $webConfigPath)

# Find or create environmentVariables node
$aspNetCore = $webConfig.configuration.'system.webServer'.aspNetCore
if (-not $aspNetCore.environmentVariables) {
    $envVars = $webConfig.CreateElement('environmentVariables')
    $aspNetCore.AppendChild($envVars) | Out-Null
} else {
    $envVars = $aspNetCore.environmentVariables
}

# Remove existing ASPNETCORE_ENVIRONMENT if present
$existing = $envVars.SelectSingleNode('environmentVariable[@name=\"ASPNETCORE_ENVIRONMENT\"]')
if ($existing) {
    $envVars.RemoveChild($existing) | Out-Null
}

# Add new ASPNETCORE_ENVIRONMENT=Production
$newEnv = $webConfig.CreateElement('environmentVariable')
$newEnv.SetAttribute('name', 'ASPNETCORE_ENVIRONMENT')
$newEnv.SetAttribute('value', 'Production')
$envVars.AppendChild($newEnv) | Out-Null

$webConfig.Save($webConfigPath)
Write-Host 'web.config updated with ASPNETCORE_ENVIRONMENT=Production'

# Show the updated section
$updated = [xml](Get-Content $webConfigPath)
$updated.configuration.'system.webServer'.aspNetCore.environmentVariables.environmentVariable | Format-Table name,value
"''')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Restart App Pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool LearningToolPool"')
print('Restarted')

import time
time.sleep(5)

print('\n=== Test ===')
stdin, stdout, stderr = ssh.exec_command('curl -s http://localhost:5192/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
if 'healthy' in response.lower():
    print('SUCCESS!')
elif response and len(response) < 200:
    print('Response:', response)
else:
    print('Still getting error page')

    # Check if migration is being attempted
    stdin2, stdout2, stderr2 = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -Source \\".NET Runtime\\" -Newest 1 | Select-Object -ExpandProperty Message"')
    error = stdout2.read().decode('utf-8', errors='ignore')
    if 'Applying migration' in error or 'line 152' in error:
        print('\nSTILL trying to run migration at line 152!')
    else:
        print('\nDifferent error now')

ssh.close()
