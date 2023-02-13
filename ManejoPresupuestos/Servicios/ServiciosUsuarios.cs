using System.Security.Claims;

namespace ManejoPresupuestos.Servicios
{

    public interface IServiciosUsuarios 
    {
        int ObtenerUsuarioId();
    }

    public class ServiciosUsuarios: IServiciosUsuarios
    {
        private readonly HttpContext httpContext;

        public ServiciosUsuarios(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContext = httpContextAccessor.HttpContext;
        }
        public int ObtenerUsuarioId() 
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                var idClaim = httpContext.User.Claims
                            .Where(x => x.Type == ClaimTypes.NameIdentifier)
                            .FirstOrDefault();

                var id = int.Parse(idClaim.Value);
                return id;
            }
            else 
            {
                throw new ApplicationException("El usuario no está autenticado");
            } 

            return 1;
        }

    }
}
