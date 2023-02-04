using AutoMapper;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Runtime.CompilerServices;

namespace ManejoPresupuestos.Models
{
    public class TransaccionesController : Controller
    {
        private readonly IServiciosUsuarios serviciosUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IMapper mapper;

        public TransaccionesController(IServiciosUsuarios serviciosUsuarios, 
                                       IRepositorioCuentas repositorioCuentas,
                                       IRepositorioCategorias repositorioCategorias,
                                       IRepositorioTransacciones repositorioTransacciones,
                                       IMapper mapper) 
        {
            this.serviciosUsuarios = serviciosUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
            this.repositorioTransacciones = repositorioTransacciones;
            this.mapper = mapper;
        }


        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Crear() 
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionVieWModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionVieWModel modelo) 
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            
            if (!ModelState.IsValid) 
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                
                return View(modelo);
            }

            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId); 
            
            if (cuenta is null) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

           modelo.UsuarioId= usuarioId;

            if (modelo.TipoOperacionId == TipoOperacion.Gasto) 
            {
                modelo.Monto *= -1;
            }

            await repositorioTransacciones.Crear(modelo);

            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId) 
        {
            var cuentas = await repositorioCuentas.Buscar(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
        //private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId,
        //                                                    TipoOperacion tipoOperacion)
        //{
        //    var categorias = await repositorioCategorias.Obtener(usuarioId, tipoOperacion);
        //    return categorias.Select(x => new SelectListItem(x.Nombre,x.Id.ToString()));    
        //}

        //[HttpPost]
        //public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion) 
        //{
        //    var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
        //    var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
        //    return Ok(categorias);

        //}

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId,
           TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategorias.Obtener(usuarioId, tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int Id) 
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorId(Id, usuarioId);

            if (transaccion == null) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transaccion);

            modelo.MontoAnterior = modelo.Monto;

            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }
           

            modelo.CuentaIdAnterior = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);

            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo) 
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();

            if(!ModelState.IsValid)
            {
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                return View(modelo);
            }

            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
           
            if(cuenta== null) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);
            
            if (categoria == null) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var transaccion = mapper.Map<Transaccion>(modelo);

            if (transaccion.TipoOperacionId == TipoOperacion.Gasto) 
            {
                transaccion.Monto *= -1;
            }

            await repositorioTransacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaIdAnterior);

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Borrar(int id) {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);

            if(transaccion== null)
            {
               return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTransacciones.Borrar(id);

            return RedirectToAction("Index");
        }
    }
}
