using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.ValidationData.Api.Config;

[ExcludeFromCodeCoverage]
public class AccountsDatabaseConfig
{
    public const string Section = "AccountsDatabase";
    public string ConnectionString { get; set; }
}