using FluentValidation;

using LawnCare.Shared.MessageContracts;
using LawnCare.Shared.OpenTelemetry;
using LawnCare.Shared.Pipelines;
using LawnCare.Shared.ProjectSetup;
using LawnCare.Shared.EntityFramework;

using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<SagaDbContext>(opts =>
{
	opts.UseNpgsql(builder.Configuration.GetConnectionString("saga-connection"));

});

//builder.EnrichNpgsqlDbContext<ProductDbContext>(); // comes from aspire ngpsql?

builder.Services.AddMigration<SagaDbContext>();


builder.Services.AddMediatR(cfg =>
{
	cfg.RegisterServicesFromAssemblyContaining<Program>();
	cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
	cfg.AddOpenBehavior(typeof(HandlerBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);
builder.Services.AddMassTransit(x =>
{
	x.AddSagaStateMachine<EstimateProcessingSaga, EstimateProcessingState>()
		.EntityFrameworkRepository(r =>
		{
			r.ConcurrencyMode = ConcurrencyMode.Optimistic;
			r.AddDbContext<DbContext, SagaDbContext>((provider, bx) =>
			{
				bx.UseNpgsql(builder.Configuration.GetConnectionString("saga-connection"))
					.UseSnakeCaseNamingConvention();
			});
		});
	
	x.SetKebabCaseEndpointNameFormatter();
	x.UsingRabbitMq((context, configuration) =>
	{
		configuration.Host(builder.Configuration.GetConnectionString("rabbitmq"));
		configuration.ConfigureEndpoints(context);
	});
});
builder.Services.AddSingleton<IActivityScope, ActivityScope>();
builder.Services.AddSingleton<CommandHandlerMetrics>();
builder.Services.AddSingleton<QueryHandlerMetrics>();
// builder.Services.AddDbContext<CustomerDbContext>(dbContextOptionsBuilder =>
// {
// 	dbContextOptionsBuilder.UseNpgsql(
// 			builder.Configuration.GetConnectionString("postgres"))
// 		.UseSnakeCaseNamingConvention();
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}
app.UseHttpsRedirection();
app.Run();


public class SagaDbContext : DbContext
{
    public SagaDbContext(DbContextOptions<SagaDbContext> options) : base(options)
    {
    }

    public DbSet<EstimateProcessingState> EstimateProcessingStates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the EstimateProcessingState entity
        modelBuilder.Entity<EstimateProcessingState>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.CorrelationId);
            
            // Table name
            entity.ToTable("estimate_processing_states");
            
            // Configure properties
            entity.Property(e => e.CorrelationId)
                .HasColumnName("correlation_id")
                .IsRequired();
                
            entity.Property(e => e.CurrentState)
                .HasColumnName("current_state")
                .HasMaxLength(100)
                .IsRequired();
                
            entity.Property(e => e.EstimateId)
                .HasColumnName("estimate_id")
                .IsRequired();
                
            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired();
                
            entity.Property(e => e.CustomerId)
                .HasColumnName("customer_id");
                
            entity.Property(e => e.JobId)
                .HasColumnName("job_id");
                
            entity.Property(e => e.EstimatorId)
                .HasColumnName("estimator_id")
                .HasMaxLength(100)
                .IsRequired();
                
            entity.Property(e => e.IsNewCustomer)
                .HasColumnName("is_new_customer")
                .HasDefaultValue(false);
                
            entity.Property(e => e.ErrorReason)
                .HasColumnName("error_reason")
                .HasMaxLength(1000);
                
            entity.Property(e => e.WelcomeEmailError)
                .HasColumnName("welcome_email_error")
                .HasMaxLength(1000);
                
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
                
            entity.Property(e => e.CompletedAt)
                .HasColumnName("completed_at");
                
            // Concurrency token for optimistic locking
            entity.Property(e => e.Version)
                .HasColumnName("version")
                .IsRowVersion();

            // Configure complex properties as JSON
            entity.Property(e => e.CustomerInfo)
                .HasColumnName("customer_info")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<CustomerInfo>(v, (System.Text.Json.JsonSerializerOptions?)null)!);
                    
            entity.Property(e => e.JobDetails)
                .HasColumnName("job_details")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<JobDetails>(v, (System.Text.Json.JsonSerializerOptions?)null)!);

            // Indexes for better query performance
            entity.HasIndex(e => e.EstimateId)
                .HasDatabaseName("ix_estimate_processing_states_estimate_id");
                
            entity.HasIndex(e => e.TenantId)
                .HasDatabaseName("ix_estimate_processing_states_tenant_id");
                
            entity.HasIndex(e => e.CurrentState)
                .HasDatabaseName("ix_estimate_processing_states_current_state");
                
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("ix_estimate_processing_states_created_at");
        });
    }
}