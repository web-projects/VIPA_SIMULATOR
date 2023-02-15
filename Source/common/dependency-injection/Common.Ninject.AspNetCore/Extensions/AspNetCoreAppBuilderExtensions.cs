using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Ninject;
using System;
using System.Linq;

namespace IPA5.Common.Ninject.AspNetCore.Extensions
{
    internal static class AspNetCoreAppBuilderExtensions
    {
        public static void BindToMethod<T>(this IKernel config, Func<T> method) => config.Bind<T>().ToMethod(c => method());

        public static Type[] GetControllerTypes(this IApplicationBuilder builder)
        {
            ApplicationPartManager manager = builder.ApplicationServices.GetRequiredService<ApplicationPartManager>();

            ControllerFeature feature = new ControllerFeature();
            manager.PopulateFeature(feature);

            return feature.Controllers.Select(t => t.AsType()).ToArray();
        }

        public static T GetRequestService<T>(this IApplicationBuilder builder) where T : class
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return GetRequestServiceProvider(builder).GetService<T>();
        }

        private static IServiceProvider GetRequestServiceProvider(IApplicationBuilder builder)
        {
            IHttpContextAccessor accessor = builder.ApplicationServices.GetService<IHttpContextAccessor>();

            if (accessor == null)
            {
                throw new InvalidOperationException(typeof(IHttpContextAccessor).FullName);
            }

            HttpContext context = accessor.HttpContext;

            if (context == null)
            {
                throw new InvalidOperationException("No HttpContext available.");
            }

            return context.RequestServices;
        }
    }
}
