using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Entities.Conversions;
using BackendAccountService.Data.Entities.EntityConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using EnrolmentStatus = BackendAccountService.Data.Entities.EnrolmentStatus;
using InterOrganisationRole = BackendAccountService.Data.Entities.InterOrganisationRole;

namespace BackendAccountService.Data.Infrastructure;

public class AccountsDbContext : DbContext
{
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    public DbSet<Enrolment> Enrolments { get; set; } = null!;

    public DbSet<EnrolmentStatus> EnrolmentStatuses { get; set; } = null!;

    public DbSet<ApprovedPersonEnrolment> ApprovedPersonEnrolments { get; set; } = null!;

    public DbSet<DelegatedPersonEnrolment> DelegatedPersonEnrolments { get; set; } = null!;

    public DbSet<InterOrganisationRole> InterOrganisationRoles { get; set; } = null!;

    public DbSet<InterPersonRole> InterPersonRoles { get; set; } = null!;

    public DbSet<Nation> Nations { get; set; } = null!;

    public DbSet<Organisation> Organisations { get; set; } = null!;

    public DbSet<LaOrganisation> LaOrganisations { get; set; } = null!;

    public DbSet<OrganisationToPersonRole> OrganisationToPersonRoles { get; set; } = null!;

    public DbSet<OrganisationsConnection> OrganisationsConnections { get; set; } = null!;

    public DbSet<OrganisationType> OrganisationTypes { get; set; } = null!;

    public DbSet<OrganisationRelationship> OrganisationRelationships { get; set; } = null!;

    public DbSet<OrganisationRelationshipType> OrganisationRelationshipTypes { get; set; } = null!;

    public DbSet<OrganisationRegistrationType> OrganisationRegistrationTypes { get; set; } = null!;

    public DbSet<SubsidiaryOrganisation> SubsidiaryOrganisations { get; set; } = null!;

    public DbSet<Person> Persons { get; set; } = null!;

    public DbSet<PersonInOrganisationRole> PersonInOrganisationRoles { get; set; } = null!;

    public DbSet<PersonOrganisationConnection> PersonOrganisationConnections { get; set; } = null!;

    public DbSet<PersonOrganisationConnectionInvite> PersonOrganisationConnectionInvites { get; set; } = null!;

    public DbSet<PersonsConnection> PersonsConnections { get; set; } = null!;

    public DbSet<Service> Services { get; set; } = null!;

    public DbSet<ServiceRole> ServiceRoles { get; set; } = null!;

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<ComplianceScheme> ComplianceSchemes { get; set; }

    public DbSet<SelectedScheme> SelectedSchemes { get; set; }

    public DbSet<ProducerType> ProducerTypes { get; set; }

    public DbSet<RegulatorComment> RegulatorComments { get; set; }

    public DbSet<EnrolmentReport> EnrolmentReport { get; set; }

    public DbSet<OfflineApprovalErrorLog> OfflineApprovalErrorLog { get; set; }

    public DbSet<ComplianceSchemeMemberRemovalReason> ComplianceSchemeMemberRemovalReasons { get; set; }

    public DbSet<ComplianceSchemeMemberRemovalAuditLog> ComplianceSchemeMemberRemovalAuditLogs { get; set; } = null;

    public DbSet<ComplianceSchemeMemberRemovalAuditLogsReason> ComplianceSchemeMemberRemovalAuditLogsReasons { get; set; }

    public DbSet<ChangeHistory> ChangeHistory { get; set; } = null!;

    public DbSet<LeaverCode> LeaverCodes { get; set; } = null!;

    public DbSet<PartnerRole> PartnerRoles { get; set; } = null!;

    public DbSet<OrganisationToPartnerRole> OrganisationToPartnerRoles { get; set; } = null!;

    public DbSet<CodeClassificationLookup> CodeClassificationLookups { get; set; } = null;

    public DbSet<CodeStatusConfig> CodeStatusConfigs { get; set; } = null;

    public DbSet<ScenarioReference> ScenarioReferences { get; set; } = null;


