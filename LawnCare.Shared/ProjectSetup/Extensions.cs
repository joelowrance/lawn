﻿using LawnCare.Shared.Logging;
using LawnCare.Shared.OpenTelemetry;

using Microsoft.Extensions.Hosting;
using MassTransit.Logging;
using MassTransit.Monitoring;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace LawnCare.Shared.ProjectSetup
{
	public static class Extensions
	{
		public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
		{
			builder.ConfigureOpenTelemetry();

			builder.AddDefaultHealthChecks();

			builder.Services.AddHttpContextAccessor();

			builder.Services.AddServiceDiscovery();

			builder.Services.ConfigureHttpClientDefaults(http =>
			{
				// Turn on resilience by default
				http.AddStandardResilienceHandler();

				// Turn on service discovery by default
				http.AddServiceDiscovery();
			});

			return builder;
		}
		
		public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
		{
			builder.Logging.EnableEnrichment();
			builder.Services.AddLogEnricher<ApplicationEnricher>();

			builder.Logging.AddOpenTelemetry(logging =>
			{
				logging.IncludeFormattedMessage = true;
				logging.IncludeScopes = true;
			});

			builder.Services.AddOpenTelemetry()
				.WithMetrics(metrics =>
				{
					metrics.AddAspNetCoreInstrumentation()
						.AddHttpClientInstrumentation()
						.AddRuntimeInstrumentation()
						.AddMeter(InstrumentationOptions.MeterName)
						.AddMeter("Marten")
						.AddMeter(ActivitySourceProvider.DefaultSourceName);
				})
				.WithTracing(tracing =>
				{
					tracing.AddAspNetCoreInstrumentation()
						.AddHttpClientInstrumentation()
						.AddSource(DiagnosticHeaders.DefaultListenerName)
						.AddSource("Marten")
						.AddSource(ActivitySourceProvider.DefaultSourceName)
						.AddSource("Yarp.ReverseProxy");
				});

			builder.AddOpenTelemetryExporters();

			return builder;
		}
		
		private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
		{
			var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

			if (useOtlpExporter)
			{
				builder.Services.AddOpenTelemetry().UseOtlpExporter();
			}

			return builder;
		}
		public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
		{
			builder.Services.AddHealthChecks()
				// Add a default liveness check to ensure app is responsive
				.AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

			return builder;
		}
		
		public static WebApplication MapDefaultEndpoints(this WebApplication app)
		{
			// Adding health checks endpoints to applications in non-development environments has security implications.
			// See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
			if (app.Environment.IsDevelopment())
			{
				// All health checks must pass for app to be considered ready to accept traffic after starting
				app.MapHealthChecks("/health");

				// Only health checks tagged with the "live" tag must pass for app to be considered alive
				app.MapHealthChecks("/alive", new HealthCheckOptions
				{
					Predicate = r => r.Tags.Contains("live")
				});
			}

			return app;
		}
	}
}