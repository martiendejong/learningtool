# Deploy LearningTool using Windows Administrative Shares
# Fast, reliable file transfer using SMB protocol

$ErrorActionPreference = "Stop"

# Configuration
$serverHost = "85.215.217.154"
$username = "administrator"
$password = "3WsXcFr$7YhNmKi*" | ConvertTo-SecureString -AsPlainText -Force
$credential = New-Object System.Management.Automation.PSCredential($username, $password)

# Paths
$frontendSource = "C:\Projects\learningtool\publish\frontend"
$backendSource = "C:\Projects\learningtool\publish\backend"
$remoteRoot = "\\$serverHost\C$\stores\learningtool"

Write-Host "`n============================================" -ForegroundColor Cyan
Write-Host "  LearningTool Administrative Share Deploy" -ForegroundColor Cyan
Write-Host "============================================`n" -ForegroundColor Cyan

# Map network drive with credentials
Write-Host "[1/7] Mapping network drive..." -ForegroundColor Yellow
# Remove existing mapping if present
if (Test-Path "Z:") {
    Remove-PSDrive -Name Z -ErrorAction SilentlyContinue
}

try {
    New-PSDrive -Name Z -PSProvider FileSystem -Root "\\$serverHost\C$" -Credential $credential -ErrorAction Stop | Out-Null
    Write-Host "   ✓ Mapped Z: to \\$serverHost\C$" -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] Failed to map network drive: $_" -ForegroundColor Red
    exit 1
}

# Create directory structure
Write-Host "`n[2/7] Creating directory structure..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path "Z:\stores\learningtool\backend" -Force | Out-Null
New-Item -ItemType Directory -Path "Z:\stores\learningtool\www" -Force | Out-Null
New-Item -ItemType Directory -Path "Z:\stores\learningtool\data" -Force | Out-Null
Write-Host "   ✓ Created Z:\stores\learningtool\" -ForegroundColor Green

# Copy Frontend
Write-Host "`n[3/7] Copying Frontend (480KB)..." -ForegroundColor Yellow
robocopy $frontendSource "Z:\stores\learningtool\www" /E /NFL /NDL /NJH /NJS /NC /NS /NP
if ($LASTEXITCODE -le 7) {
    Write-Host "   ✓ Frontend deployed" -ForegroundColor Green
} else {
    Write-Host "[ERROR] Frontend copy failed" -ForegroundColor Red
    exit 1
}

# Copy Backend
Write-Host "`n[4/7] Copying Backend (37.6MB)..." -ForegroundColor Yellow
robocopy $backendSource "Z:\stores\learningtool\backend" /E /NFL /NDL /NJH /NJS /NC /NS /NP
if ($LASTEXITCODE -le 7) {
    Write-Host "   ✓ Backend deployed" -ForegroundColor Green
} else {
    Write-Host "[ERROR] Backend copy failed" -ForegroundColor Red
    exit 1
}

# Verify files
Write-Host "`n[5/7] Verifying deployment..." -ForegroundColor Yellow
$indexHtml = Test-Path "Z:\stores\learningtool\www\index.html"
$apiDll = Test-Path "Z:\stores\learningtool\backend\LearningTool.API.dll"
if ($indexHtml -and $apiDll) {
    Write-Host "   ✓ index.html: Present" -ForegroundColor Green
    Write-Host "   ✓ API DLL: Present" -ForegroundColor Green
} else {
    Write-Host "[ERROR] Critical files missing" -ForegroundColor Red
    exit 1
}

# Configure IIS via SSH
Write-Host "`n[6/7] Configuring IIS..." -ForegroundColor Yellow
$iisScript = @'
Import-Module WebAdministration

# Create app pool
$poolName = 'LearningToolPool'
if (Test-Path IIS:\AppPools\$poolName) {
    Remove-WebAppPool -Name $poolName -ErrorAction SilentlyContinue
}
New-WebAppPool -Name $poolName | Out-Null
Set-ItemProperty IIS:\AppPools\$poolName -Name managedRuntimeVersion -Value ''

# Create website
$siteName = 'LearningToolApp'
if (Test-Path IIS:\Sites\$siteName) {
    Stop-Website -Name $siteName -ErrorAction SilentlyContinue
    Remove-Website -Name $siteName -ErrorAction SilentlyContinue
}
New-Website -Name $siteName -PhysicalPath C:\stores\learningtool\www -ApplicationPool $poolName -Port 5192 | Out-Null

# Create API application
New-WebApplication -Site $siteName -Name api -PhysicalPath C:\stores\learningtool\backend -ApplicationPool $poolName | Out-Null

# Start website
Start-Website -Name $siteName

Write-Host 'IIS configured'
'@

# Save script to remote server
$iisScript | Out-File "Z:\Temp\configure-iis.ps1" -Encoding UTF8 -Force

# Execute via SSH
ssh administrator@$serverHost "powershell -ExecutionPolicy Bypass -File C:\Temp\configure-iis.ps1"

# Verify IIS
Write-Host "`n[7/7] Verifying IIS..." -ForegroundColor Yellow
$verifyScript = @'
Import-Module WebAdministration
$site = Get-Website -Name LearningToolApp
Write-Host "Site: $($site.Name)"
Write-Host "State: $($site.State)"
Write-Host "Port: $($site.Bindings.Collection[0].bindingInformation)"

$listening = netstat -an | Select-String ':5192'
if ($listening) {
    Write-Host 'Port 5192: LISTENING'
} else {
    Write-Host 'Port 5192: Not listening (may need time to start)'
}
'@

$verifyScript | Out-File "Z:\Temp\verify-iis.ps1" -Encoding UTF8 -Force
ssh administrator@$serverHost "powershell -ExecutionPolicy Bypass -File C:\Temp\verify-iis.ps1"

# Cleanup
Remove-PSDrive -Name Z -ErrorAction SilentlyContinue

Write-Host "`n============================================" -ForegroundColor Green
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host "`nApplication URL: http://${serverHost}:5192" -ForegroundColor Cyan
Write-Host "API Endpoint: http://${serverHost}:5192/api" -ForegroundColor Cyan
Write-Host "`nTest in browser now!" -ForegroundColor Yellow
