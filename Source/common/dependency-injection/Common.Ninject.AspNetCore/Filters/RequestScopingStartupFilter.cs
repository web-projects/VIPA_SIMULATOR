﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;

namespace IPA5.Common.Ninject.AspNetCore.Filters
{
    internal sealed class RequestScopingStartupFilter : IStartupFilter
    {
        private readonly Func<IDisposable> requestScopeProvider;

        public RequestScopingStartupFilter(Func<IDisposable> requestScopeProvider)
        {
            if (requestScopeProvider == null)
            {
                throw new ArgumentNullException(nameof(requestScopeProvider));
            }

            this.requestScopeProvider = requestScopeProvider;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                ConfigureRequestScoping(builder);

                next(builder);
            };
        }

        internal void ConfigureRequestScoping(IApplicationBuilder builder)
        {
            builder.Use(async (context, next) =>
            {
                using (var scope = this.requestScopeProvider())
                {
                    await next();
                }
            });
        }
    }
}
