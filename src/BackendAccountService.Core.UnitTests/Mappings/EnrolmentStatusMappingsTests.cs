using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;

namespace BackendAccountService.Core.UnitTests.Mappings;

[TestClass]
public class EnrolmentStatusMappingsTests
{
    [TestMethod]
    [DataRow(Data.DbConstants.EnrolmentStatus.Enrolled, EnrolmentStatus.Enrolled)]
    [DataRow(Data.DbConstants.EnrolmentStatus.Approved, EnrolmentStatus.Approved)]
    [DataRow(Data.DbConstants.EnrolmentStatus.Invited, EnrolmentStatus.Invited)]
    [DataRow(Data.DbConstants.EnrolmentStatus.Nominated, EnrolmentStatus.Nominated)]
    [DataRow(Data.DbConstants.EnrolmentStatus.Pending, EnrolmentStatus.Pending)]
    [DataRow(Data.DbConstants.EnrolmentStatus.Rejected, EnrolmentStatus.Rejected)]
    [DataRow(Data.DbConstants.EnrolmentStatus.OnHold, EnrolmentStatus.OnHold)]
    public void EnrolmentStatusMappings_WhenValidEnrolmentStatusIdIsMapped_ThenReturnNation(int enrolmentStatusId, EnrolmentStatus expectedEnrolmentStatus)
    {
        var enrolmentStatus = EnrolmentStatusMappings.MapEnrolmentStatus(enrolmentStatusId);
        enrolmentStatus.Should().Be(expectedEnrolmentStatus);
    }

    [TestMethod]
    public void EnrolmentStatusMappings_WhenInvalidEnrolmentStausIsMapped_ThenThrowException()
    {
        Action getNation = () => EnrolmentStatusMappings.MapEnrolmentStatus(123);

        getNation
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("Not recognised enrolmentStatusId: '123'");
    }
}