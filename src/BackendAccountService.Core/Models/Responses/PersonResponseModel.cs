namespace BackendAccountService.Core.Models.Responses;

public class PersonResponseModel : PersonModel
{
    public DateTimeOffset CreatedOn { get; set; }

    public Guid? UserId { get; set; }
}
