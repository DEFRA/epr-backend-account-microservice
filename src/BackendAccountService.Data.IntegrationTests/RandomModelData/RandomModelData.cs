using BackendAccountService.Core.Models;

namespace BackendAccountService.Data.IntegrationTests
{
    public static class RandomModelData
    {
        public static AccountModel GetAccountModel(string serviceRoleKey, bool isComplianceScheme = false)
        {
            var random = Random.Shared.Next(0, int.MaxValue);

            return new AccountModel
            {
                Connection = new ConnectionModel
                {
                    ServiceRole = serviceRoleKey,
                    JobTitle = "Job Title"
                },
                Organisation = new OrganisationModel
                {
                    Address = new AddressModel
                    {
                        SubBuildingName = $"SubBuildingName({random})",
                        BuildingName = $"BuildingName({random})",
                        BuildingNumber = $"BuildingNumber({random})",
                        Street = $"Street({random})",
                        Locality = $"Locality({random})",
                        DependentLocality = $"DependentLocality({random})",
                        County = $"County({random})",
                        Country = $"Country({random})",
                        Postcode = $"PC({random}))",
                        Town = $"Town({random})"
                    },
                    CompaniesHouseNumber = $"CH({random})",
                    Name = $"Name({random})",
                    OrganisationType = OrganisationType.CompaniesHouseCompany,
                    Nation = Nation.Scotland,
                    IsComplianceScheme = isComplianceScheme
                },
                Person = new PersonModel
                {
                    ContactEmail = $"ContactEmail@{random}.com",
                    FirstName = $"FirstName({random})",
                    LastName = $"LastName({random})",
                    TelephoneNumber = $"TelephoneNumber({random})",
                },
                User = new UserModel
                {
                    Email = $"Email@{random}.com",
                    UserId = Guid.NewGuid()
                }
            };
        }
    }
}
