using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BackendAccountService.Data.Entities.Conversions;

public class DelegatedPersonEnrolmentConfig : IEntityTypeConfiguration<DelegatedPersonEnrolment>
{
    public void Configure(EntityTypeBuilder<DelegatedPersonEnrolment> builder)
    {
        builder
            .Property(data => data.RelationshipType)
            .HasConversion(new EnumToStringConverter<RelationshipType>());
    }
}
