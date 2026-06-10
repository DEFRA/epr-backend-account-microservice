using System.Diagnostics;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BackendAccountService.Data.IntegrationTests.ConnectionWithEnrolmentsTests;

[Collection(SharedSqlCollection.Name)]
[Trait("Category", "IntegrationTest")]
[Trait("Category", "Nominating Delegated Person")]
public class NominateToDelegatedPersonTests : IClassFixture<PerClassDbFixture>, IAsyncLifetime
{
    private readonly PerClassDbFixture _db;
    private DbContextOptions<AccountsDbContext> _options = null!;
    private AccountsDbContext _writeDbContext = null!;
    private RoleManagementService _connectionsService = null!;

    public NominateToDelegatedPersonTests(PerClassDbFixture db)
    {
        _db = db;
    }

    public async Task InitializeAsync()
    {
        _options = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseSqlServer(_db.ConnectionString)
            .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
            .EnableSensitiveDataLogging()
            .Options;

        _writeDbContext = new AccountsDbContext(_options);

        await _writeDbContext.Database.EnsureCreatedAsync();

        _connectionsService = new RoleManagementService(_writeDbContext,
            new ValidationService(_writeDbContext, NullLogger<ValidationService>.Instance));
    }

    public async Task DisposeAsync()
    {
        await _writeDbContext.DisposeAsync();
    }

    [Theory]
    [InlineData(DbConstants.PersonRole.Admin)]
    [InlineData(DbConstants.PersonRole.Employee)]
    public async Task WhenApprovedPersonNominatesEnrolledPersonInEmployment_ThenNominationIsSuccessful(int personRole)
    {
        Guid organisationId = Guid.NewGuid();

        var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, personRole, DbConstants.EnrolmentStatus.Enrolled);

        var result = await _connectionsService.NominateToDelegatedPerson(
            connectionId: editedEnrolment.Connection.ExternalId,
            userId: authorisedPersonEnrolment.Connection.Person.User.UserId.Value,
            organisationId: organisationId,
            serviceKey: "Packaging",
            nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
            {
                RelationshipType = Entities.Conversions.RelationshipType.Employment,
                JobTitle = "Job Title of nominated person",
            });

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();

        await using var readDbContext = new AccountsDbContext(_options);

        var nominatedPersonEnrolment = await readDbContext
            .Enrolments
            .Where(enrolment =>
                enrolment.ConnectionId == editedEnrolment.ConnectionId &&
                enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Nominated)
            .FirstOrDefaultAsync(default);

        var delegatedPersonEnrolment = await readDbContext
            .DelegatedPersonEnrolments
            .Where(enrolment => enrolment.EnrolmentId == nominatedPersonEnrolment.Id)
            .Include(enrolment => enrolment.Enrolment.Connection)
            .FirstOrDefaultAsync(default);

        delegatedPersonEnrolment.Should().NotBeNull();

        delegatedPersonEnrolment.EnrolmentId.Should().NotBe(authorisedPersonEnrolment.Id);
        delegatedPersonEnrolment.EnrolmentId.Should().NotBe(editedEnrolment.Id);
        delegatedPersonEnrolment.NominatorEnrolmentId.Should().Be(authorisedPersonEnrolment.Id);
        delegatedPersonEnrolment.RelationshipType.Should().Be(Entities.Conversions.RelationshipType.Employment);
        delegatedPersonEnrolment.ConsultancyName.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.ComplianceSchemeName.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.OtherOrganisationName.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.OtherRelationshipDescription.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.NominatorDeclaration.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.NominatorDeclarationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        delegatedPersonEnrolment.NomineeDeclaration.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.NomineeDeclarationTime.Should().BeNull();
        delegatedPersonEnrolment.Enrolment.Connection.JobTitle.Should().Be("Job Title of nominated person");

        delegatedPersonEnrolment.Enrolment.EnrolmentStatusId.Should().Be(DbConstants.EnrolmentStatus.Nominated);
        delegatedPersonEnrolment.Enrolment.ServiceRoleId.Should().Be(DbConstants.ServiceRole.Packaging.DelegatedPerson.Id);

