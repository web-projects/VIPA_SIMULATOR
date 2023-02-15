using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;

namespace IPA5.Common.Ninject.AspNetCore.Activators
{
    internal sealed class ControllerActivator : IControllerActivator
    {
        private readonly Func<ControllerContext, object> controllerCreator;
        private readonly Action<ControllerContext, object> controllerReleaser;

        public ControllerActivator(Func<ControllerContext, object> controllerCreator,
            Action<ControllerContext, object> controllerReleaser = null)
        {
            this.controllerCreator = controllerCreator ??
                throw new ArgumentNullException(nameof(controllerCreator));

            this.controllerReleaser = controllerReleaser ?? ((_, __) => { });
        }

        public object Create(ControllerContext context) => this.controllerCreator(context);

        public void Release(ControllerContext context, object controller) => controllerReleaser(context, controller);
    }
}
