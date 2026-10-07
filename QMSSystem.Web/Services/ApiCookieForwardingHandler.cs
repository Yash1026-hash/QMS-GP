namespace QMSSystem.Web.Services;

public sealed class ApiCookieForwardingHandler(IHttpContextAccessor httpContextAccessor)
    : DelegatingHandler
{
    private const string AuthenticationCookieName = "QMS.Auth";

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var cookieHeader = httpContextAccessor.HttpContext?.Request.Headers.Cookie.ToString();
        if (string.IsNullOrWhiteSpace(cookieHeader))
        {
            return base.SendAsync(request, cancellationToken);
        }

        var chunkPrefix = AuthenticationCookieName + "C";
        var authenticationCookies = cookieHeader
            .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(cookie =>
            {
                var separator = cookie.IndexOf('=');
                if (separator <= 0)
                {
                    return false;
                }

                var name = cookie[..separator];
                if (name == AuthenticationCookieName)
                {
                    return true;
                }

                if (!name.StartsWith(chunkPrefix, StringComparison.Ordinal) ||
                    name.Length == chunkPrefix.Length)
                {
                    return false;
                }

                foreach (var character in name.AsSpan(chunkPrefix.Length))
                {
                    if (!char.IsAsciiDigit(character))
                    {
                        return false;
                    }
                }

                return true;
            });

        var forwardedCookies = string.Join("; ", authenticationCookies);
        if (forwardedCookies.Length > 0)
        {
            request.Headers.TryAddWithoutValidation("Cookie", forwardedCookies);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
