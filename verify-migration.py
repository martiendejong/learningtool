#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

# Download again to verify
print('=== Downloading to verify ===')
sftp = ssh.open_sftp()
sftp.get(r'C:\stores\learningtool\data\learningtool.db', r'C:\Projects\learningtool\verify-db.db')
sftp.close()

import sqlite3
conn = sqlite3.connect(r'C:\Projects\learningtool\verify-db.db')
cursor = conn.cursor()

print('\n=== Checking ChatMessages table structure ===')
cursor.execute("PRAGMA table_info(ChatMessages)")
for row in cursor.fetchall():
    print(f"  {row[1]}: {row[2]}")

print('\n=== Checking migration history ===')
cursor.execute("SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId")
for row in cursor.fetchall():
    print(f"  {row[0]} ({row[1]})")

conn.close()

import os
os.remove(r'C:\Projects\learningtool\verify-db.db')

ssh.close()
