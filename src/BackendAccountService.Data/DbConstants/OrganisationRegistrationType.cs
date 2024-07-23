using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BackendAccountService.Data.DbConstants;

public static class OrganisationRegistrationType
{
    public const int NotSet = 0;
    public const int Group = 1;
    public const int Individual = 2;
}