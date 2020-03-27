using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VideoManager.Models
{
    public class Room
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? Owner { get; set; }

        [Required]
        public bool IsPrivate { get; set; } = true;

        [Required]
        public Guid CreatedById { get; set; }
        [Required]
        public User CreatedByUser { get; set; }

        public ICollection<Video> Videos { get; set; }
    }
}
