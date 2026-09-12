namespace eProrab.API.Middleware;

/// <summary>
/// Middleware to add security headers to all HTTP responses.
/// Helps prevent common web vulnerabilities like XSS, clickjacking, and MIME-type sniffing.
/// </summary>
public class SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Prevent MIME-type sniffing (XSS prevention)
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Clickjacking protection - prevent framing
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // XSS protection (older browsers that don't understand Content-Security-Policy)
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Content Security Policy - restrict resource loading to prevent inline script execution
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self' https://accounts.google.com https://graph.facebook.com");

        // Referrer policy - control what referrer info is sent
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Force HTTPS in production (for supported browsers)
        if (!context.Request.IsHttps && !context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            context.Response.Headers.Append("Strict-Transport-Security",
                "max-age=31536000; includeSubDomains; preload");
        }

        // Permissions policy / Feature policy
        context.Response.Headers.Append("Permissions-Policy",
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

        await next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
