using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Data.Entities;
using ProducerType = BackendAccountService.Core.Models.ProducerType;

namespace BackendAccountService.Core.UnitTests.Mappings;

[TestClass]
public class OrganisationMappingsTests
{
    [TestMethod]
    public void OrganisationMappings_WhenProducerTypeIsNull_ThenReturnNull()
    {
        var producerType = OrganisationMappings.GetProducerTypeId(null);
        producerType.Should().BeNull();
    }

    [TestMethod]
    public void OrganisationMappings_WhenAnyProducerTypeIsMapped_ThenReturnIntValue()
    {
        foreach (var producerType in Enum.GetValues<ProducerType>())
        {
            int? producerTypeId = OrganisationMappings.GetProducerTypeId(producerType);
            producerTypeId.Should().NotBeNull();
        }
    }
    
    [TestMethod]
    [DataRow(ProducerType.NotSet, Data.DbConstants.ProducerType.NotSet)]
    [DataRow(ProducerType.Partnership, Data.DbConstants.ProducerType.Partnership)]
    [DataRow(ProducerType.UnincorporatedBody, Data.DbConstants.ProducerType.UnincorporatedBody)]
    [DataRow(ProducerType.NonUkOrganisation, Data.DbConstants.ProducerType.NonUkOrganisation)]
    [DataRow(ProducerType.SoleTrader, Data.DbConstants.ProducerType.SoleTrader)]
    [DataRow(ProducerType.Other, Data.DbConstants.ProducerType.Other)]
    [DataRow(ProducerType.LimitedPartnership, Data.DbConstants.ProducerType.LimitedPartnership)]
    [DataRow(ProducerType.LimitedLiabilityPartnership, Data.DbConstants.ProducerType.LimitedLiabilityPartnership)]
    public void OrganisationMappings_WhenRecognisedProducerTypeIsMapped_ThenReturnExpectedValue(ProducerType producerType, int expectedProducerTypeId)
    {
        var producerTypeId = OrganisationMappings.GetProducerTypeId(producerType);
        producerTypeId.Should().Be(expectedProducerTypeId);
    }
    
    [TestMethod]
    public void OrganisationMappings_WhenProducerTypeIdIsNull_ThenReturnNull()
    {
        var producerType = OrganisationMappings.GetProducerType(null);
        producerType.Should().BeNull();
    }

    [TestMethod]
    [DataRow(Data.DbConstants.ProducerType.NotSet, ProducerType.NotSet)]
    [DataRow(Data.DbConstants.ProducerType.Partnership, ProducerType.Partnership)]
    [DataRow(Data.DbConstants.ProducerType.UnincorporatedBody, ProducerType.UnincorporatedBody)]
    [DataRow(Data.DbConstants.ProducerType.NonUkOrganisation, ProducerType.NonUkOrganisation)]
    [DataRow(Data.DbConstants.ProducerType.SoleTrader, ProducerType.SoleTrader)]
    [DataRow(Data.DbConstants.ProducerType.Other, ProducerType.Other)]
    [DataRow(Data.DbConstants.ProducerType.LimitedLiabilityPartnership, ProducerType.LimitedLiabilityPartnership)]
    [DataRow(Data.DbConstants.ProducerType.LimitedPartnership, ProducerType.LimitedPartnership)]
    public void OrganisationMappings_WhenRecognisedProducerTypeIdIsMapped_ThenReturnExpectedValue(int producerTypeId, ProducerType expectedProducerType)
    {
        var producerType = OrganisationMappings.GetProducerType(producerTypeId);
        producerType.Should().Be(expectedProducerType);
    }
    
    [TestMethod]
    public void OrganisationMappings_WhenInvalidProducerTypeIdIsMapped_ThenThrowException()
    {
        Action getNation = () => OrganisationMappings.GetProducerType(12345);

        getNation
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("Unrecognised organisation type ID: '12345'");
    }

