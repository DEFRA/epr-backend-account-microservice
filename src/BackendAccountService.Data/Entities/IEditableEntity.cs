namespace BackendAccountService.Data.Entities;

public interface IEditableEntity
{
    public DateTimeOffset CreatedOn { get; }

    public DateTimeOffset LastUpdatedOn { get; }
}
