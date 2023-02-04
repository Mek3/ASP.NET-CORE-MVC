using AutoMapper;
using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuestos.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServiciosUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IMapper mapper;

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,
                                IServiciosUsuarios servicioUsuarios,
                                IRepositorioCuentas repositorioCuentas,
                                IMapper mapper)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> Index() 
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuentasConTipoCuenta = await repositorioCuentas.Buscar(usuarioId);

            var modelo = cuentasConTipoCuenta
                .GroupBy(x => x.TipoCuenta)
                .Select(grupo => new IndiceCuentasViewModel 
                {
                    TipoCuenta = grupo.Key,
                    Cuentas = grupo.AsEnumerable()
                }).ToList();


            return View(modelo);  
        }
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var modelo = new CuentaCreacionViewModel();
            modelo.TiposCuentas = await obtenerTiposCuentas(usuarioId);


            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta) 
        {
            var usuarioid = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuenta.TipoCuentaId, usuarioid);
            if (tipoCuenta == null) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if (!ModelState.IsValid) 
            {
                cuenta.TiposCuentas = await obtenerTiposCuentas(usuarioid);
                return View(cuenta);
            }

            await repositorioCuentas.Crear(cuenta);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int id) 
        {
            var usuarioid = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioid);
            
            if(cuenta is null) 
                return RedirectToAction("NoEncontrado", "Home");

            var modelo = mapper.Map<CuentaCreacionViewModel>(cuenta);

            modelo.TiposCuentas = await obtenerTiposCuentas(usuarioid);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuentaEditar)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(cuentaEditar.Id, usuarioId);

            if(cuenta== null)
                return RedirectToAction("NoEncontrado", "Home");

            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuentaEditar.TipoCuentaId, usuarioId);
           
            if(tipoCuenta is null) 
                return RedirectToAction("NoEncontrado", "Home");

            await repositorioCuentas.Actualizar(cuentaEditar);
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Borrar(int id) 
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);

            if(cuenta is null) 
                return RedirectToAction("NoEncontrado", "Home");


            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id) 
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);

            if (cuenta is null)
                return RedirectToAction("NoEncontrado", "Home");


            await repositorioCuentas.Borrar(id);

            return RedirectToAction("Index");
        }


        private async Task<IEnumerable<SelectListItem>> obtenerTiposCuentas(int usuarioId) 
        {
            var tiposCuentas = await repositorioTiposCuentas.ObtenerTipoCuentaPorIdUsuario(usuarioId);
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
    }
}
