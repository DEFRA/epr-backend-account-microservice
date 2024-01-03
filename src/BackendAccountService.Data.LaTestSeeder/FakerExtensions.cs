using BackendAccountService.Data.Entities;
using Bogus;
using Bogus.Extensions;
using OrganisationType = BackendAccountService.Data.DbConstants.OrganisationType;
using Person = BackendAccountService.Data.Entities.Person;

namespace BackendAccountService.Data.LaTestSeeder;

internal static class FakerExtensions
{
    internal static Organisation CreateOrg(this Faker faker, string companyNumber, int nationId)
    {
        var area = faker.Address.City();
        return new ()
        {
            Name = $"{area} {faker.PickRandom("District", "Borough", "City")} Council",
            CompaniesHouseNumber = $"{companyNumber}{faker.Random.Number(9999)}".ClampLength(min: 8, paddingChar: '0'),
            BuildingNumber = faker.Address.BuildingNumber(),
            Street = faker.Address.StreetName(),
            Town = area,
            County = faker.Address.County(),
            Country = "United Kingdom",
            NationId = nationId,
            Postcode = faker.Random.Replace("??# #??"),
            OrganisationTypeId = OrganisationType.WasteDisposalAuthority
        };
    }

    internal static User CreateUser(
        this Faker faker,
        IEnumerable<Organisation> orgs,
        (string Email, string Id)? userId = null)
    {
        var firstName = faker.Name.FirstName();
        var lastName = faker.Name.LastName();
        var area = faker.Address.City().Replace(" ", string.Empty);
        var email = userId == null ?
            $"{firstName.ToLower()}.{lastName.ToLower()}@{area.ToLower()}.gov.uk"
            : userId.Value.Email;
        var externalId = userId?.Id;

        return new()
        {
            UserId = Guid.NewGuid(),
            Email = email,
            ExternalIdpUserId = externalId,
            Person = new Person
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Telephone = faker.Phone.PhoneNumber("(0###) ### ####"),
                OrganisationConnections = orgs
                    .Select(x => new PersonOrganisationConnection
                    {
                        OrganisationId = x.Id,
                        OrganisationRoleId = faker.PickRandom(2, 3),
                        PersonRoleId = 2,
                        JobTitle = faker.Name.JobTitle()
                    })
                    .ToList()
            }
        };
    }

    internal static Enrolment CreateEnrolment(
        this Faker faker,
        PersonOrganisationConnection connection,
        int serviceRoleId)
    {
        return new Enrolment
        {
            ServiceRoleId = serviceRoleId,
            ConnectionId = connection.Id,
            EnrolmentStatusId = 1,
            ValidFrom = faker.Date.Past(yearsToGoBack: 1),
            ValidTo = faker.Date.Future(yearsToGoForward: 1)
        };
    }
}