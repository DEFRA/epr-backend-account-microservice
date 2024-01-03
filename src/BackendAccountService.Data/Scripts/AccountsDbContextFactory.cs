using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BackendAccountService.Data;

public class AccountsDbContextFactory : IDesignTimeDbContextFactory<AccountsDbContext>
{
    public AccountsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AccountsDbContext>();

        optionsBuilder.UseSqlServer(args.Length > 0 ? args[0] : "-");

        return new AccountsDbContext(optionsBuilder.Options);
    }
}
