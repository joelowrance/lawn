using LawnCare.JobApi.Application.DTOs;
using LawnCare.JobApi.Domain.Common;
using LawnCare.JobApi.Domain.Entities;
using LawnCare.JobApi.Domain.Enums;
using LawnCare.JobApi.Domain.Repositories;
using LawnCare.JobApi.Domain.ValueObjects;
using LawnCare.Shared.MessageContracts;
using MassTransit;
using Address = LawnCare.JobApi.Domain.Entities.Address;
using JobServiceItem = LawnCare.Shared.MessageContracts.JobServiceItem;

namespace LawnCare.JobApi.Domain.Services;

public interface IJobApplicationService
{
	Task<EstimateCreatedResponse> CreateJobFromFieldEstimateAsync(FieldEstimate estimate);
	Task<JobResponse?> GetJobAsync(Guid jobId, Guid tenantId);
	Task<List<JobResponse>> GetJobsByTenantAsync(Guid tenantId, JobStatus? status = null);
	Task<bool> ScheduleJobAsync(Guid jobId, Guid tenantId, DateTime scheduledDate, Guid technicianId);
	Task<JobResponse> SetPending(Guid jobId, Guid customerId);
}

public class JobApplicationService : IJobApplicationService
{
        private readonly IJobRepository _jobRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly JobDomainService _jobDomainService;
        private readonly IUnitOfWork _unitOfWork;
        ILogger<JobApplicationService> _logger;


        public JobApplicationService(
            IJobRepository jobRepository,
            JobDomainService jobDomainService,
            IUnitOfWork unitOfWork, ILogger<JobApplicationService> logger, ICustomerRepository customerRepository, IPublishEndpoint publishEndpoint)
        {
            _jobRepository = jobRepository;
            _jobDomainService = jobDomainService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _customerRepository = customerRepository;
            _publishEndpoint = publishEndpoint;
        }



        public async Task<EstimateCreatedResponse> CreateJobFromFieldEstimateAsync(FieldEstimate estimate)
        {

	        var existingCustomer = await _customerRepository.FindByFieldEstimateAttributes(estimate);
            bool isNewCustomer = existingCustomer == null;

	        if (existingCustomer == null)
	        {
		        _logger.LogInformation("Customer not found, creating a new one");
		        existingCustomer = new Customer(
			        estimate.CustomerFirstName,
			        estimate.CustomerLastName,
			        estimate.CustomerEmail,
			        estimate.CustomerCellPhone,
			        estimate.CustomerHomePhone,
			        new Address(
				        estimate.CustomerAddress1,
				        estimate.CustomerAddress2,
				        estimate.CustomerAddress2,
				        estimate.CustomerCity,
				        estimate.CustomerState,
				        estimate.CustomerZip));

		        await _customerRepository.AddAsync(existingCustomer);
	        }




            var tenantId = TenantId.From(estimate.TenantId);

            // var address = new Address(
	           //  estimate.CustomerAddress1,
	           //  estimate.CustomerAddress2,
	           //  estimate.CustomerAddress2,
	           //  estimate.CustomerCity,
	           //  estimate.CustomerState,
	           //  estimate.CustomerZip
            // );


            var estimatedDuration = new EstimatedDuration(estimate.EstimatedDuration);
            var estimatedCost = new Money(estimate.EstimatedCost);

            var job = new Job(
	            tenantId,
	            $"{estimate.CustomerFirstName} {estimate.CustomerLastName}",
	            existingCustomer.Address,
	            estimate.Description,
                estimate.ScheduledDate,
                estimatedDuration,
                estimatedCost
            );

            foreach (JobServiceItem item in estimate.Services)
            {
	            job.AddService(new Entities.JobServiceItem(item.ServiceName, item.Quantity, item.Comment, item.Price));
            }

            // Calculate priority using domain service
            //var priority = _jobDomainService.CalculatePriority(serviceType, request.RequestedDate);
            job.UpdatePriority(JobPriority.Normal);

            await _jobRepository.AddAsync(job);
            await _unitOfWork.SaveChangesAsync();

            return new EstimateCreatedResponse(MapToJobResponse(job), isNewCustomer);
        }

        public async Task<JobResponse?> GetJobAsync(Guid jobId, Guid tenantId)
        {
            var job = await _jobRepository.GetByIdAsync(JobId.From(jobId), TenantId.From(tenantId));
            return job != null ? MapToJobResponse(job) : null;
        }

        public async Task<List<JobResponse>> GetJobsByTenantAsync(Guid tenantId, JobStatus? status = null)
        {
            var jobs = await _jobRepository.GetByTenantAsync(TenantId.From(tenantId), status);
            return jobs.Select(MapToJobResponse).ToList();
        }

        public async Task<bool> ScheduleJobAsync(Guid jobId, Guid tenantId, DateTime scheduledDate, Guid technicianId)
        {
            var job = await _jobRepository.GetByIdAsync(JobId.From(jobId), TenantId.From(tenantId));
            if (job == null) return false;

            var existingJobs = await _jobRepository.GetByTenantAsync(TenantId.From(tenantId));

            if (!_jobDomainService.CanScheduleJob(job, scheduledDate, existingJobs))
                return false;

            job.ScheduleJob(scheduledDate, TechnicianId.From(technicianId));
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<JobResponse> SetPending(Guid jobId, Guid customerId)
        {
	        try
	        {
		        //TODO:  figure out this tenant shit.  Job will always be unique
		        var job = await _jobRepository.GetByIdAsync(JobId.From(jobId), TenantId.From(Guid.Empty));

		        //TODO:  better exceptions
		        job = job ?? throw new ApplicationException("Job not found");
		        job.ChangeStatus(JobStatus.Pending);

		        await _unitOfWork.SaveChangesAsync();
		        return MapToJobResponse(job);


	        }
	        catch (Exception wtf)
	        {
		        _logger.LogCritical(wtf, "Problem with something");;
	        }

	        return null!;
        }

        private static JobResponse MapToJobResponse(Job job)
        {
            return new JobResponse(
                job.JobId.Value,
                job.TenantId.Value,
                job.CustomerId?.Value,
                job.CustomerName,
                new JobAddressResponse(
                    job.ActualServiceAddress.Street1,
                    job.ActualServiceAddress.Street2,
                    job.ActualServiceAddress.Street3,
                    job.ActualServiceAddress.City,
                    job.ActualServiceAddress.State,
                    job.ActualServiceAddress.ZipCode,
                    job.ActualServiceAddress.FullAddress,
                    job.ActualServiceAddress.Latitude,
                    job.ActualServiceAddress.Longitude
                ),
                job.Status.ToString(),
                job.Priority.ToString(),
                job.Description,
                job.SpecialInstructions,
                job.EstimatedDuration.TotalHours,
                job.EstimatedCost.Amount,
                "USD",
                job.ActualCost?.Amount,
                job.ScheduledDate,
                job.ScheduledDate,
                job.CompletedDate,
                job.AssignedTechnicianId?.Value,
                job.ServiceItems.Select(r => new JobServiceItemResponse(
                    r.ServiceName,
                    r.Quantity,
                    r.Comment ?? string.Empty,
                    r.Price
                )).ToList(),
                job.Notes.Select(n => new JobNoteResponse(n.Id,

                    n.Author,
                    n.Content,
                    n.CreatedAt
                )).ToList(),
                job.CreatedAt,
                job.UpdatedAt
            );
        }
    }
