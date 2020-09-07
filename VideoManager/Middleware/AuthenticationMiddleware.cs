using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;
using VideoManager.Models;
using VideoManager.Services;

namespace VideoManager.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private (string, string) ExtractAuthenticationInformation(StringValues authorizationHeader)
        {
            string scheme = null;
            string parameter = null;

            if (string.IsNullOrWhiteSpace(authorizationHeader)) return (scheme, parameter);

            string headerValue = authorizationHeader[0];
            string[] splitHeader = headerValue.Split(' ');

            if (splitHeader.Length == 2)
            {
                scheme = splitHeader[0];
                parameter = splitHeader[1];
            }

            return (scheme, parameter);
        }

        public async Task InvokeAsync(HttpContext context, IAuth0Service auth0Service, IUserService userService)
        {
            User user = null;
            StringValues authorizationHeader = context.Request.Headers["Authorization"];

            (string scheme, string parameter) = ExtractAuthenticationInformation(authorizationHeader);

            if (string.Equals("Bearer", scheme, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(parameter))
            {
                string auth0UserId = await auth0Service.GetAuth0UserId(parameter);
                user = await userService.CreateOrGetByAuthId(auth0UserId);
            }
            else if(context.Request.Query.TryGetValue("accessToken", out StringValues accessTokenValues) && accessTokenValues.Count > 0)
            {
                string accessToken = accessTokenValues[0];

                if (!string.IsNullOrEmpty(accessToken))
                {
                    string auth0UserId = await auth0Service.GetAuth0UserId(accessToken);
                    user = await userService.CreateOrGetByAuthId(auth0UserId);
                }
            }

            if (user == null)
            {
                context.Response.StatusCode = 401;
            }
            else
            {
                context.Items["UserId"] = user.UserId;
                await _next(context);
            }
        }
    }
}
