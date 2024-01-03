using BackendAccountService.Core.Models;
using BackendAccountService.Core.Services;
using DbConstants = BackendAccountService.Data.DbConstants;

namespace BackendAccountService.Api.UnitTests.Controllers.ConnectionControllerTests
{
    [TestClass]
    public class MapEnrolmentStatusTests
    {
        [TestMethod]
        [DataRow(DbConstants.EnrolmentStatus.Enrolled, EnrolmentStatus.Enrolled)]
        [DataRow(DbConstants.EnrolmentStatus.Pending, EnrolmentStatus.Pending)]
        [DataRow(DbConstants.EnrolmentStatus.Approved, EnrolmentStatus.Approved)]
        [DataRow(DbConstants.EnrolmentStatus.Rejected, EnrolmentStatus.Rejected)]
        [DataRow(DbConstants.EnrolmentStatus.Invited, EnrolmentStatus.Invited)]
        [DataRow(DbConstants.EnrolmentStatus.Nominated, EnrolmentStatus.Nominated)]
        public void WhenValidIntegersAreProvided_ThenTheyAreMappedToCorrespondingStrings(int enrolmentStatusId, EnrolmentStatus expectedEnrolmentStatus)
        {
            RoleManagementService.MapEnrolmentStatus(enrolmentStatusId).Should().Be(expectedEnrolmentStatus);
        }

        [TestMethod]
        public void WhenEnrolmentStatusNotSetIsProvided_ThenArgumentExceptionIsThrown()
        {
            Action enrolmentStatusMap = () => RoleManagementService.MapEnrolmentStatus(DbConstants.EnrolmentStatus.NotSet);

            enrolmentStatusMap.Should()
                .Throw<ArgumentException>()
                .WithMessage("No longer supported value NotSet.");
        }

        [TestMethod]
        public void WhenInvalidIntegerIsProvided_ThenArgumentExceptionIsThrown()
        {
            Action enrolmentStatusMap = () => RoleManagementService.MapEnrolmentStatus(1234);

            enrolmentStatusMap.Should()
                .Throw<ArgumentException>()
                .WithMessage("Not recognised enrolment status ID.");
        }
    }
}
