# Configuration Guide

## Required Configuration

Before running the application, you need to configure the following in `src/LearningTool.API/appsettings.json`:

### 1. OpenAI API Key

Replace `your-openai-api-key-here` with your actual OpenAI API key:

```json
"OpenAI": {
  "ApiKey": "sk-your-actual-key-here",
  "Model": "gpt-4",
  "MaxTokens": 500,
  "Temperature": 0.7
}
```

**How to get an OpenAI API key:**
1. Go to https://platform.openai.com/
2. Sign in or create an account
3. Navigate to API Keys section
4. Create a new secret key
5. Copy the key (starts with `sk-`)

### 2. JWT Secret Key

Replace the default JWT key with a strong secret:

```json
"Jwt": {
  "Key": "your-strong-secret-key-at-least-32-characters",
  "Issuer": "LearningTool",
  "Audience": "LearningTool",
  "ExpiryDays": 7
}
```

**Security Notes:**
- Use a strong random string (at least 32 characters)
- Never commit your actual keys to source control
- Use environment variables in production

### 3. Database Connection (Optional)

The default SQLite database works out of the box:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=learningtool.db"
}
```

To use a different database, update the connection string accordingly.

## Environment-Specific Configuration

For production, create `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-production-database-connection"
  },
  "OpenAI": {
    "ApiKey": "${OPENAI_API_KEY}"
  },
  "Jwt": {
    "Key": "${JWT_SECRET_KEY}"
  }
}
```

Then set environment variables:
```bash
export OPENAI_API_KEY="sk-..."
export JWT_SECRET_KEY="..."
```

## Verification

After configuration:

1. **Test the API:**
   ```bash
   cd src/LearningTool.API
   dotnet run
   ```

2. **Check the logs** for any configuration errors

3. **Test OpenAI integration:**
   - Register a new user
   - Send a chat message: "I want to learn Python"
   - Verify AI responds and adds the skill

## Troubleshooting

### OpenAI 401 Unauthorized
- Verify your API key is correct
- Check you have credits available at https://platform.openai.com/

### JWT Authentication Fails
- Ensure the JWT key is at least 32 characters
- Verify the key matches across all instances

### Database Errors
- Run migrations: `dotnet ef database update`
- Check file permissions for SQLite database file

## See Also

- [OPENAI_INTEGRATION.md](docs/OPENAI_INTEGRATION.md) - Detailed OpenAI integration guide
- [COMPLETE_ARCHITECTURE.md](docs/COMPLETE_ARCHITECTURE.md) - Full system architecture
- [DEVELOPER_GUIDE.md](docs/DEVELOPER_GUIDE.md) - Development guidelines
- [README.md](README.md) - Quick start guide
