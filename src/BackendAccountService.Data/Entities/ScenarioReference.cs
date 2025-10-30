using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Data.Entities
{
    [ExcludeFromCodeCoverage]
    public class ScenarioReference : DataEntity
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string ScenarioCode { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [MaxLength(50)]
        public string? ObligationFlag { get; set; }

        public bool? Active { get; set; }
    }
}