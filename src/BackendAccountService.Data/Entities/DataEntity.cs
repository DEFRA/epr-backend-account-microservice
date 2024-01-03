namespace BackendAccountService.Data.Entities;

// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local
public abstract class DataEntity : IEditableEntity, ISoftDeletableEntity
{
    public Guid ExternalId { get; set; }

    public DateTimeOffset CreatedOn { get; private set; }

    public DateTimeOffset LastUpdatedOn { get; private set; }

    public bool IsDeleted { get; set; }
}
