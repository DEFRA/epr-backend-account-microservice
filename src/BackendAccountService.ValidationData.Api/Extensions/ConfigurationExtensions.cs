using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BackendAccountService.ValidationData.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class ConfigurationExtensions
{
    public static void ConfigureSection<TOptions>(
        this IServiceCollection services,
        string sectionKey,
        bool validate = true)
        where TOptions : class, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(sectionKey);
        services.AddOptions<TOptions>().Configure(delegate(TOptions options, IConfiguration config)
        {
            config.GetSection(sectionKey).Bind(options);

            if (!validate)
            {
                return;
            }

            var validationContext = new ValidationContext(options);
            var list = new List<ValidationResult>();

            if (Validator.TryValidateObject(options, validationContext, list, validateAllProperties: true))
            {
                return;
            }

            IEnumerable<string> failureMessages = list.Select(r => r.ErrorMessage);
            throw new OptionsValidationException(string.Empty, typeof(TOptions), failureMessages);
        });
    }
}