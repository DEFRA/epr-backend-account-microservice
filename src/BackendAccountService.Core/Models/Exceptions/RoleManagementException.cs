using System.Runtime.Serialization;

namespace BackendAccountService.Core.Models.Exceptions;

[Serializable]
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

    protected RoleManagementException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

