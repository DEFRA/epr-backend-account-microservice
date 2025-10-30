using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Data.Entities
{
    [ExcludeFromCodeCoverage]
    public class CodeClassificationLookup : DataEntity
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string CodeClass { get; set; }

        [MaxLength(100)]
        public string Description { get; set; }

        [MaxLength(50)]
        public string GroupType { get; set; }
    }
}