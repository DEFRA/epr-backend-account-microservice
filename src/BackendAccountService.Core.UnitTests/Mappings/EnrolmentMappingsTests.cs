using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Data.Entities;
using ServiceRole = BackendAccountService.Data.DbConstants.ServiceRole;

namespace BackendAccountService.Core.UnitTests.Mappings;

[TestClass]
public class EnrolmentMappingsTests
{
    private ReExEnrolmentMaps? _reExEnrolmentMaps;

    [TestInitialize]
    public void TestInitialize()
    {
        _reExEnrolmentMaps = new ReExEnrolmentMaps();
    }

    [TestMethod]
    public void GetEnrolmentFromReprocessorExporterAddOrganisation_WhenUserIsApproved_ShouldMapCorrectly()
    {
        // Arrange
        var personEntity = new Person
        {
            Id = 101,
            FirstName = "Approved",
            LastName = "User"
        };

        var account = new ReprocessorExporterAddOrganisation
        {
            User = new ReprocessorExporterUserModel
            {
                UserId = Guid.NewGuid(),
                JobTitle = "Director",
                IsApprovedUser = true
            },
            Organisation = new ReprocessorExporterOrganisationModel
            {
                OrganisationType = Models.OrganisationType.CompaniesHouseCompany,
                ProducerType = Models.ProducerType.Partnership,
                CompaniesHouseNumber = "CH12345",
                Name = "Approved Org Inc.",
                Address = new AddressModel
                {
                    SubBuildingName = "Unit A",
                    BuildingName = "Approval House",
                    BuildingNumber = "10",
                    Street = "Validation Road",
                    Locality = "Test District",
                    DependentLocality = "Near Test Center",
                    Town = "Testville",
                    County = "Testshire",
                    Country = "Testland",
                    Postcode = "T5T 1AP"
                },
                ValidatedWithCompaniesHouse = true,
                IsComplianceScheme = false,
                Nation = Models.Nation.England
            }
        };

        // Act
        var enrolment = _reExEnrolmentMaps!.GetAdminEnrolmentForCurrentUser(account, personEntity);

        // Assert
        enrolment.Should().NotBeNull();
        enrolment.EnrolmentStatusId.Should().Be(Data.DbConstants.EnrolmentStatus.Enrolled);

        // Assert unmapped Enrolment properties remain default
        enrolment.Id.Should().Be(0);
        enrolment.ConnectionId.Should().Be(0);
        enrolment.ValidFrom.Should().BeNull();
        enrolment.ValidTo.Should().BeNull();
        enrolment.ServiceRole.Should().BeNull();
        enrolment.EnrolmentStatus.Should().BeNull();
        enrolment.DelegatedPersonEnrolment.Should().BeNull();
        enrolment.IsDeleted.Should().BeFalse();

        enrolment.Connection.Should().NotBeNull();
        enrolment.Connection.JobTitle.Should().Be(account.User.JobTitle);
        enrolment.Connection.OrganisationRoleId.Should().Be(Data.DbConstants.OrganisationRole.Employer);
        enrolment.Connection.PersonId.Should().Be(personEntity.Id);
        enrolment.Connection.PersonRoleId.Should().Be(Data.DbConstants.PersonRole.Admin);

        enrolment.Connection.Organisation.Should().NotBeNull();
        var org = enrolment.Connection.Organisation;
        org.OrganisationTypeId.Should().Be(OrganisationTypeMappings.GetOrganisationTypeId(account.Organisation.OrganisationType));
        org.ProducerTypeId.Should().Be(OrganisationMappings.GetProducerTypeId(account.Organisation.ProducerType));
        org.CompaniesHouseNumber.Should().Be(account.Organisation.CompaniesHouseNumber);
        org.Name.Should().Be(account.Organisation.Name);
        org.SubBuildingName.Should().Be(account.Organisation.Address.SubBuildingName);
        org.BuildingName.Should().Be(account.Organisation.Address.BuildingName);
        org.BuildingNumber.Should().Be(account.Organisation.Address.BuildingNumber);
        org.Street.Should().Be(account.Organisation.Address.Street);
        org.Locality.Should().Be(account.Organisation.Address.Locality);
        org.DependentLocality.Should().Be(account.Organisation.Address.DependentLocality);
        org.Town.Should().Be(account.Organisation.Address.Town);
        org.County.Should().Be(account.Organisation.Address.County);
        org.Country.Should().Be(account.Organisation.Address.Country);
        org.Postcode.Should().Be(account.Organisation.Address.Postcode);
        org.ValidatedWithCompaniesHouse.Should().Be(account.Organisation.ValidatedWithCompaniesHouse);
        org.IsComplianceScheme.Should().Be(account.Organisation.IsComplianceScheme);
        org.NationId.Should().Be(NationMappings.GetNationId((Models.Nation)account.Organisation.Nation));
        org.ExternalId.Should().NotBeEmpty(); // it's a new Guid

        enrolment.ApprovedPersonEnrolment.Should().BeNull();
    }

