using System;
using System.Globalization;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;

namespace BackendAccountService.Api.Controllers;
public static class JoinerDateValueCheck
{
    public static DateTime? GetJoinerDate<T>(T request)
    {
        DateTime dateValue = DateTime.MinValue;
        var validDate = false;
        string[] formats = { "dd/MM/yyyy", "dd/MMM/yyyy" };

        if (typeof(T).Equals(typeof(BulkSubsidiaryUpdateRequestModel)))
        {
            var requestUpdate = (BulkSubsidiaryUpdateRequestModel)(object)request;
            var joinerDate = string.Empty;
            if (requestUpdate != null)
            {
                joinerDate = requestUpdate.JoinerDate.ToString();
            }
            validDate = DateTime.TryParseExact(joinerDate, formats, new CultureInfo("en-GB"), DateTimeStyles.None, out dateValue);

        }
        else if (typeof(T).Equals(typeof(BulkSubsidiaryAddRequestModel)))
        {
            var requestInsert = (BulkSubsidiaryAddRequestModel)(object)request;
            var joinerDate = string.Empty;
            if (request != null)
            {
                joinerDate = requestInsert.JoinerDate;
            }

            validDate = (DateTime.TryParseExact(joinerDate, formats, new CultureInfo("en-GB"), DateTimeStyles.None, out dateValue));
        }
        else if (typeof(T).Equals(typeof(BulkOrganisationRequestModel)))
        {
            var requestAddSub = (BulkOrganisationRequestModel)(object)request;

            var joinerDate = string.Empty;
            if (request != null && requestAddSub.Subsidiary != null)
            {
                joinerDate = requestAddSub.Subsidiary.JoinerDate;
            }

            validDate = (DateTime.TryParseExact(joinerDate, formats, new CultureInfo("en-GB"), DateTimeStyles.None, out dateValue));
        }

        if (!validDate)
        {
            return null;
        }

        return dateValue;
    }
}