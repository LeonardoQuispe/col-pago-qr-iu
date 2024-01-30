using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagoQR_IU_Cliente
{
    public class AFXServicioRespuestaEntity<T>
    {
        public T Info { get; set; }
        public long NumError { get; set; } = 0;
        public List<string> Mensajes { get; set; } = new List<string>();
    }

    public class ResultQRTransaccionTangoImagenDTO
    {
        public int ID_QRTransaccionTango { get; set; }
        public string EntidadFinanciera { get; set; }
        public string Moneda { get; set; }
        public decimal Monto { get; set; }
        public string GlosaFinanciera { get; set; }
        public string IDTransaccionFinanciera { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public DateTime FechaDocGeneracionQR { get; set; }
        public string Estado { get; set; }
        public string Mensaje { get; set; }
        public string EstadoVisor { get; set; }
        public DateTime FechaDocVisorFin { get; set; }
        public string TipoComprobante { get; set; }
        public string NumeroComprobante { get; set; }
        public DateTime FechaDocSolicitud { get; set; }
        public string Terminal { get; set; }
        public string Llave { get; set; }
        public string Nombre { get; set; }
        public string Usuario { get; set; }
        public string ImagenQR { get; set; }
        public string EstadoIU { get; set; }
    }
    public class ResultQRConfiguracionTerminalDTOXToken
    {
        public int ID_QRConfiguracionTerminal { get; set; }
        public string CadenaConexion { get; set; }
    }

}