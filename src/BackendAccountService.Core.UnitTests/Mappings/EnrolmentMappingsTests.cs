using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;

namespace BackendAccountService.Core.UnitTests.Mappings;

[TestClass]
public class EnrolmentMappingsTests
{
    [TestMethod]
    public void EnrolmentMappings_GetEnrolmentFromApprovedUserAccountModel_ReturnsCorrectly()
    {
        // arrange
        var street = "some street";
        var account = new ApprovedUserAccountModel
        {
            Connection = new ConnectionModel(),
            Person = new PersonModel(),
            Organisation = new OrganisationModel
            {
                Address = new AddressModel
                {
                    Street = street
                }
            }
        };
        var serviceRoleId = 2;
        var user = new UserModel();
        
        // act
        var enrolment = EnrolmentMappings.GetEnrolmentFromApprovedUserAccountModel(account, serviceRoleId, user);
        
        // assert
        enrolment.ServiceRoleId.Should().Be(serviceRoleId);
        enrolment.Connection.Organisation.Street.Should().Be(street);
    }
}