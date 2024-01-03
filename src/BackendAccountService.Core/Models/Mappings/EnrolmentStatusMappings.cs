namespace BackendAccountService.Core.Models.Mappings;

public static class EnrolmentStatusMappings
{
    public static EnrolmentStatus MapEnrolmentStatus(int enrolmentStatusId)
    {
        return enrolmentStatusId switch
        {
            Data.DbConstants.EnrolmentStatus.Enrolled => Models.EnrolmentStatus.Enrolled,
            Data.DbConstants.EnrolmentStatus.Pending => Models.EnrolmentStatus.Pending,
            Data.DbConstants.EnrolmentStatus.Approved => Models.EnrolmentStatus.Approved,
            Data.DbConstants.EnrolmentStatus.Rejected => Models.EnrolmentStatus.Rejected,
            Data.DbConstants.EnrolmentStatus.Invited => Models.EnrolmentStatus.Invited,
            Data.DbConstants.EnrolmentStatus.OnHold => Models.EnrolmentStatus.OnHold,
            Data.DbConstants.EnrolmentStatus.Nominated => Models.EnrolmentStatus.Nominated,
            _ => throw new ArgumentException($"Not recognised enrolmentStatusId: '{enrolmentStatusId}'")
        };
    }
}