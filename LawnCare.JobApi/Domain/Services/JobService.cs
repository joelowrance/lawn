using LawnCare.JobApi.Application.DTOs;
using LawnCare.JobApi.Domain.Common;
using LawnCare.JobApi.Domain.Entities;
using LawnCare.JobApi.Domain.Enums;
using LawnCare.JobApi.Domain.Repositories;
using LawnCare.JobApi.Domain.ValueObjects;

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

        public async Task<JobResponse> CreateJobAsync(CreateJobRequest request)
        {
            var tenantId = TenantId.From(request.TenantId);
            var customerId = CustomerId.From(request.CustomerId);
            
            var address = new ServiceAddress(
                request.Address.Street,
                request.Address.City,
                request.Address.State,
                request.Address.ZipCode,
                request.Address.ApartmentUnit,
                request.Address.Latitude,
                request.Address.Longitude
            );

            var serviceType = new ServiceType(
                request.Service.Category,
                request.Service.ServiceName,
                request.Service.Description
            );

            var estimatedDuration = new EstimatedDuration(request.EstimatedHours, request.EstimatedMinutes);
            var estimatedCost = new Money(request.EstimatedCost);

            var job = new Job(
                tenantId,
                customerId,
                request.CustomerName,
                address,
                serviceType,
                request.Description,
                request.RequestedDate,
                estimatedDuration,
                estimatedCost
            );

            if (!string.IsNullOrEmpty(request.SpecialInstructions))
            {
                job.AddNote("Customer", request.SpecialInstructions);
            }

            // Calculate priority using domain service
            var priority = _jobDomainService.CalculatePriority(serviceType, request.RequestedDate);
            job.UpdatePriority(priority);

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
                job.CustomerId.Value,
                job.CustomerName,
                new JobAddressResponse(
                    job.ServiceAddress.Street,
                    job.ServiceAddress.City,
                    job.ServiceAddress.State,
                    job.ServiceAddress.ZipCode,
                    job.ServiceAddress.ApartmentUnit,
                    job.ServiceAddress.FullAddress,
                    null,
                    null
                ),
                new JobServiceResponse(
                    job.ServiceType.Category,
                    job.ServiceType.ServiceName,
                    job.ServiceType.Description
                ),
                job.Status.ToString(),
                job.Priority.ToString(),
                job.Description,
                job.SpecialInstructions,
                job.EstimatedDuration.TotalHours,
                job.EstimatedCost.Amount,
                "USD",
                job.ActualCost?.Amount,
                job.RequestedDate,
                job.ScheduledDate,
                job.CompletedDate,
                job.AssignedTechnicianId?.Value,
                job.Requirements.Select(r => new JobRequirementResponse(
                    r.RequirementType,
                    r.Description,
                    r.IsRequired,
                    r.IsFulfilled
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
