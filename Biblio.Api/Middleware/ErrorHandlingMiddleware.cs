using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Biblio.Api.Middleware;

public class ErrorHandlingMiddleware
{
 private readonly RequestDelegate _next;
 private readonly ILogger<ErrorHandlingMiddleware> _logger;

 public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
 _logger.LogError(ex, "Unhandled exception");
 await HandleExceptionAsync(context, ex);
 }
 }

 private static Task HandleExceptionAsync(HttpContext context, Exception exception)
 {
 var traceId = context.TraceIdentifier;
 var problem = new ProblemDetails
 {
 Title = "Er is iets misgegaan",
 Detail = exception.Message,
 Status = (int)HttpStatusCode.InternalServerError,
 Instance = context.Request.Path
 };
 // Add trace id to extensions for diagnostics
 problem.Extensions["traceId"] = traceId;

 var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
 var payload = JsonSerializer.Serialize(problem, options);
 context.Response.ContentType = "application/problem+json";
 context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
 return context.Response.WriteAsync(payload);
 }
}
