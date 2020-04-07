using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VideoManager.Models
{
    public class BaseEntity
    {
        public DateTimeOffset? CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public Guid? CreatedByUserId { get; set; }
        [JsonIgnore]
        public User CreatedByUser { get; set; }
        public Guid? ModifiedByUserId { get; set; }
        [JsonIgnore]
        public User ModifiedByUser { get; set; }
    }
}
