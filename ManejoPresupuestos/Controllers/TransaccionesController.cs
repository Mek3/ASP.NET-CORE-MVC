﻿using AutoMapper;
using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ManejoPresupuestos.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IServiciosUsuarios serviciosUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IServicioReportes servicioReportes;
        private readonly IMapper mapper;

        public TransaccionesController(IServiciosUsuarios serviciosUsuarios,
                                       IRepositorioCuentas repositorioCuentas,
                                       IRepositorioCategorias repositorioCategorias,
                                       IRepositorioTransacciones repositorioTransacciones,
                                       IServicioReportes servicioReportes,
                                       IMapper mapper)
        {
            this.serviciosUsuarios = serviciosUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
            this.repositorioTransacciones = repositorioTransacciones;
            this.servicioReportes = servicioReportes;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index(int mes, int anyo)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            
            var modelo = await servicioReportes
                            .ObtenerReporteTransaccionesDetalladas(usuarioId, mes, anyo, ViewBag); 


            return View(modelo);
        }
        public async Task<IActionResult> Semanal(int mes, int anyo)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            IEnumerable<ResultadoObtenerPorSemana> transaccionesPorSemana = await servicioReportes.ObtenerReporteSemanal(usuarioId, mes, anyo, ViewBag);

            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select(x => 
                            new ResultadoObtenerPorSemana() 
                            {
                                Semana= x.Key,
                                Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso)
                                            .Select(x => x.Monto).FirstOrDefault(),
                                Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto)
                                            .Select(x => x.Monto).FirstOrDefault(),
                            }).ToList();

            if (anyo == 0 || mes == 0) 
            {
                var hoy = DateTime.Today;
                anyo= hoy.Year;
                mes= hoy.Month;
            }

            var fechaReferencia = new DateTime(anyo, mes, 1);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(anyo, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(anyo, mes, diasSegmentados[i].Last());  
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana== semana);

                if (grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                }
                else 
                {
                    grupoSemana.FechaInicio = fechaInicio;   
                    grupoSemana.FechaFin = fechaFin;
                }
            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

            var modelo = new ReporteSemanalViewModel();
            modelo.FechaReferencia = fechaReferencia;   
            modelo.TransaccionesPorSemana = agrupado;

            return View(modelo);
        }

        public async Task<IActionResult> Mensual(int anyo)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();

            if (anyo <= 0 || anyo <= 1900) 
            {
                anyo = DateTime.Today.Year;
            }

            var transaccionesPorMes = await repositorioTransacciones
                                            .ObtenerPorMes(usuarioId, anyo);
            if (transaccionesPorMes is null) { return RedirectToAction("NoEncontrado", "Home"); }
          
            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes)
                                    .Select(x => new ResultadoObtenerPorMes()
                                    {
                                        Mes = x.Key,
                                        Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso)
                                                        .Select(x => x.Monto).FirstOrDefault(),
                                        Gasto = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto)
                                                        .Select(x => x.Monto).FirstOrDefault()

                                    }).ToList();

            for (int mes = 1; mes <= 12; mes++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);

                var fechaReferencia = new DateTime(anyo, mes, 1);

                if (transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ResultadoObtenerPorMes()
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia,
                    });
                }
                else 
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }

            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

            var modelo = new ReporteMensualViewModel();
            
            modelo.TransaccionesPorMes = transaccionesAgrupadas;
            modelo.Anyo = anyo;

            return View(modelo);
        }


        public async Task<IActionResult> ExcelReporte()
        {
            return View();
        }

        public async Task<IActionResult> Calendario()
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

            modelo.UsuarioId = usuarioId;

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
        public async Task<IActionResult> Editar(int Id, string UrlRetorno = null)
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
            modelo.UrlRetorno= UrlRetorno;

            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                return View(modelo);
            }

            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if (cuenta == null)
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

            if(string.IsNullOrEmpty(modelo.UrlRetorno))
                return RedirectToAction("Index");
            else 
                return LocalRedirect(modelo.UrlRetorno);
        }


        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string UrlRetorno = null)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);

            if (transaccion == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTransacciones.Borrar(id);

            if (string.IsNullOrEmpty(UrlRetorno))
                return RedirectToAction("Index");
            else
                return LocalRedirect(UrlRetorno);

        }
    }
}