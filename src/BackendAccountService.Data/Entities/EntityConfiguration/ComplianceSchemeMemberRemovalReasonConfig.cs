using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendAccountService.Data.Entities.EntityConfiguration;

public class ComplianceSchemeMemberRemovalReasonConfig : IEntityTypeConfiguration<ComplianceSchemeMemberRemovalReason>
{ 
    public void Configure(EntityTypeBuilder<ComplianceSchemeMemberRemovalReason> builder)
    {   
        builder.HasData( 
        new ComplianceSchemeMemberRemovalReason
        {
            Id = 1,
            Code = "A",
            Name = "It won't be able to give us its organisation details or packaging data by the deadline",
            RequiresReason = false,
            DisplayOrder = 1,
            IsActive = true
        },
        new ComplianceSchemeMemberRemovalReason
        {
            Id = 2,
            Code = "B",
            Name = "The amount of packaging it handled in the threshold calculation year is less than or equal to 25 tonnes",
            RequiresReason = false,
            DisplayOrder = 2,
            IsActive = true
        },
        new ComplianceSchemeMemberRemovalReason
        {
            Id = 3,
            Code = "C",
            Name = "It has ended its membership with this scheme",
            RequiresReason = false,
            DisplayOrder = 3,
            IsActive = true
        },
        new ComplianceSchemeMemberRemovalReason
        {
            Id = 4,
            Code = "D",
            Name = "It has merged with another company",
            RequiresReason = false,
            DisplayOrder = 4,
            IsActive = true
        },
        new ComplianceSchemeMemberRemovalReason
        {
            Id = 5,
            Code = "E",
            Name = "It is no longer in operation",
            RequiresReason = true,
            DisplayOrder = 5,
            IsActive = true
        },
        new ComplianceSchemeMemberRemovalReason
        {
            Id = 6,
            Code = "F",
            Name = "It has gone into administration",
            RequiresReason = false,
            DisplayOrder = 6,
            IsActive = true
        },
        new ComplianceSchemeMemberRemovalReason
        {
            Id = 7,
            Code = "G",
            Name = "It is now a subsidiary of another company",
            RequiresReason = false,
            DisplayOrder = 7,
            IsActive = true
        },
        new ComplianceSchemeMemberRemovalReason
        {
            Id = 8,
            Code = "H",
            Name = "Its turnover in the last financial year before the relevant date is less than or equal to £1 million",
            RequiresReason = false,
            DisplayOrder = 8,
            IsActive = true
        },
        new ComplianceSchemeMemberRemovalReason
        {
            Id = 9,
            Code = "I",
            Name = "We've ended its membership with this scheme",
            RequiresReason = false,
            DisplayOrder = 9,
            IsActive = true
        },
        new ComplianceSchemeMemberRemovalReason
        {
            Id = 10,
            Code = "J",
            Name = "None of the above",
            RequiresReason = true,
            DisplayOrder = 10,
            IsActive = true
        });
    }
}
