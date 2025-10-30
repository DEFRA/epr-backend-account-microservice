using BackendAccountService.Core.Models;
using BackendAccountService.Data.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Core.Extensions;

public static class IQueryableExtensions
{    public static IQueryable<ExportOrganisationSubsidiariesResponseModel> GetCombinedParentChildQuery(this IQueryable<ExportOrganisationSubsidiariesQueryModel> query)
    {
        var parentOnly =
            from p in query
            group p by new { p.OrganisationId, p.ParentOrganisationName, p.ParentCompaniesHouseNumber } into g
            select new ExportOrganisationSubsidiariesResponseModel
            {
                OrganisationId = g.Key.OrganisationId,
                SubsidiaryId = null,
                OrganisationName = g.Key.ParentOrganisationName,
                CompaniesHouseNumber = g.Key.ParentCompaniesHouseNumber,
                JoinerDate = null
            };

        var parentWithChildren =
            from p in query
            where p.SubsidiaryId != null && p.OrganisationId != p.SubsidiaryId
            select new ExportOrganisationSubsidiariesResponseModel
            {
                OrganisationId = p.OrganisationId,
                SubsidiaryId = p.SubsidiaryId,
                OrganisationName = p.ChildOrganisationName,
                CompaniesHouseNumber = p.ChildCompaniesHouseNumber,
                JoinerDate = p.ChildJoinerDate
            };

        return parentOnly.Union(parentWithChildren)
            .OrderBy(p => p.OrganisationId)
            .ThenBy(p => p.SubsidiaryId)
            .AsNoTracking()
            .WithRecompile();
    }
}
