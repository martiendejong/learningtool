#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

# Create a simple C# console program to add the columns
csharp_code = r'''
using Microsoft.Data.Sqlite;
using System;

class Program
{
    static void Main()
    {
        var connectionString = "Data Source=C:\\stores\\learningtool\\backend\\learningtool.db";
        using var conn = new SqliteConnection(connectionString);
        conn.Open();

        var commands = new[] {
            "ALTER TABLE Courses ADD COLUMN LearningPlan TEXT;",
            "ALTER TABLE Courses ADD COLUMN SystemPrompt TEXT;",
            "ALTER TABLE Courses ADD COLUMN ContentGeneratedAt TEXT;"
        };

        foreach (var cmdText in commands)
        {
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = cmdText;
                cmd.ExecuteNonQuery();
                Console.WriteLine($"SUCCESS: {cmdText}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {cmdText} - {ex.Message}");
            }
        }
    }
}
'''

print('=== 1. Create C# program file ===')
# Escape the C# code for PowerShell
escaped_code = csharp_code.replace("'", "''").replace('"', '`"').replace('$', '`$')
stdin, stdout, stderr = ssh.exec_command(f'powershell -Command "[System.IO.File]::WriteAllText(\'C:\\stores\\learningtool\\backend\\AddColumns.cs\', \'{escaped_code}\')"')
print(stdout.read().decode('utf-8', errors='ignore'))
err = stderr.read().decode('utf-8', errors='ignore')
if err:
    print('Error:', err)

print('\n=== 2. Compile and run C# program ===')
stdin, stdout, stderr = ssh.exec_command('cd C:\\stores\\learningtool\\backend && dotnet run --project . --no-build AddColumns.cs')
result = stdout.read().decode('utf-8', errors='ignore')
err = stderr.read().decode('utf-8', errors='ignore')
print('Result:', result)
if err:
    print('Error:', err)

ssh.close()
