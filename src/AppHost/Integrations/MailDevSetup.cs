namespace AppHost.Integrations;

public sealed class MailDevResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
	// Constants used to refer to well known-endpoint names, this is specific
	// for each resource type. MailDev exposes an SMTP endpoint and a HTTP
	// endpoint.
	internal const string SmtpEndpointName = "smtp";
	internal const string HttpEndpointName = "http";

	// An EndpointReference is a core .NET Aspire type used for keeping
	// track of endpoint details in expressions. Simple literal values cannot
	// be used because endpoints are not known until containers are launched.
	private EndpointReference? _smtpReference;

	public EndpointReference SmtpEndpoint =>
		_smtpReference ??= new(this, SmtpEndpointName);

	// Required property on IResourceWithConnectionString. Represents a connection
	// string that applications can use to access the MailDev server. In this case
	// the connection string is composed of the SmtpEndpoint endpoint reference.
	public ReferenceExpression ConnectionStringExpression =>
		ReferenceExpression.Create(
			$"smtp://{SmtpEndpoint.Property(EndpointProperty.HostAndPort)}"
		);
}

public static class MailDevResourceBuilderExtensions
{
	/// <summary>
	/// Adds the <see cref="MailDevResource"/> to the given
	/// <paramref name="builder"/> instance. Uses the "2.1.0" tag.
	/// </summary>
	/// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
	/// <param name="name">The name of the resource.</param>
	/// <param name="httpPort">The HTTP port.</param>
	/// <param name="smtpPort">The SMTP port.</param>
	/// <returns>
	/// An <see cref="IResourceBuilder{MailDevResource}"/> instance that
	/// represents the added MailDev resource.
	/// </returns>
	public static IResourceBuilder<MailDevResource> AddMailDev(
		this IDistributedApplicationBuilder builder,
		string name,
		int? httpPort = null,
		int? smtpPort = null)
	{
		// The AddResource method is a core API within .NET Aspire and is
		// used by resource developers to wrap a custom resource in an
		// IResourceBuilder<T> instance. Extension methods to customize
		// the resource (if any exist) target the builder interface.
		var resource = new MailDevResource(name);

		return builder.AddResource(resource)
			.WithImage(MailDevContainerImageTags.Image)
			.WithImageRegistry(MailDevContainerImageTags.Registry)
			.WithImageTag(MailDevContainerImageTags.Tag)
			.WithHttpEndpoint(
				targetPort: 1080,
				port: httpPort,
				name: MailDevResource.HttpEndpointName)
			.WithEndpoint(
				targetPort: 1025,
				port: smtpPort,
				name: MailDevResource.SmtpEndpointName);
	}
}

// This class just contains constant strings that can be updated periodically
// when new versions of the underlying container are released.
internal static class MailDevContainerImageTags
{
	internal const string Registry = "docker.io";

	internal const string Image = "maildev/maildev";

	internal const string Tag = "2.1.0";
}