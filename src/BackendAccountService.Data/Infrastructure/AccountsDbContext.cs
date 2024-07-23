using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Entities.Conversions;
using BackendAccountService.Data.Entities.EntityConfiguration;
using Microsoft.EntityFrameworkCore;
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

    public DbSet<Person> Persons { get; set; } = null!;

    public DbSet<PersonInOrganisationRole> PersonInOrganisationRoles { get; set; } = null!;

    public DbSet<PersonOrganisationConnection> PersonOrganisationConnections { get; set; } = null!;

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

    public DbSet<ComplianceSchemeMemberRemovalAuditLogsReason>ComplianceSchemeMemberRemovalAuditLogsReasons{ get; set; }

    public AccountsDbContext(DbContextOptions<AccountsDbContext> options)
        : base(options)
    {
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

            entity.HasOne(relationtype => relationtype.OrganisationRelationshipType)
                .WithMany()
                .HasForeignKey(connection => connection.OrganisationRelationshipTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(relationtype => relationtype.OrganisationRegistrationType)
                .WithMany()
                .HasForeignKey(connection => connection.OrganisationRegistrationTypeId)
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
                new Nation { Id = DbConstants.Nation.NotSet, Name = "Not Set" },
                new Nation { Id = DbConstants.Nation.England, Name = "England" },
                new Nation { Id = DbConstants.Nation.NorthernIreland, Name = "Northern Ireland" },
                new Nation { Id = DbConstants.Nation.Scotland, Name = "Scotland" },
                new Nation { Id = DbConstants.Nation.Wales, Name = "Wales" });
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
                new OrganisationType {Id = DbConstants.OrganisationType.NotSet, Name = "Not Set"},
                new OrganisationType
                    {Id = DbConstants.OrganisationType.CompaniesHouseCompany, Name = "Companies House Company"},
                new OrganisationType
                    {Id = DbConstants.OrganisationType.NonCompaniesHouseCompany, Name = "Non Companies House Company"},
                new OrganisationType
                    {Id = DbConstants.OrganisationType.WasteCollectionAuthority, Name = "Waste Collection Authority"},
                new OrganisationType
                    {Id = DbConstants.OrganisationType.WasteDisposalAuthority, Name = "Waste Disposal Authority"},
                new OrganisationType
                {
                    Id = DbConstants.OrganisationType.WasteCollectionAuthorityWasteDisposalAuthority,
                    Name = "Waste Collection Authority & Waste Disposal Authority"
                },
                new OrganisationType {Id = DbConstants.OrganisationType.Regulators, Name = "Regulators"});
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
                new ProducerType { Id = DbConstants.ProducerType.Other, Name = "Other" });
        });

        modelBuilder.Entity<PersonInOrganisationRole>(entity =>
        {
            entity.Property(role => role.Id).ValueGeneratedNever();
            entity.HasData(
                new PersonInOrganisationRole { Id = DbConstants.PersonRole.NotSet, Name = "Not Set" },
                new PersonInOrganisationRole { Id = DbConstants.PersonRole.Admin, Name = "Admin" },
                new PersonInOrganisationRole { Id = DbConstants.PersonRole.Employee, Name = "Employee" }
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
					Id = DbConstants.Service.RegulatorEnrolement,
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
					ServiceId = DbConstants.Service.RegulatorEnrolement,
					Key = DbConstants.ServiceRole.Regulator.Admin.Key,
					Name = "Regulator Admin",
					Description = "Regulator Admin Service Role"
				},
				new ServiceRole
				{
					Id = DbConstants.ServiceRole.Regulator.Basic.Id,
					ServiceId = DbConstants.Service.RegulatorEnrolement,
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
                new OrganisationRelationshipType { Id = DbConstants.OrganisationRelationshipType.Holding, Name = "Holding" },
                new OrganisationRelationshipType { Id = DbConstants.OrganisationRelationshipType.Subsidary, Name = "Subsidary" });
        });

        modelBuilder.Entity<OrganisationRegistrationType>(entity =>
        {
            entity.Property(type => type.Id).ValueGeneratedNever();
            entity.HasData(
                new OrganisationRegistrationType { Id = DbConstants.OrganisationRegistrationType.NotSet, Name = "Not Set" },
                new OrganisationRegistrationType { Id = DbConstants.OrganisationRegistrationType.Group, Name = "Group" },
                new OrganisationRegistrationType { Id = DbConstants.OrganisationRegistrationType.Individual, Name = "Individual" });
        });
    }

    [Obsolete("Use the audited version of SaveChanges instead.", true)]
    public override int SaveChanges()
    {
        throw new InvalidOperationException("Use the audited version of SaveChanges instead.");
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
            using var transaction = Database.BeginTransaction();
            var result = TwoPhaseSaveChanges(auditEntries);
            transaction.Commit();
            return result;
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

    public async Task<int> SaveChangesAsync(Guid userId, Guid organisationId, CancellationToken cancellationToken = default)
        => await SaveChangesAsync(userId, organisationId, null, cancellationToken);

    public async Task<int> SaveChangesAsync(string serviceId, CancellationToken cancellationToken = default)
        => await SaveChangesAsync(null, null, serviceId, cancellationToken);

    private async Task<int> SaveChangesAsync(Guid? userId, Guid? organisationId, string? serviceId, CancellationToken cancellationToken)
    {
        var dynamicAuditEntries = HandleAuditBeforeSave(userId, organisationId, serviceId, DateTimeOffset.Now);
        if (dynamicAuditEntries.Count > 0)
        {
            return await TwoPhaseTransactionSaveChangesAsync(dynamicAuditEntries, cancellationToken);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> TwoPhaseTransactionSaveChangesAsync(IEnumerable<AuditEntry> auditEntries, CancellationToken cancellationToken)
    {
        if (Database.CurrentTransaction is null)
        {
            await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
            var result = await TwoPhaseSaveChangesAsync(auditEntries, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
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