    [TestMethod]
    public void GetEnrolmentFromReprocessorExporterAddOrganisation_WhenUserIsNotApproved_ShouldMapCorrectly()
    {
        // Arrange
        var personEntity = new Person
        {
            Id = 102,
            FirstName = "Basic",
            LastName = "User"
        };

        var account = new ReprocessorExporterAddOrganisation
        {
            User = new ReprocessorExporterUserModel
            {
                UserId = Guid.NewGuid(),
                JobTitle = "Secretary",
                IsApprovedUser = false
            },
            Organisation = new ReprocessorExporterOrganisationModel
            {
                OrganisationType = Models.OrganisationType.NonCompaniesHouseCompany,
                ProducerType = Models.ProducerType.SoleTrader,
                Name = "Basic Org Ltd.",
                Address = new AddressModel { Town = "Basicville", Country = "Testland", Postcode = "B4S 1IC" },
                Nation = Models.Nation.Scotland
            },
            DeclarationTimeStamp = null
        };

        // Act
        var enrolment = _reExEnrolmentMaps!.GetAdminEnrolmentForCurrentUser(account, personEntity);

        // Assert
        enrolment.Should().NotBeNull();
        enrolment.ServiceRoleId.Should().Be(ServiceRole.ReprocessorExporter.AdminUser.Id);
        enrolment.EnrolmentStatusId.Should().Be(Data.DbConstants.EnrolmentStatus.Enrolled);

        // Assert unmapped Enrolment properties remain default
        enrolment.ValidFrom.Should().BeNull();
        enrolment.DelegatedPersonEnrolment.Should().BeNull();

        enrolment.Id.Should().Be(0);
        enrolment.ConnectionId.Should().Be(0);
        enrolment.ValidFrom.Should().BeNull();
        enrolment.ValidTo.Should().BeNull();
        enrolment.ServiceRole.Should().BeNull();
        enrolment.EnrolmentStatus.Should().BeNull();
        enrolment.DelegatedPersonEnrolment.Should().BeNull();
        enrolment.IsDeleted.Should().BeFalse();

        enrolment.Connection.Should().NotBeNull();
        enrolment.Connection.JobTitle.Should().Be(account.User.JobTitle);
        enrolment.Connection.OrganisationRoleId.Should().Be(Data.DbConstants.OrganisationRole.Employer);
        enrolment.Connection.PersonId.Should().Be(personEntity.Id);
        enrolment.Connection.PersonRoleId.Should().Be(Data.DbConstants.PersonRole.Admin);

        enrolment.Connection.Organisation.Should().NotBeNull();
        var org = enrolment.Connection.Organisation;
        org.OrganisationTypeId.Should().Be(OrganisationTypeMappings.GetOrganisationTypeId(account.Organisation.OrganisationType));
        org.ProducerTypeId.Should().Be(OrganisationMappings.GetProducerTypeId(account.Organisation.ProducerType));
        org.Name.Should().Be(account.Organisation.Name);
        org.NationId.Should().Be(NationMappings.GetNationId((Core.Models.Nation)account.Organisation.Nation));
        org.ExternalId.Should().NotBeEmpty();

        enrolment.ApprovedPersonEnrolment.Should().BeNull();
    }

