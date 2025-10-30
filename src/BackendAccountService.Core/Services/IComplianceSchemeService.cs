namespace BackendAccountService.Core.Services;

using Models.Responses;
using Data.Entities;
using Models;
using Models.Request;
using Models.Result;
using BackendAccountService.Core.Models;

public interface IComplianceSchemeService
{
    Task<Result<ComplianceSchemeMembershipResponse>> GetComplianceSchemeMembersAsync(Guid organisationId, Guid complianceSchemeId, string? query, int pageSize, int page, bool hideNoSubsidiaries);
    Task<Result> RemoveComplianceSchemeAsync(RemoveComplianceSchemeRequest removeComplianceScheme);
    Task<Result<SelectedScheme>> SelectComplianceSchemeAsync(SelectComplianceSchemeRequest request);
    Task<Result<SelectedScheme>> UpdateSelectedComplianceSchemeAsync(UpdateSelectedComplianceSchemeRequest request);
    Task<IEnumerable<ComplianceSchemeDto>> GetAllComplianceSchemesAsync();
    Task<Result<ProducerComplianceSchemeDto>> GetComplianceSchemeForProducer(Guid organisationId);
    Task<IEnumerable<ComplianceSchemeDto>> GetComplianceSchemesForOperatorAsync(Guid organisationId);
    Task<Result<ComplianceSchemeMemberDetailDto>> GetComplianceSchemeMemberDetailsAsync(Guid organisationId, Guid selectedSchemeId);
    Task<ComplianceSchemeSummary?> GetComplianceSchemeSummary(Guid organisationId, Guid complianceSchemeId);
    Task<IEnumerable<ComplianceSchemeRemovalReasonResponse>> GetComplianceSchemeReasonsForRemovalAsync();
    Task<Result<InfoForSelectedSchemeRemoval>> GetInfoForSelectedSchemeRemoval(Guid organisationId, Guid selectedSchemeId, Guid userId);
    Task<Result<RemoveComplianceSchemeMemberResponse>> RemoveComplianceSchemeMember(Guid organisationId, Guid selectedSchemeId, Guid userId, RemoveComplianceSchemeMemberRequest request);
    Task<List<ExportOrganisationSubsidiariesResponseModel>> ExportComplianceSchemeSubsidiaries(Guid userId, Guid organisationId, Guid complianceSchemeId);
}
