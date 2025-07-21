var builder = DistributedApplication.CreateBuilder(args);

//TODO: these should be in secrets
var rabbitUser = builder.AddParameter("mquser", "rabbituser");
var rabbitPass = builder.AddParameter("mqpassword", "rabbitpass");
var postgresUser = builder.AddParameter("postgresUser", "sqluser");
var postgresPass = builder.AddParameter("postgresPass", "sqlpass");

// 3rd party infrastructure
var rabbitMq = builder.AddRabbitMQ("rabbitmq", rabbitUser, rabbitPass)
	.WithLifetime(ContainerLifetime.Persistent) //stays around even when the app host stops.
	.WithDataBindMount(@"c:\temp\LawnCare-RabbitMq", isReadOnly: false)
	.WithManagementPlugin();
// //TODO:  figure out health check
// //TODO:  figure out persistent port (.WithManagementPlugin(port: 99898);?


// var postgres = builder.AddPostgres("postgres-sql", postgresUser, postgresPass)
// 	.WithLifetime(ContainerLifetime.Persistent)
// 	.WithPgAdmin();

// Other projects in the solution
builder.AddProject<Projects.LawnCare_JobApi>("job-api");


builder.Build().Run();