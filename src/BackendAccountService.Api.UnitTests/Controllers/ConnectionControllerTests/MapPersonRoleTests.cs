using BackendAccountService.Core.Models;
using BackendAccountService.Core.Services;
using DbConstants = BackendAccountService.Data.DbConstants;

namespace BackendAccountService.Api.UnitTests.Controllers.ConnectionControllerTests
{
    [TestClass]
    public class MapPersonRoleTests
    {
        [TestMethod]
        [DataRow(DbConstants.PersonRole.Admin, PersonRole.Admin)]
        [DataRow(DbConstants.PersonRole.Employee, PersonRole.Employee)]
        public void WhenValidIntegersAreProvided_ThenTheyAreMappedToCorrespondingStrings(int enrolmentStatusId, PersonRole expectedPersonRole)
        {
            RoleManagementService.MapPersonRole(enrolmentStatusId).Should().Be(expectedPersonRole);
        }

        [TestMethod]
        public void WhenPersonRoleNotSetIsProvided_ThenArgumentExceptionIsThrown()
        {
            Action enrolmentStatusMap = () => RoleManagementService.MapEnrolmentStatus(DbConstants.EnrolmentStatus.NotSet);

            enrolmentStatusMap.Should()
                .Throw<ArgumentException>()
                .WithMessage("No longer supported value NotSet.");
        }

        [TestMethod]
        public void WhenInvalidIntegerIsProvided_ThenArgumentExceptionIsThrown()
        {
            Action personRoleMap = () => RoleManagementService.MapPersonRole(1234);

            personRoleMap.Should()
                .Throw<ArgumentException>()
                .WithMessage("Not recognised person role ID.");
        }
    }
}
