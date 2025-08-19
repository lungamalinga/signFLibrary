using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SigniflowMiddlewareCSharp.Loggings;

public class AuthHelper
{
    private string authToken;

    public AuthHelper( string authToken) { 
        this.authToken = authToken;
    }
    public IActionResult ValidateToken(HttpRequest req, MyLogs log)
    {
        string authHeader = req.Headers["Authorization"];

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            log.LogWarning("Authorization header missing or malformed.");
            return new UnauthorizedResult();
        }

        string token = authHeader.Substring("Bearer ".Length).Trim();

        string expectedToken = this.authToken;
        if (token != expectedToken)
        {
            log.LogWarning("Invalid token provided.");
            return new UnauthorizedResult();
        }
        return null;
    }
}
