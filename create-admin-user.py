#!/usr/bin/env python3
"""Create admin user or promote existing user to admin"""

import paramiko
import sys

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"

def create_admin():
    """Promote first user or create admin"""
    ssh = paramiko.SSHClient()
    ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

    print("Creating/promoting admin user...")

    # Check if database exists
    cmd = 'powershell -Command "Test-Path C:\\stores\\learningtool\\data\\learningtool.db"'
    stdin, stdout, stderr = ssh.exec_command(cmd)
    db_exists = stdout.read().decode().strip()

    if "True" not in db_exists:
        print("Database doesn't exist yet - register first user via web UI")
        print("Then run this script again to promote them to admin")
        ssh.close()
        return

    print("\nTo make a user admin, you need to:")
    print("1. Register user via: http://learning.prospergenics.com/register")
    print("2. Note the email address")
    print("3. Run this SQL on the server:")
    print()
    print("   sqlite3 C:\\stores\\learningtool\\data\\learningtool.db")
    print("   UPDATE AspNetUsers SET IsAdmin = 1 WHERE Email = 'user@example.com';")
    print()

    ssh.close()

if __name__ == "__main__":
    create_admin()
