using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;

namespace BackendAccountService.Core.UnitTests.TestHelpers;

public static class AccountManagementTestHelper
{
    public const int UserIdToEnroll = 2;
    
    public static void SetupDatabaseForInviteUser(AccountsDbContext setupContext)
    {
        setupContext.Database.EnsureDeleted();
        setupContext.Database.EnsureCreated();

        var user = new User
        {
            Id = 1,
            UserId = Guid.NewGuid(),
            Email = "invitee@test.com",
            IsDeleted = false
        };
        setupContext.Users.Add(user);

        var organisation1 = new Organisation
        {
            Id = 1,
            Name = "Org 1",
            OrganisationTypeId = 1,
            ExternalId = new Guid("00000000-0000-0000-0000-000000000001")
        };
        setupContext.Organisations.Add(organisation1);

        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }

    public static void SetupDatabaseForEnrolUser(AccountsDbContext setupContext)
    {
        var enrolment = new Enrolment()
        {
            ServiceRoleId = 2,
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Invited,
            Connection = new PersonOrganisationConnection()
            {
                Organisation = new Organisation()
                {
                    Name = "Org 2"
                },
                Person = new Person()
                {
                    Email = string.Empty,
                    FirstName = String.Empty,
                    LastName = String.Empty,
                    Telephone = String.Empty,
                    User = new User()
                    {
                        Id = UserIdToEnroll,
                        UserId = Guid.Empty,
                        Email = "invitee2@test.com",
                        InviteToken = "_inviteToken_"
                    }
                }
            }
        };
        
        setupContext.Enrolments.Add(enrolment);
        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }

    public static void SetupDatabaseForInvitingUser(AccountsDbContext setupContext)
    {
        var enrolment = new Enrolment
        {
            ServiceRoleId = 1,
            EnrolmentStatusId = Data.DbConstants.EnrolmentStatus.Approved,
            Connection = new PersonOrganisationConnection
            {
                Organisation = setupContext.Organisations.First(),
                Person = new Person
                {
                    Email = "inviter@test.com",
                    FirstName = "Inviter",
                    LastName = "InviterSurname",
                    Telephone = "0123456789",
                    User = new User
                    {
                        Id = 3,
                        UserId = new Guid("00000003-0003-0003-0003-000000000003"),
                        Email = "inviter@test.com",
                        InviteToken = "_inviteToken_"
                    }
                }
            }
        };
        
        setupContext.Enrolments.Add(enrolment);
        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }

    public static void SetupDatabaseForOrganisations(AccountsDbContext setupContext)
    {
        setupContext.ComplianceSchemes.Add(new ComplianceScheme
        {
            ExternalId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            CompaniesHouseNumber = "CH123457",
            Name = "Test Compliance Scheme2"
        });
        setupContext.Organisations.Add(new Organisation
        {
            ExternalId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            CompaniesHouseNumber = "CH123456",
            Name = "Test Compliance Scheme"
        });
        
        setupContext.SaveChanges(Guid.Empty, Guid.Empty);
    }
}