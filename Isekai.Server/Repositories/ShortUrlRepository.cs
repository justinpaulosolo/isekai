using Dapper;
using Isekai.Server.Models;
using Npgsql;

namespace Isekai.Server.Repositories;

public interface IShortUrlRepository
{
    Task<ShortUrl?> GetByCodeAsync(string code);
    Task<long> CreateAsync(ShortUrl shortUrl);
    Task DeleteAsync(string code);
}

public class ShortUrlRepository(NpgsqlDataSource dataSource) : IShortUrlRepository
{
    public async Task<long> CreateAsync(ShortUrl shortUrl)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();

        // code is NOT NULL and is Base62(id), so it cannot be known until the row exists.
        // A placeholder outside the Base62 alphabet satisfies the constraint until the id is known.
        var id = await conn.QuerySingleAsync<long>(
            """
            INSERT INTO short_urls (code, long_url, created_at, title)
            VALUES (@Code, @LongUrl, @CreatedAt, @Title)
            RETURNING id
            """,
            new
            {
                Code = NewPlaceholderCode(),
                shortUrl.LongUrl,
                shortUrl.CreatedAt,
                shortUrl.Title
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

    public async Task DeleteAsync(string code)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(
            "DELETE FROM short_urls WHERE code = @Code",
            new { Code = code }
            );
    }

    private static string NewPlaceholderCode() => $"_{Guid.NewGuid():N}"[..10];
}
