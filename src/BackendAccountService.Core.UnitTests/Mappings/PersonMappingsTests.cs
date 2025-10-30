using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.UnitTests.Mappings;

[TestClass]
public class PersonMappingsTests
{
    [TestMethod]
    public void PersonMappings_WhenPersonIsNull_ThenReturnsNull()
    {
        // Arrange
        Person person = null;

        // Act
        var responseModel = PersonMappings.GetPersonModelFromPerson(person);

        // Assert
        responseModel.Should().BeNull();

    }

    [TestMethod]
    public void PersonMappings_WhenValidPerson_ThenReutrnPopulatedModel()
    {
        // Arrange
        var telphoneNumber = "0110 30393939";
        var firstName = "Test";
        var lastName = "Person";
        var email = "test@person.com";
        var isDeleted = true;
        var userId = Guid.NewGuid();

        var person = new Person
        {
            Telephone = telphoneNumber,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            IsDeleted = isDeleted,
            User = new User { UserId = userId }
        };

        // Act
        var responseModel = PersonMappings.GetPersonModelFromPerson(person);

        // Assert
        responseModel.Should().NotBeNull();
        responseModel.TelephoneNumber.Should().Be(telphoneNumber);
        responseModel.FirstName.Should().Be(firstName);
        responseModel.LastName.Should().Be(lastName);
        responseModel.ContactEmail.Should().Be(email);
        responseModel.IsDeleted.Should().BeTrue();
        responseModel.UserId.Should().Be(userId);
    }

    [TestMethod]
    public void GetPersonModel_WhenValidReprocessorExporterAccount_ThenReturnsCorrectPerson()
    {
        // Arrange
        const string firstName = "FirstName";
        const string lastName = "LastName";
        const string email = "test@example.com";
        const string telephone = "01234567890";
        Guid userId = Guid.NewGuid();

        var account = new ReprocessorExporterAccount
        {
            Person = new PersonModel
            {
                FirstName = firstName,
                LastName = lastName,
                ContactEmail = email,
                TelephoneNumber = telephone
            },
            User = new UserModel
            {
                UserId = userId,
                ExternalIdpId = null,
                ExternalIdpUserId = null,
                Email = email
            }
        };

        // Act
        var responseModel = PersonMappings.GetPersonModel(account);

        // Assert
        responseModel.Should().NotBeNull();
        responseModel.Telephone.Should().Be(telephone);
        responseModel.FirstName.Should().Be(firstName);
        responseModel.LastName.Should().Be(lastName);
        responseModel.Email.Should().Be(email);
        responseModel.IsDeleted.Should().BeFalse();
        responseModel.User.Should().NotBeNull();
        responseModel.User!.UserId.Should().Be(userId);

        responseModel.UserId.Should().BeNull();
        responseModel.OrganisationConnections.Should().BeNull();
        responseModel.FromPersonConnections.Should().BeNull();
        responseModel.ToPersonConnections.Should().BeNull();
        responseModel.RegulatorComments.Should().BeNull();
    }
}