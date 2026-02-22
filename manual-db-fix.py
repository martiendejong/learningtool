#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Checking if CourseId column exists ===')
stdin, stdout, stderr = ssh.exec_command(r'''powershell -Command "
$db = 'C:\stores\learningtool\data\learningtool.db'
$query = 'PRAGMA table_info(ChatMessages);'
Add-Type -Path 'C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Data.SQLite\v4.0_1.0.118.0__db937bc2d44ff139\System.Data.SQLite.dll'
$conn = New-Object System.Data.SQLite.SQLiteConnection(\"Data Source=$db\")
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = $query
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Host $reader[1] $reader[2]
}
$conn.Close()
"''')
columns = stdout.read().decode('utf-8', errors='ignore')
print(columns)

if 'CourseId' not in columns:
    print('\n=== Adding CourseId column ===')
    stdin, stdout, stderr = ssh.exec_command(r'''powershell -Command "
$db = 'C:\stores\learningtool\data\learningtool.db'
Add-Type -Path 'C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Data.SQLite\v4.0_1.0.118.0__db937bc2d44ff139\System.Data.SQLite.dll'
$conn = New-Object System.Data.SQLite.SQLiteConnection(\"Data Source=$db\")
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = 'ALTER TABLE ChatMessages ADD COLUMN CourseId INTEGER NULL;'
$cmd.ExecuteNonQuery()
$conn.Close()
Write-Host 'Column added successfully'
"''')
    print(stdout.read().decode('utf-8', errors='ignore'))
    err = stderr.read().decode('utf-8', errors='ignore')
    if err:
        print('ERROR:', err)

    # Mark migration as applied
    print('\n=== Marking migration as applied ===')
    stdin, stdout, stderr = ssh.exec_command(r'''powershell -Command "
$db = 'C:\stores\learningtool\data\learningtool.db'
Add-Type -Path 'C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Data.SQLite\v4.0_1.0.118.0__db937bc2d44ff139\System.Data.SQLite.dll'
$conn = New-Object System.Data.SQLite.SQLiteConnection(\"Data Source=$db\")
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = \"INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260221213536_AddCourseIdToChatMessages', '8.0.0');\"
$cmd.ExecuteNonQuery()
$conn.Close()
Write-Host 'Migration marked as applied'
"''')
    print(stdout.read().decode('utf-8', errors='ignore'))
else:
    print('CourseId column already exists')

print('\n=== Restart App Pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool LearningToolPool"')
print('Restarted')

import time
time.sleep(3)

print('\n=== Test API ===')
stdin, stdout, stderr = ssh.exec_command('curl -s http://localhost:5192/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
if response and not ('error' in response.lower() or '<!DOCTYPE' in response):
    print('SUCCESS! API is responding:')
    print(response[:200])
else:
    print('Still getting error - trying to fetch details...')
    stdin, stdout, stderr = ssh.exec_command('curl -s http://localhost:5192/api/auth/health 2>&1')
    print(stdout.read().decode('utf-8', errors='ignore')[:500])

ssh.close()
