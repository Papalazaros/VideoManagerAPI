using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VideoManager.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        public string Auth0Id { get; set; }
    }
}
