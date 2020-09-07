using System;
using System.Text.Json.Serialization;

namespace VideoManager.Models
{
    public class BaseEntity
    {
        public DateTimeOffset? CreatedDate { get; set; }

        public DateTimeOffset? ModifiedDate { get; set; }

        public int? CreatedByUserId { get; set; }

        [JsonIgnore]
        public User CreatedByUser { get; set; }

        public int? ModifiedByUserId { get; set; }

        [JsonIgnore]
        public User ModifiedByUser { get; set; }
    }
}
