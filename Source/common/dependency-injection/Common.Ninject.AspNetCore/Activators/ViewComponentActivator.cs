using Microsoft.AspNetCore.Mvc.ViewComponents;
using System;

namespace IPA5.Common.Ninject.AspNetCore.Activators
{
    internal sealed class ViewComponentActivator : IViewComponentActivator
    {
        private readonly Func<Type, object> viewComponentCreator;
        private readonly Action<object> viewComponentReleaser;

        public ViewComponentActivator(Func<Type, object> viewComponentCreator,
            Action<object> viewComponentReleaser = null)
        {
            this.viewComponentCreator = viewComponentCreator ??
                throw new ArgumentNullException(nameof(viewComponentCreator));

            this.viewComponentReleaser = viewComponentReleaser ?? (_ => { });
        }

        public object Create(ViewComponentContext context) =>
            this.viewComponentCreator(context.ViewComponentDescriptor.TypeInfo.AsType());

        public void Release(ViewComponentContext context, object viewComponent) =>
            this.viewComponentReleaser(viewComponent);
    }
}