    [TestMethod]
    public void GetEnrolmentFromReprocessorExporterAddOrganisation_WhenProducerTypeIsNull_ShouldMapProducerTypeIdAsNull()
    {
        // Arrange
        var personEntity = new Person { Id = 103, FirstName = "Nullable", LastName = "Producer" };
        var account = new ReprocessorExporterAddOrganisation
        {
            User = new ReprocessorExporterUserModel { IsApprovedUser = false },
            Organisation = new ReprocessorExporterOrganisationModel
            {
                OrganisationType = Models.OrganisationType.NonCompaniesHouseCompany,
                ProducerType = null,
                Name = "No Producer Type Org",
                Address = new AddressModel { Town = "Anytown" },
                Nation = Models.Nation.Wales
            },
            DeclarationTimeStamp = DateTime.UtcNow
        };

        // Act
        var enrolment = _reExEnrolmentMaps!.GetAdminEnrolmentForCurrentUser(
            account,
            personEntity);

        // Assert
        enrolment.Should().NotBeNull();
        enrolment.Connection.Should().NotBeNull();
        enrolment.Connection.Organisation.Should().NotBeNull();
        enrolment.Connection.Organisation.ProducerTypeId.Should().BeNull();
    }

    [TestMethod]
    public void GetApprovedPersonEnrolmentForCurrentUser_ShouldMapCorrectly()
    {
        // Arrange
        const int connectionId = 12345;
        DateTime declarationTimeStamp = new DateTime(2072, 12, 24, 1, 2, 3, DateTimeKind.Utc);
        DateTimeOffset expectedDeclarationTimeStamp = new DateTimeOffset(declarationTimeStamp);

        var person = new Person
        {
            Id = 102,
            FirstName = "Approved",
            LastName = "User"
        };

        var account = new ReprocessorExporterAddOrganisation
        {
            User = new ReprocessorExporterUserModel
            {
                UserId = Guid.NewGuid(),
                JobTitle = "Secretary",
                IsApprovedUser = false
            },
            DeclarationTimeStamp = declarationTimeStamp
        };

        var personOrganisationConnection = new PersonOrganisationConnection
        {
            Id = connectionId
        };

        // Act
        var enrolment = _reExEnrolmentMaps!.GetApprovedPersonEnrolmentForCurrentUser(
            account, person, personOrganisationConnection);

        enrolment.Should().NotBeNull();
        enrolment.ServiceRoleId.Should().Be(ServiceRole.ReprocessorExporter.ApprovedPerson.Id);
        enrolment.EnrolmentStatusId.Should().Be(Data.DbConstants.EnrolmentStatus.Enrolled);

        enrolment.Connection.Should().NotBeNull();
        enrolment.Connection.Id.Should().Be(connectionId);

        enrolment.ApprovedPersonEnrolment.Should().NotBeNull();
        enrolment.ApprovedPersonEnrolment.NomineeDeclaration.Should().Be("Approved User");
        enrolment.ApprovedPersonEnrolment.NomineeDeclarationTime.Should().Be(expectedDeclarationTimeStamp);
    }

