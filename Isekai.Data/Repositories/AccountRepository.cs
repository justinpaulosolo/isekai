using Dapper;
using Npgsql;

namespace Isekai.Data.Repositories;

public interface IAccountRepository
{
    Task<UserLogin?> FindUserIdAsync(string provider, string subject);
    Task CreateUserLoginAsync(UserLogin model);
    Task<long> UpsertExternalUserAsync(string provider, string subject, string? name, string? email);
}

public class AccountRepository(NpgsqlDataSource dataSource) : IAccountRepository
{
    public async Task<UserLogin?> FindUserIdAsync(string provider, string subject)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        return await conn.QueryFirstOrDefaultAsync<UserLogin>(
            """
            SELECT provider as Provider,
                   provider_subject as ProviderSubject,
                   user_id as UserId,
                   created_at as CreatedAt
            FROM user_logins
            WHERE provider = @Provider,
                AND provider_subject = @Subject
            """, 
            new { Provider = provider, Subject = subject });
    }

    public async Task CreateUserLoginAsync(UserLogin model)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await conn.ExecuteAsync(
            """
            INSERT INTO user_logins (provider, provider_subject, user_id, created_at
            VALUES (@Provider, @ProviderSubject, @UserId, @CreatedAt);
            """, model);
    }

    public async Task<long> UpsertExternalUserAsync(string provider, string subject, string? name, string? email)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        // This advanced SQL tries to find the login. If it doesn't exist, it creates the user,
        // then creates the login, all atomically inside Postgres.
        return await conn.QuerySingleAsync<long>(
            """
            WITH existing_login AS (
                SELECT user_id FROM user_logins 
                WHERE provider = @Provider AND provider_subject = @Subject
            ),
            inserted_user AS (
                INSERT INTO users (display_name, email)
                SELECT @Name, @Email
                WHERE NOT EXISTS (SELECT 1 FROM existing_login)
                RETURNING id
            ),
            inserted_login AS (
                INSERT INTO user_logins (provider, provider_subject, user_id)
                SELECT @Provider, @Subject, id FROM inserted_user
                ON CONFLICT (provider, provider_subject) DO NOTHING
            )
            SELECT COALESCE(
                (SELECT user_id FROM existing_login),
                (SELECT id FROM inserted_user),
                (SELECT user_id FROM user_logins WHERE provider = @Provider AND provider_subject = @Subject)
            );
            """,
            new { Provider = provider, Subject = subject, Name = name, Email = email }
        );
    }
}