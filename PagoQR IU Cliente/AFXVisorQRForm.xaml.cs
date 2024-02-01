using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace PagoQR_IU_Cliente
{
    /// <summary>
    /// Lógica de interacción para AFXVisorQRForm.xaml
    /// </summary>
    /// 

    public partial class AFXVisorQRForm : Window
    {

        private ResultQRTransaccionTangoImagenDTO etResultQRTransaccionTangoImagenDTOActual;
        private string IDTransaccionFinanciera;
        private string EstadoVisor;
        private AFXVisorConfig config;
        private BitmapImage bitmapImage = null;

        private async Task<long> DesplegarQR(string UriServicio, string Token)
        {
            long NumError = 0;
            string Respuesta = "";
            string UriServicioSufijo = "/QRTransaccionTango/UltimoXToken?Token=" + Token;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(UriServicio + UriServicioSufijo);
            httpWebRequest.Method = "GET";
            httpWebRequest.ContentType = "application/json";
            try
            {
                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                Respuesta = streamReader.ReadToEnd();
                streamReader.Close();
                AFXServicioRespuestaEntity<List<ResultQRTransaccionTangoImagenDTO>> etAFXServicioRespuesta = JsonConvert.DeserializeObject<AFXServicioRespuestaEntity<List<ResultQRTransaccionTangoImagenDTO>>>(Respuesta);

                if (etAFXServicioRespuesta.Info.Count > 0)
                {
                    etResultQRTransaccionTangoImagenDTOActual = etAFXServicioRespuesta.Info.First<ResultQRTransaccionTangoImagenDTO>();

                    if (string.Compare(etResultQRTransaccionTangoImagenDTOActual.EstadoIU, "Verificado", true) == 0)
                    {
                        imaAFXVisorQR.Source = null;
                        lblNombre.Content = string.Empty;
                        lblNombre.Background = Brushes.Transparent;
                        lblEstado.Content = string.Empty;
                        lblEstado.Background = Brushes.Transparent;
                    }
                    else
                    {
                        if (string.Compare(etResultQRTransaccionTangoImagenDTOActual.IDTransaccionFinanciera, string.Empty, true) == 0 && (string.Compare(etResultQRTransaccionTangoImagenDTOActual.IDTransaccionFinanciera, IDTransaccionFinanciera, true) != 0 || string.Compare(etResultQRTransaccionTangoImagenDTOActual.EstadoVisor, EstadoVisor, true) != 0))
                        {
                            IDTransaccionFinanciera = etResultQRTransaccionTangoImagenDTOActual.IDTransaccionFinanciera;
                            EstadoVisor = etResultQRTransaccionTangoImagenDTOActual.EstadoVisor;
                            BitmapImage bmpDescanso = new BitmapImage();
                            bmpDescanso.BeginInit();
                            bmpDescanso.UriSource = new Uri("Esperando.png", UriKind.Relative);
                            bmpDescanso.EndInit();
                            imaAFXVisorQR.Source = bmpDescanso;
                            lblNombre.Content = etResultQRTransaccionTangoImagenDTOActual.Nombre;
                            lblNombre.Background = Brushes.Transparent;
                            lblEstado.Content = "SOLICITANDO BANCO";
                            lblEstado.Background = Brushes.Orange;
                            tbiIconoTarea.Visibility = Visibility.Collapsed;
                            this.Show();
                            this.Focus();
                            bsfImprimir.Visibility = Visibility.Collapsed;
                        }
                        else if (string.Compare(etResultQRTransaccionTangoImagenDTOActual.IDTransaccionFinanciera, IDTransaccionFinanciera, true) != 0 || string.Compare(etResultQRTransaccionTangoImagenDTOActual.EstadoVisor, EstadoVisor, true) != 0)
                        {
                            IDTransaccionFinanciera = etResultQRTransaccionTangoImagenDTOActual.IDTransaccionFinanciera;
                            EstadoVisor = etResultQRTransaccionTangoImagenDTOActual.EstadoVisor;
                            byte[] imaImagenQR = Convert.FromBase64String(etResultQRTransaccionTangoImagenDTOActual.ImagenQR);
                            var imagen = new BitmapImage();
                            using (var mem = new MemoryStream(imaImagenQR))
                            {
                                mem.Position = 0;
                                imagen.BeginInit();
                                imagen.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                                imagen.CacheOption = BitmapCacheOption.OnLoad;
                                imagen.UriSource = null;
                                imagen.StreamSource = mem;
                                imagen.EndInit();
                            }
                            imagen.Freeze();
                            imaAFXVisorQR.Source = imagen;
                            lblNombre.Content = etResultQRTransaccionTangoImagenDTOActual.Nombre;
                            lblNombre.Background = Brushes.Transparent;

                            if (string.Compare(etResultQRTransaccionTangoImagenDTOActual.EstadoVisor, "GENERADO", true) == 0)
                            {
                                lblEstado.Content = "ESPERANDO PAGO";
                                lblEstado.Background = Brushes.Orange;
                                bsfImprimir.Visibility = Visibility.Collapsed;
                            }
                            else if (string.Compare(etResultQRTransaccionTangoImagenDTOActual.EstadoVisor, "ERROR-GENERADO", true) == 0)
                            {
                                lblEstado.Content = "ERROR GENERAR QR";
                                lblEstado.Background = Brushes.Red;
                                bsfImprimir.Visibility = Visibility.Collapsed;
                            }
                            else if (string.Compare(etResultQRTransaccionTangoImagenDTOActual.EstadoVisor, "PAGADO", true) == 0 || string.Compare(etResultQRTransaccionTangoImagenDTOActual.EstadoVisor, "DESCARGADO", true) == 0)
                            {
                                lblEstado.Content = "PAGADO";
                                lblEstado.Background = Brushes.Green;
                                bsfImprimir.Visibility = Visibility.Visible;
                                bitmapImage = (BitmapImage)imaAFXVisorQR.Source;
                                AccionImprimir(true);
                            }
                            else if (string.Compare(etResultQRTransaccionTangoImagenDTOActual.EstadoVisor, "ERROR-PAGAR", true) == 0)
                            {
                                lblEstado.Content = "ERROR PAGAR";
                                lblEstado.Background = Brushes.Red;
                                bsfImprimir.Visibility = Visibility.Collapsed;
                            }
                            tbiIconoTarea.Visibility = Visibility.Collapsed;
                            this.Show();
                            this.Focus();
                            grdenviarWpp.Visibility = Visibility.Visible;

                        }
                    }
                }
                else
                {
                    imaAFXVisorQR.Source = null;
                    lblNombre.Content = string.Empty;
                    lblNombre.Background = Brushes.Transparent;
                    lblEstado.Content = string.Empty;
                    lblEstado.Background = Brushes.Transparent;
                }
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                    //WebException webException = e as WebException;
                    //WebResponse response = webException.Response;
                    //StreamReader streamReader = new StreamReader(response.GetResponseStream());
                    //Respuesta = streamReader.ReadToEnd();
                    //streamReader.Close();
                    NumError = 1;
                }
            }
            return NumError;
        }
        public AFXVisorQRForm()
        {
            InitializeComponent();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config = new AFXVisorConfig();
            config.TiempoSegundos = int.Parse(builder.Build().GetSection("VisorConfig").GetSection("TiempoSegundos").Value);
            config.UriServicio = builder.Build().GetSection("VisorConfig").GetSection("UriServicio").Value;
            config.Token = builder.Build().GetSection("VisorConfig").GetSection("Token").Value;
            tbiIconoTarea.Visibility = Visibility.Visible;
            this.Hide();
            ActualizarIU();
        }

        private long ActualizarEstadoIU(string UriServicio, string Token)
        {
            long NumError = 0;
            string Respuesta = "";
            if (string.Compare(IDTransaccionFinanciera, string.Empty, true) != 0)
            {
                string UriServicioSufijo = "/QRTransaccionTango/GuardarTrxEstadoIU?Token=" + Token;
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(UriServicio + UriServicioSufijo);
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/json";
                try
                {
                    HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                    StreamReader streamReader = new StreamReader(response.GetResponseStream());
                    Respuesta = streamReader.ReadToEnd();
                    streamReader.Close();
                    AFXServicioRespuestaEntity<long> etAFXServicioRespuesta = JsonConvert.DeserializeObject<AFXServicioRespuestaEntity<long>>(Respuesta);
                    IDTransaccionFinanciera = string.Empty;
                    EstadoVisor = string.Empty;

                }
                catch (Exception e)
                {
                    if (e is WebException)
                    {
                        //WebException webException = e as WebException;
                        //WebResponse response = webException.Response;
                        //StreamReader streamReader = new StreamReader(response.GetResponseStream());
                        //Respuesta = streamReader.ReadToEnd();
                        //streamReader.Close();
                        NumError = 1;
                    }
                }
            }
            return NumError;
        }

        private async Task<AFXServicioRespuestaEntity<ResultQRConfiguracionTerminalDTOXToken>> ObtenerQRConfiguracionTerminal(string UriServicio, string Token)
        {
            string Respuesta = "";
            string UriServicioSufijo = "/QRConfiguracion/QRConfiguracionTerminalXToken?Token=" + Token;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(UriServicio + UriServicioSufijo);
            httpWebRequest.Method = "GET";
            httpWebRequest.ContentType = "application/json";

            AFXServicioRespuestaEntity<ResultQRConfiguracionTerminalDTOXToken> etAFXServicioRespuesta = new AFXServicioRespuestaEntity<ResultQRConfiguracionTerminalDTOXToken>();
            try
            {
                HttpWebResponse response = (HttpWebResponse)(await httpWebRequest.GetResponseAsync());
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                Respuesta = streamReader.ReadToEnd();
                streamReader.Close();
                etAFXServicioRespuesta = JsonConvert.DeserializeObject<AFXServicioRespuestaEntity<ResultQRConfiguracionTerminalDTOXToken>>(Respuesta);
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                    //WebException webException = e as WebException;
                    //WebResponse response = webException.Response;
                    //StreamReader streamReader = new StreamReader(response.GetResponseStream());
                    //Respuesta = streamReader.ReadToEnd();
                    //streamReader.Close();
                    etAFXServicioRespuesta.NumError = 1;
                }
            }
            return etAFXServicioRespuesta;
        }


        public void ActualizarIU()
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(config.TiempoSegundos);
            timer.Tick += new EventHandler((object s, EventArgs a) =>
            {
                _ = DesplegarQR(config.UriServicio, config.Token);
            });
            timer.Start();
        }

        private void bsfAceptar_Click(object sender, RoutedEventArgs e)
        {
            AccionCerrar();
        }

        private void AccionCerrar()
        {

            ActualizarEstadoIU(config.UriServicio, config.Token);
            tbiIconoTarea.Visibility = Visibility.Visible;
            this.Hide();
        }

        private void tbiIconoTarea_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            tbiIconoTarea.Visibility = Visibility.Collapsed;
            this.Show();
            this.Focus();
        }

        private string ObtenerImpresora(string Impresora,string CadenaConexion)
        {
            AKERPNetTangoClass AKERPNetTango = new AKERPNetTangoClass(CadenaConexion);
            string PuertoImpresora = AKERPNetTango.GVA43SeleccionarPuertoImpresora(etResultQRTransaccionTangoImagenDTOActual.TipoComprobante, etResultQRTransaccionTangoImagenDTOActual.NumeroComprobante);
            if (string.Compare(PuertoImpresora, string.Empty, true) != 0)
            {
                System.Management.ManagementObjectCollection moReturn;
                System.Management.ManagementObjectSearcher moSearch;
                moSearch = new System.Management.ManagementObjectSearcher("Select * from Win32_Printer");
                moReturn = moSearch.Get();

                foreach (System.Management.ManagementObject mo in moReturn)
                {
                    string NombrePuerto = (string)mo["PortName"];
                    NombrePuerto = NombrePuerto.Replace(":", "");
                    PuertoImpresora = PuertoImpresora.Replace(":", "");
                    if (string.Compare(NombrePuerto, PuertoImpresora, true) == 0)
                    {
                        Impresora = (string)mo["Name"];
                        break;
                    }
                }
            }
            AKERPNetTango.CerrarConexion();
            return Impresora;
        }

        private void bsfImprimir_Click(object sender, RoutedEventArgs e)
        {
            _ = AccionImprimir();
        }

        private async Task AccionImprimir(bool ejecutarAccionCerrar = false)
        {
            AFXServicioRespuestaEntity<ResultQRConfiguracionTerminalDTOXToken> etAFXServicioRespuesta = await ObtenerQRConfiguracionTerminal(config.UriServicio, config.Token);
            if (bitmapImage != null)
            {
                PrintDocument pd = new PrintDocument();
                if (etAFXServicioRespuesta.NumError == 0)
                {
                    pd.PrinterSettings.PrinterName = ObtenerImpresora(pd.PrinterSettings.PrinterName, etAFXServicioRespuesta.Info.CadenaConexion);
                }
                //Margins margenes = new Margins(0, 0, 0, 0);
                //pd.DefaultPageSettings.Margins = margenes;
                //pd.DefaultPageSettings.Landscape = false;
                //pd.OriginAtMargins = true;
                pd.PrintPage += ImprimirPagina;
                pd.Print();
                if (ejecutarAccionCerrar)
                {
                    this.AccionCerrar();
                }
            }
        }

        private void ImprimirPagina(object o, PrintPageEventArgs e)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                float AnchoNuevo =  (bitmap.Width / bitmap.HorizontalResolution);
                float AltoNuevo = (bitmap.Height / bitmap.VerticalResolution);

                //float AnchoFactor = (e.MarginBounds.Width + 150) / AnchoNuevo;
                //float AltoFactor = (e.MarginBounds.Height + 150) / AltoNuevo;

                float AnchoFactor = 170 / AnchoNuevo;
                float AltoFactor = 170 / AltoNuevo;

                if (AnchoFactor < AltoFactor)
                {
                    AnchoNuevo = AnchoNuevo * AnchoFactor;
                    AltoNuevo = AltoNuevo * AnchoFactor;
                }
                else
                {
                    AnchoNuevo = AnchoNuevo * AltoFactor;
                    AltoNuevo = AltoNuevo * AltoFactor;
                }
                e.Graphics.DrawImage(bitmap, 0, 0, (int)AnchoNuevo, (int)AltoNuevo);

                System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 10);
                System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

                System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat();
                drawFormat.FormatFlags = System.Drawing.StringFormatFlags.NoWrap;

                String drawString = etResultQRTransaccionTangoImagenDTOActual.Nombre + " " + etResultQRTransaccionTangoImagenDTOActual.Monto.ToString("F") + " " + etResultQRTransaccionTangoImagenDTOActual.Moneda;
                e.Graphics.DrawString(drawString, drawFont, drawBrush, 0, (int)AltoNuevo + 2, drawFormat);
                drawString = "----";
                e.Graphics.DrawString(drawString, drawFont, drawBrush, 0, (int)AltoNuevo + 12, drawFormat);

            }
        }


        private void bsfEnviarWpp_Click(object sender, RoutedEventArgs e)
        {
            if (txtNumeroWpp.Text.Length < 7)
            {
                txtberror.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                txtberror.Visibility = Visibility.Collapsed;
            }
            SendWppMessage(txtNumeroWpp.Text.Trim(), config.Token);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SendWppMessage(string number, string token)
        {
            string wppMessage = "Ingresa a este enlace para pagar con QR https://pagoqr.com.bo:9001/Api00/QRTransaccionTango/HtmlQR?Token=" + token;
            string encodedMessage = Uri.EscapeDataString(wppMessage);
            string encodedPhoneNumber = Uri.EscapeDataString(number);

            string wppUrl = $"https://api.whatsapp.com/send?phone={encodedPhoneNumber}&text={encodedMessage}";
            Process.Start(new ProcessStartInfo
            {
                FileName = wppUrl,
                UseShellExecute = true
            });
        }

    }
}
