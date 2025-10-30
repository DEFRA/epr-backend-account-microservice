using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Data.Entities
{
    [ExcludeFromCodeCoverage]
    public class CodeStatusConfig : DataEntity
    {
        public int Id { get; set; }

        [MaxLength(2)]
        public string Code { get; set; }

        [MaxLength(1)]
        public string LegacyCode { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public int? ClassificationId { get; set; }

        public bool? RequiresJoinerDate { get; set; }

        public bool? RequiresLeaverDate { get; set; }

        public bool? RequiresRegType { get; set; }

        [MaxLength(50)]
        public string? MatchType { get; set; }

        [MaxLength(50)]
        public string? MappedOldCodes { get; set; }

        public bool? Enabled { get; set; }

        public CodeClassificationLookup? CodeClassificationLookup { get; set; }
    }
}