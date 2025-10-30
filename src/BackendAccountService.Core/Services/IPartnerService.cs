using System.Collections.Immutable;
using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.Services;

public interface IPartnerService
{
    Task<IImmutableDictionary<string, PartnerRole>> GetPartnerRoles();
}