    public AccountsDbContext(DbContextOptions<AccountsDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        new DelegatedPersonEnrolmentConfig().Configure(modelBuilder.Entity<DelegatedPersonEnrolment>());

        new ComplianceSchemeMemberRemovalReasonConfig().Configure(modelBuilder.Entity<ComplianceSchemeMemberRemovalReason>());

        modelBuilder.Entity<ComplianceScheme>()
            .Property(p => p.NationId);

        modelBuilder.Entity<DelegatedPersonEnrolment>(entity =>
        {
            entity.HasOne(delegatedPersonEnrolment => delegatedPersonEnrolment.NominatorEnrolment)
                .WithMany()
                .HasForeignKey(enrolment => enrolment.NominatorEnrolmentId);

            entity.HasOne(delegatedPersonEnrolment => delegatedPersonEnrolment.Enrolment)
                .WithOne(enrolment => enrolment.DelegatedPersonEnrolment)
                .HasForeignKey<DelegatedPersonEnrolment>(enrolment => enrolment.EnrolmentId);
        });

        modelBuilder.Entity<ApprovedPersonEnrolment>(entity =>
        {
            entity.HasOne(a => a.Enrolment)
                .WithOne(enrolment => enrolment.ApprovedPersonEnrolment)
                .HasForeignKey<ApprovedPersonEnrolment>(e => e.EnrolmentId);
        });

        modelBuilder.Entity<OrganisationsConnection>(entity =>
        {
            entity.HasOne(connection => connection.FromOrganisation)
                .WithMany(organisation => organisation.FromOrganisationConnections)
                .HasForeignKey(connection => connection.FromOrganisationId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(connection => connection.ToOrganisation)
                .WithMany(organisation => organisation.ToOrganisationsConnections)
                .HasForeignKey(connection => connection.ToOrganisationId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(connection => connection.FromOrganisationRole)
                .WithMany()
                .HasForeignKey(connection => connection.FromOrganisationRoleId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(connection => connection.ToOrganisationRole)
                .WithMany()
                .HasForeignKey(connection => connection.ToOrganisationRoleId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<OrganisationRelationship>(entity =>
        {
            entity.HasOne(relation => relation.FirstOrganisation)
                .WithMany(organisation => organisation.OrganisationRelationships)
                .HasForeignKey(connection => connection.FirstOrganisationId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(relationshipType => relationshipType.OrganisationRelationshipType)
                .WithMany()
                .HasForeignKey(connection => connection.OrganisationRelationshipTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(registrationType => registrationType.OrganisationRegistrationType)
                .WithMany()
                .HasForeignKey(connection => connection.OrganisationRegistrationTypeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(leaverCode => leaverCode.LeaverCode)
                .WithMany()
                .HasForeignKey(connection => connection.LeaverCodeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(codeStatusConfig => codeStatusConfig.CodeStatusConfig)
                .WithMany()
                .HasForeignKey(connection => connection.CodeStatusConfigId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

        });

        modelBuilder.Entity<SubsidiaryOrganisation>(entity =>
        {
            entity.HasOne(subsidiaryOrganisation => subsidiaryOrganisation.Organisation)
                .WithMany(organisation => organisation.SubsidiaryOrganisations)
                .HasForeignKey(subsidiaryOrganisation => subsidiaryOrganisation.OrganisationId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PersonOrganisationConnectionInvite>(entity =>
        {
            entity.HasOne(connection => connection.Organisation)
                .WithMany()
                .HasForeignKey(connection => connection.OrganisationId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(connection => connection.Person)
                .WithMany()
                .HasForeignKey(connection => connection.InviteePersonId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(connection => connection.User)
                .WithMany()
                .HasForeignKey(connection => connection.InvitedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(connection => connection.Service)
                .WithMany()
                .HasForeignKey(connection => connection.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PersonsConnection>(entity =>
        {
            entity.HasOne(connection => connection.FromPerson)
                .WithMany(organisation => organisation.FromPersonConnections)
                .HasForeignKey(connection => connection.FromPersonId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(connection => connection.ToPerson)
                .WithMany(organisation => organisation.ToPersonConnections)
                .HasForeignKey(connection => connection.ToPersonId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(connection => connection.FromPersonRole)
                .WithMany()
                .HasForeignKey(connection => connection.FromPersonRoleId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(connection => connection.ToPersonRole)
                .WithMany()
                .HasForeignKey(connection => connection.ToPersonRoleId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<LaOrganisation>(entity =>
        {
            entity.HasOne(laOrganisation => laOrganisation.Organisation)
                .WithOne()
                .HasForeignKey<LaOrganisation>(laOrganisation => laOrganisation.OrganisationId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.HasOne(e => e.DelegatedPersonEnrolment)
                .WithOne(d => d.Enrolment)
                .HasForeignKey<DelegatedPersonEnrolment>(d => d.EnrolmentId);

            entity.HasOne(e => e.ApprovedPersonEnrolment)
                .WithOne(a => a.Enrolment)
                .HasForeignKey<ApprovedPersonEnrolment>(a => a.EnrolmentId);
        });

        modelBuilder.Entity<RegulatorComment>(entity =>
        {
            entity.HasOne<Person>(e => e.Person)
                .WithMany(e => e.RegulatorComments)
                .HasForeignKey(e => e.PersonId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne<Enrolment>(e => e.Enrolment)
                .WithMany(e => e.RegulatorComments)
                .HasForeignKey(e => e.EnrolmentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<OrganisationToPartnerRole>(entity =>
        {
            entity.HasOne(otr => otr.Organisation)
                .WithMany()
                .HasForeignKey(otr => otr.OrganisationId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(otr => otr.PartnerRole)
                .WithMany()
                .HasForeignKey(otr => otr.PartnerRoleId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CodeStatusConfig>(entity =>
        {
            entity.HasOne(otr => otr.CodeClassificationLookup)
            .WithMany()
            .HasForeignKey(otr => otr.ClassificationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CodeScenarioMapping>(entity =>
        {
            entity.HasOne(otr => otr.CodeStatusConfig)
            .WithMany()
            .HasForeignKey(otr => otr.CodeStatusConfigId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(otr => otr.ScenarioReference)
            .WithMany()
            .HasForeignKey(otr => otr.ScenarioReferenceId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.NoAction);
        });

        AddSoftDeleteFilters(modelBuilder);

        AddIndexes(modelBuilder);

        SetDefaultValues(modelBuilder);

        DisableCascades(modelBuilder);

        PopulateDictionaries(modelBuilder);
    }

    private static void AddSoftDeleteFilters(ModelBuilder modelBuilder)
    {
        var hasSoftDeleteFilterMethod = typeof(ModelBuilderExtensions)
            .GetMethod(nameof(ModelBuilderExtensions.HasSoftDeleteFilter))!;

        foreach (var entityType in modelBuilder.Model
                     .GetEntityTypes()
                     .Where(entityType => entityType.ClrType.GetInterfaces().Contains(typeof(ISoftDeletableEntity))))
        {
            hasSoftDeleteFilterMethod
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(null, new object[] { modelBuilder });
        }

        modelBuilder.Entity<LaOrganisation>()
            .HasQueryFilter(organisation => !organisation.IsDeleted && !organisation.Organisation.IsDeleted);

        modelBuilder.Entity<DelegatedPersonEnrolment>()
            .HasQueryFilter(enrolment => !enrolment.IsDeleted && !enrolment.Enrolment.IsDeleted);
    }

    private static void AddIndexes(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model
                     .GetEntityTypes()
                     .Where(entityType => entityType.ClrType.IsSubclassOf(typeof(DataEntity)))
                     .Select(entityType => modelBuilder.Entity(entityType.ClrType)))
        {
            entity
                .HasIndex("ExternalId")
                .IsUnique();
        }

        modelBuilder.Entity<User>()
            .HasIndex(user => user.UserId)
            .IsUnique()
            .HasFilter("[IsDeleted] = CAST(0 AS bit) AND [UserId] <> CAST(0x0 AS UNIQUEIDENTIFIER)");

        modelBuilder.Entity<Organisation>()
            .HasIndex(organisation => organisation.CompaniesHouseNumber)
            .HasFilter("[CompaniesHouseNumber] IS NOT NULL");

        modelBuilder.Entity<ComplianceSchemeMemberRemovalReason>()
            .HasIndex(ComplianceSchemeMemberRemovalReason => ComplianceSchemeMemberRemovalReason.Code)
            .IsUnique();

        modelBuilder.Entity<Organisation>()
            .HasIndex(organisation => new
            {
                organisation.Name,
                organisation.ReferenceNumber,
                organisation.NationId
            })
            .HasFilter($"[OrganisationTypeId] <> {DbConstants.OrganisationType.Regulators} AND [IsDeleted] = CAST(0 AS bit)")
            .HasDatabaseName("IX_Name_ReferenceNumber_NationId");

        modelBuilder.Entity<Organisation>()
            .HasIndex(organisation => new
            {
                organisation.ReferenceNumber
            })
            .HasFilter($"[IsDeleted] = CAST(0 AS bit)");
    }

    private static void SetDefaultValues(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model
                     .GetEntityTypes()
                     .Where(entityType => entityType.ClrType.IsSubclassOf(typeof(DataEntity)))
                     .Select(entityType => modelBuilder.Entity(entityType.ClrType)))
        {
            entity.Property("ExternalId").HasDefaultValueSql("NEWID()");
        }

        foreach (var entity in modelBuilder.Model
                     .GetEntityTypes()
                     .Where(entityType => entityType.ClrType.GetInterfaces().Contains(typeof(IEditableEntity)))
                     .Select(entityType => modelBuilder.Entity(entityType.ClrType)))
        {
            entity.Property("CreatedOn").HasDefaultValueSql("SYSDATETIMEOFFSET()");
            entity.Property("LastUpdatedOn").HasDefaultValueSql("SYSDATETIMEOFFSET()");
        }

        modelBuilder.Entity<OrganisationRelationship>()
            .Property("RelationFromDate")
            .HasDefaultValueSql("SYSDATETIMEOFFSET()");

        modelBuilder.Entity<ComplianceSchemeMemberRemovalAuditLog>()
            .Property("RemovedOn")
            .HasDefaultValueSql("SYSDATETIMEOFFSET()");

        foreach (var entity in modelBuilder.Model
                     .GetEntityTypes()
                     .Where(entityType => entityType.ClrType.GetInterfaces().Contains(typeof(ISoftDeletableEntity)))
                     .Select(entityType => modelBuilder.Entity(entityType.ClrType)))
        {
            entity.Property("IsDeleted").HasDefaultValue(false);
        }

        const string sequenceName = "OrganisationReferenceNumber";

        modelBuilder.HasSequence<int>(sequenceName)
            .StartsAt(100001)
            .IncrementsBy(1);

        modelBuilder.Entity<Organisation>()
            .Property(organisation => organisation.ReferenceNumber)
            .HasDefaultValueSql($"NEXT VALUE FOR {sequenceName}");
    }

    private static void DisableCascades(ModelBuilder modelBuilder)
    {
        foreach (var foreignKey in modelBuilder.Model
                     .GetEntityTypes()
                     .SelectMany(entity => entity.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
        }
    }

    private static void PopulateDictionaries(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnrolmentStatus>(entity =>
        {
            entity.Property(status => status.Id).ValueGeneratedNever();
            entity.HasData(
                new EnrolmentStatus { Id = DbConstants.EnrolmentStatus.NotSet, Name = "Not Set" },
                new EnrolmentStatus { Id = DbConstants.EnrolmentStatus.Enrolled, Name = "Enrolled" },
                new EnrolmentStatus { Id = DbConstants.EnrolmentStatus.Pending, Name = "Pending" },
                new EnrolmentStatus { Id = DbConstants.EnrolmentStatus.Approved, Name = "Approved" },
                new EnrolmentStatus { Id = DbConstants.EnrolmentStatus.Rejected, Name = "Rejected" },
                new EnrolmentStatus { Id = DbConstants.EnrolmentStatus.Invited, Name = "Invited" },
                new EnrolmentStatus { Id = DbConstants.EnrolmentStatus.OnHold, Name = "On Hold" },
                new EnrolmentStatus { Id = DbConstants.EnrolmentStatus.Nominated, Name = "Nominated" });
        });

        modelBuilder.Entity<InterOrganisationRole>(entity =>
        {
            entity.Property(role => role.Id).ValueGeneratedNever();
            entity.HasData(
                new InterOrganisationRole { Id = DbConstants.InterOrganisationRole.NotSet, Name = "Not Set" },
                new InterOrganisationRole { Id = DbConstants.InterOrganisationRole.Producer, Name = "Producer" },
                new InterOrganisationRole { Id = DbConstants.InterOrganisationRole.ComplianceScheme, Name = "Compliance Scheme" });
        });

        modelBuilder.Entity<InterPersonRole>(entity =>
        {
            entity.Property(role => role.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Nation>(entity =>
        {
            entity.Property(nation => nation.Id).ValueGeneratedNever();

            entity.HasData(
                new Nation { Id = DbConstants.Nation.NotSet, Name = "Not Set", NationCode = "" },
                new Nation { Id = DbConstants.Nation.England, Name = "England", NationCode = "GB-ENG" },
                new Nation { Id = DbConstants.Nation.NorthernIreland, Name = "Northern Ireland", NationCode = "GB-NIR" },
                new Nation { Id = DbConstants.Nation.Scotland, Name = "Scotland", NationCode = "GB-SCT" },
                new Nation { Id = DbConstants.Nation.Wales, Name = "Wales", NationCode = "GB-WLS" });
        });

        modelBuilder.Entity<OrganisationToPersonRole>(entity =>
        {
            entity.Property(role => role.Id).ValueGeneratedNever();
            entity.HasData(
                new OrganisationToPersonRole { Id = DbConstants.OrganisationRole.NotSet, Name = "Not Set" },
                new OrganisationToPersonRole { Id = DbConstants.OrganisationRole.Employer, Name = "Employer" });
        });

        modelBuilder.Entity<OrganisationType>(entity =>
        {
            entity.Property(type => type.Id).ValueGeneratedNever();
            entity.HasData(
                new OrganisationType { Id = DbConstants.OrganisationType.NotSet, Name = "Not Set" },
                new OrganisationType
                { Id = DbConstants.OrganisationType.CompaniesHouseCompany, Name = "Companies House Company" },
                new OrganisationType
                { Id = DbConstants.OrganisationType.NonCompaniesHouseCompany, Name = "Non Companies House Company" },
                new OrganisationType
                { Id = DbConstants.OrganisationType.WasteCollectionAuthority, Name = "Waste Collection Authority" },
                new OrganisationType
                { Id = DbConstants.OrganisationType.WasteDisposalAuthority, Name = "Waste Disposal Authority" },
                new OrganisationType
                {
                    Id = DbConstants.OrganisationType.WasteCollectionAuthorityWasteDisposalAuthority,
                    Name = "Waste Collection Authority & Waste Disposal Authority"
                },
                new OrganisationType { Id = DbConstants.OrganisationType.Regulators, Name = "Regulators" });
        });

        modelBuilder.Entity<ProducerType>(entity =>
        {
            entity.Property(type => type.Id).ValueGeneratedNever();
            entity.HasData(
                new ProducerType { Id = DbConstants.ProducerType.NotSet, Name = "Not Set" },
                new ProducerType { Id = DbConstants.ProducerType.Partnership, Name = "Partnership" },
                new ProducerType { Id = DbConstants.ProducerType.UnincorporatedBody, Name = "Unincorporated body" },
                new ProducerType { Id = DbConstants.ProducerType.NonUkOrganisation, Name = "Non-UK organisation" },
                new ProducerType { Id = DbConstants.ProducerType.SoleTrader, Name = "Sole trader" },
                new ProducerType { Id = DbConstants.ProducerType.Other, Name = "Other" },
                new ProducerType { Id = DbConstants.ProducerType.LimitedPartnership, Name = "Limited partnership" },
                new ProducerType { Id = DbConstants.ProducerType.LimitedLiabilityPartnership, Name = "Limited Liability partnership" });
        });

        modelBuilder.Entity<PersonInOrganisationRole>(entity =>
        {
            entity.Property(role => role.Id).ValueGeneratedNever();
            entity.HasData(
                new PersonInOrganisationRole { Id = DbConstants.PersonRole.NotSet, Name = "Not Set" },
                new PersonInOrganisationRole { Id = DbConstants.PersonRole.Admin, Name = "Admin" },
                new PersonInOrganisationRole { Id = DbConstants.PersonRole.Employee, Name = "Employee" },
                new PersonInOrganisationRole { Id = DbConstants.PersonRole.Member, Name = "Member" }
            );
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasData(
                new Service
                {
                    Id = DbConstants.Service.EprPackaging,
                    Description = "Extended Producer Responsibility - Packaging",
                    Key = "Packaging",
                    Name = "EPR Packaging"
                },
                new Service
                {
                    Id = DbConstants.Service.RegulatorEnrolment,
                    Description = "Extended Producer Responsibility - Regulating",
                    Key = "Regulating",
                    Name = "EPR Regulating"
                },
                new Service
                {
                    Id = DbConstants.Service.LaPayment,
                    Description = "Local Authority Payment Service",
                    Key = "LaPayment",
                    Name = "Local Authority Payment Service"
                },
                new Service
                {
                    Id = DbConstants.Service.ReprocessorExporter,
                    Description = "Extended Producer Responsibility For Packaging: Reprocessors And Exporters",
                    Key = "ReprocessorExporter",
                    Name = "EPR for packaging: reprocessors and exporters"
                });
        });

        modelBuilder.Entity<ServiceRole>(entity =>
        {
            entity.HasData(
                new ServiceRole
                {
                    Id = DbConstants.ServiceRole.Packaging.ApprovedPerson.Id,
                    ServiceId = DbConstants.Service.EprPackaging,
                    Key = DbConstants.ServiceRole.Packaging.ApprovedPerson.Key,
                    Name = "Approved Person"
                },
                new ServiceRole
                {
                    Id = DbConstants.ServiceRole.Packaging.DelegatedPerson.Id,
                    ServiceId = DbConstants.Service.EprPackaging,
                    Key = DbConstants.ServiceRole.Packaging.DelegatedPerson.Key,
                    Name = "Delegated Person"
                },
                new ServiceRole
                {
                    Id = DbConstants.ServiceRole.Packaging.BasicUser.Id,
                    ServiceId = DbConstants.Service.EprPackaging,
                    Key = DbConstants.ServiceRole.Packaging.BasicUser.Key,
                    Name = "Basic User"
                },
                new ServiceRole
                {
                    Id = DbConstants.ServiceRole.Regulator.Admin.Id,
                    ServiceId = DbConstants.Service.RegulatorEnrolment,
                    Key = DbConstants.ServiceRole.Regulator.Admin.Key,
                    Name = "Regulator Admin",
                    Description = "Regulator Admin Service Role"
                },
                new ServiceRole
                {
                    Id = DbConstants.ServiceRole.Regulator.Basic.Id,
                    ServiceId = DbConstants.Service.RegulatorEnrolment,
                    Key = DbConstants.ServiceRole.Regulator.Basic.Key,
                    Name = "Regulator Basic",
                    Description = "Regulator Basic Service Role"
                },
                new ServiceRole
                {
                    Id = DbConstants.ServiceRole.LaPayment.Admin.Id,
                    ServiceId = DbConstants.Service.LaPayment,
                    Key = DbConstants.ServiceRole.LaPayment.Admin.Key,
                    Name = "User Administrator",
                    Description = null
                },
                new ServiceRole
                {
                    Id = DbConstants.ServiceRole.LaPayment.BasicUser.Id,
                    ServiceId = DbConstants.Service.LaPayment,
                    Key = DbConstants.ServiceRole.LaPayment.BasicUser.Key,
                    Name = "Basic User",
                    Description = null
                },
                new ServiceRole
                {
                    Id = DbConstants.ServiceRole.ReprocessorExporter.ApprovedPerson.Id,
                    ServiceId = DbConstants.Service.ReprocessorExporter,
                    Key = DbConstants.ServiceRole.ReprocessorExporter.ApprovedPerson.Key,
                    Name = "Approved Person",
                    Description = "Manage team, submit registration and accreditation"
                },
                new ServiceRole
                {
                    Id = DbConstants.ServiceRole.ReprocessorExporter.DelegatedPerson.Id,
                    ServiceId = DbConstants.Service.ReprocessorExporter,
                    Key = DbConstants.ServiceRole.ReprocessorExporter.DelegatedPerson.Key,
                    Name = "Delegated Person",
                    Description = null
                },
                new ServiceRole
                {
                    Id = DbConstants.ServiceRole.ReprocessorExporter.BasicUser.Id,
                    ServiceId = DbConstants.Service.ReprocessorExporter,
                    Key = DbConstants.ServiceRole.ReprocessorExporter.BasicUser.Key,
                    Name = "Basic User",
                    Description = "Read only"
                },
                new ServiceRole
                {
                    Id = DbConstants.ServiceRole.ReprocessorExporter.AdminUser.Id,
                    ServiceId = DbConstants.Service.ReprocessorExporter,
                    Key = DbConstants.ServiceRole.ReprocessorExporter.AdminUser.Key,
                    Name = "Admin User",
                    Description = "Manage team, submit registration and accreditation"
                },
                new ServiceRole
                {
                    Id = DbConstants.ServiceRole.ReprocessorExporter.StandardUser.Id,
                    ServiceId = DbConstants.Service.ReprocessorExporter,
                    Key = DbConstants.ServiceRole.ReprocessorExporter.StandardUser.Key,
                    Name = "Standard User",
                    Description = "Submit registration and apply for accreditation"
                });
        });

        modelBuilder.Entity<ComplianceScheme>(entity =>
        {
            entity.HasOne(complianceScheme => complianceScheme.Nation)
                .WithMany()
                .HasForeignKey(complianceScheme => complianceScheme.NationId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasData(PopulateComplianceSchemes());
        });

        modelBuilder.Entity<OrganisationRelationshipType>(entity =>
        {
            entity.Property(type => type.Id).ValueGeneratedNever();
            entity.HasData(
                new OrganisationRelationshipType { Id = DbConstants.OrganisationRelationshipType.NotSet, Name = "Not Set" },
                new OrganisationRelationshipType { Id = DbConstants.OrganisationRelationshipType.Parent, Name = "Parent" },
                new OrganisationRelationshipType { Id = DbConstants.OrganisationRelationshipType.Child, Name = "Child" });
        });

        modelBuilder.Entity<OrganisationRegistrationType>(entity =>
        {
            entity.Property(type => type.Id).ValueGeneratedNever();
            entity.HasData(
                new OrganisationRegistrationType { Id = DbConstants.OrganisationRegistrationType.NotSet, Key = "", Name = "Not Set" },
                new OrganisationRegistrationType { Id = DbConstants.OrganisationRegistrationType.Group, Key = "GR", Name = "Group" },
                new OrganisationRegistrationType { Id = DbConstants.OrganisationRegistrationType.Individual, Key = "IN", Name = "Individual" });
        });

        modelBuilder.Entity<LeaverCode>(entity =>
        {
            entity.Property(type => type.Id).ValueGeneratedNever();
            entity.HasData(
                new LeaverCode { Id = DbConstants.LeaverCode.NotSet, Key = "", ReasonsForLeaving = "Not Set" },
                new LeaverCode { Id = DbConstants.LeaverCode.A, Key = "A", ReasonsForLeaving = "Administration/Receivership" },
                new LeaverCode { Id = DbConstants.LeaverCode.B, Key = "B", ReasonsForLeaving = "Liquidation/dissolution" },
                new LeaverCode { Id = DbConstants.LeaverCode.C, Key = "C", ReasonsForLeaving = "Dropped below turnover threshold" },
                new LeaverCode { Id = DbConstants.LeaverCode.D, Key = "D", ReasonsForLeaving = "Dropped below tonnage threshold" },
                new LeaverCode { Id = DbConstants.LeaverCode.E, Key = "E", ReasonsForLeaving = "Resignation (not incapacity related)" },
                new LeaverCode { Id = DbConstants.LeaverCode.F, Key = "F", ReasonsForLeaving = "Scheme has terminated membership (not incapacity related)" },
                new LeaverCode { Id = DbConstants.LeaverCode.G, Key = "G", ReasonsForLeaving = "Business closure (not incapacity related)" },
                new LeaverCode { Id = DbConstants.LeaverCode.H, Key = "H", ReasonsForLeaving = "Bankruptcy" },
                new LeaverCode { Id = DbConstants.LeaverCode.I, Key = "I", ReasonsForLeaving = "Merged with another company (not incapacity related)" },
                new LeaverCode { Id = DbConstants.LeaverCode.J, Key = "J", ReasonsForLeaving = "Now a subsidiary of another company (not incapacity related)" },
                new LeaverCode { Id = DbConstants.LeaverCode.K, Key = "K", ReasonsForLeaving = "Not ready to register by 15th April" },
                new LeaverCode { Id = DbConstants.LeaverCode.L, Key = "L", ReasonsForLeaving = "No longer obligated (Not threshold related)" });
        });


        modelBuilder.Entity<CodeClassificationLookup>(entity =>
        {
            entity.Property(type => type.Id).ValueGeneratedNever();
            entity.HasData(
                new CodeClassificationLookup { Id = DbConstants.CodeClassificationLookup.NotSet, CodeClass = "Not Set", Description = "Not Set", GroupType = "Not Set" },
                new CodeClassificationLookup { Id = DbConstants.CodeClassificationLookup.Joiner, CodeClass = "Joiner", Description = "Joiner scenario", GroupType = "Entry" },
                new CodeClassificationLookup { Id = DbConstants.CodeClassificationLookup.Leaver, CodeClass = "Leaver", Description = "Leaver scenario", GroupType = "Exit" });
        });

        modelBuilder.Entity<ScenarioReference>(entity =>
        {
            entity.Property(type => type.Id).ValueGeneratedNever();
            entity.HasData(
                new ScenarioReference { Id = DbConstants.ScenarioReference.NotSet, ScenarioCode = "Not Set", Description = "Not Set", ObligationFlag = "Not Set", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR1, ScenarioCode = "1b", Description = "Threshold producer joins a group", ObligationFlag = "Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR2, ScenarioCode = "5b", Description = "Threshold producer joins a group", ObligationFlag = "Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR3, ScenarioCode = "2a", Description = "Below-threshold producer joins group (% obligation)", ObligationFlag = "Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR4, ScenarioCode = "2b", Description = "Below-threshold producer joins group (% obligation)", ObligationFlag = "Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR5, ScenarioCode = "3b", Description = "Meets thresholds but group responsible for obligation", ObligationFlag = "Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR6, ScenarioCode = "4a", Description = "Leaves group, holding company responsible", ObligationFlag = "Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR7, ScenarioCode = "4b", Description = "Leaves group, holding company responsible", ObligationFlag = "Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR8, ScenarioCode = "5d", Description = "Joined group mid-year, not obligated", ObligationFlag = "Not Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR9, ScenarioCode = "5d", Description = "Left group mid-year, HC still responsible", ObligationFlag = "Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR10, ScenarioCode = "5c", Description = "Joined group, not obligated", ObligationFlag = "Not Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR11, ScenarioCode = "5c", Description = "Left group, HC still responsible", ObligationFlag = "Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR12, ScenarioCode = "none", Description = "Producer no longer obligated - insolvency or ceased function", ObligationFlag = "Not Obligated - HC", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR13, ScenarioCode = "none", Description = "Resigned from compliance scheme", ObligationFlag = "Not Obligated - CS", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR14, ScenarioCode = "none", Description = "Compliance scheme terminated membership", ObligationFlag = "Not Obligated - CS", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR16, ScenarioCode = "none", Description = "Merged with another company - non-incapacity, CS only", ObligationFlag = "Not Obligated - CS", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR17, ScenarioCode = "none", Description = "Producer data changed (e.g. acquisition of insolvent entity)", ObligationFlag = "Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR18, ScenarioCode = "all", Description = "Producer who meets thresholds independently has left group. Holding Company remains responsible for obligation due to Mid Year change", ObligationFlag = "Not Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR19, ScenarioCode = "all", Description = "Producer Registered late.", ObligationFlag = "Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR20, ScenarioCode = "all", Description = "Other - Joiner", ObligationFlag = "Obligated", Active = true },
                new ScenarioReference { Id = DbConstants.ScenarioReference.SR21, ScenarioCode = "all", Description = "Other - Leaver", ObligationFlag = "Not Obligated", Active = true });
    });

        modelBuilder.Entity<CodeStatusConfig>(entity =>
        {
            entity.Property(type => type.Id).ValueGeneratedNever();
            entity.HasData(
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.A,
                    Code = "01",
                    LegacyCode = "A",
                    ClassificationId = DbConstants.CodeClassificationLookup.Joiner,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = false,
                    RequiresRegType = true,
                    MatchType = "Direct",
                    MappedOldCodes = "None",
                    Enabled = true,
                    Description = "Producer who previously met thresholds has joined a group."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.B,
                    Code = "02",
                    LegacyCode = "B",
                    ClassificationId = DbConstants.CodeClassificationLookup.Joiner,
                    RequiresJoinerDate = true,
                    RequiresLeaverDate = false,
                    RequiresRegType = true,
                    MatchType = "Direct",
                    MappedOldCodes = "None",
                    Enabled = true,
                    Description = "Producer who did not previously meet thresholds has joined a group (% obligation)."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.C,
                    Code = "03",
                    LegacyCode = "C",
                    ClassificationId = DbConstants.CodeClassificationLookup.Joiner,
                    RequiresJoinerDate = true,
                    RequiresLeaverDate = false,
                    RequiresRegType = true,
                    MatchType = "Direct",
                    MappedOldCodes = "None",
                    Enabled = true,
                    Description = "Producer who did not previously meet thresholds has joined a group (% obligation)."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.D,
                    Code = "04",
                    LegacyCode = "D",
                    ClassificationId = DbConstants.CodeClassificationLookup.Leaver,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = true,
                    RequiresRegType = true,
                    MatchType = "Direct",
                    MappedOldCodes = "C,D",
                    Enabled = true,
                    Description = "Producer left group; HC responsible for obligations due to MYC."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.E,
                    Code = "05",
                    LegacyCode = "E",
                    ClassificationId = DbConstants.CodeClassificationLookup.Leaver,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = true,
                    RequiresRegType = true,
                    MatchType = "Direct",
                    MappedOldCodes = "C,D",
                    Enabled = true,
                    Description = "Producer left group; HC responsible for obligations due to MYC."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.F,
                    Code = "06",
                    LegacyCode = "F",
                    ClassificationId = DbConstants.CodeClassificationLookup.Leaver,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = true,
                    RequiresRegType = true,
                    MatchType = "Direct",
                    MappedOldCodes = "None",
                    Enabled = true,
                    Description = "Producer meets thresholds; HC still responsible post-MYC."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.G,
                    Code = "07",
                    LegacyCode = "G",
                    ClassificationId = DbConstants.CodeClassificationLookup.Joiner,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = false,
                    RequiresRegType = true,
                    MatchType = "Direct",
                    MappedOldCodes = "None",
                    Enabled = true,
                    Description = "Producer joined group; HC not responsible due to MYC."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.H,
                    Code = "08",
                    LegacyCode = "H",
                    ClassificationId = DbConstants.CodeClassificationLookup.Leaver,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = true,
                    RequiresRegType = true,
                    MatchType = "Direct",
                    MappedOldCodes = "None",
                    Enabled = true,
                    Description = "Producer left group; HC still responsible due to MYC."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.I,
                    Code = "09",
                    LegacyCode = "I",
                    ClassificationId = DbConstants.CodeClassificationLookup.Joiner,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = false,
                    RequiresRegType = true,
                    MatchType = "Direct",
                    MappedOldCodes = "None",
                    Enabled = true,
                    Description = "Producer joined group; not obligated due to MYC."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.J,
                    Code = "10",
                    LegacyCode = "J",
                    ClassificationId = DbConstants.CodeClassificationLookup.Leaver,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = true,
                    RequiresRegType = true,
                    MatchType = "Direct",
                    MappedOldCodes = "J",
                    Enabled = true,
                    Description = "Producer left group; HC still responsible due to MYC."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.K,
                    Code = "11",
                    LegacyCode = "K",
                    ClassificationId = DbConstants.CodeClassificationLookup.Leaver,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = true,
                    RequiresRegType = true,
                    MatchType = "ManualReview",
                    MappedOldCodes = "A,B,H",
                    Enabled = true,
                    Description = "No longer obligated - insolvency event."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.L,
                    Code = "12",
                    LegacyCode = "L",
                    ClassificationId = DbConstants.CodeClassificationLookup.Leaver,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = true,
                    RequiresRegType = true,
                    MatchType = "ManualReview",
                    MappedOldCodes = "L",
                    Enabled = true,
                    Description = "Ceased performing producer function."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.M,
                    Code = "13",
                    LegacyCode = "M",
                    ClassificationId = DbConstants.CodeClassificationLookup.Leaver,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = true,
                    RequiresRegType = true,
                    MatchType = "ManualReview",
                    MappedOldCodes = "E",
                    Enabled = true,
                    Description = "Producer resigned from compliance scheme."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.N,
                    Code = "14",
                    LegacyCode = "N",
                    ClassificationId = DbConstants.CodeClassificationLookup.Leaver,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = true,
                    RequiresRegType = true,
                    MatchType = "ManualReview",
                    MappedOldCodes = "F",
                    Enabled = true,
                    Description = "CS terminated producer’s membership."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.O,
                    Code = "15",
                    LegacyCode = "O",
                    ClassificationId = DbConstants.CodeClassificationLookup.Joiner,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = false,
                    RequiresRegType = true,
                    MatchType = "ManualReview",
                    MappedOldCodes = "None",
                    Enabled = true,
                    Description = "Became a producer due to mid-year change."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.P,
                    Code = "16",
                    LegacyCode = "P",
                    ClassificationId = DbConstants.CodeClassificationLookup.Leaver,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = true,
                    RequiresRegType = true,
                    MatchType = "ManualReview",
                    MappedOldCodes = "I",
                    Enabled = true,
                    Description = "Merged with another company - not incapacity related."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.Q,
                    Code = "17",
                    LegacyCode = "Q",
                    ClassificationId = DbConstants.CodeClassificationLookup.Joiner,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = false,
                    RequiresRegType = true,
                    MatchType = "ManualReview",
                    MappedOldCodes = "A,B,H",
                    Enabled = true,
                    Description = "Person becomes a producer as a result of carrying on the activities of an incapacitated producer."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.R,
                    Code = "18",
                    LegacyCode = "",
                    ClassificationId = DbConstants.CodeClassificationLookup.Joiner,
                    RequiresJoinerDate = true,
                    RequiresLeaverDate = false,
                    RequiresRegType = false,
                    MatchType = "ManualReview",
                    MappedOldCodes = "",
                    Enabled = true,
                    Description = "Producer who meets thresholds independently has left group. Holding company remains responsible for obligation due to mid year change."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.S,
                    Code = "19",
                    LegacyCode = "",
                    ClassificationId = DbConstants.CodeClassificationLookup.Joiner,
                    RequiresJoinerDate = true,
                    RequiresLeaverDate = false,
                    RequiresRegType = true,
                    MatchType = "ManualReview",
                    MappedOldCodes = "",
                    Enabled = true,
                    Description = "Producer registered late."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.T,
                    Code = "20",
                    LegacyCode = "",
                    ClassificationId = DbConstants.CodeClassificationLookup.Joiner,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = false,
                    RequiresRegType = true,
                    MatchType = "ManualReview",
                    MappedOldCodes = "",
                    Enabled = true,
                    Description = "Other - Joiner."
                },
                new CodeStatusConfig
                {
                    Id = DbConstants.CodeStatusConfig.U,
                    Code = "21",
                    LegacyCode = "",
                    ClassificationId = DbConstants.CodeClassificationLookup.Leaver,
                    RequiresJoinerDate = false,
                    RequiresLeaverDate = false,
                    RequiresRegType = true,
                    MatchType = "ManualReview",
                    MappedOldCodes = "",
                    Enabled = true,
                    Description = "Other - Leaver."
                });
        });


        modelBuilder.Entity<CodeScenarioMapping>(entity =>
        {
            entity.Property(type => type.Id).ValueGeneratedNever();
            entity.HasData(
                new CodeScenarioMapping { Id = 1, CodeStatusConfigId = DbConstants.CodeStatusConfig.A, ScenarioReferenceId = DbConstants.ScenarioReference.SR1, Active = true },
                new CodeScenarioMapping { Id = 2, CodeStatusConfigId = DbConstants.CodeStatusConfig.A, ScenarioReferenceId = DbConstants.ScenarioReference.SR2, Active = true },
                new CodeScenarioMapping { Id = 3, CodeStatusConfigId = DbConstants.CodeStatusConfig.B, ScenarioReferenceId = DbConstants.ScenarioReference.SR3, Active = true },
                new CodeScenarioMapping { Id = 4, CodeStatusConfigId = DbConstants.CodeStatusConfig.C, ScenarioReferenceId = DbConstants.ScenarioReference.SR4, Active = true },
                new CodeScenarioMapping { Id = 5, CodeStatusConfigId = DbConstants.CodeStatusConfig.D, ScenarioReferenceId = DbConstants.ScenarioReference.SR6, Active = true },
                new CodeScenarioMapping { Id = 6, CodeStatusConfigId = DbConstants.CodeStatusConfig.E, ScenarioReferenceId = DbConstants.ScenarioReference.SR7, Active = true },
                new CodeScenarioMapping { Id = 7, CodeStatusConfigId = DbConstants.CodeStatusConfig.F, ScenarioReferenceId = DbConstants.ScenarioReference.SR5, Active = true },
                new CodeScenarioMapping { Id = 8, CodeStatusConfigId = DbConstants.CodeStatusConfig.G, ScenarioReferenceId = DbConstants.ScenarioReference.SR8, Active = true },
                new CodeScenarioMapping { Id = 9, CodeStatusConfigId = DbConstants.CodeStatusConfig.H, ScenarioReferenceId = DbConstants.ScenarioReference.SR9, Active = true },
                new CodeScenarioMapping { Id = 10, CodeStatusConfigId = DbConstants.CodeStatusConfig.I, ScenarioReferenceId = DbConstants.ScenarioReference.SR10, Active = true },
                new CodeScenarioMapping { Id = 11, CodeStatusConfigId = DbConstants.CodeStatusConfig.J, ScenarioReferenceId = DbConstants.ScenarioReference.SR11, Active = true },
                new CodeScenarioMapping { Id = 12, CodeStatusConfigId = DbConstants.CodeStatusConfig.K, ScenarioReferenceId = DbConstants.ScenarioReference.SR12, Active = true },
                new CodeScenarioMapping { Id = 13, CodeStatusConfigId = DbConstants.CodeStatusConfig.L, ScenarioReferenceId = DbConstants.ScenarioReference.SR12, Active = true },
                new CodeScenarioMapping { Id = 14, CodeStatusConfigId = DbConstants.CodeStatusConfig.M, ScenarioReferenceId = DbConstants.ScenarioReference.SR13, Active = true },
                new CodeScenarioMapping { Id = 15, CodeStatusConfigId = DbConstants.CodeStatusConfig.N, ScenarioReferenceId = DbConstants.ScenarioReference.SR14, Active = true },
                new CodeScenarioMapping { Id = 16, CodeStatusConfigId = DbConstants.CodeStatusConfig.O, ScenarioReferenceId = DbConstants.ScenarioReference.SR18, Active = true },
                new CodeScenarioMapping { Id = 17, CodeStatusConfigId = DbConstants.CodeStatusConfig.P, ScenarioReferenceId = DbConstants.ScenarioReference.SR16, Active = true },
                new CodeScenarioMapping { Id = 18, CodeStatusConfigId = DbConstants.CodeStatusConfig.Q, ScenarioReferenceId = DbConstants.ScenarioReference.SR17, Active = true },
                new CodeScenarioMapping { Id = 19, CodeStatusConfigId = DbConstants.CodeStatusConfig.R, ScenarioReferenceId = DbConstants.ScenarioReference.SR18, Active = true },
                new CodeScenarioMapping { Id = 20, CodeStatusConfigId = DbConstants.CodeStatusConfig.S, ScenarioReferenceId = DbConstants.ScenarioReference.SR19, Active = true },
                new CodeScenarioMapping { Id = 21, CodeStatusConfigId = DbConstants.CodeStatusConfig.T, ScenarioReferenceId = DbConstants.ScenarioReference.SR20, Active = true },
                new CodeScenarioMapping { Id = 22, CodeStatusConfigId = DbConstants.CodeStatusConfig.U, ScenarioReferenceId = DbConstants.ScenarioReference.SR21, Active = true });
        });

        modelBuilder.Entity<PartnerRole>(entity =>
        {
            entity.Property(s => s.Id).ValueGeneratedNever();
            entity.HasData(
                new PartnerRole { Id = DbConstants.PartnerRoleIds.NotSet, Name = "Not Set" },
                new PartnerRole { Id = DbConstants.PartnerRoleIds.IndividualPartner, Name = "Individual Partner" },
                new PartnerRole { Id = DbConstants.PartnerRoleIds.CompanyPartner, Name = "Corporate Partner" });
        });
    }

    public int SaveChanges(Guid userId, Guid organisationId)
        => SaveChanges(userId, organisationId, null);

    public int SaveChanges(string serviceId)
        => SaveChanges(null, null, serviceId);

    private int SaveChanges(Guid? userId, Guid? organisationId, string? serviceId)
    {
        var dynamicAuditEntries = HandleAuditBeforeSave(userId, organisationId, serviceId, DateTimeOffset.Now);
        if (dynamicAuditEntries.Count > 0)
        {
            return TwoPhaseTransactionSaveChanges(dynamicAuditEntries);
        }

        return base.SaveChanges();
    }

    private int TwoPhaseTransactionSaveChanges(IEnumerable<AuditEntry> auditEntries)
    {
        if (Database.CurrentTransaction is null)
        {
            IExecutionStrategy strategy = Database.CreateExecutionStrategy();
            return strategy.Execute(() =>
            {
                using var transaction = Database.BeginTransaction();
                var result = TwoPhaseSaveChanges(auditEntries);
                transaction.Commit();
                return result;
            });
        }
        return TwoPhaseSaveChanges(auditEntries);
    }

    private int TwoPhaseSaveChanges(IEnumerable<AuditEntry> auditEntries)
    {
        var result = base.SaveChanges();
        HandleAuditAfterSave(auditEntries);
        base.SaveChanges();
        return result;
    }

    [Obsolete("Use the audited version of SaveChangesAsync instead.", true)]
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Use the audited version of SaveChangesAsync instead.");
    }

    public async Task<int> SaveChangesAsync(Guid userId, string serviceId, CancellationToken cancellationToken = default)
        => await SaveChangesAsync(userId, null, serviceId, cancellationToken);

    public async Task<int> SaveChangesAsync(Guid userId, Guid organisationId, CancellationToken cancellationToken = default)
        => await SaveChangesAsync(userId, organisationId, null, cancellationToken);

    public async Task<int> SaveChangesAsync(string serviceId, CancellationToken cancellationToken = default)
        => await SaveChangesAsync(null, null, serviceId, cancellationToken);

    public async Task<int> SaveChangesAsync(Guid? userId, Guid? organisationId, string? serviceId, CancellationToken cancellationToken = default)
    {
        var dynamicAuditEntries = HandleAuditBeforeSave(userId, organisationId, serviceId, DateTimeOffset.UtcNow);
        if (dynamicAuditEntries.Count > 0)
        {
            return await TwoPhaseTransactionSaveChangesAsync(dynamicAuditEntries, cancellationToken);
        }
        // if not doing two-phase, this will use the configured retry strategy if no ambient transaction exists
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> TwoPhaseTransactionSaveChangesAsync(IEnumerable<AuditEntry> auditEntries, CancellationToken cancellationToken)
    {
        if (Database.CurrentTransaction is null)
        {
            IExecutionStrategy strategy = Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async (cancellationToken) =>
            {
                await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await TwoPhaseSaveChangesAsync(auditEntries, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    // explicitly roll back the transaction honoring the cancellation token
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }, cancellationToken);
        }
        return await TwoPhaseSaveChangesAsync(auditEntries, cancellationToken);
    }

    private async Task<int> TwoPhaseSaveChangesAsync(IEnumerable<AuditEntry> auditEntries, CancellationToken cancellationToken)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        HandleAuditAfterSave(auditEntries);
        await base.SaveChangesAsync(cancellationToken);
        return result;
    }

    private ICollection<AuditEntry> HandleAuditBeforeSave(Guid? userId, Guid? organisationId, string? serviceId, DateTimeOffset timestamp)
    {
        var changedEntries = ChangeTracker.Entries<IEditableEntity>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (var entry in changedEntries)
        {
            entry.Property("LastUpdatedOn").CurrentValue = timestamp;

            if (entry.State is EntityState.Added)
            {
                entry.Property("CreatedOn").CurrentValue = timestamp;
            }
        }

        var auditEntries = ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => new AuditEntry(entry, userId, organisationId, serviceId, timestamp))
            .ToList();

        var dynamicAuditEntries = new List<AuditEntry>();

        foreach (var auditEntry in auditEntries)
        {
            if (auditEntry.HasGeneratedProperties())
            {
                dynamicAuditEntries.Add(auditEntry);
            }
            else
            {
                AuditLogs.Add(auditEntry.ToAuditLog());
            }
        }

        return dynamicAuditEntries;
    }

    private void HandleAuditAfterSave(IEnumerable<AuditEntry> auditEntries)
    {
        foreach (var auditEntry in auditEntries)
        {
            auditEntry.UpdateGeneratedProperties();
            AuditLogs.Add(auditEntry.ToAuditLog());
        }
    }

    private static ComplianceScheme[] PopulateComplianceSchemes() =>
        new[]
        {
            new ComplianceScheme
            {
                Id = 1,
                Name = "Sustain Drinks Packaging Partnership",
                CompaniesHouseNumber = "09100134",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 2,
                Name = "Biffpack",
                CompaniesHouseNumber = "00946107",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 3,
                Name = "Biffpack (Environment Agency)",
                CompaniesHouseNumber = "00946107",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 4,
                Name = "Comply with Clarity",
                CompaniesHouseNumber = "04559478",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 5,
                Name = "Comply with Clarity (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "04559478",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 6,
                Name = "Co2 Compliance (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "SC331930",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 7,
                Name = "Co2 Compliance (SEPA)",
                CompaniesHouseNumber = "SC331930",
                NationId = 3
            },
            new ComplianceScheme
            {
                Id = 8,
                Name = "Beyondly",
                CompaniesHouseNumber = "05695937",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 9,
                Name = "Comply Direct (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "05695937",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 10,
                Name = "ComplyPak",
                CompaniesHouseNumber = "04164355",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 11,
                Name = "ComplyPak (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "04164355",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 12,
                Name = "Ecosurety (EA)",
                CompaniesHouseNumber = "04713606",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 13,
                Name = "Ecosurety (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "04713606",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 14,
                Name = "Ecosurety Scotland (SEPA)",
                CompaniesHouseNumber = "SC499366",
                NationId = 3
            },
            new ComplianceScheme
            {
                Id = 15,
                Name = "ERP UK Ltd",
                CompaniesHouseNumber = "05859725",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 16,
                Name = "ERP UK Ltd Northern Ireland Environment Agency",
                CompaniesHouseNumber = "05859725",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 17,
                Name = "Kite Environmental Solutions",
                CompaniesHouseNumber = "04592964",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 18,
                Name = "Kite Environmental Solutions",
                CompaniesHouseNumber = "04592964",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 19,
                Name = "Nipak Ltd (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "NI044560",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 20,
                Name = "Scotpak (SEPA)",
                CompaniesHouseNumber = "SC300070",
                NationId = 3
            },
            new ComplianceScheme
            {
                Id = 21,
                Name = "Paperpak",
                CompaniesHouseNumber = "07699232",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 22,
                Name = "Paperpak (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "07699232",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 23,
                Name = "Veolia (EA)",
                CompaniesHouseNumber = "03417947",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 24,
                Name = "Veolia (SEPA)",
                CompaniesHouseNumber = "SC394249",
                NationId = 3
            },
            new ComplianceScheme
            {
                Id = 25,
                Name = "Recycle Wales (NRW)",
                CompaniesHouseNumber = "04543366",
                NationId = 4
            },
            new ComplianceScheme
            {
                Id = 26,
                Name = "Recycle-Pak (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "SC225789",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 27,
                Name = "Recycle-Pak (Scottish Environment Protection Agency)",
                CompaniesHouseNumber = "SC225789",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 28,
                Name = "Recycling Lives Compliance Services",
                CompaniesHouseNumber = "04748329",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 29,
                Name = "REPIC",
                CompaniesHouseNumber = "06355083",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 30,
                Name = "REPIC NI",
                CompaniesHouseNumber = "06355083",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 31,
                Name = "Smart Comply (EA)",
                CompaniesHouseNumber = "04015442",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 32,
                Name = "Smart Comply (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "04015442",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 33,
                Name = "Synergy Compliance",
                CompaniesHouseNumber = "04835772",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 34,
                Name = "Synergy Compliance (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "04835772",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 35,
                Name = "Ethical Compliance",
                CompaniesHouseNumber = "06929701",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 36,
                Name = "Pennine-Pack",
                CompaniesHouseNumber = "06929701",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 37,
                Name = "Pennine-Pack (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "06929701",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 38,
                Name = "Wastepack (Scottish Environment Protection Agency)",
                CompaniesHouseNumber = "03985811",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 39,
                Name = "Wastepack UK (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "03985811",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 40,
                Name = "Valpak",
                CompaniesHouseNumber = "07688691",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 41,
                Name = "Valpak (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "07688691",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 42,
                Name = "Valpak Scotland Ltd (SEPA)",
                CompaniesHouseNumber = "SC245145",
                NationId = 3
            },
            new ComplianceScheme
            {
                Id = 43,
                Name = "Veolia Environmental Services (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "02215767",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 44,
                Name = "Packcare",
                CompaniesHouseNumber = "06043169",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 45,
                Name = "Compliance Link (Scottish Environment Protection Agency)",
                CompaniesHouseNumber = "SC174113",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 46,
                Name = "SWS Compak",
                CompaniesHouseNumber = "SC174113",
                IsDeleted = true,
                NationId = null
            },
            new ComplianceScheme
            {
                Id = 47,
                Name = "Wastepack GB (SEPA)",
                CompaniesHouseNumber = "SC174113",
                NationId = 3
            },
            new ComplianceScheme
            {
                Id = 48,
                Name = "Enpack",
                CompaniesHouseNumber = "04168907",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 49,
                Name = "Leafpak",
                CompaniesHouseNumber = "SC300070",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 50,
                Name = "Wastepack (EA)",
                CompaniesHouseNumber = "03985811",
                NationId = 1
            },
            new ComplianceScheme
            {
                Id = 51,
                Name = "Beyondly (NIEA)",
                CompaniesHouseNumber = "05695937",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 52,
                Name = "BiffPack (NIEA)",
                CompaniesHouseNumber = "00946107",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 53,
                Name = "Comply with Clarity (NIEA)",
                CompaniesHouseNumber = "04559478",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 54,
                Name = "Co2 Compliance (NIEA)",
                CompaniesHouseNumber = "SC331930",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 55,
                Name = "ComplyPak (NIEA)",
                CompaniesHouseNumber = "04164355",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 56,
                Name = "Ecosurety (NIEA)",
                CompaniesHouseNumber = "04713606",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 57,
                Name = "ERP (NIEA)",
                CompaniesHouseNumber = "05859725",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 58,
                Name = "Kite Environmental Solutions (NIEA)",
                CompaniesHouseNumber = "04592964",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 59,
                Name = "Leafpak (NIEA)",
                CompaniesHouseNumber = "SC300070",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 60,
                Name = "Paperpak (NIEA)",
                CompaniesHouseNumber = "07699232",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 61,
                Name = "Veolia (NIEA)",
                CompaniesHouseNumber = "03417947",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 62,
                Name = "Recycling Lives Compliance Services (NIEA)",
                CompaniesHouseNumber = "04748329",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 63,
                Name = "REPIC (NIEA)",
                CompaniesHouseNumber = "06355083",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 64,
                Name = "Smart Comply (NIEA)",
                CompaniesHouseNumber = "04015442",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 65,
                Name = "Synergy Compliance (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "04835772",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 66,
                Name = "Pennine-Pack (Northern Ireland Environment Agency)",
                CompaniesHouseNumber = "06929701",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 67,
                Name = "Wastepack UK (NIEA)",
                CompaniesHouseNumber = "03985811",
                NationId = 2
            },
            new ComplianceScheme
            {
                Id = 68,
                Name = "Valpak (NIEA)",
                CompaniesHouseNumber = "07688691",
                NationId = 2
            }
        };
}