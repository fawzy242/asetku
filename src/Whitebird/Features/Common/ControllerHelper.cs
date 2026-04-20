using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common.Service;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Common;

public static class ControllerHelper
{
    public static IActionResult HandleResult<T>(this ControllerBase controller, ServiceResult<T> result, string? actionName = null, object? routeValues = null)
    {
        if (!result.IsSuccess)
            return HandleErrorResult(controller, result);

        if (result.Data == null)
            return controller.NotFound(result);

        if (!string.IsNullOrEmpty(actionName))
        {
            if (routeValues == null)
            {
                var idProperty = typeof(T).GetProperties().FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));
                if (idProperty != null && result.Data != null)
                    routeValues = new { id = idProperty.GetValue(result.Data) };
            }
            return controller.CreatedAtAction(actionName, routeValues, result);
        }

        return controller.Ok(result);
    }

    public static IActionResult HandleResult<T>(this ControllerBase controller, ServiceResult<PaginatedResult<T>> result)
    {
        return result.IsSuccess ? controller.Ok(result) : HandleErrorResult(controller, result);
    }

    public static IActionResult HandleResult(this ControllerBase controller, ServiceResult result)
    {
        return result.IsSuccess ? controller.Ok(result) : HandleErrorResult(controller, result);
    }

    private static IActionResult HandleErrorResult(ControllerBase controller, object result)
    {
        var errorType = result.GetType().GetProperty("ErrorType")?.GetValue(result) as string;
        return errorType switch
        {
            "NotFound" => controller.NotFound(result),
            "Conflict" => controller.Conflict(result),
            "BadRequest" => controller.BadRequest(result),
            _ => controller.BadRequest(result)
        };
    }

    public static IActionResult? HandleModelState(this ControllerBase controller)
    {
        if (controller.ModelState.IsValid) return null;

        var errors = controller.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return controller.BadRequest(ServiceResult.Failure(string.Join("; ", errors), "Validation failed"));
    }
}