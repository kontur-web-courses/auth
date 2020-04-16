using System;
using System.ComponentModel.DataAnnotations;

namespace PhotosApp.Services.TicketStores
{
    public class TicketEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public byte[] Value { get; set; }
        public DateTimeOffset? LastActivity { get; set; }
        public DateTimeOffset? Expires { get; set; }
    }
}