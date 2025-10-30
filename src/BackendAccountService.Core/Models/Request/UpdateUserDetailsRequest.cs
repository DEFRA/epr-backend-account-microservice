using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Request
{
    /// <summary>
    /// User details that can be updated
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UpdateUserDetailsRequest
    {
        /// <summary>
        /// User FirstName
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User LastName
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// User Jobtitle in an organisation
        /// </summary>
        public string? JobTitle { get; set; }

        /// <summary>
        /// User Telephone
        /// </summary>
        public string? Telephone { get; set; }
    }
}
