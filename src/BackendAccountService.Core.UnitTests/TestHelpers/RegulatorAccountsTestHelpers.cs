using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;

namespace BackendAccountService.Core.UnitTests.TestHelpers
{
    public static class RegulatorAccountsTestHelpers
    {
        public static void SetupDatabaseForOrganisation(AccountsDbContext setupContext)
        {
            setupContext.Database.EnsureDeleted();
            setupContext.Database.EnsureCreated();

            var organisation = new Organisation 
            {
                Id = 1,
                Name = "Test Organisation",
                OrganisationTypeId = Data.DbConstants.OrganisationType.Regulators,                
                NationId = Data.DbConstants.Nation.England,
                ExternalId = Guid.NewGuid(),                
            };
            
            setupContext.Organisations.Add(organisation);
            setupContext.SaveChanges(Guid.Empty, Guid.Empty);
        }
    }
}
