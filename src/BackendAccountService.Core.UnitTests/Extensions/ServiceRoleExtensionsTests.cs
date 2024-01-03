using BackendAccountService.Core.Extensions;
using BackendAccountService.Data.DbConstants;
using static BackendAccountService.Data.DbConstants.ServiceRole;

namespace BackendAccountService.Core.UnitTests.Extensions;

[TestClass]
public class ServiceRoleExtensionsTests
{
    [TestMethod]
    [DataRow(new[] { Packaging.BasicUser.Key, Packaging.DelegatedPerson.Key, Packaging.ApprovedPerson.Key }, Packaging.ApprovedPerson.Key)]
    [DataRow(new[] { Packaging.BasicUser.Key, Packaging.DelegatedPerson.Key }, Packaging.DelegatedPerson.Key)]
    [DataRow(new[] { Packaging.BasicUser.Key }, Packaging.BasicUser.Key)]
    [DataRow(new[] { Packaging.ApprovedPerson.Key, Regulator.Admin.Key}, Packaging.ApprovedPerson.Key)]
    public void GetHighestServiceRole_Returns_Highest_Packing_Role(string[] serviceRoles, string expectedHighestRole)
    {
        // Act
        var result = ServiceRoleExtensions.GetHighestServiceRole(serviceRoles, Service.EprPackaging);
        
        // Assert
        result.Should().Be(expectedHighestRole);
    }
    
    [TestMethod]
    [DataRow(new[] { Packaging.ApprovedPerson.Key, Regulator.Admin.Key}, Regulator.Admin.Key)]
    [DataRow(new[] { Regulator.Basic.Key, Regulator.Admin.Key}, Regulator.Admin.Key)]
    [DataRow(new[] { Regulator.Basic.Key, Packaging.ApprovedPerson.Key}, Regulator.Basic.Key)]
    public void GetHighestServiceRole_Returns_Highest_Regulator_Role(string[] serviceRoles, string expectedHighestRole)
    {
        // Act
        var result = ServiceRoleExtensions.GetHighestServiceRole(serviceRoles, Service.RegulatorEnrolement);
        
        // Assert
        result.Should().Be(expectedHighestRole);
    }
    
    [TestMethod]
    [DataRow(new[] { Packaging.ApprovedPerson.Key }, Packaging.DelegatedPerson.Key)]
    [DataRow(new[] { Packaging.ApprovedPerson.Key, Packaging.DelegatedPerson.Key, Packaging.BasicUser.Key }, Packaging.BasicUser.Key)]
    [DataRow(new string[] {}, Packaging.ApprovedPerson.Key)]
    public void GetAuthorizedRolesToRemoveUser_Returns_Highest_Role_for_packing(string[] expectedAuthorizedRoles, string serviceRoleToRemove)
    {
        // Act
        var result = ServiceRoleExtensions.GetAuthorizedRolesToRemoveUser(serviceRoleToRemove, Service.EprPackaging);
    
        // Assert
        result.Should().BeEquivalentTo(expectedAuthorizedRoles);
    }
    
    [TestMethod]
    [DataRow(Regulator.Admin.Key)]
    [DataRow(Regulator.Basic.Key)]
    public void GetAuthorizedRolesToRemoveUser_regulators_always_returns_admin(string serviceRoleToRemove)
    {
        // Act
        var result = ServiceRoleExtensions.GetAuthorizedRolesToRemoveUser(serviceRoleToRemove, Service.RegulatorEnrolement);
    
        // Assert
        result.Should().BeEquivalentTo(Regulator.Admin.Key);
    }
}