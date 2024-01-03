using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;

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
}