using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VideoManager.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string EmailAddress { get; set; }

        [Required]
        public string Auth0Id { get; set; }
    }
}
