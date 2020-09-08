using System;

namespace VideoManager.Models
{
    public class AuthUser
    {
        public string sub { get; set; }
        public string given_name { get; set; }
        public string nickname { get; set; }
        public string name { get; set; }
        public string picture { get; set; }
        public string locale { get; set; }
        public DateTime updated_at { get; set; }
        public string email { get; set; }
        public bool email_verified { get; set; }
    }
}