    [TestMethod]
    public void GetEnrolmentForInvitedApprovedUser_ShouldMapCorrectly()
    {
        // Arrange
        var organisationEntity = new Organisation
        {
            Id = 201, // existing Org ID
            Name = "Existing Inviting Company",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany
        };

        const string inviteeEmail = "invited.nominee@example.com";

        var invitedApprovedUser = new InvitedApprovedUserModel
        {
            Person = new PersonModel
            {
                FirstName = "Invited",
                LastName = "Nominee",
                ContactEmail = inviteeEmail,
                TelephoneNumber = "01234567892"
            },
            JobTitle = "Secretary"
        };
        const string inviterEmail = "current.admin@example.com";

        var personToInvite = new Person
        {
            FirstName = "Invited",
            LastName = "Nominee",
            Email = inviteeEmail,
            Telephone = "01234567892",
            User = new User
            {
                UserId = Guid.Empty,
                Email = inviteeEmail,
                InvitedBy = inviterEmail
            }
        };
        var expectedPersonToInvite = personToInvite;

        // Act
        var enrolment = _reExEnrolmentMaps!.GetEnrolmentForInvitedApprovedUser(
            organisationEntity,
            invitedApprovedUser,
            inviterEmail,
            personToInvite);

        // Assert
        enrolment.Should().NotBeNull();
        enrolment.ServiceRoleId.Should().Be(ServiceRole.ReprocessorExporter.ApprovedPerson.Id);
        enrolment.EnrolmentStatusId.Should().Be(Data.DbConstants.EnrolmentStatus.Invited);

        // assert unmapped Enrolment properties remain default
        enrolment.ValidFrom.Should().BeNull();
        enrolment.DelegatedPersonEnrolment.Should().BeNull();
        enrolment.ApprovedPersonEnrolment.Should().BeNull(); // Explicitly null as per logic

        enrolment.Connection.Should().NotBeNull();
        enrolment.Connection.JobTitle.Should().Be(invitedApprovedUser.JobTitle);
        enrolment.Connection.OrganisationRoleId.Should().Be(Data.DbConstants.OrganisationRole.Employer);
        enrolment.Connection.PersonRoleId.Should().Be(Data.DbConstants.PersonRole.Employee);

        enrolment.Connection.Organisation.Should().BeSameAs(organisationEntity); // uses the passed-in entity instance
        enrolment.Connection.OrganisationId.Should().Be(0);

        enrolment.Connection.Person.Should().NotBeNull();
        var person = enrolment.Connection.Person;
        person.FirstName.Should().Be(expectedPersonToInvite.FirstName);
        person.LastName.Should().Be(expectedPersonToInvite.LastName);
        person.Email.Should().Be(expectedPersonToInvite.Email);
        person.Telephone.Should().Be(expectedPersonToInvite.Telephone);

        person.User.Should().NotBeNull();
        var user = person.User;
        user.UserId.Should().Be(Guid.Empty);
        user.Email.Should().Be(inviteeEmail);
        user.InviteToken.Should().BeNull();
        user.InvitedBy.Should().Be(inviterEmail);
        user.ExternalIdpId.Should().BeNull();
        user.ExternalIdpUserId.Should().BeNull();
    }

    [DataRow(Models.ProducerType.LimitedLiabilityPartnership, Data.DbConstants.PersonRole.Member)]
    [DataRow(Models.ProducerType.LimitedPartnership, Data.DbConstants.PersonRole.Member)]
    [DataRow(Models.ProducerType.Partnership, Data.DbConstants.PersonRole.Employee)]
    [DataRow(Models.ProducerType.NotSet, Data.DbConstants.PersonRole.Employee)]
    [DataRow(null, Data.DbConstants.PersonRole.Employee)]
    [TestMethod]
    public void GetEnrolmentForInvitedApprovedUser_ShouldSetCorrectPersonRoleId_DependingOnProducers(Models.ProducerType? producerType, int personRole)
    {
        // Arrange
        var organisationEntity = new Organisation
        {
            Id = 201, // existing Org ID
            Name = "Existing Inviting Company",
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            ProducerTypeId = OrganisationMappings.GetProducerTypeId(producerType)
        };

        var invitedApprovedUser = new InvitedApprovedUserModel
        {
            Person = new PersonModel
            {
                FirstName = "Invited",
                LastName = "Nominee",
                ContactEmail = "invited.nominee@example.com",
                TelephoneNumber = "01234567892"
            },
            JobTitle = "Secretary"
        };
        var inviterEmail = "current.admin@example.com";

        var personToInvite = new Person();

        // Act
        var enrolment = _reExEnrolmentMaps!.GetEnrolmentForInvitedApprovedUser(
            organisationEntity,
            invitedApprovedUser,
            inviterEmail,
            personToInvite);

        // Assert
        enrolment.Should().NotBeNull();
        enrolment.Connection.Should().NotBeNull();
        enrolment.Connection.PersonRoleId.Should().Be(personRole);
    }
}