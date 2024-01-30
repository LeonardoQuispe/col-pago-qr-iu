using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagoQR_IU_Cliente
{
    public class AKERPNetTangoClass
    {
        private SqlTransaction sqlTransaccion;
        private SqlConnection sqlConexion;
        private SqlCommand sqlComando;
        private SqlDataAdapter sqlAdaptador;
        public AKERPNetTangoClass(SqlConnection isqlConexion)
        {
            sqlConexion = isqlConexion;
            sqlComando = sqlConexion.CreateCommand();
            sqlAdaptador = new SqlDataAdapter(sqlComando);
        }
        public AKERPNetTangoClass(string CadenaConexion)
        {
            SqlConnection isqlConexion;
            isqlConexion = new SqlConnection(CadenaConexion);
            isqlConexion.Open();
            sqlConexion = isqlConexion;
            sqlComando = sqlConexion.CreateCommand();
            sqlAdaptador = new SqlDataAdapter(sqlComando);
        }
        public AKERPNetTangoClass(string ServidorSQL, string BaseSQL, string UsuarioSQL, string ClaveSQL)
        {
            string CadenaConexion;
            CadenaConexion = "Server=@ServidorSQL@;UID=@UsuarioSQL@;password=@ClaveSQL@;database=@BaseSQL@";
            CadenaConexion = CadenaConexion.Replace("@ServidorSQL@", ServidorSQL);
            CadenaConexion = CadenaConexion.Replace("@UsuarioSQL@", UsuarioSQL);
            CadenaConexion = CadenaConexion.Replace("@ClaveSQL@", ClaveSQL);
            CadenaConexion = CadenaConexion.Replace("@BaseSQL@", BaseSQL);

            SqlConnection isqlConexion;
            isqlConexion = new SqlConnection(CadenaConexion);
            isqlConexion.Open();
            sqlConexion = isqlConexion;
            sqlComando = sqlConexion.CreateCommand();
            sqlAdaptador = new SqlDataAdapter(sqlComando);
        }
        public SqlConnection Conexion
        {
            get
            {
                return (sqlConexion);
            }
            set
            {
                sqlConexion = value;
                sqlComando = sqlConexion.CreateCommand();
                sqlAdaptador = new SqlDataAdapter(sqlComando);
            }
        }
        public long EjecutarNonQuery(string Sql)
        {
            long NumError;
            NumError = 0;
            sqlComando.CommandType = CommandType.Text;
            sqlComando.CommandText = Sql;
            sqlComando.Transaction = sqlTransaccion;
            sqlComando.ExecuteNonQuery();
            return (NumError);
        }
        public long Transaccion()
        {
            long NumError;
            NumError = 0;
            sqlComando.Transaction = sqlTransaccion;
            sqlTransaccion = sqlConexion.BeginTransaction();
            return (NumError);
        }
        public long RollBack()
        {
            long NumError;
            NumError = 0;
            sqlTransaccion.Rollback();
            return (NumError);
        }
        public long Commit()
        {
            long NumError;
            NumError = 0;
            sqlTransaccion.Commit();
            return (NumError);
        }

        public long CerrarConexion()
        {
            long NumError = 0;
            try
            {
                if (sqlConexion.State == ConnectionState.Open)
                {
                    sqlConexion.Close();
                }
            }
            catch (Exception)
            {
                NumError = 1;
            }
            return NumError;
        }

        public string GVA43SeleccionarPuertoImpresora(string TipoDoc, string NumDoc)
        {
            DataTable dtResultado;
            string PuertoImpresora = string.Empty;
            try
            {

                string strConsulta = string.Empty;
                strConsulta = strConsulta + "SELECT b.Destino as Destino FROM GVA12 a, GVA43 b WHERE a.Talonario = b.Talonario and T_COMP = '" + TipoDoc + "' AND N_COMP = '" + NumDoc + "'";
                dtResultado = new DataTable();
                dtResultado.TableName = "GVA43";
                sqlComando.Parameters.Clear();
                sqlComando.CommandType = CommandType.Text;
                sqlComando.CommandText = strConsulta;
                sqlAdaptador.Fill(dtResultado);
                if (dtResultado.Rows.Count > 0)
                {
                    PuertoImpresora = (string)dtResultado.Rows[0]["Destino"];
                }

            }
            catch (Exception)
            {
                PuertoImpresora = string.Empty;
            }
            return (PuertoImpresora);
        }
    }
}
