using System;
using System.ComponentModel.DataAnnotations;

namespace EXAM.Models
{
    public enum Priority
    {
        Low,
        Medium,
        High
    }

    public enum TicketStatus
    {
        Open,
        InProgress,
        Closed
    }

    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public Priority Priority { get; set; } = Priority.Medium;

        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        [Display(Name = "Created at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // URL of the attachment stored in S3 (optional)
        [Display(Name = "Attachment URL")]
        public string? AttachmentUrl { get; set; }
    }
}
