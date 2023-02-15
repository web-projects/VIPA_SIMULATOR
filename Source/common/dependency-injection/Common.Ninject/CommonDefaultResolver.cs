using Ninject;
using Ninject.Modules;

namespace Common.Ninject
{
    public sealed class CommonDefaultResolver : IKernelModuleResolver
    {
        public IKernel ResolveKernel(params NinjectModule[] modules)
        {
            IKernel kernel = new StandardKernel(modules);
            kernel.Settings.InjectNonPublic = true;
            kernel.Settings.InjectParentPrivateProperties = true;
            return kernel;
        }
    }
}
