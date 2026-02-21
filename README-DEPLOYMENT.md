# LearningTool - Deployment Guide

## Quick Deploy (1 Command)

```bash
# Build + Deploy in one go
python deploy-complete.py
```

Dit script doet ALLES automatisch:
- ✓ Verificatie dat build bestaat
- ✓ SSH verbinding
- ✓ Directory structuur aanmaken
- ✓ Frontend uploaden (70+ files via SFTP)
- ✓ Backend uploaden
- ✓ IIS configureren (app pool + website + API app)
- ✓ Domain binding toevoegen (learning.prospergenics.com)
- ✓ Alles verifiëren

**Deployment tijd:** ~1 minuut

---

## Prerequisites

### 1. Build de applicatie eerst

```bash
# Frontend
cd frontend
npm install
npm run build

# Backend
cd ..
dotnet publish -c Release
```

### 2. Controleer dat builds bestaan

- Frontend: `publish/frontend/index.html`
- Backend: `publish/backend/LearningTool.API.dll`

### 3. Run deployment

```bash
python deploy-complete.py
```

---

## URLs na deployment

- **Productie:** http://learning.prospergenics.com
- **Direct IP:** http://85.215.217.154:5192
- **API Docs:** http://learning.prospergenics.com/api/swagger

---

## Wat als het fout gaat?

### Build bestaat niet
```bash
# Frontend build ontbreekt
cd frontend && npm run build

# Backend build ontbreekt
dotnet publish -c Release
```

### SSH verbinding faalt
- Check of server bereikbaar is: `ping 85.215.217.154`
- Credentials staan hardcoded in script (administrator / 3WsXcFr$7YhNmKi*)

### Website start niet
```python
# Run verification script
python verify-deployment.py
```

### Domain werkt niet
```python
# Add domain binding manually
python add-domain-binding.py
```

---

## Server Details

- **IP:** 85.215.217.154
- **OS:** Windows Server met IIS
- **User:** administrator
- **Protocol:** SSH (port 22)

### Applicatie Paden

- Frontend: `C:\stores\learningtool\www`
- Backend: `C:\stores\learningtool\backend`
- Data: `C:\stores\learningtool\data`

### IIS Configuratie

- **App Pool:** LearningToolPool (No Managed Code)
- **Website:** LearningToolApp
- **Bindings:**
  - Port 5192: `*:5192:` (direct access)
  - Port 80: `*:80:learning.prospergenics.com` (production)

---

## SSL/HTTPS Setup (TODO)

### Stap 1: Certbot installeren op server

```powershell
# Via SSH op server
winget install Certbot.Certbot
```

### Stap 2: Certificate aanvragen

```powershell
certbot certonly --webroot -w C:\stores\learningtool\www -d learning.prospergenics.com
```

### Stap 3: IIS binding toevoegen

```powershell
Import-Module WebAdministration
$cert = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*learning.prospergenics.com*" }
New-WebBinding -Name LearningToolApp -Protocol https -Port 443 -HostHeader learning.prospergenics.com -SslFlags 1
```

---

## Deployment Scripts Overzicht

| Script | Doel | Status |
|--------|------|--------|
| `deploy-complete.py` | **GEBRUIK DIT** - Complete deployment | ✓ RECOMMENDED |
| `verify-deployment.py` | Controleer deployment status | ✓ Works |
| `add-domain-binding.py` | Voeg domain binding toe | ✓ Works |
| `configure-iis-simple.py` | IIS configuratie only | ✓ Works |
| `deploy-ssh-copy.py` | Files uploaden only | ✓ Works |
| `deploy-msdeploy.ps1` | Web Deploy alternatief | ⚠️ Not tested |
| `deploy-admin-share.ps1` | SMB share alternatief | ❌ Auth failed |

---

## Development Workflow

1. **Lokaal ontwikkelen**
   ```bash
   # Frontend
   cd frontend && npm run dev

   # Backend
   dotnet run --project src/LearningTool.API
   ```

2. **Testen**
   ```bash
   # Unit tests
   dotnet test

   # Integration tests
   # (TODO)
   ```

3. **Builden**
   ```bash
   # Frontend
   cd frontend && npm run build

   # Backend
   dotnet publish -c Release
   ```

4. **Deployen**
   ```bash
   python deploy-complete.py
   ```

5. **Verificeren**
   - Open http://learning.prospergenics.com
   - Test login/register
   - Test AI chat
   - Check API docs: http://learning.prospergenics.com/api/swagger

---

## Veelvoorkomende Problemen

### "Frontend build not found"
➜ Run `cd frontend && npm run build`

### "Backend build not found"
➜ Run `dotnet publish -c Release`

### "Connection failed"
➜ Check VPN, firewall, server status

### "Port not listening"
➜ IIS niet gestart - run `configure-iis-simple.py`

### "404 on domain"
➜ Binding ontbreekt - run `add-domain-binding.py`

### "API not working"
➜ Check `appsettings.Production.json` has OpenAI key

---

## Monitoring

### Check deployment status
```bash
python verify-deployment.py
```

### Check IIS via SSH
```powershell
ssh administrator@85.215.217.154 "powershell -Command \"Get-Website LearningToolApp\""
```

### Check logs
```powershell
ssh administrator@85.215.217.154 "powershell -Command \"Get-EventLog -LogName Application -Source 'IIS*' -Newest 20\""
```

---

## Rollback

Als deployment fout gaat:

```python
# Stop website
ssh administrator@85.215.217.154 "powershell -Command \"Stop-Website LearningToolApp\""

# Restore previous version
# (TODO: implement versioning)

# Start website
ssh administrator@85.215.217.154 "powershell -Command \"Start-Website LearningToolApp\""
```

---

**Laatste update:** 2026-02-21
**Deployment method:** SFTP + PowerShell over SSH
**Status:** ✓ Production Ready
