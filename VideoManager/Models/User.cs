using System;
using System.ComponentModel.DataAnnotations;

namespace VideoManager.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
    }
}
