using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Entidades;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Claims;

namespace TareasMVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ApplicationDbContext context;

        public TokenController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string email)
        {
            var resultado = await context.Users.Where(e => e.Email == email).FirstOrDefaultAsync();
           if(resultado is null)
            {
                Console.WriteLine("el objeto es nulo");
            }
            if (resultado is not null)
            {
                var token = Guid.NewGuid().ToString();
                var expiration = DateTime.UtcNow.AddMinutes(30);

                var loginToken = new LoginToken
                {
                    UserId = resultado.Id,
                    Token = token,
                    Expiration = expiration
                };

                context.LoginTokens.Add(loginToken);
                await context.SaveChangesAsync();
                await EnviarTokenPorCorreo(email, token);
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Post( string email, string token)
        {
            
            var user = await userManager.FindByEmailAsync(email);
            var Token = await context.LoginTokens.Where(t => t.Token == token).FirstOrDefaultAsync();
            if (user is not null)
            {

                if (Token.Token == token)
                {

                    var identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                    identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

                    await signInManager.SignInWithClaimsAsync(user, isPersistent: false, identity.Claims);
                    context.LoginTokens.Remove(Token);
                    await context.SaveChangesAsync();
                    return Ok(new { message = "Inicio de sesión exitoso" });
                }
                else
                {
                    return Unauthorized(new { message = "Token inválido" });
                }
            }
            return NotFound(new { message = "Correo electrónico no encontrado" });
            
        }



        public async Task EnviarTokenPorCorreo(string email, string token)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("TareaMVC", "portilloernesto0902@gmail.com"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Token de inicio de sesión";

            message.Body = new TextPart("plain")
            {
                Text = $"Su token de inicio de sesión es: {token}"
            };

            using (var client = new SmtpClient())
            {
                // Conéctate al servidor SMTP. Aquí puedes usar tu servidor SMTP
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                // Autenticarse con el servidor SMTP
                await client.AuthenticateAsync("portilloernesto0902@gmail.com", "omwvhkrnbeqnolnu");

                // Enviar el mensaje de correo electrónico
                await client.SendAsync(message);

                // Desconectarse del servidor SMTP
                await client.DisconnectAsync(true);
            }
        }
    }
}
