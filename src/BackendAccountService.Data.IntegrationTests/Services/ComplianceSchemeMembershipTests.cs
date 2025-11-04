using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Services;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Data.IntegrationTests.Containers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using BackendAccountService.Core.Models.Responses;

namespace BackendAccountService.Data.IntegrationTests.Services;

[TestClass]
public class ComplianceSchemeMembershipTests
{
    private static AzureSqlDbContainer _database = null!;
    private static DbContextOptions<AccountsDbContext> _options = null!;
    private AccountsDbContext _writeDbContext = null!;
    private ComplianceSchemeService _complianceSchemeService = null!;

    [ClassInitialize]
    public static async Task TestFixtureSetup(TestContext _)
    {
        _database = await AzureSqlDbContainer.StartDockerDbAsync();

        _options = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseSqlServer(_database.ConnectionString)
            .LogTo(message =>
            {
                Debug.WriteLine(message);
            }, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .Options;
    }
    
    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static async Task TestFixtureTearDown()
    {
        await _database.StopAsync();
    }
    
    [TestInitialize]
    public async Task Setup()
    {
        _writeDbContext = new AccountsDbContext(_options);

        // Ensure a clean database state before each test
        await _writeDbContext.Database.EnsureDeletedAsync();
        await _writeDbContext.Database.EnsureCreatedAsync();

        _complianceSchemeService = new ComplianceSchemeService(_writeDbContext, NullLogger<ComplianceSchemeService>.Instance);
    }

    [TestCleanup]
    public async Task TearDown()
    {
        await _writeDbContext.DisposeAsync();
    }
    
    [TestMethod]
    [TestCategory("ComplianceSchemeMembership")]
    public async Task WhenProducerIsDissociatedFromComplianceScheme_ThenGetInfoForSelectedSchemeRemovalContainsMultipleProducerEmails()
    {
        var producerOrganisationId = Guid.NewGuid();

        var producerApprovedPerson = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext,
            producerOrganisationId,
            DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, 
            DbConstants.PersonRole.Admin, 
            DbConstants.EnrolmentStatus.Approved);

        var producerDelegatedPerson = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext,
            producerOrganisationId,
            DbConstants.ServiceRole.Packaging.DelegatedPerson.Key, 
            DbConstants.PersonRole.Admin, 
            DbConstants.EnrolmentStatus.Approved);

