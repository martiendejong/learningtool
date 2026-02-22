#!/usr/bin/env python3
import paramiko
import json

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== 1. Check database schema ===')
stdin, stdout, stderr = ssh.exec_command('sqlite3 C:\\stores\\learningtool\\identity.db ".schema Courses"')
schema = stdout.read().decode('utf-8', errors='ignore')
print(schema)

if 'LearningPlan' in schema and 'SystemPrompt' in schema and 'ContentGeneratedAt' in schema:
    print('[OK] All new columns present in database')
else:
    print('[FAIL] Missing columns in database')
    print('  LearningPlan:', 'LearningPlan' in schema)
    print('  SystemPrompt:', 'SystemPrompt' in schema)
    print('  ContentGeneratedAt:', 'ContentGeneratedAt' in schema)

print('\n=== 2. Test chat endpoint (health) ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -k https://learning.prospergenics.com/api/auth/health')
health = stdout.read().decode('utf-8', errors='ignore')
print('Health check:', health)

print('\n=== 3. Check recent application logs ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Content C:\\stores\\learningtool\\logs\\*.log -Tail 30 -ErrorAction SilentlyContinue"')
logs = stdout.read().decode('utf-8', errors='ignore')
if logs.strip():
    print('Recent logs:')
    print(logs)
else:
    print('No recent logs found')

print('\n=== 4. Check Event Viewer for errors (last 5) ===')
stdin, stdout, stderr = ssh.exec_command(r'''powershell -Command "Get-EventLog -LogName Application -Source 'ASP.NET Core*' -EntryType Error -Newest 5 -ErrorAction SilentlyContinue | Select-Object TimeGenerated,Message | ConvertTo-Json"''')
events = stdout.read().decode('utf-8', errors='ignore')
if events.strip() and events.strip() != '':
    try:
        event_data = json.loads(events)
        print('Recent errors:')
        print(json.dumps(event_data, indent=2))
    except:
        print('No recent errors or unable to parse')
else:
    print('No recent errors')

ssh.close()
