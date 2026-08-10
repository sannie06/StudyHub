using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudyHub.Application.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudyHub.Web.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERROR SERVER 500/EXCEPTION] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n");
                _logger.LogError(ex, "An unhandled exception occurred during HTTP request execution: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var statusCode = HttpStatusCode.InternalServerError;
            var title = "An error occurred while processing your request.";
            var detail = _env.IsDevelopment() ? $"{exception.Message} ---> Inner: {exception.InnerException?.Message} \n {exception.StackTrace}" : exception.Message;
            IDictionary<string, string[]>? validationErrors = null;

            if (exception is AppException appEx)
            {
                statusCode = appEx.StatusCode;
                title = appEx.Message;
                
                if (exception is ValidationException valEx)
                {
                    validationErrors = valEx.Errors;
                }
            }

            context.Response.StatusCode = (int)statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = _env.IsDevelopment() ? exception.StackTrace : detail,
                Instance = context.Request.Path
            };

            if (validationErrors != null)
            {
                problemDetails.Extensions["errors"] = validationErrors;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(problemDetails, options);
            await context.Response.WriteAsync(json);
        }
    }
}
