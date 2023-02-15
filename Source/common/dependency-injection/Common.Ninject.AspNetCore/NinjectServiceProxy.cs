using IPA5.Common.Ninject.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using System;
using System.Threading;

namespace IPA5.Common.Ninject.AspNetCore
{
    public class NinjectServiceProxy : INinjectServiceProxy
    {
        private readonly AsyncLocal<NinjectServiceScope> localScopeProvider = new AsyncLocal<NinjectServiceScope>();
        public static IKernel LocalKernel { get; set; }

        private object Resolve(Type type) => LocalKernel.Get(type);
        private object RequestScope(IContext ninjectContext) => localScopeProvider.Value;

        [Obsolete("Please utilize the Configure method for now until IServiceProvider integration is completed next year.")]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddRequestScopingMiddleware(() => localScopeProvider.Value = new NinjectServiceScope());

            services.AddCustomControllerActivation(Resolve);

            services.AddCustomViewComponentActivation(Resolve);
        }

        public void Configure(IApplicationBuilder app, params NinjectModule[] modules)
        {
            IKernel kernel = new IPA5DefaultResolver().ResolveKernel(modules);

            foreach (Type controllerType in app.GetControllerTypes())
            {
                kernel.Bind(controllerType).ToSelf().InScope(RequestScope);
            }

            LocalKernel = kernel;
        }
    }
}
