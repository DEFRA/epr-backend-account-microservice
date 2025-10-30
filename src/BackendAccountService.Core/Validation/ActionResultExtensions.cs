using Microsoft.AspNetCore.Mvc;

namespace BackendAccountService.Core.Validation;

public static class ActionResultExtensions
{
    public static ActionResult? ThenValidate(this ActionResult? existingProblem, Func<ActionResult?> nextValidation)
    {
        return existingProblem ?? nextValidation();
    }

    public static async Task<ActionResult?> ThenValidate(this Task<ActionResult?> existingProblemTask,
        Func<ActionResult?> nextValidation)
    {
        var existingProblem = await existingProblemTask;
        return existingProblem ?? nextValidation();
    }

    public static async Task<ActionResult?> ThenValidateAsync(this Task<ActionResult?> existingProblemTask,
        Func<Task<ActionResult?>> nextValidationAsync)
    {
        var existingProblem = await existingProblemTask;
        return existingProblem ?? await nextValidationAsync();
    }

    public static async Task<ActionResult?> ThenValidateAsync(this ActionResult? existingProblem,
        Func<Task<ActionResult?>> nextValidationAsync)
    {
        return existingProblem ?? await nextValidationAsync();
    }
}