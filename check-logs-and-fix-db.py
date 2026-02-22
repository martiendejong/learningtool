#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== 1. Check recent log files ===')
stdin, stdout, stderr = ssh.exec_command('dir C:\\stores\\learningtool\\backend\\logs /od')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== 2. Read most recent log ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-ChildItem C:\\stores\\learningtool\\backend\\logs\\*.log | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content -Tail 50"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== 3. Add missing columns using PowerShell ===')
commands = [
    'ALTER TABLE Courses ADD COLUMN LearningPlan TEXT;',
    'ALTER TABLE Courses ADD COLUMN SystemPrompt TEXT;',
    'ALTER TABLE Courses ADD COLUMN ContentGeneratedAt TEXT;'
]

for cmd in commands:
    print(f'Executing: {cmd}')
    sql_cmd = f'powershell -Command "Add-Type -Path C:\\stores\\learningtool\\backend\\Microsoft.Data.Sqlite.dll; $conn = New-Object Microsoft.Data.Sqlite.SqliteConnection(\'Data Source=C:\\stores\\learningtool\\backend\\learningtool.db\'); $conn.Open(); $cmd = $conn.CreateCommand(); $cmd.CommandText = \'{cmd}\'; try {{ $cmd.ExecuteNonQuery(); Write-Host \'SUCCESS\' }} catch {{ Write-Host \'ERROR:\' $_.Exception.Message }}; $conn.Close()"'
    stdin, stdout, stderr = ssh.exec_command(sql_cmd)
    result = stdout.read().decode('utf-8', errors='ignore')
    error = stderr.read().decode('utf-8', errors='ignore')
    print('Result:', result)
    if error:
        print('Error:', error)

print('\n=== 4. Verify columns added ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Add-Type -Path C:\\stores\\learningtool\\backend\\Microsoft.Data.Sqlite.dll; $conn = New-Object Microsoft.Data.Sqlite.SqliteConnection(\'Data Source=C:\\stores\\learningtool\\backend\\learningtool.db\'); $conn.Open(); $cmd = $conn.CreateCommand(); $cmd.CommandText = \'PRAGMA table_info(Courses);\'; $reader = $cmd.ExecuteReader(); while ($reader.Read()) { Write-Host $reader[\'name\'] }; $conn.Close()"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== 5. Restart app pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool -Name LearningToolPool"')
print(stdout.read().decode('utf-8', errors='ignore'))
print('App pool restarted')

ssh.close()
