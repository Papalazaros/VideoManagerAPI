using System.ComponentModel.DataAnnotations;

namespace VideoManager.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string Auth0Id { get; set; }

        public string Email { get; set; }

        public User(string auth0Id, string email)
        {
            Auth0Id = auth0Id;
            Email = email;
        }
    }
}
