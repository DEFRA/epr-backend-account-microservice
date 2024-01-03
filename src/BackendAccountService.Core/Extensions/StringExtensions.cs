namespace BackendAccountService.Core.Extensions;

public static class StringExtensions
{
    public static string TryExtractReferenceNumberFromQuery(this String source)
    {
        source = source.Replace(" ", "");
        if (source.Length == 6 && int.TryParse(source, out _))
        {
            return source;
        }
        return null;
    }
}

