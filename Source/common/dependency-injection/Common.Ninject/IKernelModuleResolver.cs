using Ninject;
using Ninject.Modules;

namespace Common.Ninject
{
    public interface IKernelModuleResolver
    {
        IKernel ResolveKernel(params NinjectModule[] modules);
    }
}
