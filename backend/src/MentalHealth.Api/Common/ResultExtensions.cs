using MentalHealth.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MentalHealth.Api.Common;

public static class ResultExtensions
{
    public static ActionResult ToOk<T>(this Result<T> result, ControllerBase controller) =>
        result.IsSuccess ? controller.Ok(result.Value) : result.ToProblem(controller);

    public static ActionResult ToProblem(this Result result, ControllerBase controller)
    {
        return result.ErrorType switch
        {
            ErrorType.Validation => controller.ValidationProblem(BuildModelState(result)),
            ErrorType.NotFound => controller.Problem(detail: result.Error, statusCode: StatusCodes.Status404NotFound),
            ErrorType.Conflict => controller.Problem(detail: result.Error, statusCode: StatusCodes.Status409Conflict),
            _ => controller.Problem(detail: result.Error, statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    private static ModelStateDictionary BuildModelState(Result result)
    {
        var modelState = new ModelStateDictionary();
        foreach (var (field, messages) in result.ValidationErrors)
            foreach (var message in messages)
                modelState.AddModelError(field, message);

        return modelState;
    }
}
