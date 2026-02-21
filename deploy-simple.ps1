# Simple deployment using administrative shares
$ErrorActionPreference = "Stop"

$serverHost = "85.215.217.154"
$username = "administrator"
$securePassword = ConvertTo-SecureString "3WsXcFr$7YhNmKi*" -AsPlainText -Force
$credential = New-Object System.Management.Automation.PSCredential($username, $securePassword)

$frontendSource = "C:\Projects\learningtool\publish\frontend"
$backendSource = "C:\Projects\learningtool\publish\backend"

Write-Host "Deployment Starting..." -ForegroundColor Cyan

# Map network drive
Write-Host "[1/7] Mapping network drive..." -ForegroundColor Yellow
if (Test-Path "Z:") {
    Remove-PSDrive -Name Z -Force -ErrorAction SilentlyContinue
}

New-PSDrive -Name Z -PSProvider FileSystem -Root "\\$serverHost\C$" -Credential $credential -ErrorAction Stop | Out-Null
Write-Host "   Mapped successfully" -ForegroundColor Green

# Create directories
Write-Host "[2/7] Creating directories..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path "Z:\stores\learningtool\backend" -Force | Out-Null
New-Item -ItemType Directory -Path "Z:\stores\learningtool\www" -Force | Out-Null
New-Item -ItemType Directory -Path "Z:\stores\learningtool\data" -Force | Out-Null
Write-Host "   Directories created" -ForegroundColor Green

# Copy Frontend
Write-Host "[3/7] Copying Frontend..." -ForegroundColor Yellow
robocopy $frontendSource "Z:\stores\learningtool\www" /E /NFL /NDL /NJH /NJS /NC /NS /NP
Write-Host "   Frontend copied" -ForegroundColor Green

# Copy Backend
Write-Host "[4/7] Copying Backend..." -ForegroundColor Yellow
robocopy $backendSource "Z:\stores\learningtool\backend" /E /NFL /NDL /NJH /NJS /NC /NS /NP
Write-Host "   Backend copied" -ForegroundColor Green

# Verify
Write-Host "[5/7] Verifying files..." -ForegroundColor Yellow
$hasIndex = Test-Path "Z:\stores\learningtool\www\index.html"
$hasDll = Test-Path "Z:\stores\learningtool\backend\LearningTool.API.dll"
if ($hasIndex -and $hasDll) {
    Write-Host "   Files verified" -ForegroundColor Green
}

# Configure IIS
Write-Host "[6/7] Configuring IIS..." -ForegroundColor Yellow
$cmd1 = "Import-Module WebAdministration; "
$cmd1 += 'if (Test-Path IIS:\AppPools\LearningToolPool) { Remove-WebAppPool -Name LearningToolPool }; '
$cmd1 += 'New-WebAppPool -Name LearningToolPool | Out-Null; '
$cmd1 += "Set-ItemProperty IIS:\AppPools\LearningToolPool -Name managedRuntimeVersion -Value ''"

ssh administrator@$serverHost "powershell -Command `"$cmd1`""

$cmd2 = "Import-Module WebAdministration; "
$cmd2 += 'if (Test-Path IIS:\Sites\LearningToolApp) { Stop-Website -Name LearningToolApp -EA SilentlyContinue; Remove-Website -Name LearningToolApp }; '
$cmd2 += 'New-Website -Name LearningToolApp -PhysicalPath C:\stores\learningtool\www -ApplicationPool LearningToolPool -Port 5192 | Out-Null; '
$cmd2 += 'New-WebApplication -Site LearningToolApp -Name api -PhysicalPath C:\stores\learningtool\backend -ApplicationPool LearningToolPool | Out-Null; '
$cmd2 += 'Start-Website -Name LearningToolApp'

ssh administrator@$serverHost "powershell -Command `"$cmd2`""
Write-Host "   IIS configured" -ForegroundColor Green

# Verify
Write-Host "[7/7] Verifying deployment..." -ForegroundColor Yellow
ssh administrator@$serverHost "powershell -Command `"Get-Website LearningToolApp | Select-Object Name,State`""

# Cleanup
Remove-PSDrive -Name Z -Force -ErrorAction SilentlyContinue

Write-Host "`nDeployment Complete!" -ForegroundColor Green
Write-Host "URL: http://${serverHost}:5192" -ForegroundColor Cyan
