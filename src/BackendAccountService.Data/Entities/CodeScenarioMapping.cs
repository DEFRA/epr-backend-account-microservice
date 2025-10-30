using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Data.Entities
{
    [ExcludeFromCodeCoverage]
    public class CodeScenarioMapping : DataEntity
    {
        public int Id { get; set; }

        public int CodeStatusConfigId { get; set; }

        public int ScenarioReferenceId { get; set; }

        public bool? Active { get; set; }

        public CodeStatusConfig CodeStatusConfig { get; set; }

        public ScenarioReference ScenarioReference { get; set; }
    }

}