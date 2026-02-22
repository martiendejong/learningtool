#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== 1. Check if dotnet CLI is available ===')
stdin, stdout, stderr = ssh.exec_command('dotnet --version')
print('dotnet version:', stdout.read().decode('utf-8', errors='ignore'))

print('\n=== 2. Look for migration files in backend ===')
stdin, stdout, stderr = ssh.exec_command('dir C:\\stores\\learningtool\\backend\\*Migration*.* /s /b')
migrations = stdout.read().decode('utf-8', errors='ignore')
print('Migration files:', migrations if migrations else 'None found')

print('\n=== 3. Check local project for migrations ===')
stdin, stdout, stderr = ssh.exec_command('dir C:\\Projects\\learningtool\\src\\LearningTool.Infrastructure\\Migrations /b')
local_migrations = stdout.read().decode('utf-8', errors='ignore')
print('Local migrations:', local_migrations if local_migrations else 'Not found')

print('\n=== 4. Try using Python sqlite3 to add columns ===')
python_script = r'''
import sqlite3
import sys

try:
    conn = sqlite3.connect(r'C:\stores\learningtool\backend\learningtool.db')
    cursor = conn.cursor()

    commands = [
        "ALTER TABLE Courses ADD COLUMN LearningPlan TEXT",
        "ALTER TABLE Courses ADD COLUMN SystemPrompt TEXT",
        "ALTER TABLE Courses ADD COLUMN ContentGeneratedAt TEXT"
    ]

    for cmd in commands:
        try:
            cursor.execute(cmd)
            print(f"SUCCESS: {cmd}")
        except Exception as e:
            print(f"ERROR: {cmd} - {str(e)}")

    conn.commit()
    conn.close()
    print("Database updated successfully")
except Exception as e:
    print(f"FATAL ERROR: {str(e)}")
    sys.exit(1)
'''

stdin, stdout, stderr = ssh.exec_command(f'echo {repr(python_script)} | python')
result = stdout.read().decode('utf-8', errors='ignore')
err = stderr.read().decode('utf-8', errors='ignore')
print('\nPython execution result:')
print(result)
if err:
    print('Errors:', err)

print('\n=== 5. Verify columns were added ===')
verify_script = r'''
import sqlite3
conn = sqlite3.connect(r'C:\stores\learningtool\backend\learningtool.db')
cursor = conn.cursor()
cursor.execute("PRAGMA table_info(Courses)")
for row in cursor.fetchall():
    print(row[1])  # column name
conn.close()
'''

stdin, stdout, stderr = ssh.exec_command(f'echo {repr(verify_script)} | python')
columns = stdout.read().decode('utf-8', errors='ignore')
print('Courses table columns:')
print(columns)

ssh.close()
