using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VideoManager.Models
{
    public class Room
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public Guid? OwnerId { get; set; }
        public User Owner { get; set; }

        [Required]
        public bool IsPrivate { get; set; } = true;

        [Required]
        public Guid CreatedById { get; set; }
        [Required]
        public User CreatedBy { get; set; }

        public ICollection<Video> Videos { get; set; }
    }
}
