using Portafolio.Models;

namespace Portafolio.Servicios
{

    public interface IRepositorioProyectos {

        List<Proyecto> obtenerProyectos();

    }
    public class RepositorioProyectos : IRepositorioProyectos
    {

        public List<Proyecto> obtenerProyectos()
        {
            return new List<Proyecto>(){
                new Proyecto
                {
                    Titulo = "Amazon",
                    Descripcion = "Proyecto .NET",
                    ImagenURL = "/imagenes/amazon.png",
                    Link = "www.Amazon.com"
                },
                new Proyecto
                {
                    Titulo = "New times",
                    Descripcion = "Proyecto React",
                    ImagenURL = "/imagenes/nyt.png",
                    Link = "www.nytimes.com"
                },
                new Proyecto
                {
                    Titulo = "Reddit",
                    Descripcion = "Proyecto Redit",
                    ImagenURL = "/imagenes/reddit.png",
                    Link = "www.reddit.com"
                },
                new Proyecto
                {
                    Titulo = "Steam",
                    Descripcion = "Proyecto videojuegos",
                    ImagenURL = "/imagenes/steam.png",
                    Link = "www.steam.com"
                }
            };
        }
    }
}
