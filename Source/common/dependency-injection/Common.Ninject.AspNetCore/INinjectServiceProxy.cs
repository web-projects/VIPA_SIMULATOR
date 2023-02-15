using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Modules;

namespace IPA5.Common.Ninject.AspNetCore
{
    public interface INinjectServiceProxy
    {
        void ConfigureServices(IServiceCollection services);
        void Configure(IApplicationBuilder app, params NinjectModule[] modules);
    }
}
