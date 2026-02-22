#!/usr/bin/env python3
"""Check SSL certificate status and IIS binding"""

import paramiko

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"
DOMAIN = "learning.prospergenics.com"

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

print("=" * 70)
print("  SSL Certificate Status Check")
print("=" * 70)

# Check 1: List certificates in store
print("\n[1] Checking certificates in store...")
cert_cmd = f'''powershell -Command "
Get-ChildItem -Path Cert:\\LocalMachine\\My |
Where-Object {{ $_.Subject -like '*{DOMAIN}*' }} |
Select-Object Subject, Thumbprint, NotAfter, NotBefore |
Format-List
"'''
stdin, stdout, stderr = ssh.exec_command(cert_cmd)
certs = stdout.read().decode()
print(certs if certs.strip() else "  No certificates found for this domain")

# Check 2: Get IIS binding details including certificate hash
print("\n[2] Checking IIS HTTPS binding details...")
binding_cmd = '''powershell -Command "
Import-Module WebAdministration
Get-WebBinding -Name LearningToolApp -Protocol https |
Select-Object protocol, bindingInformation, sslFlags, certificateHash, certificateStoreName |
Format-List
"'''
stdin, stdout, stderr = ssh.exec_command(binding_cmd)
binding = stdout.read().decode()
print(binding if binding.strip() else "  No HTTPS binding found")

# Check 3: List Win-ACME renewals
print("\n[3] Checking Win-ACME renewal configuration...")
renewal_cmd = r'C:\win-acme\wacs.exe --list --verbose'
stdin, stdout, stderr = ssh.exec_command(renewal_cmd, timeout=30)
renewals = stdout.read().decode()
print(renewals[:1000] if renewals.strip() else "  No renewals configured")

# Check 4: Check if port 443 is listening
print("\n[4] Checking if port 443 is listening...")
port_cmd = 'netstat -an | findstr :443'
stdin, stdout, stderr = ssh.exec_command(port_cmd)
ports = stdout.read().decode()
print(ports if ports.strip() else "  Port 443 not listening")

# Check 5: Try to get certificate from the server
print(f"\n[5] Testing SSL handshake on {DOMAIN}...")
ssl_test_cmd = f'''powershell -Command "
try {{
    $tcp = New-Object System.Net.Sockets.TcpClient
    $tcp.Connect('{DOMAIN}', 443)
    $stream = $tcp.GetStream()
    $ssl = New-Object System.Net.Security.SslStream($stream, $false)
    $ssl.AuthenticateAsClient('{DOMAIN}')
    Write-Output \\"Connected: $($ssl.RemoteCertificate.Subject)\\"
    Write-Output \\"Valid until: $($ssl.RemoteCertificate.GetExpirationDateString())\\"
    $ssl.Close()
    $tcp.Close()
}} catch {{
    Write-Output \\"Failed: $($_.Exception.Message)\\"
}}
"'''
stdin, stdout, stderr = ssh.exec_command(ssl_test_cmd)
ssl_test = stdout.read().decode()
print(ssl_test.strip())

ssh.close()

print("\n" + "=" * 70)
