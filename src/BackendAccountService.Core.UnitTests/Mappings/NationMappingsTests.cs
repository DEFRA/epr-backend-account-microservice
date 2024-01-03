using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;

namespace BackendAccountService.Core.UnitTests.Mappings;

[TestClass]
public class NationMappingsTests
{
    [TestMethod]
    [DataRow(Nation.NotSet, Data.DbConstants.Nation.NotSet)]
    [DataRow(Nation.England, Data.DbConstants.Nation.England)]
    [DataRow(Nation.NorthernIreland, Data.DbConstants.Nation.NorthernIreland)]
    [DataRow(Nation.Scotland, Data.DbConstants.Nation.Scotland)]
    [DataRow(Nation.Wales, Data.DbConstants.Nation.Wales)]
    public void NationMappings_WhenKnownNationIsMapped_ThenReturnIntValue(Nation nation, int expectedNationId)
    { 
        int nationId = NationMappings.GetNationId(nation);
        nationId.Should().Be(expectedNationId);
    }

    [TestMethod]
    public void NationMappings_WhenAnyNationIsMapped_ThenReturnIntValue()
    {
        try
        {
            foreach (var nation in Enum.GetValues<Nation>())
            {
                NationMappings.GetNationId(nation);
            }
        }
        catch
        {
            Assert.Fail("An unsupported mapping exists");
        }
    }

    [TestMethod]
    [DataRow(Data.DbConstants.Nation.NotSet, Nation.NotSet)]
    [DataRow(Data.DbConstants.Nation.England, Nation.England)]
    [DataRow(Data.DbConstants.Nation.NorthernIreland, Nation.NorthernIreland)]
    [DataRow(Data.DbConstants.Nation.Scotland, Nation.Scotland)]
    [DataRow(Data.DbConstants.Nation.Wales, Nation.Wales)]
    public void NationMappings_WhenValidNationIdIsMapped_ThenReturnNation(int nationId, Nation expectedNation)
    {
        var nation = NationMappings.GetNation(nationId);
        nation.Should().Be(expectedNation);
    }

    [TestMethod]
    public void NationMappings_WhenInvalidNationIsMapped_ThenThrowException()
    {
        Action getNation = () => NationMappings.GetNation(12345);

        getNation
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("Unrecognised nation ID: '12345'");
    }
}