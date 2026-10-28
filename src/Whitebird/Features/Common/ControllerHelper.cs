using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Features.Common;

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

    /// <summary>
    /// Validates ModelState and returns a structured <c>400 BadRequest</c> when
    /// invalid. Response includes both the flat <c>errors</c> list (backward
    /// compatible) and a per-field <c>fieldErrors</c> dictionary so callers can
    /// highlight specific inputs.
    /// </summary>
    public static IActionResult? HandleModelState(this ControllerBase controller)
    {
        if (controller.ModelState.IsValid) return null;

        var fieldErrors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var flatErrors = new List<string>();

        foreach (var kvp in controller.ModelState)
        {
            var fieldName = string.IsNullOrEmpty(kvp.Key) ? "(general)" : kvp.Key;
            var messages = new List<string>();

            foreach (var error in kvp.Value.Errors)
            {
                var message = string.IsNullOrEmpty(error.ErrorMessage)
                    ? error.Exception?.Message ?? "Validation failed"
                    : error.ErrorMessage;

                if (!string.IsNullOrEmpty(message))
                    messages.Add(message);
            }

            if (messages.Count == 0) continue;

            // Accumulate under the field name (ModelState can report multiple
            // entries for the same key if multiple attributes fail).
            if (fieldErrors.TryGetValue(fieldName, out var existing))
            {
                existing.AddRange(messages);
            }
            else
            {
                fieldErrors[fieldName] = messages;
            }

            foreach (var m in messages)
                flatErrors.Add($"{fieldName}: {m}");
        }

        var result = ServiceResult.BadRequest(flatErrors, "Validation failed");
        result.FieldErrors = fieldErrors;
        return controller.BadRequest(result);
    }
}