using Microsoft.AspNetCore.Mvc.Filters;

namespace TheGarage.ActionFilters;

public class AddSasTokenHeaderActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
        var sasToken = configuration.GetValue(AppConfiguration.Keys.AzureStorageSasToken,
            AppConfiguration.Defaults.AzureStorageSasToken);
        context.HttpContext.Response.Headers.Add(AppConfiguration.SasHeaderName, sasToken);
    }
}
  