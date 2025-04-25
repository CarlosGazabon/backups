
using System.Net;
using Newtonsoft.Json;

namespace inventio.Middleware
{
    public class ManagerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ManagerMiddleware> _logger;

        private async Task ManagerExceptionAsync(HttpContext context, Exception ex, ILogger<ManagerMiddleware> logger)
        {

            object? errors = null;

            switch (ex)
            {
                case MiddlewareException me:
                    logger.LogError(ex, "Middleware Error");
                    errors = me.Errors;
                    context.Response.StatusCode = (int)me.Code;
                    break;
                case Exception e:
                    logger.LogError(ex, "Server Error");
                    errors = string.IsNullOrWhiteSpace(e.Message) ? "Error" : e.Message;
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            context.Response.ContentType = "application/json";
            var result = string.Empty;

            if (errors != null)
            {
                result = JsonConvert.SerializeObject(new { errors });
            }

            await context.Response.WriteAsync(result);
        }

        public ManagerMiddleware(
            RequestDelegate next,
            ILogger<ManagerMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await ManagerExceptionAsync(context, ex, _logger);
            }
        }


    }
}