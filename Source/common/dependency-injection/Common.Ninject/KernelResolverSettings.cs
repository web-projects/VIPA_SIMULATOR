using Ninject.Modules;

namespace Common.Ninject
{
    public class KernelResolverSettings
    {
        public NinjectModule[] NinjectModules { get; private set; }

        public KernelResolverSettings SetNinjectModules(params NinjectModule[] ninjectModules)
        {
            NinjectModules = ninjectModules;
            return this;
        }
    }
}
