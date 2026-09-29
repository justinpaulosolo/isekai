using Dapper;
using Isekai.Server.Models;
using Npgsql;

namespace Isekai.Server.Repositories;

public interface IShortUrlRepository
{
    Task<ShortUrl?> GetByCodeAsync(string code);
    Task<long> CreateAsync(ShortUrl shortUrl);
}

public class ShortUrlRepository(IConfiguration config) : IShortUrlRepository
{
    private readonly string _connectionString = config.GetConnectionString("Default")!;

    public async Task<long> CreateAsync(ShortUrl shortUrl)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        const string sql = @"
            INSERT INTO short_urls (long_url, created_at)
            VALUES (@long_url, created_at)
            RETURNING id";
        return await conn.QuerySingleAsync<long>(sql, shortUrl);
    }

    public async Task<ShortUrl?> GetByCodeAsync(string code)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        return await conn.QueryFirstOrDefaultAsync<ShortUrl>(
            @"SELECT * FROM short_urls WHERE code = @code", new { Code = code });
    }
}
