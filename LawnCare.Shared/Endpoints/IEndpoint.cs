using Microsoft.AspNetCore.Routing;

namespace LawnCare.Shared.Endpoints
{
	public interface IEndpoint
	{
		void MapEndpoint(IEndpointRouteBuilder app);
	}
}