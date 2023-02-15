using Microsoft.AspNetCore.Mvc;
using Ninject;
using IPA5.Common.Internal;
using System;

namespace IPA5.Common.Ninject.AspNetCore
{
    public class SelfInjectingController : ControllerBase
    {
        public SelfInjectingController()
        {
            if (!TestRunnerHelper.IsUnitTestActive)
            {
                NinjectServiceProxy.LocalKernel.Inject(this);
            }
        }

        [Obsolete("This has been deprecated and controllers should be injected directly from your unit test")]
        public SelfInjectingController(IKernel kernel) => kernel.Inject(this);
    }
}
