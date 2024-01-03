namespace BackendAccountService.Core.Models.Responses
{
    public class LocalAuthorityResponseModel
    {
        public DateTimeOffset ModifiedOn { get; set; } = DateTimeOffset.Now;
        public string DistrictCode { get; set; } = string.Empty;
        public OrganisationType OrganisationType { get; set; }
        public string? CompaniesHouseNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? TradingName { get; set; }
        public string? ReferenceNumber { get; set; }
        public AddressModel? Address { get; set; }
        public bool ValidatedWithCompaniesHouse { get; set; }
        public bool IsComplianceScheme { get; set; }
        public Nation? Nation { get; set; }
        public Guid ExternalId { get; set; }
    }
}