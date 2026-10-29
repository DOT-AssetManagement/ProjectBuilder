using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Helpers.Exceptions;
using ProjectBuilder.Models;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Authorization;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var watch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected or navigated away before the request finished (e.g. EF Core
            // query cancelled mid-read). There's no connection left to write an error response to,
            // so just stop — treating this as an internal-server-error and trying to redirect only
            // produces log noise for a request nobody is waiting on anymore.
            watch.Stop();
        }
        catch (Exception ex)
        {
            watch.Stop();
            await HandleExceptionAsync(context, ex, watch.ElapsedMilliseconds);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, long elapsedMs)
    {
        var endpoint = context.GetEndpoint();
        var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

        string? serviceName = actionDescriptor?.ControllerName ?? endpoint?.DisplayName;
        string? methodName = actionDescriptor?.ActionName ?? exception.TargetSite?.Name;

        ExceptionHandlerResult result;

        switch (exception)
        {
            case AppException ex:
                result = HandleAppException(context, ex, serviceName, methodName, elapsedMs);
                break;

            case AbpAuthorizationException ex:
                result = HandleAuthorizationException(context, ex, serviceName, methodName, elapsedMs);
                break;

            case UnauthorizedAccessException ex:
                result = HandleUnauthorizedException(context, ex, serviceName, methodName, elapsedMs);
                break;

            case ArgumentException ex:
                result = HandleArgumentException(context, ex, serviceName, methodName, elapsedMs);
                break;

            case TimeoutException ex:
                result = HandleTimeoutException(context, ex, serviceName, methodName, elapsedMs);
                break;

            case DbException ex:
                result = HandleDbException(context, ex, serviceName, methodName, elapsedMs);
                break;

            default:
                result = HandleDefaultException(context, exception, serviceName, methodName, elapsedMs);
                break;
        }

        if (context.Response.HasStarted)
            throw exception;

        context.Response.Clear();

        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = result.StatusCode;

            await context.Response.WriteAsync(JsonSerializer.Serialize(result.ErrorResponse));
            return;
        }

        context.Session.SetString("ErrorMessage", result.ErrorResponse.Message ?? "Something went wrong");
        context.Session.SetString("ErrorDetails", result.ErrorResponse.Details ?? "");
        context.Session.SetString("ErrorCode", result.ErrorResponse.Code ?? "");
        context.Session.SetString("ErrorServiceName", result.ErrorResponse.ServiceName ?? "");
        context.Session.SetString("ErrorMethodName", result.ErrorResponse.MethodName ?? "");
        context.Session.SetString("ErrorTraceId", result.ErrorResponse.TraceId ?? "");

        context.Response.Redirect("/ErrorPage/Error");
    }

    private ExceptionHandlerResult HandleAppException(
        HttpContext context,
        AppException ex,
        string? serviceName,
        string? methodName,
        long elapsedMs)
    {
        int statusCode = ex.Code switch
        {
            ExceptionCodes.NotFound => (int)HttpStatusCode.NotFound,
            ExceptionCodes.BusinessRule => (int)HttpStatusCode.UnprocessableEntity,
            ExceptionCodes.Forbidden => (int)HttpStatusCode.Forbidden,
            ExceptionCodes.Validation => (int)HttpStatusCode.BadRequest,
            ExceptionCodes.Unauthorized => (int)HttpStatusCode.Unauthorized,
            ExceptionCodes.Conflict => (int)HttpStatusCode.Conflict,
            ExceptionCodes.BadRequest => (int)HttpStatusCode.BadRequest,
            ExceptionCodes.Timeout => (int)HttpStatusCode.RequestTimeout,
            ExceptionCodes.Database => (int)HttpStatusCode.InternalServerError,
            _ => (int)HttpStatusCode.InternalServerError
        };

        ex.ExceptionType = ex.GetType().FullName;
        ex.ExecutionTime = DateTime.UtcNow;
        ex.ExecutionDuration = (int)elapsedMs;
        ex.ServiceName ??= serviceName;
        ex.MethodName ??= methodName;
        ex.Path = context.Request.Path;
        ex.HttpMethod = context.Request.Method;
        ex.TraceId = context.TraceIdentifier;
        ex.UserId = GetUserId(context);
        ex.UserName = context.User?.Identity?.Name;

        _logger.LogWarning(ex,
            "[{Code}] {Message} | Details: {Details} | Service: {ServiceName} | Method: {MethodName} | Duration: {Duration}ms | TraceId: {TraceId}",
            ex.Code, ex.Message, ex.Details, ex.ServiceName, ex.MethodName, ex.ExecutionDuration, ex.TraceId);

        return new ExceptionHandlerResult
        {
            StatusCode = statusCode,
            ErrorResponse = BuildResponse(
                context,
                ex.Code,
                ex.Message,
                ex.Details,
                ex.GetType(),
                ex.ServiceName,
                ex.MethodName,
                elapsedMs)
        };
    }

    private ExceptionHandlerResult HandleAuthorizationException(
        HttpContext context,
        AbpAuthorizationException ex,
        string? serviceName,
        string? methodName,
        long elapsedMs)
    {
        _logger.LogWarning(ex,
            "[{Code}] {Message} | Service: {ServiceName} | Method: {MethodName} | Duration: {Duration}ms | TraceId: {TraceId}",
            ExceptionCodes.Forbidden, ex.Message, serviceName, methodName, elapsedMs, context.TraceIdentifier);

        return new ExceptionHandlerResult
        {
            StatusCode = (int)HttpStatusCode.Forbidden,
            ErrorResponse = BuildResponse(
                context,
                ExceptionCodes.Forbidden,
                ex.Message,
                null,
                ex.GetType(),
                serviceName,
                methodName,
                elapsedMs)
        };
    }

    private ExceptionHandlerResult HandleUnauthorizedException(
        HttpContext context,
        UnauthorizedAccessException ex,
        string? serviceName,
        string? methodName,
        long elapsedMs)
    {
        _logger.LogWarning(ex,
            "[{Code}] {Message} | Service: {ServiceName} | Method: {MethodName} | Duration: {Duration}ms | TraceId: {TraceId}",
            ExceptionCodes.Unauthorized, ex.Message, serviceName, methodName, elapsedMs, context.TraceIdentifier);

        return new ExceptionHandlerResult
        {
            StatusCode = (int)HttpStatusCode.Unauthorized,
            ErrorResponse = BuildResponse(
                context,
                ExceptionCodes.Unauthorized,
                ex.Message,
                ex.InnerException?.Message ?? ex.Message,
                ex.GetType(),
                serviceName,
                methodName,
                elapsedMs)
        };
    }

    private ExceptionHandlerResult HandleArgumentException(
        HttpContext context,
        ArgumentException ex,
        string? serviceName,
        string? methodName,
        long elapsedMs)
    {
        _logger.LogWarning(ex,
            "[{Code}] {Message} | Details: {Details} | Service: {ServiceName} | Method: {MethodName}",
            ExceptionCodes.Validation, "Invalid request.", ex.Message, serviceName, methodName);

        return new ExceptionHandlerResult
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            ErrorResponse = BuildResponse(
                context,
                ExceptionCodes.Validation,
                "Invalid request.",
                ex.Message,
                ex.GetType(),
                serviceName,
                methodName,
                elapsedMs)
        };
    }

    private ExceptionHandlerResult HandleTimeoutException(
        HttpContext context,
        TimeoutException ex,
        string? serviceName,
        string? methodName,
        long elapsedMs)
    {
        _logger.LogError(ex,
            "[{Code}] {Message} | Service: {ServiceName} | Method: {MethodName}",
            ExceptionCodes.Timeout, "The request timed out.", serviceName, methodName);

        return new ExceptionHandlerResult
        {
            StatusCode = (int)HttpStatusCode.RequestTimeout,
            ErrorResponse = BuildResponse(
                context,
                ExceptionCodes.Timeout,
                "The request timed out.",
                ex.Message,
                ex.GetType(),
                serviceName,
                methodName,
                elapsedMs)
        };
    }

    private ExceptionHandlerResult HandleDbException(
        HttpContext context,
        DbException ex,
        string? serviceName,
        string? methodName,
        long elapsedMs)
    {
        _logger.LogError(ex,
            "[{Code}] {Message} | Details: {Details} | Service: {ServiceName} | Method: {MethodName}",
            ExceptionCodes.Database, "A database error occurred.", ex.Message, serviceName, methodName);

        return new ExceptionHandlerResult
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            ErrorResponse = BuildResponse(
                context,
                ExceptionCodes.Database,
                "A database error occurred.",
                ex.Message,
                ex.GetType(),
                serviceName,
                methodName,
                elapsedMs)
        };
    }

    private ExceptionHandlerResult HandleDefaultException(
        HttpContext context,
        Exception ex,
        string? serviceName,
        string? methodName,
        long elapsedMs)
    {
        _logger.LogError(ex,
            "[{Code}] {Message} | Details: {Details} | Service: {ServiceName} | Method: {MethodName} | Duration: {Duration}ms | TraceId: {TraceId}",
            ExceptionCodes.Internal, "Something went wrong", ex.Message, serviceName, methodName, elapsedMs, context.TraceIdentifier);

        return new ExceptionHandlerResult
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            ErrorResponse = BuildResponse(
                context,
                ExceptionCodes.Internal,
                "Something went wrong",
                ex.Message,
                ex.GetType(),
                serviceName,
                methodName,
                elapsedMs)
        };
    }

    private ExceptionResponseDto BuildResponse(
        HttpContext context,
        string code,
        string message,
        string? details,
        Type exceptionType,
        string? serviceName,
        string? methodName,
        long elapsedMs)
    {
        return new ExceptionResponseDto
        {
            Code = code,
            Message = message,
            Details = details,
            Exception = exceptionType.FullName,
            ExecutionDuration = (int)elapsedMs,
            ExecutionTime = DateTime.UtcNow,
            MethodName = methodName,
            ServiceName = serviceName,
            Path = context.Request.Path,
            HttpMethod = context.Request.Method,
            TraceId = context.TraceIdentifier,
            UserId = GetUserId(context),
            UserName = context.User?.Identity?.Name
        };
    }

    private string? GetUserId(HttpContext context)
    {
        return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.User?.FindFirst("sub")?.Value
            ?? context.User?.FindFirst("userid")?.Value;
    }
}

public class ExceptionHandlerResult
{
    public int StatusCode { get; set; }
    public ExceptionResponseDto ErrorResponse { get; set; } = new ExceptionResponseDto();
}