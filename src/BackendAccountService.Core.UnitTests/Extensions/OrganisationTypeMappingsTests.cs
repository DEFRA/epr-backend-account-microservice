using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;

namespace BackendAccountService.Core.UnitTests.Extensions;

[TestClass]
public class OrganisationTypeMappingsTests
{
    [TestMethod]
    [DataRow(Data.DbConstants.OrganisationType.CompaniesHouseCompany, OrganisationType.CompaniesHouseCompany)]
    [DataRow(Data.DbConstants.OrganisationType.WasteCollectionAuthority, OrganisationType.WasteCollectionAuthority)]
    [DataRow(Data.DbConstants.OrganisationType.NonCompaniesHouseCompany, OrganisationType.NonCompaniesHouseCompany)]
    [DataRow(Data.DbConstants.OrganisationType.WasteDisposalAuthority, OrganisationType.WasteDisposalAuthority)]
    [DataRow(Data.DbConstants.OrganisationType.NotSet, OrganisationType.NotSet)]
    [DataRow(Data.DbConstants.OrganisationType.Regulators, OrganisationType.Regulators)]
    [DataRow(Data.DbConstants.OrganisationType.WasteCollectionAuthorityWasteDisposalAuthority, OrganisationType.WasteCollectionAuthorityWasteDisposalAuthority)]
    public void Organisation_type_maps_correctly_from_dbType_to_OrganisationType(int dbType, OrganisationType expectedType)
    {
        // Act
        var result = OrganisationTypeMappings.GetOrganisationType(dbType);
        
        // Assert
        result.Should().Be(expectedType);
    }
    
    [TestMethod]
    [DataRow(Data.DbConstants.OrganisationType.CompaniesHouseCompany, OrganisationType.CompaniesHouseCompany)]
    [DataRow(Data.DbConstants.OrganisationType.WasteCollectionAuthority, OrganisationType.WasteCollectionAuthority)]
    [DataRow(Data.DbConstants.OrganisationType.NonCompaniesHouseCompany, OrganisationType.NonCompaniesHouseCompany)]
    [DataRow(Data.DbConstants.OrganisationType.WasteDisposalAuthority, OrganisationType.WasteDisposalAuthority)]
    [DataRow(Data.DbConstants.OrganisationType.NotSet, OrganisationType.NotSet)]
    [DataRow(Data.DbConstants.OrganisationType.Regulators, OrganisationType.Regulators)]
    [DataRow(Data.DbConstants.OrganisationType.WasteCollectionAuthorityWasteDisposalAuthority, OrganisationType.WasteCollectionAuthorityWasteDisposalAuthority)]
    public void Organisation_type_maps_correctly_from_OrganisationType_to_db_type(int expectedDbType, OrganisationType organisationType)
    {
        // Act
        var result = OrganisationTypeMappings.GetOrganisationTypeId(organisationType);
        
        // Assert
        result.Should().Be(expectedDbType);
    }
}