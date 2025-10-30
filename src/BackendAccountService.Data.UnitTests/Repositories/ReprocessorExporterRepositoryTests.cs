using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;

namespace BackendAccountService.Data.UnitTests.Repositories;

[TestClass]
public class ReprocessorExporterRepositoryTests
{
    private AccountsDbContext _accountsDbContext = null!;
    private ReprocessorExporterRepository _repository = null!;
    private static DbContextOptions<AccountsDbContext> _contextOptions = default!;

    private const int ValidNationId = 1;
    private const int InvalidNationId = 999;
    private static Guid ValidOrganisationId = Guid.Parse("e3db9f93-2301-471d-b944-f05dfab5f3e7");
    private readonly List<Guid> ValidUserIds = new List<Guid>() { Guid.Parse("5a562763-7a07-4290-bad9-a42ad1e6cc18"), Guid.Parse("0293d53c-2c6c-4ea0-bd64-0f4b386e03f5") };

    [TestInitialize]
    public void Setup()
    {
        _contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase($"ReprocessorExporterRepositoryTests_{Guid.NewGuid()}")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _accountsDbContext = new AccountsDbContext(_contextOptions);
        _repository = new ReprocessorExporterRepository(_accountsDbContext);

        SetUpDatabase(_contextOptions);
    }

    [TestMethod]
    public async Task GetNationDetailsByNationId_WhenNationExists_ReturnsNation()
    {
        // Act
        var result = await _repository.GetNationDetailsByNationId(ValidNationId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(ValidNationId);
        result.Name.Should().Be("England");
    }

    [TestMethod]
    public async Task GetNationDetailsByNationId_WhenNationDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() => _repository.GetNationDetailsByNationId(InvalidNationId));
    }

    [TestMethod]
    public async Task GetOrganisationDetailsByOrgId_WhenOrganisationExists_ReturnsOrganisation()
    {
        // Act
        var result = await _repository.GetOrganisationDetailsByOrgId(ValidOrganisationId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Organisation");
        result.CompaniesHouseNumber.Should().Be("Test Company House Number");
    }

    [TestMethod]
    public async Task GetOrganisationDetailsByOrgId_WhenOrganisationDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(() => _repository.GetOrganisationDetailsByOrgId(Guid.Empty));
    }

    [TestMethod]
    public async Task GetPersonDetailsByIds_WhenPersonExists_ReturnsOrganisation()
    {
        // Act
        var result = await _repository.GetPersonDetailsByIds(ValidOrganisationId, ValidUserIds);

        // Assert
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
    }

    private static void SetUpDatabase(DbContextOptions<AccountsDbContext> options)
    {
        using var context = new AccountsDbContext(options);
        context.Database.EnsureCreated();

        context.Nations.RemoveRange(context.Nations);
        context.Organisations.RemoveRange(context.Organisations);
        context.SaveChanges(Guid.Empty, Guid.Empty);

        var nation = new Nation
        {
            Id = ValidNationId,
            Name = "England",
            NationCode = "ENG"
        };

        var organisation = new Organisation
        {
            Id = 5,
            ExternalId = ValidOrganisationId,
            Name = "Test Organisation",
            TradingName = "Test Trading Name",
            CompaniesHouseNumber = "Test Company House Number",
            BuildingNumber = "1",
            BuildingName = "Test Building Name",
            SubBuildingName = "Test SubBuilding Name",
            Street = "Test Street",
            Locality = "Test Locality",
            Town = "Test Town",
            Country = "Test Country",
            Postcode = "Test Postcode",
            OrganisationType = new OrganisationType
            {
                Id = 10,
                Name = "Test Organisation Type"
            },
            PersonOrganisationConnections = new[]
            {
                new PersonOrganisationConnection{
                    Id = 10,
                    JobTitle = "Test Job Title",
                    Person = new()
                    {
                        Id = 8,
                        FirstName = "Test First Name",
                        LastName = "Test Last Name",
                        Email = "Test Email",
                        Telephone = "Test Telephone",
                        User = new User
                        {
                           UserId = Guid.Parse("5a562763-7a07-4290-bad9-a42ad1e6cc18")
                        }
                    }
               },
               new PersonOrganisationConnection{
                    Id = 20,
                    JobTitle = "Test Job Title 2",
                    Person = new()
                    {
                        Id = 12,
                        FirstName = "Test First Name 2",
                        LastName = "Test Last Name 2",
                        Email = "Test Email 2",
                        Telephone = "Test Telephone 2",
                        User = new User
                        {
                           UserId = Guid.Parse("0293d53c-2c6c-4ea0-bd64-0f4b386e03f5")
                        }
                    }
               }
            }
        };

        context.Nations.Add(nation);
        context.Organisations.Add(organisation);
        context.SaveChanges(Guid.Empty, Guid.Empty);
    }
}