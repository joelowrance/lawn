﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.Enrichment;

namespace LawnCare.Shared.Logging;

/// <summary>
/// Ref: https://andrewlock.net/customising-the-new-telemetry-logging-source-generator/
/// </summary>
/// <param name="httpContextAccessor"></param>
internal class ApplicationEnricher(IHttpContextAccessor httpContextAccessor) : ILogEnricher
{
	public void Enrich(IEnrichmentTagCollector collector)
	{
		collector.Add("MachineName", Environment.MachineName);

		var httpContext = httpContextAccessor.HttpContext;
		if (httpContext is not null)
		{
			collector.Add("IsAuthenticated", httpContext?.User?.Identity?.IsAuthenticated!);
		}
	}
}