using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Borrar(int Id);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioCuentas: IRepositorioCuentas
    {
        private readonly string connectionString;
        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta) 
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"
                                INSERT INTO Cuentas (Nombre, TipoCuentaId, Descripcion, Balance)
                                VALUES (@Nombre, @TipoCuentaId, @Descripcion, @Balance);
                                SELECT SCOPE_IDENTITY();", cuenta);

            cuenta.Id = id;

        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var con = new SqlConnection(connectionString);
            return await con.QueryAsync<Cuenta>(@"
                                SELECT c.Id, c.Nombre, Balance, tc.Nombre AS TipoCuenta
                                FROM Cuentas c
                                INNER JOIN TiposCuentas tc
                                on  c.TipoCuentaId = tc.Id
                                WHERE tc.UsuarioId = @UsuarioId
                                ORDER BY tc.Orden;", new {usuarioId});
        }

        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId) 
        {
            using var con = new SqlConnection(connectionString);
            return await con.QueryFirstOrDefaultAsync<Cuenta>(@"
                                SELECT c.Id, c.Nombre, Balance, c.TipoCuentaId
                                FROM Cuentas c
                                INNER JOIN TiposCuentas tc
                                on  c.TipoCuentaId = tc.Id
                                WHERE tc.UsuarioId = @UsuarioId AND c.Id = @Id",
                                new { id, usuarioId });
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta) 
        {
            using var con = new SqlConnection(connectionString);
            await con.ExecuteAsync(@"
                        UPDATE Cuentas 
	                    SET Nombre = @Nombre, TipoCuentaId = @TipoCuentaId, Balance = @Balance, Descripcion = @Descripcion
	                    WHERE Id = @Id; ", cuenta);
        }

        public async Task Borrar(int Id) 
        {
            using var con = new SqlConnection(connectionString);
            await con.ExecuteAsync(@"
                                DELETE FROM Cuentas
                                WHERE Id=@Id;", new { Id });
        }

    }
}
