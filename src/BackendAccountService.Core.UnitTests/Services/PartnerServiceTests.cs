using BackendAccountService.Core.Services;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class PartnerServiceTests
{
    private AccountsDbContext? _accountContext;
    private PartnerService? _partnerService;

    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("PartnerServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        SetUpDatabase(contextOptions);
        _accountContext = new AccountsDbContext(contextOptions);
        _partnerService = new PartnerService(_accountContext);
    }

    [TestMethod]
    public async Task GetPersonByUserIdAsync_WhenValidId_ThenReturnPerson()
    {
        // Act
        var partnerRoles = await _partnerService!.GetPartnerRoles();

        // Assert
        partnerRoles.Should().BeEquivalentTo(new Dictionary<string, PartnerRole>
        {
            { "Not Set", new PartnerRole { Id = 0, Name = "Not Set" }},
            { "Individual Partner", new PartnerRole { Id = 1, Name = "Individual Partner" }},
            { "Corporate Partner", new PartnerRole { Id = 2, Name = "Corporate Partner" }}
        }, options => options.ComparingByMembers<PartnerRole>());
    }

    private static void SetUpDatabase(DbContextOptions<AccountsDbContext> contextOptions)
    {
        using var setupContext = new AccountsDbContext(contextOptions);

        // Critical to avoid tests affecting one another, and previous runs holding old data
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();
    }
}
