using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Scheme = "TestScheme";
    private const string HeaderName = "Test-User";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var headerValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var header = headerValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(header))
            return Task.FromResult(AuthenticateResult.NoResult());

        try
        {
            var claims = ParseHeaderToClaims(header);
            if (!claims.Any())
                return Task.FromResult(AuthenticateResult.NoResult());

            var identity = new ClaimsIdentity(claims, Scheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "TestAuthHandler: failed parsing header");
            return Task.FromResult(AuthenticateResult.Fail("Invalid Test-User header"));
        }
    }

    private static IEnumerable<Claim> ParseHeaderToClaims(string header)
    {
        // Expect header in format: "id:<userId>;roles:Role1,Role2;name:DisplayName;email:me@example.com"
        var parts = header.Split(';', StringSplitOptions.RemoveEmptyEntries)
                          .Select(p => p.Trim())
                          .Where(p => p.Length > 0);

        var claims = new List<Claim>();

        foreach (var part in parts)
        {
            var kv = part.Split(new[] { ':' }, 2);
            if (kv.Length != 2) continue;
            var key = kv[0].Trim().ToLowerInvariant();
            var value = kv[1].Trim();

            switch (key)
            {
                case "id":
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, value));
                    if (!claims.Any(c => c.Type == ClaimTypes.Name))
                        claims.Add(new Claim(ClaimTypes.Name, value));
                    break;

                case "name": 
                    claims.Add(new Claim(ClaimTypes.Name, value));
                    break;

                case "email":
                    claims.Add(new Claim(ClaimTypes.Email, value));
                    break;

                case "roles":
                case "role":
                    // Accept both "roles:Admin,User" and "roles: Admin, User"
                    var roles = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(r => r.Trim())
                                     .Where(r => r.Length > 0);
                    foreach (var r in roles)
                        claims.Add(new Claim(ClaimTypes.Role, r));
                    break;

                default:
                    // allow arbitrary claim types using "claim:{type}={value}" pattern
                    if (key.StartsWith("claim:", StringComparison.OrdinalIgnoreCase))
                    {
                        var claimType = key.Substring("claim:".Length);
                        if (!string.IsNullOrWhiteSpace(claimType))
                            claims.Add(new Claim(claimType, value));
                    }
                    break;
            }
        }

        return claims;
    }
}
