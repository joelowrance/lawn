using AppHost.Integrations;

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

	//..var paperCut = builder.AddPapercutSmtp("smtp", 80, 25);
var mailDev = builder.AddMailDev("maildev", 1080, 25);


// //TODO:  figure out health check
// //TODO:  figure out persistent port (.WithManagementPlugin(port: 99898);?

var postgreSQL = builder.AddPostgres(
		"postgres-sql", postgresUser, postgresPass, port: 5432)
	.WithLifetime(ContainerLifetime.Persistent)
	.WithDataBindMount(@"c:\temp\LawnCare-Postgres");
 	// .WithPgAdmin(cfg =>
  //   {
	 //    cfg.WithHostPort(15432);
  //   });


 //var customerDatabase = postgreSQL.AddDatabase("customers-connection", "customers");
 var coreApiDatabase = postgreSQL.AddDatabase("job-connection", "Core");
 //var sagaDb = postgreSQL.AddDatabase("saga-connection", "sagas");
 var communicationsDb = postgreSQL.AddDatabase("communications-connection", "communications");

// Other projects in the solution
 // builder.AddProject<Projects.LawnCare_JobApi>("job-api")
	//  .WithReference(rabbitMq)
	//  .WaitFor(rabbitMq)
	//  .WithReference(jobDatabase)
	//  .WaitFor(jobDatabase);


 // builder.AddProject<Projects.LawnCare_StateMachine>("state-machine")
	//  .WithReference(rabbitMq)
	//  .WaitFor(rabbitMq)
	//  .WithReference(sagaDb)
	//  .WaitFor(sagaDb);


 // builder.AddProject<Projects.LawnCare_CustomerApi>("customer-api")
 // 	.WithReference(rabbitMq)
 // 	.WaitFor(rabbitMq)
 // 	.WithReference(customerDatabase)
 // 	.WaitFor(customerDatabase);

 var coreApi = builder.AddProject<Projects.LawnCare_CoreApi>("core-api")
	 .WithReference(rabbitMq)
	 .WaitFor(rabbitMq)
	 .WithReference(coreApiDatabase)
	 .WaitFor(coreApiDatabase);

 builder.AddProject<Projects.LawnCare_Communications>("communications")
	.WithReference(rabbitMq)
    .WaitFor(rabbitMq)
	.WithReference(communicationsDb)
	.WaitFor(communicationsDb)
	.WithReference(mailDev);

 builder.AddProject<Projects.LawnCare_ManagementUI>("management-ui")
	 .WithReference(coreApi)
	 .WaitFor(coreApi)
	 .WithEnvironment("CoreApi__BaseUrl", coreApi.GetEndpoint("https"));

builder.Build().Run();
