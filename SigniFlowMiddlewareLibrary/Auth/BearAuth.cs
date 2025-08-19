using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace SigniFlowMiddlewareLibrary.Auth
{
    public class BearAuth
    {
        private readonly RequestDelegate _next;
        private const string AUTH_HEADER = "Authorization";

        public BearAuth(RequestDelegate next)
        {
            _next = next;
        }

        // ... rest of the file remains unchanged

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            if (!context.Request.Headers.ContainsKey(AUTH_HEADER))
            {
                context.Response.StatusCode = 401;
                Console.WriteLine("Authorization header missing.");
                await context.Response.WriteAsync("Authorization header missing.");
                return;
            }

            var authHeader = context.Request.Headers[AUTH_HEADER].ToString();

            if (!authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = 401;
                Console.WriteLine("Invalid authorization header format.");
                await context.Response.WriteAsync("Invalid authorization header.");
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            // Replace GetValue<T> with direct access to configuration value
            var validToken = configuration["BearerToken"];

            if (token != validToken)
            {
                context.Response.StatusCode = 403;
                Console.WriteLine("Invalid Bearer token.");
                await context.Response.WriteAsync("Invalid token.");
                return;
            }

            await _next(context);
        }
    }

}
