namespace ManejoPresupuestos.Models
{
    public class TransaccionActualizacionViewModel: TransaccionCreacionVieWModel
    {
        public int CuentaIdAnterior { get; set; }
        public decimal MontoAnterior { get; set; }
        public string UrlRetorno { get; set; }

    }
}
