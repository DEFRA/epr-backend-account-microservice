using BackendAccountService.Core.Models.Responses;

namespace BackendAccountService.Core.Models.Mappings;

public static class PersonMappings
{

    public static PersonResponseModel? GetPersonModelFromPerson(Data.Entities.Person? person)
    {
        if(person == null)
        {
            return null;
        }

        var personModel = new PersonResponseModel
        {
            CreatedOn = person.CreatedOn,
            TelephoneNumber = person.Telephone,
            FirstName = person.FirstName,
            LastName = person.LastName,
            ContactEmail = person.Email
        };
        if (person.User != null)
        {
            personModel.UserId = person.User.UserId;
        }
        return personModel;
    }
}
