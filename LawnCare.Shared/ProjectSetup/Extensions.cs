using LawnCare.Shared.Logging;
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
	/// <summary>
	/// Provides extension methods to configure common service defaults,
	/// OpenTelemetry (logging, metrics, tracing), and standard health endpoints.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Adds common service defaults to the host builder:
		/// OpenTelemetry configuration, default health checks, HTTP context accessor,
		/// service discovery, and resilient HTTP client defaults.
		/// </summary>
		/// <param name="builder">The host application builder.</param>
		/// <returns>The same <see cref="IHostApplicationBuilder"/> for chaining.</returns>
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
		
		/// <summary>
		/// Configures OpenTelemetry for logging, metrics, and tracing,
		/// including common ASP.NET Core and HTTP client instrumentation,
		/// runtime metrics, and custom meters/sources.
		/// </summary>
		/// <param name="builder">The host application builder.</param>
		/// <returns>The same <see cref="IHostApplicationBuilder"/> for chaining.</returns>
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
		
		/// <summary>
		/// Adds OpenTelemetry exporters based on configuration.
		/// Currently enables the OTLP exporter when the
		/// OTEL_EXPORTER_OTLP_ENDPOINT configuration value is provided.
		/// </summary>
		/// <param name="builder">The host application builder.</param>
		/// <returns>The same <see cref="IHostApplicationBuilder"/> for chaining.</returns>
		private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
		{
			var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

			if (useOtlpExporter)
			{
				builder.Services.AddOpenTelemetry().UseOtlpExporter();
			}

			return builder;
		}

		/// <summary>
		/// Registers default health checks, including a simple liveness check named "self"
		/// that is tagged with "live" to distinguish it from readiness checks.
		/// </summary>
		/// <param name="builder">The host application builder.</param>
		/// <returns>The same <see cref="IHostApplicationBuilder"/> for chaining.</returns>
		public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
		{
			builder.Services.AddHealthChecks()
				// Add a default liveness check to ensure app is responsive
				.AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

			return builder;
		}
		
		/// <summary>
		/// Maps default health endpoints for development environments:
		/// - GET /health: readiness probe (all checks must pass)
		/// - GET /alive: liveness probe (only checks tagged "live" must pass)
		/// </summary>
		/// <param name="app">The web application.</param>
		/// <returns>The same <see cref="WebApplication"/> for chaining.</returns>
		/// <remarks>
		/// Health endpoints are mapped only in development to avoid exposing them
		/// in production without proper security controls.
		/// </remarks>
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