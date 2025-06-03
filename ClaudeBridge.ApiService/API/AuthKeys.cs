public static class AuthKeys
{
    public const string Claude = "Claude";

    public const string Notion = "Notion";

    public static string GetToken(this Dictionary<string, string> values, string key)
    {
        var authorization = values.GetValueOrDefault(key);
        // Remove "Bearer " prefix if present
        var token = authorization != null && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization.Substring("Bearer ".Length)
            : authorization;


        return token;
    }
}