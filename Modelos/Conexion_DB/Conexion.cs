using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Modelos.Conexion_DB
{
    public class Conexion
    {
       private static string servidor = "DESKTOP-S8LD4U5\\SQLEXPRESS";
       private static string baseDatos = "Imprexa";

       public static SqlConnection Conectar()
       {
        try
        {
        string cadena = $"Data Source={servidor}; Initial Catalog={baseDatos}; Integrated Security=True";
         SqlConnection conection = new SqlConnection(cadena);
          conection.Open();

          return conection;
        }
        catch (Exception ex)
        {
           MessageBox.Show($"No se pudo conectar al servidor{servidor}", "Error:" + ex, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
       }
    }
}
