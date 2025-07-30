using LawnCare.JobApi.Application.DTOs;
using LawnCare.JobApi.Domain.Common;
using LawnCare.JobApi.Domain.Entities;
using LawnCare.JobApi.Domain.Enums;
using LawnCare.JobApi.Domain.Repositories;
using LawnCare.JobApi.Domain.ValueObjects;
using LawnCare.Shared.MessageContracts;

using JobServiceItem = LawnCare.Shared.MessageContracts.JobServiceItem;

namespace LawnCare.JobApi.Domain.Services;
    public class JobApplicationService
    {
        private readonly IJobRepository _jobRepository;
        private readonly JobDomainService _jobDomainService;
        private readonly IUnitOfWork _unitOfWork;

        public JobApplicationService(
            IJobRepository jobRepository,
            JobDomainService jobDomainService,
            IUnitOfWork unitOfWork)
        {
            _jobRepository = jobRepository;
            _jobDomainService = jobDomainService;
            _unitOfWork = unitOfWork;
        }

        public async Task<JobResponse> CreateJobFromFieldEstimateAsync(FieldEstimate estimate)
        {
	        
            var tenantId = TenantId.From(estimate.TenantId);
            
            var  address = new ServiceAddress(
	            estimate.CustomerAddress1,
	            estimate.CustomerAddress2,
	            estimate.CustomerAddress2,
	            estimate.CustomerCity,
	            estimate.CustomerState,
	            estimate.CustomerZip
            );
            

            var estimatedDuration = new EstimatedDuration(estimate.EstimatedDuration);
            var estimatedCost = new Money(estimate.EstimatedCost);

            var job = new Job(
	            tenantId,
	            $"{estimate.CustomerFirstName} {estimate.CustomerLastName}",
	            address,
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

            return MapToJobResponse(job);
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

        private static JobResponse MapToJobResponse(Job job)
        {
            return new JobResponse(
                job.JobId.Value,
                job.TenantId.Value,
                job.CustomerId?.Value,
                job.CustomerName,
                new JobAddressResponse(
                    job.ServiceAddress.Street1,
                    job.ServiceAddress.Street2,
                    job.ServiceAddress.Street3,
                    job.ServiceAddress.City,
                    job.ServiceAddress.State,
                    job.ServiceAddress.ZipCode,
                    job.ServiceAddress.FullAddress,
                    job.ServiceAddress.Latitude,
                    job.ServiceAddress.Longitude
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
