#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Testing Health Endpoint ===')
stdin, stdout, stderr = ssh.exec_command('curl -s http://localhost:5192/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')

if 'healthy' in response.lower():
    print('SUCCESS - API is healthy!')
    print(response)
elif '<!DOCTYPE' in response:
    print('ERROR - Getting HTML error page, not JSON')
    # Get the actual error
    stdin2, stdout2, stderr2 = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -Source \\"IIS AspNetCore Module V2\\" -Newest 1 | Select-Object -ExpandProperty Message"')
    error_msg = stdout2.read().decode('utf-8', errors='ignore')
    if 'readonly database' in error_msg:
        print('\nStill getting readonly database error despite permissions!')
        print('\nLet me try commenting out the auto-migration in Program.cs...')
    else:
        print('\nLatest error:')
        print(error_msg[:1000])
else:
    print(f'Unexpected response ({len(response)} chars):')
    print(response[:500])

ssh.close()