        nominatedPersonEnrolment.Connection.PersonRoleId.Should().Be(editedEnrolment.Connection.PersonRoleId);
    }

    [Theory]
    [InlineData(DbConstants.PersonRole.Admin)]
    [InlineData(DbConstants.PersonRole.Employee)]
    public async Task WhenApprovedPersonNominatesEnrolledPersonFromConsultancy_ThenNominationIsSuccessful(int personRole)
    {
        Guid organisationId = Guid.NewGuid();

        var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, personRole, DbConstants.EnrolmentStatus.Enrolled);

        var result = await _connectionsService.NominateToDelegatedPerson(
            connectionId: editedEnrolment.Connection.ExternalId,
            userId: authorisedPersonEnrolment.Connection.Person.User.UserId.Value,
            organisationId: organisationId,
            serviceKey: "Packaging",
            nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
            {
                RelationshipType = Entities.Conversions.RelationshipType.Consultancy,
                ConsultancyName = "Test Consultancy",
                NominatorDeclaration = "Test Tester from Consultancy"
            });

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();

        await using var readDbContext = new AccountsDbContext(_options);

        var nominatedPersonEnrolment = await readDbContext
            .Enrolments
            .Where(enrolment =>
                enrolment.ConnectionId == editedEnrolment.ConnectionId &&
                enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Nominated)
            .FirstOrDefaultAsync(default);

        var delegatedPersonEnrolment = await readDbContext
            .DelegatedPersonEnrolments
            .Where(enrolment => enrolment.EnrolmentId == nominatedPersonEnrolment.Id)
            .Include(enrolment => enrolment.Enrolment.Connection)
            .FirstOrDefaultAsync(default);

        delegatedPersonEnrolment.Should().NotBeNull();

        delegatedPersonEnrolment.EnrolmentId.Should().NotBe(authorisedPersonEnrolment.Id);
        delegatedPersonEnrolment.EnrolmentId.Should().NotBe(editedEnrolment.Id);
        delegatedPersonEnrolment.NominatorEnrolmentId.Should().Be(authorisedPersonEnrolment.Id);
        delegatedPersonEnrolment.RelationshipType.Should().Be(Entities.Conversions.RelationshipType.Consultancy);
        delegatedPersonEnrolment.ConsultancyName.Should().Be("Test Consultancy");
        delegatedPersonEnrolment.ComplianceSchemeName.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.OtherOrganisationName.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.OtherRelationshipDescription.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.NominatorDeclaration.Should().Be("Test Tester from Consultancy");
        delegatedPersonEnrolment.NominatorDeclarationTime.Should().BeCloseTo(DateTime.UtcNow,TimeSpan.FromSeconds(5));
        delegatedPersonEnrolment.NomineeDeclaration.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.NomineeDeclarationTime.Should().BeNull();
        delegatedPersonEnrolment.Enrolment.Connection.JobTitle.Should().Be("Job Title");

        delegatedPersonEnrolment.Enrolment.EnrolmentStatusId.Should().Be(DbConstants.EnrolmentStatus.Nominated);
        delegatedPersonEnrolment.Enrolment.ServiceRoleId.Should().Be(DbConstants.ServiceRole.Packaging.DelegatedPerson.Id);
    }

    [Theory]
    [InlineData(DbConstants.PersonRole.Admin)]
    [InlineData(DbConstants.PersonRole.Employee)]
    public async Task WhenApprovedPersonNominatesEnrolledPersonFromComplianceScheme_ThenNominationIsSuccessful(int personRole)
    {
        Guid organisationId = Guid.NewGuid();

        var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, personRole, DbConstants.EnrolmentStatus.Enrolled);

        var result = await _connectionsService.NominateToDelegatedPerson(
            connectionId: editedEnrolment.Connection.ExternalId,
            userId: authorisedPersonEnrolment.Connection.Person.User.UserId.Value,
            organisationId: organisationId,
            serviceKey: "Packaging",
            nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
            {
                RelationshipType = Entities.Conversions.RelationshipType.ComplianceScheme,
                ComplianceSchemeName = "Test Compliance Scheme",
                NominatorDeclaration = "Test Tester from Compliance Scheme"
            });

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();

        await using var readDbContext = new AccountsDbContext(_options);

        var nominatedPersonEnrolment = await readDbContext
            .Enrolments
            .Where(enrolment =>
                enrolment.ConnectionId == editedEnrolment.ConnectionId &&
                enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Nominated)
            .FirstOrDefaultAsync(default);

        var delegatedPersonEnrolment = await readDbContext
            .DelegatedPersonEnrolments
            .Where(enrolment => enrolment.EnrolmentId == nominatedPersonEnrolment.Id)
            .Include(enrolment => enrolment.Enrolment.Connection)
            .FirstOrDefaultAsync(default);

        delegatedPersonEnrolment.Should().NotBeNull();

        delegatedPersonEnrolment.EnrolmentId.Should().NotBe(authorisedPersonEnrolment.Id);
        delegatedPersonEnrolment.EnrolmentId.Should().NotBe(editedEnrolment.Id);
        delegatedPersonEnrolment.NominatorEnrolmentId.Should().Be(authorisedPersonEnrolment.Id);
        delegatedPersonEnrolment.RelationshipType.Should().Be(Entities.Conversions.RelationshipType.ComplianceScheme);
        delegatedPersonEnrolment.ConsultancyName.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.ComplianceSchemeName.Should().Be("Test Compliance Scheme");
        delegatedPersonEnrolment.OtherOrganisationName.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.OtherRelationshipDescription.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.NominatorDeclaration.Should().Be("Test Tester from Compliance Scheme");
        delegatedPersonEnrolment.NominatorDeclarationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        delegatedPersonEnrolment.NomineeDeclaration.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.NomineeDeclarationTime.Should().BeNull();
        delegatedPersonEnrolment.Enrolment.Connection.JobTitle.Should().Be("Job Title");

        delegatedPersonEnrolment.Enrolment.EnrolmentStatusId.Should().Be(DbConstants.EnrolmentStatus.Nominated);
        delegatedPersonEnrolment.Enrolment.ServiceRoleId.Should().Be(DbConstants.ServiceRole.Packaging.DelegatedPerson.Id);
    }

    [Theory]
    [InlineData(DbConstants.PersonRole.Admin)]
    [InlineData(DbConstants.PersonRole.Employee)]
    public async Task WhenApprovedPersonNominatesEnrolledPersonFromOtherOrganisation_ThenNominationIsSuccessful(int personRole)
    {
        Guid organisationId = Guid.NewGuid();

        var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, personRole, DbConstants.EnrolmentStatus.Enrolled);

        var result = await _connectionsService.NominateToDelegatedPerson(
            connectionId: editedEnrolment.Connection.ExternalId,
            userId: authorisedPersonEnrolment.Connection.Person.User.UserId.Value,
            organisationId: organisationId,
            serviceKey: "Packaging",
            nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
            {
                RelationshipType = Entities.Conversions.RelationshipType.Other,
                OtherOrganisationName = "Test Other Organisation",
                OtherRelationshipDescription = "Other Organisation Description",
                NominatorDeclaration = "Test Tester from Other Organisation"
            });

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();

        await using var readDbContext = new AccountsDbContext(_options);

        var nominatedPersonEnrolment = await readDbContext
            .Enrolments
            .Where(enrolment =>
                enrolment.ConnectionId == editedEnrolment.ConnectionId &&
                enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Nominated)
            .FirstOrDefaultAsync(default);

        var delegatedPersonEnrolment = await readDbContext
            .DelegatedPersonEnrolments
            .Where(enrolment => enrolment.EnrolmentId == nominatedPersonEnrolment.Id)
            .Include(enrolment => enrolment.Enrolment.Connection)
            .FirstOrDefaultAsync(default);

        delegatedPersonEnrolment.Should().NotBeNull();

        delegatedPersonEnrolment.EnrolmentId.Should().NotBe(authorisedPersonEnrolment.Id);
        delegatedPersonEnrolment.EnrolmentId.Should().NotBe(editedEnrolment.Id);
        delegatedPersonEnrolment.NominatorEnrolmentId.Should().Be(authorisedPersonEnrolment.Id);
        delegatedPersonEnrolment.RelationshipType.Should().Be(Entities.Conversions.RelationshipType.Other);
        delegatedPersonEnrolment.ConsultancyName.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.ComplianceSchemeName.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.OtherOrganisationName.Should().Be("Test Other Organisation");
        delegatedPersonEnrolment.OtherRelationshipDescription.Should().Be("Other Organisation Description");
        delegatedPersonEnrolment.NominatorDeclaration.Should().Be("Test Tester from Other Organisation");
        delegatedPersonEnrolment.NominatorDeclarationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        delegatedPersonEnrolment.NomineeDeclaration.Should().BeNullOrEmpty();
        delegatedPersonEnrolment.NomineeDeclarationTime.Should().BeNull();
        delegatedPersonEnrolment.Enrolment.Connection.JobTitle.Should().Be("Job Title");

        delegatedPersonEnrolment.Enrolment.EnrolmentStatusId.Should().Be(DbConstants.EnrolmentStatus.Nominated);
        delegatedPersonEnrolment.Enrolment.ServiceRoleId.Should().Be(DbConstants.ServiceRole.Packaging.DelegatedPerson.Id);
    }

    [Theory]
    [InlineData(DbConstants.PersonRole.Admin)]
    [InlineData(DbConstants.PersonRole.Employee)]
    public async Task WhenPendingApprovalApprovedPersonNominatesEnrolledPerson_ThenNominationIsSuccessful(int personRole)
    {
        Guid organisationId = Guid.NewGuid();

        var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Pending);

        var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment
            (_writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, personRole, DbConstants.EnrolmentStatus.Enrolled);

        var result = await _connectionsService.NominateToDelegatedPerson(
            connectionId: editedEnrolment.Connection.ExternalId,
            userId: authorisedPersonEnrolment.Connection.Person.User.UserId.Value,
            organisationId: organisationId,
            serviceKey: "Packaging",
            nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
            {
                RelationshipType = Entities.Conversions.RelationshipType.Consultancy,
                ConsultancyName = "Test Consultancy",
                NominatorDeclaration = "Test Tester"
            });

        result.Succeeded.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();

        await using var readDbContext = new AccountsDbContext(_options);

        var nominatedPersonEnrolment = await readDbContext
            .Enrolments
            .Where(enrolment =>
                enrolment.ConnectionId == editedEnrolment.ConnectionId &&
                enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Nominated)
            .FirstOrDefaultAsync(default);

        var delegatedPersonEnrolment = await readDbContext
            .DelegatedPersonEnrolments
            .Where(enrolment => enrolment.EnrolmentId == nominatedPersonEnrolment.Id)
            .FirstOrDefaultAsync(default);

        delegatedPersonEnrolment.Enrolment.EnrolmentStatusId.Should().Be(DbConstants.EnrolmentStatus.Nominated);
        delegatedPersonEnrolment.Enrolment.ServiceRoleId.Should().Be(DbConstants.ServiceRole.Packaging.DelegatedPerson.Id);
    }


    [Theory]
    [InlineData(DbConstants.EnrolmentStatus.NotSet)]
    [InlineData(DbConstants.EnrolmentStatus.Rejected)]
    public async Task WhenApprovedPersonTriesToNominateNonActiveEnrolmentUser_ThenNominationFails(int nonEnrolledStatus)
    {
        Guid organisationId = Guid.NewGuid();

        var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment
            (_writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, nonEnrolledStatus);

        var result = await _connectionsService.NominateToDelegatedPerson(
            connectionId: editedEnrolment.Connection.ExternalId,
            userId: authorisedPersonEnrolment.Connection.Person.User.UserId.Value,
            organisationId: organisationId,
            serviceKey: "Packaging",
            nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
            {
                RelationshipType = Entities.Conversions.RelationshipType.Consultancy,
                ConsultancyName = "Test Consultancy",
                NominatorDeclaration = "Test Tester"
            });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Not enrolled user cannot be nominated");

        await using var readDbContext = new AccountsDbContext(_options);

        var updatedEnrolment = await readDbContext.Enrolments
            .Where(enrolment => enrolment.Id == editedEnrolment.Id)
            .FirstOrDefaultAsync(default);

        updatedEnrolment.EnrolmentStatusId.Should().Be(nonEnrolledStatus);
        updatedEnrolment.ServiceRoleId.Should().Be(DbConstants.ServiceRole.Packaging.BasicUser.Id);
    }

    [Fact]
    public async Task WhenApprovedPersonTriesToNominateInvitedPerson_ThenNominationFails()
    {
        Guid organisationId = Guid.NewGuid();

        var authorisedPersonEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Approved);

        var editedEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment
            (_writeDbContext, organisationId, DbConstants.ServiceRole.Packaging.BasicUser.Key, DbConstants.PersonRole.Admin, DbConstants.EnrolmentStatus.Invited);

        var result = await _connectionsService.NominateToDelegatedPerson(
            connectionId: editedEnrolment.Connection.ExternalId,
            userId: authorisedPersonEnrolment.Connection.Person.User.UserId.Value,
            organisationId: organisationId,
            serviceKey: "Packaging",
            nominationRequest: new Core.Models.Request.DelegatedPersonNominationRequest
            {
                RelationshipType = Entities.Conversions.RelationshipType.Consultancy,
                ConsultancyName = "Test Consultancy",
                NominatorDeclaration = "Test Tester"
            });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invited user cannot be nominated");

        await using var readDbContext = new AccountsDbContext(_options);

        var updatedEnrolment = await readDbContext.Enrolments
            .Where(enrolment => enrolment.Id == editedEnrolment.Id)
            .FirstOrDefaultAsync(default);

        updatedEnrolment.EnrolmentStatusId.Should().Be(DbConstants.EnrolmentStatus.Invited);
        updatedEnrolment.ServiceRoleId.Should().Be(DbConstants.ServiceRole.Packaging.BasicUser.Id);
    }
}
