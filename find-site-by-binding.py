#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== All IIS Sites ===')
stdin, stdout, stderr = ssh.exec_command(r'''powershell -Command "
Import-Module WebAdministration
Get-Website | ForEach-Object {
    Write-Host \"======================================\"
    Write-Host \"Site: $($_.name)\"
    Write-Host \"Path: $($_.physicalPath)\"
    Write-Host \"State: $($_.state)\"
    Write-Host \"Bindings:\"
    Get-WebBinding -Name $_.name | Select-Object protocol,bindingInformation | Format-Table -AutoSize
}
"''')
print(stdout.read().decode('utf-8', errors='ignore'))

ssh.close()
