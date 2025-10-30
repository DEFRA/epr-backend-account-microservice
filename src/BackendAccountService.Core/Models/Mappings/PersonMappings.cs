using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.Models.Mappings;

public static class PersonMappings
{

    public static PersonResponseModel? GetPersonModelFromPerson(Person? person)
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
            ContactEmail = person.Email,
            IsDeleted = person.IsDeleted
        };
        if (person.User != null)
        {
            personModel.UserId = person.User.UserId;
        }
        return personModel;
    }

    public static Person GetPersonModel(ReprocessorExporterAccount account)
    {
        return new Person
        {
            FirstName = account.Person.FirstName,
            LastName = account.Person.LastName,
            Email = account.Person.ContactEmail,
            Telephone = account.Person.TelephoneNumber,
            User = new User
            {
                UserId = account.User.UserId,
                ExternalIdpId = account.User.ExternalIdpId,
                ExternalIdpUserId = account.User.ExternalIdpUserId,
                Email = account.User.Email
            }
        };
    }
}
