using Modelos.Conexion_DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Modelos.Conexion_DB.Conexion;

namespace Modelos.Entidades
{
    public class Clientes
    {
        private int idCliente;
        private string nombreCliente;
        private string apellidoCliente;
        private string dUICliente;
        private string telefonoCliente;
        private string correoCliente;
        private bool eliminadoCliente;

        public int IdCliente { get => idCliente; set => idCliente = value; }
        public string NombreCliente { get => nombreCliente; set => nombreCliente = value; }
        public string ApellidoCliente { get => apellidoCliente; set => apellidoCliente = value; }
        public string DUICliente { get => dUICliente; set => dUICliente = value; }
        public string TelefonoCliente { get => telefonoCliente; set => telefonoCliente = value; }
        public string CorreoCliente { get => correoCliente; set => correoCliente = value; }
        public bool EliminadoCliente { get => eliminadoCliente; set => eliminadoCliente = value; }

        public static DataTable MostrarClient()
        {
            using (SqlConnection conection = Conexion.Conectar())
            {
                string comando = "SELECT * FROM VistaClienteGerente;";
                SqlDataAdapter ad = new SqlDataAdapter(comando, conection);
                DataTable dt = new DataTable();
                ad.Fill(dt);
                return dt;
            }
        }
    }
}
