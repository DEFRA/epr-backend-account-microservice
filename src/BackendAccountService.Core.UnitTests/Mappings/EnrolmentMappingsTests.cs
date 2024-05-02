using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;
using FluentAssertions.Extensions;

namespace BackendAccountService.Core.UnitTests.Mappings;

[TestClass]
public class EnrolmentMappingsTests
{
    [TestMethod]
    public void EnrolmentMappings_GetEnrolmentFromApprovedUserAccountModel_ReturnsCorrectly()
    {
        // arrange
        var street = "some street";
        var declarationName = "Firstname Lastname";
        var declarationTime = DateTime.Now;
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
            },
            DeclarationFullName = declarationName,
            DeclarationTimeStamp = declarationTime,
        };
        var serviceRoleId = 2;
        var user = new UserModel();
        
        // act
        var enrolment = EnrolmentMappings.GetEnrolmentFromApprovedUserAccountModel(account, serviceRoleId, user);
        
        // assert
        enrolment.ServiceRoleId.Should().Be(serviceRoleId);
        enrolment.Connection.Organisation.Street.Should().Be(street);
        enrolment.ApprovedPersonEnrolment.NomineeDeclaration.Should().Be(declarationName);
        enrolment.ApprovedPersonEnrolment.NomineeDeclarationTime.Should().Be(declarationTime);
    }
}