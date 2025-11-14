using System;
using System.IO;
using Oracle.ManagedDataAccess.Client;

public class OLTPSeedService
{
    private readonly string _connectionString;

    public OLTPSeedService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SeedIfEmptyAsync(string sqlFilePath)
    {
        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync();

        using var checkCmd = new OracleCommand("SELECT COUNT(*) FROM Employee", conn);
        var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

        if (count > 0)
        {
            Console.WriteLine("Database already seeded. Skipping.");
            return;
        }
        
        string sql = await File.ReadAllTextAsync(sqlFilePath);
        // Split by semicolon and execute each command
        var commands = sql
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var cmdText in commands)
        {
            if (string.IsNullOrWhiteSpace(cmdText)) continue;
            using var cmd = new OracleCommand(cmdText, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        Console.WriteLine("OLTP database seeded successfully.");
    }
}
