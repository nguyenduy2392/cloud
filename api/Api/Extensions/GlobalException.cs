using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;
using System.Net;
using Core.Common;

namespace Api.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
                        await context.Response.WriteAsJsonAsync(Response.Fail(
                            message: $"Internal Server Error. TraceId: {traceId}",
                            errorCode: context.Response.StatusCode
                        ));
                    }
                });
            });
        }
    }
}
