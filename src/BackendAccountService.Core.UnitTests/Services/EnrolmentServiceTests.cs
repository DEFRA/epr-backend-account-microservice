using BackendAccountService.Data.Infrastructure;
using BackendAccountService.Core.UnitTests.TestHelpers;
using BackendAccountService.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;

namespace BackendAccountService.Core.UnitTests.Services;

[TestClass]
public class EnrolmentServiceTests
{
    private readonly NullLogger<EnrolmentsService> _logger = new();
    private AccountsDbContext _dbContext = null!;
    private IEnrolmentsService _sut = null!;
    
    [TestInitialize]
    public void Setup()
    {
        var contextOptions = new DbContextOptionsBuilder<AccountsDbContext>()
            .UseInMemoryDatabase("AccountsServiceTests")
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        _dbContext = new AccountsDbContext(contextOptions);
        
        EnrolmentsTestHelper.SetUpDatabase(_dbContext);

        _sut = new EnrolmentsService(_dbContext, _logger);
    }

    [TestMethod]
    public async Task DeleteEnrolmentsForPersonAsync_Should_Remove_Enrollment()
    {
        // Arrange
        var organisationId = _dbContext.Organisations.Single(x => x.Name == "organisation1").ExternalId;
        var person = _dbContext.Persons.Single(x => x.Email == "basicuser1@test.com");
        var personOrgConnection = person.OrganisationConnections.Single();
        var enrollment = personOrgConnection.Enrolments.Single();
        var serviceRoleId = 1;
        
        // Act
        var result = await _sut.DeleteEnrolmentsForPersonAsync(
            Guid.NewGuid(), 
            person.ExternalId, 
            organisationId, 
            serviceRoleId);
        
        // Assert
        result.Should().BeTrue();
        _dbContext.Enrolments.Where(x => x.Id == enrollment.Id).Should().BeEmpty();
    }
    
    [TestMethod]
    public async Task DeleteEnrolmentsForPersonAsync_Should_Remove_Enrollment_And_DelegatePersonEnrolment()
    {
        // Arrange
        var organisationId = _dbContext.Organisations.Single(x => x.Name == "organisation1").ExternalId;
        var approvedPerson = _dbContext.Persons.Single(x => x.Email == "approvedperson1@test.com");
        var person = _dbContext.Persons.Single(x => x.Email == "delegatedperson1@test.com");
        var personOrgConnection = person.OrganisationConnections.Single();
        var enrolment = personOrgConnection.Enrolments.Single();
        var serviceRoleId = 1;

        _dbContext.AddDelegatedPersonEnrolment(enrolment, approvedPerson.OrganisationConnections.Single().Enrolments.Single());
        
        // Act
        var result = await _sut.DeleteEnrolmentsForPersonAsync(
            Guid.NewGuid(), 
            person.ExternalId, 
            organisationId, 
            serviceRoleId);
        
        // Assert
        result.Should().BeTrue();
        _dbContext.Enrolments.Where(x => x.Id == enrolment.Id).Should().BeEmpty();
        _dbContext.DelegatedPersonEnrolments.Should().BeEmpty();
    }
    
    [TestMethod]
    public async Task DeleteEnrolmentsForPersonAsync_When_Wrong_ServiceRole_Should_Return_False()
    {
        // Arrange
        var organisationId = _dbContext.Organisations.Single(x => x.Name == "organisation1").ExternalId;
        var person = _dbContext.Persons.Single(x => x.Email == "basicuser1@test.com");
        var personOrgConnection = person.OrganisationConnections.Single();
        var enrollment = personOrgConnection.Enrolments.Single();
        var serviceRoleId = 999;
        
        // Act
        var result = await _sut.DeleteEnrolmentsForPersonAsync(
            Guid.NewGuid(), 
            person.ExternalId, 
            organisationId, 
            serviceRoleId);
        
        // Assert
        result.Should().BeFalse();
        _dbContext.Enrolments.Where(x => x.Id == enrollment.Id).Should().NotBeEmpty();
    }
    
    [TestMethod]
    public async Task DeleteEnrolmentsForPersonAsync_When_Different_Org_Should_Not_Delete_Enrollments()
    {
        // Arrange
        var organisationId = _dbContext.Organisations.Single(x => x.Name == "organisation1").ExternalId;
        var person = _dbContext.Persons.Single(x => x.Email == "basicuser1@test.com");
        var personOrgConnection = person.OrganisationConnections.Single();
        var enrollment = personOrgConnection.Enrolments.Single();
        var serviceRoleId = 1;
        
        // Act
        var result = await _sut.DeleteEnrolmentsForPersonAsync(
            Guid.NewGuid(), 
            person.ExternalId, 
            Guid.NewGuid(), 
            serviceRoleId);
        
        // Assert
        result.Should().BeTrue();
        _dbContext.Enrolments.Where(x => x.Id == enrollment.Id).Should().NotBeEmpty();
    }
    
    [TestMethod]
    public async Task IsUserEnrolledAsync_When_User_Enrolled_Should_Return_True()
    {
        // Arrange
        var organisationId = _dbContext.Organisations.Single(x => x.Name == "organisation1").ExternalId;
        var userEmail = "basicuser1@test.com";
        var person = _dbContext.Persons.Single(x => x.Email == userEmail);
        var personOrgConnection = person.OrganisationConnections.Single();
        var enrollment = personOrgConnection.Enrolments.Single();
        
        // Act
        var result = await _sut.IsUserEnrolledAsync(userEmail, organisationId);
        
        // Assert
        result.Should().BeTrue();
        _dbContext.Enrolments.Where(x => x.Id == enrollment.Id).Should().NotBeEmpty();
    }
    
    [TestMethod]
    public async Task IsUserEnrolledAsync_When_User_Has_No_Enrolment_Should_Return_False()
    {
        // Arrange
        var organisationId = _dbContext.Organisations.Single(x => x.Name == "organisation1").ExternalId;

        var userEmail = "basicuser1@test.com";
        var person = _dbContext.Persons.Single(x => x.Email == userEmail);
        var personOrganisationConnection = person.OrganisationConnections.Single();
        var enrollment = personOrganisationConnection.Enrolments.Single();
        var serviceRoleId = 1;
        
        var isPersonEnrolmentDeleted = await _sut.DeleteEnrolmentsForPersonAsync(
            Guid.NewGuid(), 
            person.ExternalId, 
            organisationId, 
            serviceRoleId);
        
        // Act
        var isUserEnrolled = await _sut.IsUserEnrolledAsync(userEmail, organisationId);
        
        // Assert
        isPersonEnrolmentDeleted.Should().BeTrue();
        isUserEnrolled.Should().BeFalse();
        _dbContext.Enrolments.Where(x => x.Id == enrollment.Id).Should().BeEmpty();
    }
}