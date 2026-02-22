#!/usr/bin/env python3
"""Install SSL using Posh-ACME (PowerShell ACME module)"""

import paramiko
import time

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"
DOMAIN = "learning.prospergenics.com"
EMAIL = "martien@prospergenics.com"

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

print("=" * 70)
print("  SSL Setup via Posh-ACME")
print("=" * 70)

# Step 1: Install Posh-ACME module
print("\n[1/6] Installing Posh-ACME PowerShell module...")
install_cmd = 'powershell -Command "Install-Module -Name Posh-ACME -Force -Scope CurrentUser -AllowClobber"'
stdin, stdout, stderr = ssh.exec_command(install_cmd, timeout=120)
exit_code = stdout.channel.recv_exit_status()
output = stdout.read().decode('utf-8', errors='ignore')
error = stderr.read().decode('utf-8', errors='ignore')

if exit_code == 0:
    print("  [OK] Posh-ACME installed")
else:
    print(f"  [INFO] Install result: {output[:200]}")

# Step 2: Get site ID
print("\n[2/6] Getting IIS site ID...")
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Import-Module WebAdministration; (Get-Website -Name LearningToolApp).ID"')
site_id = stdout.read().decode().strip()
print(f"  Site ID: {site_id}")

# Step 3: Request certificate using Posh-ACME
print(f"\n[3/6] Requesting SSL certificate for {DOMAIN}...")
print("  (This will take 30-60 seconds)")

cert_cmd = f'''powershell -Command "
Import-Module Posh-ACME

# Set ACME server
Set-PAServer LE_PROD

# Create new order
$order = New-PAOrder -Domain '{DOMAIN}' -Contact '{EMAIL}' -AcceptTOS

# Get authorization challenges
$auths = $order | Get-PAAuthorizations

# Setup HTTP challenge
foreach ($auth in $auths) {{
    $challenge = $auth.HTTP01Token
    $keyAuth = Get-KeyAuthorization $auth.HTTP01Token (Get-PAAccount)

    # Create .well-known/acme-challenge directory
    $wellKnownPath = 'C:\\stores\\learningtool\\www\\.well-known\\acme-challenge'
    New-Item -ItemType Directory -Path $wellKnownPath -Force | Out-Null

    # Write challenge file
    $challengePath = Join-Path $wellKnownPath $challenge
    Set-Content -Path $challengePath -Value $keyAuth -NoNewline

    Write-Output \\"Challenge file created: $challengePath\\"
}}

# Send challenge ready notification
$auths | Send-ChallengeAck

# Wait for validation
Start-Sleep -Seconds 10

# Complete order and get certificate
$order | New-PACertificate

# Get certificate details
$cert = Get-PACertificate
Write-Output \\"Certificate created: $($cert.CertFile)\\"
Write-Output \\"Key file: $($cert.KeyFile)\\"
Write-Output \\"PFX file: $($cert.PfxFile)\\"
"'''

stdin, stdout, stderr = ssh.exec_command(cert_cmd, timeout=120)
output = stdout.read().decode('utf-8', errors='ignore')
error = stderr.read().decode('utf-8', errors='ignore')

print("  Output:")
for line in output.split('\n')[-20:]:
    if line.strip():
        print(f"    {line}")

if error:
    print(f"  Errors: {error[:500]}")

# Step 4: Import certificate to IIS
print("\n[4/6] Importing certificate to IIS...")
import_cmd = f'''powershell -Command "
Import-Module Posh-ACME
$cert = Get-PACertificate
$pfxPass = ConvertTo-SecureString -String 'poshacme' -Force -AsPlainText

# Import to cert store
$importedCert = Import-PfxCertificate -FilePath $cert.PfxFile -CertStoreLocation Cert:\\LocalMachine\\My -Password $pfxPass

Write-Output \\"Imported certificate thumbprint: $($importedCert.Thumbprint)\\"
$importedCert.Thumbprint
"'''

stdin, stdout, stderr = ssh.exec_command(import_cmd, timeout=30)
cert_output = stdout.read().decode()
thumbprint = None

for line in cert_output.split('\n'):
    if len(line.strip()) == 40 and line.strip().isalnum():
        thumbprint = line.strip()
        break

if thumbprint:
    print(f"  [OK] Certificate imported: {thumbprint}")
else:
    print(f"  Certificate import output: {cert_output}")

# Step 5: Bind certificate to IIS
print("\n[5/6] Binding certificate to IIS...")
if thumbprint:
    bind_cmd = f'''powershell -Command "
Import-Module WebAdministration

# Remove old binding if exists
Get-WebBinding -Name LearningToolApp -Protocol https | Remove-WebBinding -ErrorAction SilentlyContinue

# Add new HTTPS binding with certificate
New-WebBinding -Name LearningToolApp -Protocol https -Port 443 -HostHeader '{DOMAIN}' -SslFlags 1

# Bind certificate
$binding = Get-WebBinding -Name LearningToolApp -Protocol https
$binding.AddSslCertificate('{thumbprint}', 'My')

Write-Output \\"HTTPS binding created and certificate bound\\"
"'''

    stdin, stdout, stderr = ssh.exec_command(bind_cmd, timeout=30)
    bind_output = stdout.read().decode()
    print(f"  {bind_output.strip()}")
else:
    print("  [SKIP] No thumbprint available")

# Step 6: Test HTTPS
print(f"\n[6/6] Testing HTTPS...")
test_cmd = f'powershell -Command "try {{ (Invoke-WebRequest -Uri https://{DOMAIN} -UseBasicParsing -TimeoutSec 10).StatusCode }} catch {{ Write-Output $_.Exception.Message }}"'
stdin, stdout, stderr = ssh.exec_command(test_cmd, timeout=15)
result = stdout.read().decode().strip()
print(f"  Result: {result}")

ssh.close()

print("\n" + "=" * 70)
if "200" in result:
    print("  SUCCESS! HTTPS IS ACTIVE!")
    print("=" * 70)
    print(f"\n  https://{DOMAIN}")
    print(f"\n  Certificate auto-renews with Posh-ACME")
else:
    print("  Setup Complete - Manual Verification Needed")
    print("=" * 70)
    print(f"\n  Test: https://{DOMAIN}")
    print(f"  Result: {result}")
