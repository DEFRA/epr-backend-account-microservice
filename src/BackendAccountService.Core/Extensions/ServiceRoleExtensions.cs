using BackendAccountService.Data.DbConstants;
using static BackendAccountService.Data.DbConstants.ServiceRole.Packaging;

namespace BackendAccountService.Core.Extensions;

public static class ServiceRoleExtensions
{
    public static string? GetHighestServiceRole(string[] roles, int serviceId)
    {
        Dictionary<string, int> serviceRoleHierarchy;
        switch (serviceId)
        {
            case Service.RegulatorEnrolment:
                serviceRoleHierarchy = new Dictionary<string, int>
                {
                    { ServiceRole.Regulator.Basic.Key, 1 },
                    { ServiceRole.Regulator.Admin.Key, 2 },
                };
                break;
            default:
                serviceRoleHierarchy = new Dictionary<string, int>
                {
                    { BasicUser.Key, 1 },
                    { DelegatedPerson.Key, 2 },
                    { ApprovedPerson.Key, 3 }
                };
                break;
        }

        return roles.Where(r => serviceRoleHierarchy.ContainsKey(r)).MaxBy(role => serviceRoleHierarchy[role]);
    }
    
    public static string[] GetAuthorizedRolesToRemoveUser(string? highestServiceRole, int serviceId)
    {
        return (highestServiceRole, serviceId) switch
        {
            (_, Service.RegulatorEnrolment) => new [] { ServiceRole.Regulator.Admin.Key },
            (DelegatedPerson.Key, _) => new[] { ApprovedPerson.Key },
            (BasicUser.Key, _) => new [] { ApprovedPerson.Key, DelegatedPerson.Key, BasicUser.Key },
            (ApprovedPerson.Key, _) => Array.Empty<string>(),
            _ => Array.Empty<string>()
        };
    }
}