using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.UnitTests.Mappings;

[TestClass]
public class LaOrganisationMappingsTests
{
    [TestMethod]
    public void LaOrganisationMappings_WhenOrganisationIsMapped_ThenAllFieldsArePopulated()
    {
        var organisation = new Organisation()
        {
            ExternalId = Guid.NewGuid(),
            OrganisationTypeId = Data.DbConstants.OrganisationType.CompaniesHouseCompany,
            CompaniesHouseNumber = "12345678",
            Name = "Organisation Name",
            TradingName = "Trading Name",
            ReferenceNumber = "112233",
            ValidatedWithCompaniesHouse = true,
            IsComplianceScheme = true,
            SubBuildingName = "Sub-building Name",
            BuildingName = "Building Name",
            BuildingNumber = "1234",
            Street = "Street",
            Locality = "Locality",
            DependentLocality = "Dependent Locality",
            Town = "Town",
            County = "County",
            Postcode = "Postcode",
            Country = "Country",
            NationId = Data.DbConstants.Nation.England
        };

        var laOrganisation = new LaOrganisation()
        {
            DistrictCode = "District Code"
        };

        var localAuthorityResponseModel = LaOrganisationMappings.GetLaOrganisationModelFromOrganisation(organisation, laOrganisation);

        localAuthorityResponseModel.DistrictCode.Should().Be(laOrganisation.DistrictCode);
        
        localAuthorityResponseModel.ExternalId.Should().Be(organisation.ExternalId);
        localAuthorityResponseModel.ModifiedOn.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
        localAuthorityResponseModel.OrganisationType.Should().Be(OrganisationTypeMappings.GetOrganisationType(organisation.OrganisationTypeId));
        localAuthorityResponseModel.CompaniesHouseNumber.Should().Be(organisation.CompaniesHouseNumber);
        localAuthorityResponseModel.Name.Should().Be(organisation.Name);
        localAuthorityResponseModel.TradingName.Should().Be(organisation.TradingName);
        localAuthorityResponseModel.ReferenceNumber.Should().Be(organisation.ReferenceNumber);
        localAuthorityResponseModel.ValidatedWithCompaniesHouse.Should().Be(organisation.ValidatedWithCompaniesHouse);
        localAuthorityResponseModel.IsComplianceScheme.Should().Be(organisation.IsComplianceScheme);
        localAuthorityResponseModel.Nation.Should().Be(NationMappings.GetNation(organisation.NationId.Value));
        localAuthorityResponseModel.Address.SubBuildingName.Should().Be(organisation.SubBuildingName);
        localAuthorityResponseModel.Address.BuildingName.Should().Be(organisation.BuildingName);
        localAuthorityResponseModel.Address.BuildingNumber.Should().Be(organisation.BuildingNumber);
        localAuthorityResponseModel.Address.Street.Should().Be(organisation.Street);
        localAuthorityResponseModel.Address.Locality.Should().Be(organisation.Locality);
        localAuthorityResponseModel.Address.DependentLocality.Should().Be(organisation.DependentLocality);
        localAuthorityResponseModel.Address.Town.Should().Be(organisation.Town);
        localAuthorityResponseModel.Address.County.Should().Be(organisation.County);
        localAuthorityResponseModel.Address.Postcode.Should().Be(organisation.Postcode);
        localAuthorityResponseModel.Address.Country.Should().Be(organisation.Country);
    }
}