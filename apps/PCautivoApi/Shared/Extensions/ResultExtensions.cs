using PCautivoCore.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace PCautivoApi.Shared.Extensions;

public static class ResultExtensions
{
    public static ActionResult ToActionResult(this Result result)
    {
        if (!result.IsSuccess)
        {
            return new ObjectResult(new
            {
                message = result.Error!.Message
            })
            {
                StatusCode = (int)result.Error!.StatusCode
            };
        }

        return new StatusCodeResult((int)result.StatusCode);
    }

    public static ActionResult<T> ToActionResult<T>(this Result<T> result)
    {
        if (!result.IsSuccess)
        {
            return new ObjectResult(new
            {
                message = result.Error!.Message
            })
            {
                StatusCode = (int)result.Error!.StatusCode
            };
        }

        return new ObjectResult(result.Value)
        {
            StatusCode = (int)result.StatusCode
        };
    }
}