    [TestMethod]
    public void GetUpdatedProducers_WhenOrganisationListIsEmpty_ThenReturnEmptyList()
    {
        // Arrange
        var organisations = new List<Organisation>();

        // Act
        var result = OrganisationMappings.GetUpdatedProducers(organisations);

        // Assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetUpdatedProducers_WhenOrganisationListHasOneOrganisation_ThenReturnMappedList()
    {
        // Arrange
        var organisation = new Organisation
        {
            Name = "Org1",
            IsDeleted = false,
            ReferenceNumber = "Ref123",
            BuildingName = "Building1",
            BuildingNumber = "1A",
            CompaniesHouseNumber = "CH12345",
            Country = "Country1",
            County = "County1",
            DependentLocality = "Dependent Locality",
            IsComplianceScheme = true,
            Locality = "Locality1",
            Id = 1,
            Postcode = "PST123",
            ProducerTypeId = 1,
            Street = "Street1",
            SubBuildingName = "SubBuilding1",
            Town = "Town1",
            TradingName = "TradingName1",
            ValidatedWithCompaniesHouse = true
        };

        var organisations = new List<Organisation> { organisation };

        // Act
        var result = OrganisationMappings.GetUpdatedProducers(organisations);

        // Assert
        result.Should().HaveCount(1);
        var firstResult = result.First();
        firstResult.ProducerName.Should().Be(organisation.Name);
        firstResult.IsDeleted.Should().Be(organisation.IsDeleted);
        firstResult.ReferenceNumber.Should().Be(organisation.ReferenceNumber);
        firstResult.BuildingName.Should().Be(organisation.BuildingName);
        firstResult.BuildingNumber.Should().Be(organisation.BuildingNumber);
        firstResult.CompaniesHouseNumber.Should().Be(organisation.CompaniesHouseNumber);
        firstResult.Country.Should().Be(organisation.Country);
        firstResult.County.Should().Be(organisation.County);
        firstResult.DependentLocality.Should().Be(organisation.DependentLocality);
        firstResult.IsComplianceScheme.Should().Be(organisation.IsComplianceScheme);
        firstResult.Locality.Should().Be(organisation.Locality);
        firstResult.OrganisationId.Should().Be(organisation.Id);
        firstResult.Postcode.Should().Be(organisation.Postcode);
        firstResult.ProducerTypeId.Should().Be(organisation.ProducerTypeId);
        firstResult.Street.Should().Be(organisation.Street);
        firstResult.SubBuildingName.Should().Be(organisation.SubBuildingName);
        firstResult.Town.Should().Be(organisation.Town);
        firstResult.TradingName.Should().Be(organisation.TradingName);
        firstResult.ValidatedWithCompaniesHouse.Should().Be(organisation.ValidatedWithCompaniesHouse);
        firstResult.ExternalId.Should().Be(organisation.ExternalId.ToString());
    }

    [TestMethod]
    public void GetUpdatedProducers_WhenOrganisationListHasMultipleOrganisations_ThenReturnMappedList()
    {
        // Arrange
        var organisation1 = new Organisation
        {
            Name = "Org1",
            IsDeleted = false,
            ReferenceNumber = "Ref123",
            BuildingName = "Building1",
            BuildingNumber = "1A",
            CompaniesHouseNumber = "CH12345",
            Country = "Country1",
            County = "County1",
            DependentLocality = "Dependent Locality",
            IsComplianceScheme = true,
            Locality = "Locality1",
            Id = 1,
            Postcode = "PST123",
            ProducerTypeId = 1,
            Street = "Street1",
            SubBuildingName = "SubBuilding1",
            Town = "Town1",
            TradingName = "TradingName1",
            ValidatedWithCompaniesHouse = true,
            ExternalId = Guid.NewGuid()
        };

        var organisation2 = new Organisation
        {
            Name = "Org2",
            IsDeleted = true,
            ReferenceNumber = "Ref456",
            BuildingName = "Building2",
            BuildingNumber = "2B",
            CompaniesHouseNumber = "CH67890",
            Country = "Country2",
            County = "County2",
            DependentLocality = "Dependent Locality 2",
            IsComplianceScheme = false,
            Locality = "Locality2",
            Id = 2,
            Postcode = "PST456",
            ProducerTypeId = 2,
            Street = "Street2",
            SubBuildingName = "SubBuilding2",
            Town = "Town2",
            TradingName = "TradingName2",
            ValidatedWithCompaniesHouse = false,
            ExternalId = Guid.NewGuid()
        };

        var organisations = new List<Organisation> { organisation1, organisation2 };

        // Act
        var result = OrganisationMappings.GetUpdatedProducers(organisations);

        // Assert
        result.Should().HaveCount(2);

        // Check organisation 1
        var firstResult = result[0];
        firstResult.ProducerName.Should().Be(organisation1.Name);
        firstResult.IsDeleted.Should().Be(organisation1.IsDeleted);
        firstResult.ReferenceNumber.Should().Be(organisation1.ReferenceNumber);
        firstResult.OrganisationId.Should().Be(organisation1.Id);
        firstResult.ExternalId.Should().Be(organisation1.ExternalId.ToString());

        // Check organisation 2
        var secondResult = result[1];
        secondResult.ProducerName.Should().Be(organisation2.Name);
        secondResult.IsDeleted.Should().Be(organisation2.IsDeleted);
        secondResult.ReferenceNumber.Should().Be(organisation2.ReferenceNumber);
        secondResult.OrganisationId.Should().Be(organisation2.Id);
        secondResult.ExternalId.Should().Be(organisation2.ExternalId.ToString());
    }
}