using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;

namespace BackendAccountService.Core.UnitTests.Mappings;

[TestClass]
public class PersonRoleMappingsTests
{
    [TestMethod]
    [DataRow(Data.DbConstants.PersonRole.Admin, PersonRole.Admin)]
    [DataRow(Data.DbConstants.PersonRole.Employee, PersonRole.Employee)]
    public void PersonRoleMappings_WhenValidPersonRoleIdIsMapped_ThenReturnPersonRole(int personRoleId, PersonRole expectedPersonRole)
    {
        var personRole = PersonRoleMappings.MapPersonRole(personRoleId);
        personRole.Should().Be(expectedPersonRole);
    }

    [TestMethod]
    public void PersonRoleMappings_WhenInvalidPersonRoleIdIsMapped_ThenThrowException()
    {
        Action getNation = () => PersonRoleMappings.MapPersonRole(123);

        getNation
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("Not recognised personRoleId: '123'");
    }

    [TestMethod]
    [DataRow(Data.DbConstants.PersonRole.Admin, PersonRole.Admin)]
    [DataRow(Data.DbConstants.PersonRole.Employee, PersonRole.Employee)]
    public void PersonRoleMappings_WhenValidPersonRoleIsMapped_ThenReturnId(int expectedPersonRoleId, PersonRole personRole)
    {
        var personRoleId = PersonRoleMappings.GetPersonRoleId(personRole);
        personRoleId.Should().Be(expectedPersonRoleId);
    }

    [TestMethod]
    public void PersonRoleMappings_WhenInvalidPersonRoleIsMapped_ThenThrowException()
    {
        var personRoleMock = 12345;

        Action getNation = () => PersonRoleMappings.GetPersonRoleId((PersonRole)personRoleMock);

        getNation
            .Should()
            .Throw<ArgumentException>()
            .WithMessage($"Unrecognised Person Role: '{personRoleMock}'");
    }

}