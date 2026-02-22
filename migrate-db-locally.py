#!/usr/bin/env python3
import paramiko
import os
import subprocess

# Download database
print('=== Downloading production database ===')
ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

sftp = ssh.open_sftp()
sftp.get(r'C:\stores\learningtool\data\learningtool.db', r'C:\Projects\learningtool\learningtool-prod.db')
sftp.close()
print('Downloaded')

# Apply migration locally
print('\n=== Applying migration locally ===')
import sqlite3
conn = sqlite3.connect(r'C:\Projects\learningtool\learningtool-prod.db')
cursor = conn.cursor()

# Check if column exists
cursor.execute("PRAGMA table_info(ChatMessages)")
columns = [row[1] for row in cursor.fetchall()]

if 'CourseId' not in columns:
    print('Adding CourseId column...')
    cursor.execute('ALTER TABLE ChatMessages ADD COLUMN CourseId INTEGER NULL')
    cursor.execute("INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260221213536_AddCourseIdToChatMessages', '8.0.0')")
    conn.commit()
    print('Migration applied')
else:
    print('CourseId already exists')

conn.close()

# Upload back
print('\n=== Uploading modified database ===')
ssh2 = paramiko.SSHClient()
ssh2.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh2.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

# Stop app pool first
stdin, stdout, stderr = ssh2.exec_command('powershell -Command "Stop-WebAppPool LearningToolPool"')
stdout.read()
print('App pool stopped')

import time
time.sleep(2)

sftp2 = ssh2.open_sftp()
sftp2.put(r'C:\Projects\learningtool\learningtool-prod.db', r'C:\stores\learningtool\data\learningtool.db')
sftp2.close()
print('Uploaded')

# Start app pool
stdin, stdout, stderr = ssh2.exec_command('powershell -Command "Start-WebAppPool LearningToolPool"')
stdout.read()
print('App pool started')

time.sleep(4)

# Test
print('\n=== Testing ===')
stdin, stdout, stderr = ssh2.exec_command('curl -s http://localhost:5192/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
if response and len(response) < 200:
    print('SUCCESS!')
    print(response)
else:
    print('Response:', response[:300])

ssh2.close()

# Cleanup
os.remove(r'C:\Projects\learningtool\learningtool-prod.db')
print('\nLocal temp file deleted')
