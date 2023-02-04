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
        Task<Transaccion> ObtenerPorId(int Id, int usuarioId);
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

        public async Task Borrar(int Id) 
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"Transaccion_Borrar", new { Id },
                        commandType: System.Data.CommandType.StoredProcedure);        
        }


    }
}
