using Ninject;
using Ninject.Modules;
using System.Collections.Concurrent;

namespace Common.Ninject
{
    public abstract class StaticKernelResolverBase : KernelResolverBase
    {
        private static ConcurrentDictionary<string, IKernel> kernels = new ConcurrentDictionary<string, IKernel>();

        public override IKernel ResolveKernel(params NinjectModule[] modules)
        {
            string classType = GetType().Name;

            if (kernels.ContainsKey(classType))
            {
                return kernels[classType];
            }
            else
            {
                IKernel kernel = base.ResolveKernel(modules);
                kernels.TryAdd(classType, kernel);
                return kernel;
            }
        }
    }
}
