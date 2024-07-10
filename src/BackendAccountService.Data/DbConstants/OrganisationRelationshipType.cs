using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BackendAccountService.Data.DbConstants;

public static class OrganisationRelationshipType
{
    public const int NotSet = 0;
    public const int Parent = 1;
    public const int Holding = 2;
    public const int Subsidary= 3;
}
