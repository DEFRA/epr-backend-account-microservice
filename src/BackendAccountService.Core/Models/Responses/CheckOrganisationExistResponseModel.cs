namespace BackendAccountService.Core.Models.Responses
{
    public class CheckOrganisationExistResponseModel
    {
        public DateTimeOffset CreatedOn { get; set; }

        public Guid ExternalId { get; set; }

        public string OrganisationName { get; set; }
    }
}
