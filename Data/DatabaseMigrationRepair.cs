using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgileTaskManager.Data;

/// <summary>
/// Handles inconsistent local databases (e.g. Identity tables without app tables, or migration history out of sync).
/// </summary>
public static class DatabaseMigrationRepair
{
    public const string InitialMigrationId = "20260512192712_InitialCreate";
    public const string InitialMigrationProductVersion = "9.0.0";

    /// <summary>
    /// If the database claims the initial migration is applied (or has partial Identity tables) but core app tables
    /// such as <c>Tenants</c> are missing, the schema is unusable. In Development, drop and recreate the database so
    /// <see cref="RelationalDatabaseFacadeExtensions.Migrate"/> can run a full initial migration.
    /// </summary>
    public static async Task TryRecoverBrokenSchemaAsync(
        ApplicationDbContext context,
        IHostEnvironment environment,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (!await context.Database.CanConnectAsync(cancellationToken))
            return;

        if (await TableExistsAsync(context, "Tenants", cancellationToken))
            return;

        var applied = await context.Database.GetAppliedMigrationsAsync(cancellationToken);
        var historyClaimsInitial = applied.Contains(InitialMigrationId);
        var identityPresent = await TableExistsAsync(context, "AspNetRoles", cancellationToken);

        if (!historyClaimsInitial && !identityPresent)
            return;

        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "The database is missing required tables (for example 'Tenants') while migration history or Identity tables suggest a partial setup. " +
                "Delete the database or align the schema before starting the app, or run in Development to allow an automatic reset.");
        }

        logger.LogWarning(
            "The database is missing app tables (e.g. Tenants) but has Identity tables and/or migration history. " +
            "Deleting and recreating the database (Development only) so migrations can apply cleanly.");
        await context.Database.EnsureDeletedAsync(cancellationToken);
    }

    /// <summary>
    /// Records <see cref="InitialMigrationId"/> in history only when the <strong>full</strong> initial schema is present
    /// (Identity + <c>Tenants</c>). Stamping when only Identity tables exist caused EF to skip creating app tables.
    /// </summary>
    public static async Task TryStampInitialMigrationIfSchemaExistsAsync(
        ApplicationDbContext context,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (!await context.Database.CanConnectAsync(cancellationToken))
            return;

        var applied = await context.Database.GetAppliedMigrationsAsync(cancellationToken);
        if (applied.Contains(InitialMigrationId))
            return;

        if (!await TableExistsAsync(context, "AspNetRoles", cancellationToken)
            || !await TableExistsAsync(context, "Tenants", cancellationToken))
            return;

        var conn = context.Database.GetDbConnection();
        var openedHere = false;
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken);
            openedHere = true;
        }

        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = '__EFMigrationsHistory')
                THEN 1 ELSE 0 END
                """;
            var historyExists = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken)) == 1;
            if (!historyExists)
            {
                logger.LogWarning(
                    "Identity and app tables exist but __EFMigrationsHistory is missing. Drop the database or recreate __EFMigrationsHistory before running migrations.");
                return;
            }

            await context.Database.ExecuteSqlRawAsync(
                """
                IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {0})
                INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ({0}, {1})
                """,
                InitialMigrationId, InitialMigrationProductVersion);

            logger.LogInformation(
                "Recorded migration {Migration} in __EFMigrationsHistory because the full schema was already present.",
                InitialMigrationId);
        }
        finally
        {
            if (openedHere && conn.State == ConnectionState.Open)
                await conn.CloseAsync();
        }
    }

    private static async Task<bool> TableExistsAsync(
        ApplicationDbContext context,
        string tableName,
        CancellationToken cancellationToken)
    {
        var conn = context.Database.GetDbConnection();
        var openedHere = false;
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken);
            openedHere = true;
        }

        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @name)
                THEN 1 ELSE 0 END
                """;
            var p = cmd.CreateParameter();
            p.ParameterName = "@name";
            p.Value = tableName;
            cmd.Parameters.Add(p);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken)) == 1;
        }
        finally
        {
            if (openedHere && conn.State == ConnectionState.Open)
                await conn.CloseAsync();
        }
    }
}
