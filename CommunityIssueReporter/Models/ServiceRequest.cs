using System;

namespace CommunityIssueReporter.Models
{
    public class ServiceRequest
    {
        public int RequestID { get; set; }
        public int UserID { get; set; }
        public string ServiceType { get; set; }
        public string Description { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; }
        public DateTime? CompletionDate { get; set; }

        // Navigation properties
        public string UserName { get; set; }

        // Constructor
        public ServiceRequest()
        {
            SubmissionDate = DateTime.Now;
            Status = "Pending";
        }

        // Copy constructor
        public ServiceRequest(ServiceRequest source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            RequestID = source.RequestID;
            UserID = source.UserID;
            ServiceType = source.ServiceType;
            Description = source.Description;
            SubmissionDate = source.SubmissionDate;
            Status = source.Status;
            CompletionDate = source.CompletionDate;
            UserName = source.UserName;
        }

        // To string override
        public override string ToString()
        {
            return $"Request #{RequestID}: {ServiceType} - {Status}";
        }
    }
}