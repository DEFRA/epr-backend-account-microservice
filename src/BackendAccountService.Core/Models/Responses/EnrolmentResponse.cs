using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public class EnrolmentResponse
{
    public string ServiceRole { get; set; }
    public EnrolmentStatus EnrolmentStatus { get; set; }
}