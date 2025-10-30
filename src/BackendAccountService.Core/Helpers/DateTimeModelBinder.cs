using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BackendAccountService.Core.Helpers;

public class DateTimeModelBinder(string format) : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;

        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        DateTime parsedDateTime;

        // Append default time if only the date is provided
        if (!value.Contains(':'))
        {
            value += "T00:00:00";
        }

        if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
        {
            bindingContext.Result = ModelBindingResult.Success(parsedDateTime);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Invalid date and time format.");
        return Task.CompletedTask;
    }
}