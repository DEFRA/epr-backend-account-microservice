using System.Reflection;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using Bogus;

namespace BackendAccountService.Data.LaTestSeeder;

using Nation = DbConstants.Nation;

internal static class DataGenerator
{
    private const string LapsServiceUserRole = "LaPayment.BasicUser";

    /// <summary>
    /// Generates fake data for:
    /// 1 user who works for a Welsh LA,
    /// 1 user who works for a English LA,
    /// 1 user who works for a Scottish LA,
    /// 1 user who works for a NI LA,
    /// 1 user who works for 2 English LA,
    /// 1 user who works for 6 English LA
    /// </summary>
    /// <param name="dbContext"></param>
    internal static void GenerateStableLocalAuthorityData(AccountsDbContext dbContext)
    {
        var auditUser = Guid.NewGuid();
        var auditOrg = Guid.NewGuid();

        SeedLapsServiceData(dbContext);

        // Fetch service role by key
        var lapsServiceRole = dbContext.ServiceRoles.Single(x => x.Key == LapsServiceUserRole);

        var faker = new Faker("en_GB");
        var orgs = new List<Organisation>
        {
            faker.CreateOrg("WAL1", Nation.Wales),
            faker.CreateOrg("ENG1", Nation.England),
            faker.CreateOrg("SCO1", Nation.Scotland),
            faker.CreateOrg("NRE1", Nation.NorthernIreland),
            faker.CreateOrg("ENG2", Nation.England),
            faker.CreateOrg("ENG2", Nation.England),
            faker.CreateOrg("ENG6", Nation.England),
            faker.CreateOrg("ENG6", Nation.England),
            faker.CreateOrg("ENG6", Nation.England),
            faker.CreateOrg("ENG6", Nation.England),
            faker.CreateOrg("ENG6", Nation.England),
            faker.CreateOrg("ENG6", Nation.England),
        };

        var laOrgs = CreateLaOrganisations(faker, orgs);
        dbContext.Organisations.AddRange(orgs);
        dbContext.LaOrganisations.AddRange(laOrgs);
        dbContext.SaveChanges(auditUser, auditOrg);

        var authIds = LoadAuthIds().ToList();

        var users = new List<User>
        {
            faker.CreateUser(
                orgs.Where(x => x.CompaniesHouseNumber.StartsWith("WAL1")),
                authIds.Any() ? authIds.ElementAt(0) : null),

            faker.CreateUser(
                orgs.Where(x => x.CompaniesHouseNumber.StartsWith("ENG1")),
                authIds.Any() ? authIds.ElementAt(1) : null),

            faker.CreateUser(
                orgs.Where(x => x.CompaniesHouseNumber.StartsWith("SCO1")),
                authIds.Any() ? authIds.ElementAt(2) : null),

            faker.CreateUser(
                orgs.Where(x => x.CompaniesHouseNumber.StartsWith("NRE1")),
                authIds.Any() ? authIds.ElementAt(3) : null),

            faker.CreateUser(
                orgs.Where(x => x.CompaniesHouseNumber.StartsWith("ENG2")),
                authIds.Any() ? authIds.ElementAt(4) : null),

            faker.CreateUser(
                orgs.Where(x => x.CompaniesHouseNumber.StartsWith("ENG6")),
                authIds.Any() ? authIds.ElementAt(5) : null),
        };

        dbContext.Users.AddRange(users);
        dbContext.SaveChanges(auditUser, auditOrg);

        var enrolments = CreateEnrolments(faker, users, lapsServiceRole.Id);
        dbContext.Enrolments.AddRange(enrolments);
        dbContext.SaveChanges(auditUser, auditOrg);
    }

    /// <summary>
    /// Generates 4000 random users and 400 random local authority organisations
    /// </summary>
    /// <param name="dbContext"></param>
    internal static void GenerateRandomLocalAuthorityData(AccountsDbContext dbContext)
    {
        var faker = new Faker("en_GB");
        var auditUser = Guid.NewGuid();
        var auditOrg = Guid.NewGuid();

        SeedLapsServiceData(dbContext);

        // Fetch service role by key
        var lapsServiceRole = dbContext.ServiceRoles.Single(x => x.Key == LapsServiceUserRole);

        var orgs = Enumerable.Range(1, 400)
            .Select(i => faker.CreateOrg("Z", faker.PickRandom(Nation.England, Nation.NorthernIreland, Nation.Scotland, Nation.Wales)))
            .ToList();

        var laOrgs = CreateLaOrganisations(faker, orgs);
        dbContext.Organisations.AddRange(orgs);
        dbContext.LaOrganisations.AddRange(laOrgs);
        dbContext.SaveChanges(auditUser, auditOrg);

        var users = Enumerable.Range(1, 4000)
            .Select(i => faker.CreateUser(new []{ faker.PickRandom(orgs) }))
            .ToList();

        dbContext.Users.AddRange(users);
        dbContext.SaveChanges(auditUser, auditOrg);

        var enrolments = CreateEnrolments(faker, users, lapsServiceRole.Id);
        dbContext.Enrolments.AddRange(enrolments);
        dbContext.SaveChanges(auditUser, auditOrg);
    }

    private static List<Enrolment> CreateEnrolments(Faker faker, IEnumerable<User> users, int lapsServiceRoleId)
    {
        var enrolments = new List<Enrolment>();
        foreach (var user in users)
        {
            foreach (var connection in user.Person.OrganisationConnections)
            {
                enrolments.Add(faker.CreateEnrolment(connection, lapsServiceRoleId));
            }
        }

        return enrolments;
    }

    private static List<LaOrganisation> CreateLaOrganisations(Faker faker, List<Organisation> orgs)
    {
        var laOrgs = new List<LaOrganisation>();
        foreach (var org in orgs)
        {
            laOrgs.Add(new LaOrganisation
            {
                DistrictCode = faker.Random.Replace("E07######"),
                Organisation = org
            });
        }

        return laOrgs;
    }

    private static void SeedLapsServiceData(AccountsDbContext dbContext)
    {
        // Seed org to person roles
        var orgRoles = dbContext.OrganisationToPersonRoles
            .Where(x => x.Name == "CEO" || x.Name == "Administator")
            .ToList();

        if (!orgRoles.Any())
        {
            int idSeed = dbContext.OrganisationToPersonRoles.Max(x => x.Id);
            orgRoles.Add(new OrganisationToPersonRole { Name = "CEO", Id = idSeed + 1 });
            orgRoles.Add(new OrganisationToPersonRole { Name = "Administator", Id = idSeed + 2 });
            dbContext.OrganisationToPersonRoles.AddRange(orgRoles);
        }
    }

    private static IEnumerable<(string Email, string Id)> LoadAuthIds()
    {
        string rootPath = GetBasePath();

        var testFile = Directory.GetFiles($"{rootPath}/TestUsers/", "*.txt")
            .Select(f => new FileInfo(f))
            .MaxBy(fi => fi.CreationTime);

        if (testFile == null)
        {
            yield break;
        }

        foreach (var line in File.ReadLines(testFile.FullName))
        {
            if (line.StartsWith("//") || string.IsNullOrWhiteSpace(line)) continue;
            var data = line.Split('|');
            yield return (data.First(), data.Last());
        }

        string GetBasePath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        }
    }
}