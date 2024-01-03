namespace BackendAccountService.Data.Entities;

public class PersonsConnection : DataEntity
{
    public int Id { get; set; }

    public int FromPersonId { get; set; }

    public int FromPersonRoleId { get; set; }

    public int ToPersonId { get; set; }

    public int ToPersonRoleId { get; set; }

    public Person FromPerson { get; set; }

    public InterPersonRole FromPersonRole { get; set; }

    public Person ToPerson { get; set; }

    public InterPersonRole ToPersonRole { get; set; }
}
