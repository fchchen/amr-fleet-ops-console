using AmrFleet.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace AmrFleet.Api.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireOperatorCommandTokenAttribute : Attribute, IAuthorizationFilter
{
    public const string HeaderName = "X-Operator-Token";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var options = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<OperatorCommandOptions>>()
            .Value;

        if (string.IsNullOrWhiteSpace(options.RequiredToken))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            return;
        }

        var providedToken = context.HttpContext.Request.Headers[HeaderName].ToString();
        if (!TimeConstantEquals(providedToken, options.RequiredToken))
        {
            context.Result = new ObjectResult(new { message = "Operator command token is required." })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }
    }

    private static bool TimeConstantEquals(string left, string right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        var diff = 0;
        for (var index = 0; index < left.Length; index++)
        {
            diff |= left[index] ^ right[index];
        }

        return diff == 0;
    }
}
