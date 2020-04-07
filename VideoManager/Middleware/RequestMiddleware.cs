using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VideoManager.Models;
using VideoManager.Services;

namespace VideoManager.Middleware
{
    public class RequestMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestMiddleware(RequestDelegate next)
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
            bool userVerified = false;
            StringValues authorizationHeader = context.Request.Headers["Authorization"];

            (string scheme, string parameter) = ExtractAuthenticationInformation(authorizationHeader);

            if (string.Equals("Bearer", scheme, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(parameter))
            {
                string auth0UserId = await auth0Service.GetAuth0UserId(parameter);
                User user = await userService.CreateOrGetByAuthId(auth0UserId);

                if (user != null)
                {
                    userVerified = true;
                    context.Items["UserId"] = user.UserId;
                    await _next(context);
                }
            }

            if (!userVerified)
            {
                context.Response.StatusCode = 401;
            }
        }
    }
}
