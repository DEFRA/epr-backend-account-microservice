namespace BackendAccountService.Core.Models.Responses;

public record ComplianceSchemeDto(Guid Id, string Name, DateTimeOffset CreatedOn, int? NationId);