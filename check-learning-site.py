#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Finding learning.prospergenics.com site ===')
stdin, stdout, stderr = ssh.exec_command(r'''powershell -Command "
Import-Module WebAdministration
Get-WebBinding | Where-Object { $_.bindingInformation -like '*learning.prospergenics.com' } | ForEach-Object {
    $siteName = $_.ItemXPath -replace '.*name=''([^'']+)''.*', '$1'
    $site = Get-Website -Name $siteName
    Write-Host \"Site Name: $siteName\"
    Write-Host \"Physical Path: $($site.physicalPath)\"
    Write-Host \"State: $($site.state)\"
    Write-Host \"Bindings:\"
    Get-WebBinding -Name $siteName | Format-Table protocol,bindingInformation
}
"''')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Check if files exist at that location ===')
stdin, stdout, stderr = ssh.exec_command('dir C:\\inetpub\\wwwroot\\learning.prospergenics.com 2>&1')
result = stdout.read().decode('utf-8', errors='ignore')
if 'File Not Found' in result or 'cannot find' in result.lower():
    print('Directory does not exist - need to create it')
else:
    print(result)

ssh.close()
