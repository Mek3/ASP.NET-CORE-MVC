using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal MontoAnterior, int CuentaIdAnterior);
        Task Borrar(int Id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int Id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int anyo);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    }
    public class RepositorioTransacciones: IRepositorioTransacciones
    {
        private readonly string connectionString;

        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion) 
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"Transaccion_Insertar",
                                new
                                {
                                    transaccion.UsuarioId,
                                    transaccion.FechaTransaccion,
                                    transaccion.Monto,
                                    transaccion.CategoriaId,
                                    transaccion.CuentaId,
                                    transaccion.Nota
                                },
                                commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }
        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(
                                ObtenerTransaccionesPorCuenta modelo) 
        {
            using var con = new SqlConnection(connectionString);
            return await con.QueryAsync<Transaccion>(@"
                                    SELECT tr.Id, tr.Monto, tr.FechaTransaccion,
                                    cat.Nombre as Categoria, cu.Nombre as cuenta, TipoOperacionId
                                    FROM Transacciones tr 
                                    INNER JOIN Categorias cat 
                                    on cat.Id = tr.CategoriaId 
                                    INNER JOIN Cuentas cu 
                                    ON cu.Id = tr.CuentaId
                                    WHERE tr.CuentaId = @CuentaId AND tr.UsuarioId = @UsuarioId
                                    AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin;", modelo);
        }
        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(
                                ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var con = new SqlConnection(connectionString);
            return await con.QueryAsync<Transaccion>(@"
                                    SELECT tr.Id, tr.Monto, tr.FechaTransaccion,
                                    cat.Nombre as Categoria, cu.Nombre as cuenta, TipoOperacionId
                                    FROM Transacciones tr 
                                    INNER JOIN Categorias cat 
                                    on cat.Id = tr.CategoriaId 
                                    INNER JOIN Cuentas cu 
                                    ON cu.Id = tr.CuentaId
                                    WHERE  tr.UsuarioId = @UsuarioId
                                    AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                                    ORDER BY tr.FechaTransaccion DESC;", modelo);
        }


        public async Task Actualizar(Transaccion transaccion, decimal MontoAnterior, int CuentaIdAnterior) 
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(@"Transaccion_Actualizar", new
            {
                transaccion.Id,
                transaccion.FechaTransaccion,
                transaccion.Monto,
                MontoAnterior,
                transaccion.CategoriaId,
                transaccion.CuentaId,
                CuentaIdAnterior,
                transaccion.Nota
            },
            commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int Id, int usuarioId) 
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(@"
                        SELECT tr.*, cat.TipoOperacionId
                        FROM Transacciones tr
                        inner join Categorias cat 
                        ON cat.Id = tr.CategoriaId
                        where tr.Id=@Id 
                        and tr.UsuarioId = @UsuarioId;
                        ",
                        new { Id, usuarioId });
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>>
            ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            
            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"
                    SELECT DATEDIFF(d, @fechaInicio, tr.FechaTransaccion)/7 +1 AS Semana, 
		            SUM(tr.Monto) as Monto, cat.TipoOperacionId
                    FROM Transacciones tr
                    INNER JOIN Categorias cat ON Cat.Id = tr.CategoriaId
                    WHERE tr.UsuarioId = @usuarioId AND 
                    tr.FechaTransaccion BETWEEN @fechaInicio and @fechaFin
                    Group by DATEDIFF(d, @fechaInicio, tr.FechaTransaccion)/7, cat.TipoOperacionId;", modelo);
        }
        

        public async Task<IEnumerable<ResultadoObtenerPorMes>> 
            ObtenerPorMes(int usuarioId, int anyo)
        {
            using var connection = new SqlConnection(connectionString);
            
            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"
                    SELECT Month(tr.FechaTransaccion) as mes,  
		            SUM(tr.Monto) as Monto , cat.TipoOperacionId
                    FROM Transacciones tr
                    INNER JOIN Categorias cat ON Cat.Id = tr.CategoriaId
                    WHERE tr.UsuarioId = @usuarioId AND 
                    YEAR(tr.FechaTransaccion) = @Anyo
					Group by Month(tr.FechaTransaccion) , cat.TipoOperacionId;",
                    new {usuarioId, anyo });
        }


        public async Task Borrar(int Id) 
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"Transaccion_Borrar", new { Id },
                        commandType: System.Data.CommandType.StoredProcedure);        
        }


    }
}
