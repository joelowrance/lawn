using LawnCare.JobApi.Domain.Entities;
using LawnCare.JobApi.Domain.Enums;
using LawnCare.JobApi.Domain.ValueObjects;

namespace LawnCare.JobApi.Domain.Services;


public class JobDomainService
{
	public bool CanScheduleJob(Job job, DateTime requestedDate, List<Job> existingJobs)
	{
		if (job.Status != JobStatus.Pending)
			return false;

		// Check if there are scheduling conflicts (this would integrate with scheduling service)
		// For now, just check if the date is in the future
		return requestedDate > DateTime.UtcNow;
	}

	// public JobPriority CalculatePriority(ServiceType serviceType, DateTime requestedDate)
	// {
	// 	// Emergency services get high priority
	// 	if (serviceType.ServiceName.Contains("Emergency"))
	// 		return JobPriority.Emergency;
	//
	// 	// Tree removal gets higher priority than lawn care
	// 	if (serviceType.Category == "TreeSpecialist")
	// 		return JobPriority.High;
	//
	// 	// Jobs requested for today get high priority
	// 	if (requestedDate.Date == DateTime.Today)
	// 		return JobPriority.High;
	//
	// 	return JobPriority.Normal;
	// }
}
