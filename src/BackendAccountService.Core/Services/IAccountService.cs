using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.Entities;
using System.Collections.Immutable;

namespace BackendAccountService.Core.Services;

public interface IAccountService
{
    Task<ServiceRole?> GetServiceRoleAsync(string serviceRoleKey);
    Task<Enrolment> AddAccountAsync(AccountModel account, ServiceRole serviceRole);
    Task AddApprovedUserAccountAsync(
        ApprovedUserAccountModel account,
        ServiceRole serviceRole,
        UserModel user);

    #region ReprocessorExporter

    /// <summary>
    /// Creates an initial Reprocessor/Exporter account consisting of Person & User.
    /// This could be reused for different types of account where you just need the Person/User,
    /// in which case it might make sense to rename it.
    /// </summary>
    Task<Person> AddReprocessorExporterAccountAsync(
        ReprocessorExporterAccount account,
        string serviceKey,
        Guid userId);

    Task<ReExAddOrganisationResponse> AddReprocessorExporterOrganisationAsync(
        ReprocessorExporterAddOrganisation account,
        Person person,
        IImmutableDictionary<string, PartnerRole> partnerRoles,
        string serviceKey,
        Guid userId);

    #endregion
}
