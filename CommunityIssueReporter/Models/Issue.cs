using System;

namespace CommunityIssueReporter.Models
{
    public class Issue
    {
        public int IssueID { get; set; }
        public int? UserID { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string AttachmentPath { get; set; }
        public DateTime ReportedTime { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public int? AssignedTo { get; set; }
        public string ResolutionDetails { get; set; }
        public DateTime? ResolutionDate { get; set; }

        // Navigation properties
        public string ReporterName { get; set; }
        public string AssignedToName { get; set; }

        // Constructor
        public Issue()
        {
            ReportedTime = DateTime.Now;
            Status = "New";
            Priority = "Medium";
        }

        // Copy constructor
        public Issue(Issue source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            IssueID = source.IssueID;
            UserID = source.UserID;
            Location = source.Location;
            Category = source.Category;
            Description = source.Description;
            AttachmentPath = source.AttachmentPath;
            ReportedTime = source.ReportedTime;
            Status = source.Status;
            Priority = source.Priority;
            AssignedTo = source.AssignedTo;
            ResolutionDetails = source.ResolutionDetails;
            ResolutionDate = source.ResolutionDate;
            ReporterName = source.ReporterName;
            AssignedToName = source.AssignedToName;
        }

        // To string override
        public override string ToString()
        {
            return $"Issue #{IssueID}: {Category} at {Location} - {Status}";
        }
    }

    public class IssueStatusHistory
    {
        public int HistoryID { get; set; }
        public int IssueID { get; set; }
        public string StatusFrom { get; set; }
        public string StatusTo { get; set; }
        public int ChangedByUserID { get; set; }
        public DateTime ChangeDate { get; set; }
        public string Comments { get; set; }

        // Navigation properties
        public string ChangedByName { get; set; }

        public IssueStatusHistory()
        {
            ChangeDate = DateTime.Now;
        }
    }

    public class IssueComment
    {
        public int CommentID { get; set; }
        public int IssueID { get; set; }
        public int UserID { get; set; }
        public string CommentText { get; set; }
        public DateTime CommentDate { get; set; }
        public bool IsPublic { get; set; }

        // Navigation properties
        public string UserName { get; set; }

        public IssueComment()
        {
            CommentDate = DateTime.Now;
            IsPublic = true;
        }
    }
}