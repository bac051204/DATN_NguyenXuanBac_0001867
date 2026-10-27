using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SalesInventory.Api.Services;

public class NotFoundException(string message) : Exception(message);

public class BusinessRuleException(string message) : Exception(message);

/// <summary>Chuyển exception nghiệp vụ thành ProblemDetails (404 / 409).</summary>
public class BusinessExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            BusinessRuleException => (StatusCodes.Status409Conflict, exception.Message),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict,
                "Dữ liệu đã bị thay đổi bởi một yêu cầu khác, vui lòng thử lại."),
            _ => (0, string.Empty)
        };
        if (status == 0) return false;

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title
        }, ct);
        return true;
    }
}
