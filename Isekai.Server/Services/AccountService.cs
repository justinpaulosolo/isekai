using Isekai.Data;
using Isekai.Data.Repositories;
using Isekai.Server.Models;

namespace Isekai.Server.Services;

public interface IAccountService
{
    Task<long> GetOrCreateUserAsync(string provider, string subject, string? name, string? email);
}

public class AccountService (IAccountRepository accountRepository) : IAccountService
{
    public async Task<long> GetOrCreateUserAsync(string provider, string subject, string? name, string? email)
    {
        return await accountRepository.UpsertExternalUserAsync(provider, subject, name, email);
    }
}