using BackendAccountService.Core.Models;
using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.Services;

public interface IAccountService
{
    Task<ServiceRole?> GetServiceRoleAsync(string serviceRoleKey);
    Task<Enrolment> AddAccountAsync(AccountModel account, ServiceRole serviceRole);
}
