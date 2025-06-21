using CrudApiDemo.Responses;
using System.Text.Json;

namespace CrudApiDemo.Middleware
{
    public class AuthenticationRestrictionMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationRestrictionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            var method = context.Request.Method;

            var isLoginOrRegister =
                (path == "/api/users/login" && method == "POST") ||
                (path == "/api/users" && method == "POST");

            var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;

            if (!isLoginOrRegister && !isAuthenticated)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var response = new ApiResponse<object>(
                    401,
                    "Unauthorized access",
                    null,
                    "Unauthorized"
                );

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);

                return;
            }

            await _next(context);
        }
    }
}
