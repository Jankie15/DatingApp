using System.Net;
using System.Text.Json;
using API.Errors;

namespace API.Middleware
{
    public class ExeptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ILogger<ExeptionMiddleware> _logger { get; }
        private readonly IHostEnvironment _environment;
        public ExeptionMiddleware(RequestDelegate next, ILogger<ExeptionMiddleware> logger, IHostEnvironment environment)
        {
            _environment = environment;
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context){
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                var Response = _environment.IsDevelopment()
                ?
                    new ApiExeption(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                :
                    new ApiExeption(context.Response.StatusCode, "Internal Server Error");

                var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

                var json = JsonSerializer.Serialize(Response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}