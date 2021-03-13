using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoManager.Exceptions;

namespace VideoManager.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            } catch (NotFoundException)
            {
                context.Response.StatusCode = 404;
            } catch (UnauthorizedAccessException)
            {
                context.Response.StatusCode = 401;
            } catch (Exception)
            {
                context.Response.StatusCode = 500;
            }
        }
    }
}
