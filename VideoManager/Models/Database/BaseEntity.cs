using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VideoManager.Models
{
    public class BaseEntity
    {
        public DateTimeOffset? CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }
        public Guid? ModifiedByUserId { get; set; }
        public User ModifiedByUser { get; set; }
    }
}
