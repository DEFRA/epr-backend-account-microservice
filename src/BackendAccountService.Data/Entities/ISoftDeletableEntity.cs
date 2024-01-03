namespace BackendAccountService.Data.Entities;

public interface ISoftDeletableEntity
{
    public bool IsDeleted { get; set; }
}
