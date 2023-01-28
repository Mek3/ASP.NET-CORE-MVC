using Dapper;
using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Runtime.CompilerServices;
//using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServiciosUsuarios serviciosUsuarios;

        //private readonly string connectionString;
        //public TiposCuentasController(IConfiguration configuration)
        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServiciosUsuarios serviciosUsuarios)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.serviciosUsuarios = serviciosUsuarios;

            //connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        public async Task<IActionResult> Index() 
        {
            var idUsuario = serviciosUsuarios.ObtenerUsuarioId();

            var tipoCuenta = await repositorioTiposCuentas.ObtenerTipoCuentaPorIdUsuario(idUsuario);  

            return View(tipoCuenta);
        }
        public IActionResult Crear()
        {
            //using (var connection = new SqlConnection(connectionString)) {
            //    var query = connection.Query("SELECT 1").FirstOrDefault();
            //}
                return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta) 
        {
            if(!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId= serviciosUsuarios.ObtenerUsuarioId();

            var yaExisteTipoCuenta = 
                await repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);

            if (yaExisteTipoCuenta) 
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre),
                                    $"El nombre {tipoCuenta.Nombre} ya existe.");
                return View(tipoCuenta);
            }
            await repositorioTiposCuentas.Crear(tipoCuenta);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre) 
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var existe = await repositorioTiposCuentas.Existe(nombre, usuarioId);

            if (existe) {
                return Json($"El nombre {nombre} ya existe");
            }

            return Json(true);

        }

        [HttpGet]
        public async Task<ActionResult> Editar(int id) {

            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId( id, usuarioId);

            if (tipoCuenta == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta); 
        }

        [HttpPost]
        public async Task<ActionResult> Editar(TipoCuenta tipoCuenta) 
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var existe = await repositorioTiposCuentas.ObtenerPorId(tipoCuenta.Id, usuarioId);

            if (existe is null) 
            {
                return RedirectToAction("NoEncontrado", "Home");

            }
            await repositorioTiposCuentas.Actualizar(tipoCuenta);
            
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<ActionResult> Borrar(int id) 
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);

            if (tipoCuenta == null) {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<ActionResult> BorrarTipoCuenta(int id) 
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id, usuarioId);

            if (tipoCuenta == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Borrar(id);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids) 
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();

            var tiposCuentas = await repositorioTiposCuentas.ObtenerTipoCuentaPorIdUsuario(usuarioId);

            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

            var idsTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();

            if (idsTiposCuentasNoPertenecenAlUsuario.Count > 0) 
            {
                return Forbid();
            }

            var tiposCuentasOrdenadas = ids.Select((valor, indice) =>
            new TipoCuenta() { Id = valor, Orden = indice + 1 }).AsEnumerable();
            //  });
            await repositorioTiposCuentas.Ordenar(tiposCuentasOrdenadas);
            return Ok();
        }

    }
}
