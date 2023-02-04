using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public interface IRepositorioTiposCuentas {
       Task Crear(TipoCuenta tipoCuenta);
        Task Actualizar(TipoCuenta tipoCuenta);

       Task<bool> Existe(string nombre, int usuarioId);
       Task<IEnumerable<TipoCuenta>> ObtenerTipoCuentaPorIdUsuario(int UsuarioId);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
        Task Borrar(int id);
        Task Ordenar(IEnumerable<TipoCuenta> tipoCuentas);
    }
    public class RepositorioTiposCuentas: IRepositorioTiposCuentas
    {
        private readonly string connectionString;

        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);

            var id = await connection.QuerySingleAsync<int>
                                                ("TiposCuentas_Insertar",
                                                new
                                                {
                                                    usuarioId = tipoCuenta.UsuarioId,
                                                    nombre = tipoCuenta.Nombre
                                                },
                                                commandType: System.Data.CommandType.StoredProcedure);

            tipoCuenta.Id= id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                                    @"select 1
                                    from TiposCuentas
                                    where Nombre = @Nombre and UsuarioId = @UsuarioId;",
                                    new { nombre, usuarioId });
            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> ObtenerTipoCuentaPorIdUsuario(int UsuarioId) 
        {
            using var con = new SqlConnection(connectionString);
            return await con.QueryAsync<TipoCuenta>(@"SELECT id, Nombre, Orden 
                                                        FROM TiposCuentas
                                                        WHERE UsuarioId = @UsuarioId ORDER BY Orden;", new {UsuarioId});
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);

             await connection.ExecuteAsync( @"UPDATE TiposCuentas 
                                            SET Nombre = @Nombre
                                            WHERE Id = @Id;",  tipoCuenta);
        }

        //public async Task<TipoCuenta> ObtenerPorId(int Id, int UsuarioId) 
        //{
        //    using var connection = new SqlConnection(connectionString);
        //    return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(
        //                                        @"SELECT Id, Nombre, Orden
        //                                        FROM TiposCuentas
        //                                        WHERE Id = @Id 
        //                                        AND UsuarioId = @UsuarioId",
        //                                        new { Id, UsuarioId });
        //}
        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
           using var connection = new SqlConnection(connectionString);
           var tipoCuenta = await connection.QueryFirstOrDefaultAsync<TipoCuenta>(@"
                                                                SELECT Id, Nombre, Orden
                                                                FROM TiposCuentas
                                                                WHERE Id = @Id AND UsuarioId = @UsuarioId",
                                                                new { id, usuarioId });

            return tipoCuenta;
        }

        public async Task Borrar(int id) 
        {
            using var con = new SqlConnection(connectionString);
            await con.ExecuteAsync(@"
                            DELETE TiposCuentas
                            WHERE Id = @Id ", new { id});
        }

        public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentas) 
        {
            using var con = new SqlConnection(connectionString);
            var query = "UPDATE TiposCuentas set Orden=@Orden WHERE Id = @Id";
            await con.ExecuteAsync(query, tipoCuentas);
        }


    }
}
