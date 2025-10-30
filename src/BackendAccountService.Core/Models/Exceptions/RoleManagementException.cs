using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Exceptions;

[ExcludeFromCodeCoverage]
public class RoleManagementException : Exception
{
    public RoleManagementException()
    {
    }

    public RoleManagementException(string message)
        : base(message)
    {
    }

    public RoleManagementException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}