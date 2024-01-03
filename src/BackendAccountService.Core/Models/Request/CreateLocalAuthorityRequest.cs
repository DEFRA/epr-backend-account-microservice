using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models.Request
{
    public class CreateLocalAuthorityRequest
    {
		//Common Fields
		[Required] public Guid UserId { get; set; }
        //Local Authority Fields
        [Required][MaxLength(10)] public string DistrictCode { get; set; } = string.Empty;
        //Organisation Fields
        [Required][MaxLength(100)] public string WasteAuthorityType { get; set; } = "Not Set";
        [MaxLength(30)] public string CompaniesHouseNumber { get; set; } = string.Empty;
        [Required][MaxLength(160)] public string Name { get; set; } = string.Empty;
        [MaxLength(170)] public string TradingName { get; set; } = string.Empty;
        [MaxLength(10)] public string ReferenceNumber { get; set; } = string.Empty;
        [MaxLength(100)] public string BuildingName { get; set; } = string.Empty;
        [MaxLength(50)] public string BuildingNumber { get; set; } = string.Empty;
        [MaxLength(100)] public string Street { get; set; } = string.Empty;
        [MaxLength(100)] public string Locality { get; set; } = string.Empty;
        [MaxLength(100)] public string DependentLocality { get; set; } = string.Empty;
        [MaxLength(70)] public string Town { get; set; } = string.Empty;
        [MaxLength(50)] public string County { get; set; } = string.Empty;
        [MaxLength(54)] public string Country { get; set; } = string.Empty;
        [MaxLength(15)] public string Postcode { get; set; } = string.Empty;
        [Required] public bool ValidatedWithCompaniesHouse { get; set; }
        [Required] public bool IsComplianceScheme { get; set; }
        [MaxLength(54)] public string Nation { get; set; } = "Not Set";
    }
}