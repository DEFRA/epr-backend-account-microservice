using BackendAccountService.Data.Infrastructure;
using System.Collections.Immutable;
using BackendAccountService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Core.Services;

public class PartnerService(AccountsDbContext accountsDbContext) : IPartnerService
{
    // if we had a lot of partner roles, we could union all the role names sent down and only fetch the ones we need
    // but as there are only 2 (3 with not set), it's simpler to just load them all
    public async Task<IImmutableDictionary<string, PartnerRole>> GetPartnerRoles()
    {
        var partnerRolesDictionary = await accountsDbContext.PartnerRoles
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Name);

        return partnerRolesDictionary.ToImmutableDictionary();
    }
}