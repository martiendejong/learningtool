#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

# Create Python script on server
script_content = '''import sqlite3
import sys

try:
    conn = sqlite3.connect(r"C:\\stores\\learningtool\\backend\\learningtool.db")
    cursor = conn.cursor()

    commands = [
        "ALTER TABLE Courses ADD COLUMN LearningPlan TEXT",
        "ALTER TABLE Courses ADD COLUMN SystemPrompt TEXT",
        "ALTER TABLE Courses ADD COLUMN ContentGeneratedAt TEXT"
    ]

    for cmd in commands:
        try:
            cursor.execute(cmd)
            print("SUCCESS: " + cmd)
        except Exception as e:
            print("ERROR: " + cmd + " - " + str(e))

    conn.commit()
    conn.close()
    print("Database updated successfully")
except Exception as e:
    print("FATAL ERROR: " + str(e))
    sys.exit(1)
'''

print('=== 1. Create Python script on server ===')
sftp = ssh.open_sftp()
with sftp.file('C:\\stores\\learningtool\\backend\\add_columns.py', 'w') as f:
    f.write(script_content)
sftp.close()
print('Script created')

print('\n=== 2. Execute Python script ===')
stdin, stdout, stderr = ssh.exec_command('python C:\\stores\\learningtool\\backend\\add_columns.py')
result = stdout.read().decode('utf-8', errors='ignore')
err = stderr.read().decode('utf-8', errors='ignore')
print('Result:')
print(result)
if err:
    print('Errors:', err)

print('\n=== 3. Verify columns ===')
verify_script = '''import sqlite3
conn = sqlite3.connect(r"C:\\stores\\learningtool\\backend\\learningtool.db")
cursor = conn.cursor()
cursor.execute("PRAGMA table_info(Courses)")
for row in cursor.fetchall():
    print(row[1])
conn.close()
'''

with sftp.open('C:\\stores\\learningtool\\backend\\verify_columns.py', 'w') as f:
    f.write(verify_script)
sftp.close()

stdin, stdout, stderr = ssh.exec_command('python C:\\stores\\learningtool\\backend\\verify_columns.py')
columns = stdout.read().decode('utf-8', errors='ignore')
print('Courses table columns:')
print(columns)

if 'LearningPlan' in columns and 'SystemPrompt' in columns and 'ContentGeneratedAt' in columns:
    print('\n[OK] All columns added successfully!')

    print('\n=== 4. Restart app pool ===')
    stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool -Name LearningToolPool"')
    print('App pool restarted')
else:
    print('\n[FAIL] Some columns are still missing')

print('\n=== 5. Test chat endpoint ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -k https://learning.prospergenics.com/api/auth/health')
health = stdout.read().decode('utf-8', errors='ignore')
print('Health check:', health if health else 'No response')

ssh.close()
