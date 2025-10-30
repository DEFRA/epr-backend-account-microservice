using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses
{
    public class PersonEmailResponseModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