        var producerBasicUser = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext,
            producerOrganisationId,
            DbConstants.ServiceRole.Packaging.BasicUser.Key, 
            DbConstants.PersonRole.Employee, 
            DbConstants.EnrolmentStatus.Enrolled);
        
        var complianceSchemeOrganisationId = Guid.NewGuid();
        
        var operatorOrganisationEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext, 
            complianceSchemeOrganisationId,
            DbConstants.ServiceRole.Packaging.BasicUser.Key,
            DbConstants.PersonRole.Admin, 
            DbConstants.EnrolmentStatus.Enrolled,
            isComplianceScheme: true);

        var complianceSchemeCompaniesHouseNumber = "06929701";
        
        var readDbContext = new AccountsDbContext(_options);

        var complianceScheme = await readDbContext.ComplianceSchemes
            .Where(complianceScheme => complianceScheme.CompaniesHouseNumber == complianceSchemeCompaniesHouseNumber &&
                                       complianceScheme.NationId == DbConstants.Nation.NorthernIreland)
            .FirstOrDefaultAsync(default);

        operatorOrganisationEnrolment.Connection.Organisation.CompaniesHouseNumber =
            complianceSchemeCompaniesHouseNumber;

        await _writeDbContext.SaveChangesAsync(
            operatorOrganisationEnrolment.Connection.Person.User.UserId.Value,
            operatorOrganisationEnrolment.Connection.Organisation.ExternalId,
            default);
        
        await _complianceSchemeService.SelectComplianceSchemeAsync(new SelectComplianceSchemeRequest()
        {
            ComplianceSchemeId = complianceScheme.ExternalId,
            ProducerOrganisationId = producerOrganisationId,
            UserOId = operatorOrganisationEnrolment.Connection.Person.User.UserId.Value
        });

        var selectedScheme = await readDbContext.SelectedSchemes
            .Where(scheme => scheme.ComplianceSchemeId == complianceScheme.Id)
            .FirstOrDefaultAsync(default);

        var infoResponse = await _complianceSchemeService.GetInfoForSelectedSchemeRemoval(
            complianceSchemeOrganisationId,
            selectedScheme.ExternalId, 
            operatorOrganisationEnrolment.Connection.Person.User.UserId.Value);

        infoResponse.IsSuccess.Should().BeTrue();

        var info = infoResponse.Value;
        info.OrganisationName.Should().Be(producerApprovedPerson.Connection.Organisation.Name);
        info.OrganisationNation.Should().Be(NationMappings.GetNation(producerApprovedPerson.Connection.Organisation.NationId));
        info.OrganisationNumber.Should().Be(producerApprovedPerson.Connection.Organisation.ReferenceNumber);
        info.ComplianceSchemeName.Should().Be(operatorOrganisationEnrolment.Connection.Organisation.Name);
        info.ComplianceSchemeNation.Should().Be(NationMappings.GetNation(complianceScheme.NationId));
        info.RemovalNotificationRecipients.Count.Should().Be(2);
        info.RemovalNotificationRecipients.Should().ContainEquivalentOf(new EmailRecipient
        {
            Email = producerApprovedPerson.Connection.Person.Email,
            FirstName = producerApprovedPerson.Connection.Person.FirstName,
            LastName = producerApprovedPerson.Connection.Person.LastName
        });
        info.RemovalNotificationRecipients.Should().ContainEquivalentOf(new EmailRecipient
        {
            Email = producerDelegatedPerson.Connection.Person.Email,
            FirstName = producerDelegatedPerson.Connection.Person.FirstName,
            LastName = producerDelegatedPerson.Connection.Person.LastName
        });
        info.RemovalNotificationRecipients
            .Select(recipient => recipient.Email)
            .Should().NotContain(producerBasicUser.Connection.Person.Email);
    }
    
    //todo: this works when run individually, but failed when run as part of a full test run
    // was that it's old behaviour, or is it the new int test interfering by what it leaves in the db?
    [TestMethod]
    [TestCategory("ComplianceSchemeMembership")]
    public async Task WhenProducerIsDissociatedFromComplianceScheme_ThenPendingApprovalDelegatedPersonWillNotBeInRemovalNotificationRecipients()
    {
        var producerOrganisationId = Guid.NewGuid();

        var producerApprovedPersonApproved = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext,
            producerOrganisationId,
            DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, 
            DbConstants.PersonRole.Admin, 
            DbConstants.EnrolmentStatus.Pending);        
        
        var producerApprovedPersonPending = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext,
            producerOrganisationId,
            DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, 
            DbConstants.PersonRole.Admin, 
            DbConstants.EnrolmentStatus.Pending);

        var producerDelegatedPersonApproved = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext,
            producerOrganisationId,
            DbConstants.ServiceRole.Packaging.DelegatedPerson.Key, 
            DbConstants.PersonRole.Admin, 
            DbConstants.EnrolmentStatus.Approved);

        var producerDelegatedPersonPending = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext,
            producerOrganisationId,
            DbConstants.ServiceRole.Packaging.DelegatedPerson.Key, 
            DbConstants.PersonRole.Admin, 
            DbConstants.EnrolmentStatus.Pending);        
        
        var producerBasicUser = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext,
            producerOrganisationId,
            DbConstants.ServiceRole.Packaging.BasicUser.Key, 
            DbConstants.PersonRole.Employee, 
            DbConstants.EnrolmentStatus.Enrolled);
        
        var complianceSchemeOrganisationId = Guid.NewGuid();
        
        var operatorOrganisationEnrolment = await DatabaseDataGenerator.InsertRandomEnrolment(
            _writeDbContext, 
            complianceSchemeOrganisationId,
            DbConstants.ServiceRole.Packaging.BasicUser.Key,
            DbConstants.PersonRole.Admin, 
            DbConstants.EnrolmentStatus.Enrolled,
            isComplianceScheme: true);

        var complianceSchemeCompaniesHouseNumber = "06929701";
        
        var readDbContext = new AccountsDbContext(_options);

        var complianceScheme = await readDbContext.ComplianceSchemes
            .Where(complianceScheme => complianceScheme.CompaniesHouseNumber == complianceSchemeCompaniesHouseNumber)
            .FirstOrDefaultAsync(default);

        operatorOrganisationEnrolment.Connection.Organisation.CompaniesHouseNumber =
            complianceSchemeCompaniesHouseNumber;

        await _writeDbContext.SaveChangesAsync(
            operatorOrganisationEnrolment.Connection.Person.User.UserId.Value,
            operatorOrganisationEnrolment.Connection.Organisation.ExternalId,
            default);
        
        await _complianceSchemeService.SelectComplianceSchemeAsync(new SelectComplianceSchemeRequest()
        {
            ComplianceSchemeId = complianceScheme.ExternalId,
            ProducerOrganisationId = producerOrganisationId,
            UserOId = operatorOrganisationEnrolment.Connection.Person.User.UserId.Value
        });

        var selectedScheme = await readDbContext.SelectedSchemes
            .Where(scheme => scheme.ComplianceSchemeId == complianceScheme.Id)
            .FirstOrDefaultAsync(default);

        var infoResponse = await _complianceSchemeService.GetInfoForSelectedSchemeRemoval(
            complianceSchemeOrganisationId,
            selectedScheme.ExternalId, 
            operatorOrganisationEnrolment.Connection.Person.User.UserId.Value);

        infoResponse.IsSuccess.Should().BeTrue();

        var info = infoResponse.Value;
        info.RemovalNotificationRecipients.Count.Should().Be(3); 
        info.RemovalNotificationRecipients.Should().ContainEquivalentOf(new EmailRecipient
        {
            Email = producerApprovedPersonApproved.Connection.Person.Email,
            FirstName = producerApprovedPersonApproved.Connection.Person.FirstName,
            LastName = producerApprovedPersonApproved.Connection.Person.LastName
        });
        info.RemovalNotificationRecipients.Should().ContainEquivalentOf(new EmailRecipient
        {
            Email = producerApprovedPersonPending.Connection.Person.Email,
            FirstName = producerApprovedPersonPending.Connection.Person.FirstName,
            LastName = producerApprovedPersonPending.Connection.Person.LastName
        });
        info.RemovalNotificationRecipients.Should().ContainEquivalentOf(new EmailRecipient
        {
            Email = producerDelegatedPersonApproved.Connection.Person.Email,
            FirstName = producerDelegatedPersonApproved.Connection.Person.FirstName,
            LastName = producerDelegatedPersonApproved.Connection.Person.LastName
        });

        info.RemovalNotificationRecipients
            .Select(recipient => recipient.Email)
            .Should().NotContain(producerDelegatedPersonPending.Connection.Person.Email);        
        
        info.RemovalNotificationRecipients
            .Select(recipient => recipient.Email)
            .Should().NotContain(producerBasicUser.Connection.Person.Email);
    }    
}