using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Data.Entities;

[ExcludeFromCodeCoverage]
public class LaOrganisation : IEditableEntity, ISoftDeletableEntity
{
    public int Id { get; set; }
    public int OrganisationId { get; set; }
    [MaxLength(10)] public string DistrictCode { get; set; }
    public DateTimeOffset CreatedOn { get; private set; }
    public DateTimeOffset LastUpdatedOn { get; private set; }
    public bool IsDeleted { get; set; }
    public Organisation Organisation { get; set; }
}