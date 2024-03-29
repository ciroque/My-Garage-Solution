using Microsoft.AspNetCore.Mvc.Filters;

namespace TheGarage.ActionFilters;

public class AddSasTokenHeaderActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var appConfiguration = (AppConfiguration?)context.HttpContext.RequestServices.GetService(typeof(AppConfiguration));
        context.HttpContext.Response.Headers.Add("x-sas-token", appConfiguration.AzureStorageSasToken);
    }
}
