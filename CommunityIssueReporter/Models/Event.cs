using System;

namespace CommunityIssueReporter.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public int CreatedByUserID { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public string ImagePath { get; set; }
    }

    public class EventRegistration
    {
        public int RegistrationID { get; set; }
        public int EventID { get; set; }
        public int UserID { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string AttendanceStatus { get; set; }
        public string UserName { get; set; }
        public string EventTitle { get; set; }
    }
}