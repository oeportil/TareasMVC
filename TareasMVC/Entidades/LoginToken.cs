using Microsoft.AspNetCore.Identity;

namespace TareasMVC.Entidades
{
    public class LoginToken
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public IdentityUser User { get; set; }
    }
}
