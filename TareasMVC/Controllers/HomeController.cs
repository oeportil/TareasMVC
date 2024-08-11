using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Diagnostics;
using TareasMVC.Models;
using TareasMVC.Servicios;
using TareasMVC.Entidades;

namespace TareasMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStringLocalizer<HomeController> localizer;
        private readonly ApplicationDbContext context;
        private readonly IServicioUsuarios servicioUsuarios;

        public HomeController(ILogger<HomeController> logger,
            IStringLocalizer<HomeController> localizer,
            ApplicationDbContext context, IServicioUsuarios servicioUsuarios)
        {
            _logger = logger;
            this.localizer = localizer;
            this.context = context;
            this.servicioUsuarios = servicioUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            Console.WriteLine(usuarioId);
            var tareas = await context.Tareas
                .Where(t => t.UsuarioCreacionId == usuarioId)
            .OrderBy(t => t.Orden).ToListAsync();
            return View(tareas);
        }

        public IActionResult CrearTarea()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CrearTarea(Tarea taremodel)
        {
            if (!ModelState.IsValid)
            {
                return View(taremodel);
            }
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            taremodel.UsuarioCreacionId = usuarioId;
            await context.Tareas.AddAsync(taremodel);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditarTarea(int id)
        {
            var resultado = await context.Tareas.Where(x => x.Id == id).FirstOrDefaultAsync(); 
            return View(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> EditarTarea(Tarea modelo)
        {
            if(!ModelState.IsValid){
                return View(modelo);
            }
            var tarea = await context.Tareas.FindAsync(modelo.Id);
            if(tarea is null){
                return NotFound();
            }
            tarea.Titulo = modelo.Titulo;
            tarea.Descripcion = modelo.Descripcion;
            await context.SaveChangesAsync();
            return RedirectToAction("index");
        }
        public async Task<IActionResult> Eliminar(int id){
            var modelo = await context.Tareas.Where(x => x.Id == id).FirstOrDefaultAsync();
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(Tarea model)
        {
            var modelo = await context.Tareas.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
            context.Tareas.Remove(modelo);
            await context.SaveChangesAsync();
            return RedirectToAction("index");
        }

        [HttpPost]
        public IActionResult CambiarIdioma(string cultura, string urlRetorno)
        {
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultura)),
                 new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(5) }
                );

            return LocalRedirect(urlRetorno);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}