using Dapper;
using Isekai.Server.Models;
using Npgsql;

namespace Isekai.Data.Repositories;

public interface IShortUrlRepository
{
    Task<ShortUrl?> GetByCodeAsync(string code);
    Task<List<ShortUrl>> GetAllAsync(long userId);
    Task<long> CreateAsync(ShortUrl shortUrl);
    Task<bool> DeleteAsync(string code, long userId);
    Task RecordClickAsync(string code, DateTime occurredOn);
    Task<bool> ApplyClickBatchAsync(ClickBatch clickBatch, CancellationToken cancellationToken = default);
}

public class ShortUrlRepository(NpgsqlDataSource dataSource) : IShortUrlRepository
{
    public async Task<List<ShortUrl>> GetAllAsync(long userId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        var urls = await conn.QueryAsync<ShortUrl>("SELECT * FROM short_urls WHERE user_id = @userId;", new { userId });
        return [.. urls];
    }

    public async Task<long> CreateAsync(ShortUrl shortUrl)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();

        // code is NOT NULL and is Base62(id), so it cannot be known until the row exists.
        // A placeholder outside the Base62 alphabet satisfies the constraint until the id is known.
        var id = await conn.QuerySingleAsync<long>(
            """
            INSERT INTO short_urls (code, long_url, created_at, title, user_id)
            VALUES (@Code, @LongUrl, @CreatedAt, @Title, @UserId)
            RETURNING id
            """,
            new
            {
                Code = NewPlaceholderCode(),
                shortUrl.LongUrl,
                shortUrl.CreatedAt,
                shortUrl.Title,
                shortUrl.UserId
            },
            tx);

        await conn.ExecuteAsync(
            "UPDATE short_urls SET code = @Code WHERE id = @Id",
            new { Code = Base62.Encode(id), Id = id },
            tx);

        await tx.CommitAsync();
        return id;
    }

    public async Task<ShortUrl?> GetByCodeAsync(string code)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        return await conn.QueryFirstOrDefaultAsync<ShortUrl>(
            """
            SELECT long_url AS LongUrl,
                   created_at AS CreatedAt,
                   title AS Title,
                   click_count AS ClickCount,
                   last_clicked_at AS LastClickedAt
            FROM short_urls
            WHERE code = @Code
            """,
            new { Code = code });
    }

    public async Task<bool> DeleteAsync(string code, long userId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM short_urls WHERE code = @Code and user_id = @UserId",
            new { Code = code, UserId = userId }
            );
        return rows > 0;
    }

    public async Task RecordClickAsync (string code, DateTime occurredOn)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(
            """
            UPDATE short_urls
            SET click_count = click_count + 1, last_clicked_at = @OccurredOn
            WHERE code = @Code
            """, new { Code = code, OccurredOn = occurredOn });
    }

    public async Task<bool> ApplyClickBatchAsync(ClickBatch clickBatch, CancellationToken cancellationToken = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        // 1) Claim the batch. Zero rows inserted means we've already processed it.
        await using (var claim = new NpgsqlCommand(
                         "INSERT INTO processed_batches (batch_id) VALUES (@id) ON CONFLICT DO NOTHING", conn, tx))
        {
            claim.Parameters.AddWithValue("id", clickBatch.BatchId);
            if (await claim.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                await tx.RollbackAsync(cancellationToken);
                return false;
            }
        }

        // 2) Apply every count in one statement.
        await using (var update = new NpgsqlCommand(
                         """
                         UPDATE short_urls s
                         SET click_count     = s.click_count + v.cnt,
                             last_clicked_at = GREATEST(s.last_clicked_at, @at)
                         FROM unnest(@codes, @counts) AS v(code, cnt)
                         WHERE s.code = v.code
                         """, conn, tx))
        {
            update.Parameters.AddWithValue("at", clickBatch.BatchDate);
            update.Parameters.AddWithValue("codes", clickBatch.Counts.Keys.ToArray());
            update.Parameters.AddWithValue("counts", clickBatch.Counts.Values.ToArray());
            await update.ExecuteNonQueryAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
        return true;
    }

    private static string NewPlaceholderCode() => $"_{Guid.NewGuid():N}"[..10];
}
