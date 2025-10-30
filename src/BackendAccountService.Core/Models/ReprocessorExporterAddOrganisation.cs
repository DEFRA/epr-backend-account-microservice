using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models;

[ExcludeFromCodeCoverage]
public class ReprocessorExporterAddOrganisation
{
    [Required]
    public ReprocessorExporterUserModel User { get; set; } = null!;

    [Required]
    public ReprocessorExporterOrganisationModel Organisation { get; set; } = null!;

    public List<PartnerModel> Partners { get; set; } = new();

    public List<InvitedApprovedUserModel> InvitedApprovedUsers { get; set; } = new();

    // if current user isn't asking to be an approved user, this is not required
    public DateTime? DeclarationTimeStamp { get; set; }
}
