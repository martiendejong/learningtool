#!/usr/bin/env python3
"""Create account for Diko and send email"""

import requests
import secrets
import string

# Generate secure password
def generate_password(length=16):
    alphabet = string.ascii_letters + string.digits + "!@#$%^&*"
    return ''.join(secrets.choice(alphabet) for _ in range(length))

# User info
email = "dikomohamed287@gmail.com"
name = "Diko Mohamed"
password = generate_password()

print(f"Creating account for {email}...")
print(f"Generated password: {password}")

# Register via API
api_url = "http://learning.prospergenics.com/api/Auth/register"

payload = {
    "email": email,
    "password": password,
    "confirmPassword": password,
    "fullName": name
}

try:
    response = requests.post(api_url, json=payload, timeout=10)

    if response.status_code == 200:
        print("[OK] Account created successfully!")
        result = response.json()
        print(f"  Token: {result.get('token', 'N/A')[:50]}...")

        # Save credentials
        with open("C:\\Projects\\learningtool\\diko-credentials.txt", "w") as f:
            f.write(f"LearningTool Account - Diko Mohamed\n")
            f.write(f"=" * 50 + "\n\n")
            f.write(f"URL: http://learning.prospergenics.com\n")
            f.write(f"Email: {email}\n")
            f.write(f"Password: {password}\n\n")
            f.write(f"Created: 2026-02-21\n")

        print(f"\n[OK] Credentials saved to: diko-credentials.txt")
        print(f"\nReady to send email!")

    else:
        print(f"[ERROR] Registration failed: {response.status_code}")
        print(f"  Response: {response.text}")

except Exception as e:
    print(f"[ERROR] Error: {e}")
