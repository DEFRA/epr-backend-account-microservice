namespace BackendAccountService.Core.Models.Mappings
{
    public static class PersonRoleMappings
    {
        public static int GetPersonRoleId(PersonRole personRole)
        {
            return personRole switch
            {
                PersonRole.Admin => Data.DbConstants.PersonRole.Admin,
                PersonRole.Employee => Data.DbConstants.PersonRole.Employee,
                _ => throw new ArgumentException($"Unrecognised Person Role: '{personRole}'")
            };
        }

        public static PersonRole MapPersonRole(int personRoleId)
        {
            return personRoleId switch
            {
                Data.DbConstants.PersonRole.Admin => PersonRole.Admin,
                Data.DbConstants.PersonRole.Employee => PersonRole.Employee,
                _ => throw new ArgumentException($"Not recognised personRoleId: '{personRoleId}'")
            };
        }
    }
}