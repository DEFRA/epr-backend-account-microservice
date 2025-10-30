using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models;

[ExcludeFromCodeCoverage]
public class ReprocessorExporterUserModel
{
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// See https://developer-specs.company-information.service.gov.uk/companies-house-public-data-api/resources/officersummary?v=latest
    /// </summary>
    public string? JobTitle { get; set; }

    [Required]
    public bool IsApprovedUser { get; set; }
}