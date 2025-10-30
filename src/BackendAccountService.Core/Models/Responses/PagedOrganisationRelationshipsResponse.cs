namespace BackendAccountService.Core.Models.Responses;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class PagedOrganisationRelationshipsResponse
{
    public List<RelationshipResponseModel> Items { get; set; }
    public int CurrentPage { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; }
    public List<string> SearchTerms { get; set; }
}
