﻿// <copyright file="MetricsApplicationBuilderExtensions.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.AspNetCore.Endpoints;
using App.Metrics.DependencyInjection.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable CheckNamespace
namespace Microsoft.AspNetCore.Builder
    // ReSharper restore CheckNamespace
{
    /// <summary>
    ///     Extension methods for <see cref="IApplicationBuilder" /> to add App Metrics to the request execution pipeline.
    /// </summary>
    public static class MetricsApplicationBuilderExtensions
    {
        /// <summary>
        ///     Adds App Metrics to the <see cref="IApplicationBuilder" /> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder" />.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseMetricsEndpoints(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            // Verify if AddMetrics was done before calling UseMetricsEndpoints
            // We use the MetricsMarkerService to make sure if all the services were added.
            AppMetricsServicesHelper.ThrowIfMetricsNotRegistered(app.ApplicationServices);

            var metricsOptionsAccessor = app.ApplicationServices.GetRequiredService<IOptions<MetricsOptions>>();
            var metricsAspNetCoreOptionsAccessor = app.ApplicationServices.GetRequiredService<IOptions<MetricsAspNetCoreOptions>>();

            UseMetricsTextMiddleware(app, metricsAspNetCoreOptionsAccessor, metricsOptionsAccessor);
            UseMetricsMiddleware(app, metricsAspNetCoreOptionsAccessor, metricsOptionsAccessor);

            return app;
        }

        private static void UseMetricsMiddleware(
            IApplicationBuilder app,
            IOptions<MetricsAspNetCoreOptions> metricsAspNetCoreOptionsAccessor,
            IOptions<MetricsOptions> metricsOptionsAccessor)
        {
            app.UseWhen(
                context => context.Request.Path == metricsAspNetCoreOptionsAccessor.Value.MetricsEndpoint &&
                           metricsAspNetCoreOptionsAccessor.Value.MetricsEndpointEnabled &&
                           metricsOptionsAccessor.Value.MetricsEnabled &&
                           metricsAspNetCoreOptionsAccessor.Value.MetricsEndpoint.IsPresent(),
                appBuilder => { appBuilder.UseMiddleware<MetricsEndpointMiddleware>(); });
        }

        private static void UseMetricsTextMiddleware(
            IApplicationBuilder app,
            IOptions<MetricsAspNetCoreOptions> metricsAspNetCoreOptionsAccessor,
            IOptions<MetricsOptions> metricsOptionsAccessor)
        {
            app.UseWhen(
                context => context.Request.Path == metricsAspNetCoreOptionsAccessor.Value.MetricsTextEndpoint &&
                           metricsAspNetCoreOptionsAccessor.Value.MetricsTextEndpointEnabled &&
                           metricsOptionsAccessor.Value.MetricsEnabled &&
                           metricsAspNetCoreOptionsAccessor.Value.MetricsTextEndpoint.IsPresent(),
                appBuilder => { appBuilder.UseMiddleware<MetricsEndpointTextEndpointMiddleware>(); });
        }
    }
}