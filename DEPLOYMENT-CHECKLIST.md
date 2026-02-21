# Deployment Checklist - LearningTool

## PRE-DEPLOYMENT (5 min)

### ✓ Code Ready
- [ ] Code gecommit naar git
- [ ] Alle tests passed
- [ ] OpenAI API key geconfigureerd in `appsettings.Production.json`
- [ ] Database migrations up-to-date

### ✓ Build Ready
```bash
# Frontend
cd frontend
npm install
npm run build
# Verify: publish/frontend/index.html exists

# Backend
dotnet publish -c Release
# Verify: publish/backend/LearningTool.API.dll exists
```

### ✓ Server Ready
- [ ] Server bereikbaar: `ping 85.215.217.154`
- [ ] SSH toegang: `ssh administrator@85.215.217.154`
- [ ] IIS draait: `Get-Service W3SVC`

---

## DEPLOYMENT (1 min)

### ✓ Run Complete Deployment
```bash
python deploy-complete.py
```

**Script controleert automatisch:**
- [x] Build artifacts bestaan
- [x] SSH verbinding werkt
- [x] Directories aangemaakt
- [x] Files geupload (frontend + backend)
- [x] IIS geconfigureerd (pool + site + app)
- [x] Domain binding toegevoegd
- [x] Website gestart
- [x] Poorten listening

---

## POST-DEPLOYMENT (3 min)

### ✓ Functional Testing
- [ ] Homepage laadt: http://learning.prospergenics.com
- [ ] Login pagina werkt
- [ ] Register werkt
- [ ] API docs accessible: http://learning.prospergenics.com/api/swagger

### ✓ Integration Testing
- [ ] Login met test account
- [ ] AI chat werkt (OpenAI)
- [ ] Course creation werkt
- [ ] Skill tracking werkt

### ✓ Performance Check
- [ ] Page load < 2 sec
- [ ] API response < 500 ms
- [ ] No console errors

---

## VERIFICATION COMMANDS

```bash
# Quick status check
python verify-deployment.py

# Manual checks
curl -I http://learning.prospergenics.com              # Should be 200 OK
curl -I http://learning.prospergenics.com/api/swagger  # Should be 200 OK
ssh administrator@85.215.217.154 "powershell -Command \"Get-Website LearningToolApp\""  # Should be Started
```

---

## ROLLBACK PLAN (if deployment fails)

### Option 1: Re-deploy
```bash
python deploy-complete.py
```

### Option 2: Manual fix
```bash
# If IIS not configured
python configure-iis-simple.py

# If domain binding missing
python add-domain-binding.py

# If files not uploaded
python deploy-ssh-copy.py
```

### Option 3: Full reset
```bash
# SSH to server
ssh administrator@85.215.217.154

# Remove everything
powershell -Command "Stop-Website LearningToolApp"
powershell -Command "Remove-Website LearningToolApp"
powershell -Command "Remove-WebAppPool LearningToolPool"
powershell -Command "Remove-Item C:\stores\learningtool -Recurse -Force"

# Re-deploy
python deploy-complete.py
```

---

## CRITICAL: NEVER SKIP

### ✗ DON'T
- Deploy without building first
- Skip verification steps
- Forget domain binding
- Deploy to wrong port only
- Ignore errors in deployment log

### ✓ DO
- Always use `deploy-complete.py`
- Verify EVERY step completes
- Test on actual domain (not just IP)
- Check both port 80 and 5192
- Monitor deployment output for errors

---

## SUCCESS CRITERIA

Deployment is ONLY successful if ALL these are true:

- [x] `http://learning.prospergenics.com` returns 200 OK
- [x] `http://85.215.217.154:5192` returns 200 OK
- [x] Login page loads with Prospergenics branding
- [x] API Swagger UI loads at `/api/swagger`
- [x] Website state = "Started" in IIS
- [x] Port 80 and 5192 both LISTENING
- [x] No errors in deployment log

**If ANY of these fail → deployment FAILED → run rollback**

---

## MONITORING (Post-deployment)

### First 24 hours
- [ ] Check error logs every 4 hours
- [ ] Monitor API response times
- [ ] Check for failed requests
- [ ] Verify OpenAI quota not exceeded

### Ongoing
- [ ] Weekly: Check disk space on server
- [ ] Weekly: Review error logs
- [ ] Monthly: Update dependencies
- [ ] Monthly: Review SSL certificate expiry

---

## NEXT: SSL/HTTPS

Once HTTP deployment stable:

1. Install Certbot on server
2. Generate Let's Encrypt certificate
3. Add HTTPS binding to IIS
4. Redirect HTTP → HTTPS
5. Update all URLs to https://

**IMPORTANT:** Don't rush SSL - get HTTP working perfectly first.

---

**Last Updated:** 2026-02-21
**Status:** ✓ Checklist Validated
**Deployment Time:** ~5 min total (1 min deploy + 3 min verify)
