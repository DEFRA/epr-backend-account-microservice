using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BackendAccountService.Core.Helpers;

public class DateTimeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(DateTime) || context.Metadata.ModelType == typeof(DateTime?))
        {
            return new DateTimeModelBinder("yyyy-MM-ddTHH:mm:ss");
        }

        return null;
    }
}