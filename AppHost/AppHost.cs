var builder = DistributedApplication.CreateBuilder(args);

//TODO: these should be in secrets
var rabbitUser = builder.AddParameter("mquser", "rabbituser");
var rabbitPass = builder.AddParameter("mqpassword", "rabbitpass");
var postgresUser = builder.AddParameter("postgresUser", "sqluser");
var postgresPass = builder.AddParameter("postgresPass", "sqlpass");

// 3rd party infrastructure
var rabbitMq = builder.AddRabbitMQ("rabbitmq", rabbitUser, rabbitPass, 5672)
	.WithLifetime(ContainerLifetime.Persistent) //stays around even when the app host stops.
	.WithDataBindMount(@"c:\temp\LawnCare-RabbitMq", isReadOnly: false)
	.WithManagementPlugin( port: 15672 );
	

// //TODO:  figure out health check
// //TODO:  figure out persistent port (.WithManagementPlugin(port: 99898);?

 var postgreSQL = builder.AddPostgres(
		 "postgres-sql", postgresUser, postgresPass, port: 5432)
 	.WithLifetime(ContainerLifetime.Persistent)
    .WithDataBindMount(@"c:\temp\LawnCare-Postgres")
 	.WithPgAdmin(cfg =>
    {
	    cfg.WithHostPort(15432);
    });


var database = postgreSQL.AddDatabase("postgres-connection", "postgres");
var sagaDb = postgreSQL.AddDatabase("saga-connection", "sagas");

// Other projects in the solution
builder.AddProject<Projects.LawnCare_JobApi>("job-api")
	.WithReference(rabbitMq)
	.WaitFor(rabbitMq);

builder.AddProject<Projects.LawnCare_StateMachine>("state-machine")
	.WithReference(rabbitMq)
	.WaitFor(rabbitMq)
	.WithReference(sagaDb)
	.WaitFor(sagaDb);

// builder.AddProject<Projects.LawnCare_CustomerApi>("customer-api")
// 	.WithReference(rabbitMq)
// 	.WaitFor(rabbitMq)
// 	.WithReference(database)
// 	.WaitFor(database);
	



builder.Build().Run();