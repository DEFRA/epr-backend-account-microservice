namespace BackendAccountService.Core.Models.Responses;

public record ComplianceSchemeDto(int RowNumber, Guid Id, string Name, DateTimeOffset CreatedOn, int? NationId);