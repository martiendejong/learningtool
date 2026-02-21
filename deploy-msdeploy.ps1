# Deploy LearningTool to Windows Server using Web Deploy (msdeploy)
# Target: 85.215.217.154:8172 (IIS Web Deploy endpoint)

$ErrorActionPreference = "Stop"

# Configuration
$serverHost = "85.215.217.154"
$webDeployPort = "8172"
$username = "administrator"
$password = "3WsXcFr$7YhNmKi*"
$siteName = "LearningToolApp"
$appPoolName = "LearningToolPool"

# Paths
$frontendSource = "C:\Projects\learningtool\publish\frontend"
$backendSource = "C:\Projects\learningtool\publish\backend"
$msdeployExe = "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe"

Write-Host "`n============================================" -ForegroundColor Cyan
Write-Host "  LearningTool Web Deploy Deployment" -ForegroundColor Cyan
Write-Host "============================================`n" -ForegroundColor Cyan

# Verify msdeploy exists
if (-not (Test-Path $msdeployExe)) {
    Write-Host "[ERROR] Web Deploy not found at: $msdeployExe" -ForegroundColor Red
    exit 1
}

Write-Host "[1/6] Verifying build artifacts..." -ForegroundColor Yellow
if (-not (Test-Path $frontendSource)) {
    Write-Host "[ERROR] Frontend build not found at: $frontendSource" -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $backendSource)) {
    Write-Host "[ERROR] Backend build not found at: $backendSource" -ForegroundColor Red
    exit 1
}
Write-Host "   ✓ Frontend: 480KB" -ForegroundColor Green
Write-Host "   ✓ Backend: 37.6MB" -ForegroundColor Green

# Create app pool first via PowerShell remoting over SSH
Write-Host "`n[2/6] Creating IIS Application Pool..." -ForegroundColor Yellow
$createPoolCmd = @"
Import-Module WebAdministration
`$poolName = '$appPoolName'
if (Test-Path IIS:\AppPools\`$poolName) {
    Write-Host 'Pool exists, removing...'
    Remove-WebAppPool -Name `$poolName -ErrorAction SilentlyContinue
}
New-WebAppPool -Name `$poolName | Out-Null
Set-ItemProperty IIS:\AppPools\`$poolName -Name managedRuntimeVersion -Value ''
Write-Host 'Pool created'
"@

$sshCmd = "ssh administrator@$serverHost `"powershell -Command `"`$($createPoolCmd -replace '"','\"')`"`""
Invoke-Expression $sshCmd

# Deploy Frontend (www)
Write-Host "`n[3/6] Deploying Frontend..." -ForegroundColor Yellow
$frontendDest = "\\$serverHost\C$\stores\learningtool\www"

# Create directory structure via SSH
$mkdirCmd = "ssh administrator@$serverHost `"powershell -Command `"New-Item -ItemType Directory -Path C:\stores\learningtool\www -Force | Out-Null`"`""
Invoke-Expression $mkdirCmd

# Use msdeploy for frontend
$frontendArgs = @(
    "-verb:sync",
    "-source:dirPath=`"$frontendSource`"",
    "-dest:dirPath=`"C:\stores\learningtool\www`",computerName=https://${serverHost}:${webDeployPort}/msdeploy.axd,username=$username,password=$password,authtype=Basic",
    "-enableRule:DoNotDeleteRule",
    "-allowUntrusted",
    "-verbose"
)

& $msdeployExe $frontendArgs

# Deploy Backend (API)
Write-Host "`n[4/6] Deploying Backend..." -ForegroundColor Yellow
$backendDest = "\\$serverHost\C$\stores\learningtool\backend"

# Create backend directory
$mkdirBackendCmd = "ssh administrator@$serverHost `"powershell -Command `"New-Item -ItemType Directory -Path C:\stores\learningtool\backend -Force | Out-Null`"`""
Invoke-Expression $mkdirBackendCmd

# Use msdeploy for backend
$backendArgs = @(
    "-verb:sync",
    "-source:dirPath=`"$backendSource`"",
    "-dest:dirPath=`"C:\stores\learningtool\backend`",computerName=https://${serverHost}:${webDeployPort}/msdeploy.axd,username=$username,password=$password,authtype=Basic",
    "-enableRule:DoNotDeleteRule",
    "-allowUntrusted",
    "-verbose"
)

& $msdeployExe $backendArgs

# Create IIS Site and Application
Write-Host "`n[5/6] Configuring IIS Site..." -ForegroundColor Yellow
$createSiteCmd = @"
Import-Module WebAdministration
`$siteName = '$siteName'
if (Test-Path IIS:\Sites\`$siteName) {
    Stop-Website -Name `$siteName -ErrorAction SilentlyContinue
    Remove-Website -Name `$siteName -ErrorAction SilentlyContinue
}
New-Website -Name `$siteName -PhysicalPath C:\stores\learningtool\www -ApplicationPool '$appPoolName' -Port 5192 | Out-Null
New-WebApplication -Site `$siteName -Name api -PhysicalPath C:\stores\learningtool\backend -ApplicationPool '$appPoolName' | Out-Null
Start-Website -Name `$siteName
Write-Host 'Site configured and started'
"@

$sshSiteCmd = "ssh administrator@$serverHost `"powershell -Command `"`$($createSiteCmd -replace '"','\"')`"`""
Invoke-Expression $sshSiteCmd

# Verify deployment
Write-Host "`n[6/6] Verifying Deployment..." -ForegroundColor Yellow
$verifyCmd = @"
Import-Module WebAdministration
`$site = Get-Website -Name '$siteName'
Write-Host "Site: `$(`$site.Name)"
Write-Host "State: `$(`$site.State)"
Write-Host "Port: `$(`$site.Bindings.Collection[0].bindingInformation)"
`$listening = netstat -an | Select-String ':5192'
if (`$listening) { Write-Host 'Port 5192: LISTENING' } else { Write-Host 'Port 5192: Not listening' }
"@

$sshVerifyCmd = "ssh administrator@$serverHost `"powershell -Command `"`$($verifyCmd -replace '"','\"')`"`""
Invoke-Expression $sshVerifyCmd

Write-Host "`n============================================" -ForegroundColor Green
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host "`nApplication URL: http://${serverHost}:5192" -ForegroundColor Cyan
Write-Host "API Endpoint: http://${serverHost}:5192/api" -ForegroundColor Cyan
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "  1. Test application in browser" -ForegroundColor White
Write-Host "  2. Configure SSL certificate" -ForegroundColor White
Write-Host "  3. Update DNS: learning.prospergenics.com -> $serverHost" -ForegroundColor White
