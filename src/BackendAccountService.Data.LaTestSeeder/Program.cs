using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.LaTestSeeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

Console.WriteLine("Generating data...");

var builder = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", true, true)
    .AddEnvironmentVariables();

var config = builder.Build();

string connectionString = config.GetConnectionString("AccountsDatabase");

var dbContext = new AccountsDbContext(
    new DbContextOptionsBuilder<AccountsDbContext>()
        .UseSqlServer(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Warning)
        .Options);

DataGenerator.GenerateStableLocalAuthorityData(dbContext);
DataGenerator.GenerateRandomLocalAuthorityData(dbContext);

Console.WriteLine("Data has been seeded, exiting");