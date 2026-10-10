using Modelos.Conexion_DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Modelos.Entidades
{
    public class Maquinaria
    {
        private int idMaquinaria;
        private string nombreMaquinaria;
        private int idEstadoMaquinaria;
        private string estadoActualMaquinaria;
        private int idPersona;
        private bool eliminadoMaquinaria;
        private string foto;

        public int IdMaquinaria { get => idMaquinaria; set => idMaquinaria = value; }
        public string NombreMaquinaria { get => nombreMaquinaria; set => nombreMaquinaria = value; }
        public int IdEstadoMaquinaria { get => idEstadoMaquinaria; set => idEstadoMaquinaria = value; }
        public string EstadoActualMaquinaria { get => estadoActualMaquinaria; set => estadoActualMaquinaria = value; }
        public int IdPersona { get => idPersona; set => idPersona = value; }
        public bool EliminadoMaquinaria { get => eliminadoMaquinaria; set => eliminadoMaquinaria = value; }
        public string Foto { get => foto; set => foto = value; }

        public static DataTable MostrarMaquinariaTotales()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    string comando = "SELECT * FROM VistaMaqunariaGerente;";
                    using (SqlDataAdapter ad = new SqlDataAdapter(comando, connection))
                    {
                        ad.Fill(dt);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al obtener la maquinaria: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static DataTable MostrarMaquinariaActiva()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    string comando = "SELECT * FROM VistaMaquinariaActiva;";
                    using (SqlDataAdapter ad = new SqlDataAdapter(comando, connection))
                    {
                        ad.Fill(dt);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al obtener la maquinaria activa: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static DataTable MostrarMaquinariaInactiva()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    // OJO: La vista en tu BD se llama "istaMaquinariaInactiva" (sin la V)
                    string comando = "SELECT * FROM istaMaquinariaInactiva;";
                    using (SqlDataAdapter ad = new SqlDataAdapter(comando, connection))
                    {
                        ad.Fill(dt);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al obtener la maquinaria inactiva: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static DataTable BuscarMaquinaria(string busqueda)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    using (SqlCommand cmd = new SqlCommand("BuscarMaquinaria", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@busqueda", SqlDbType.VarChar).Value =
                            string.IsNullOrWhiteSpace(busqueda) ? (object)DBNull.Value : busqueda;

                        using (SqlDataAdapter ad = new SqlDataAdapter(cmd))
                        {
                            ad.Fill(dt);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al buscar maquinaria: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static void AgregarMaquinaria(Maquinaria nuevaMaquinaria)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    using (SqlCommand command = new SqlCommand("AgregarMaquinaria", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@NombreMaquinaria", SqlDbType.VarChar).Value = nuevaMaquinaria.NombreMaquinaria;
                        command.Parameters.Add("@Foto", SqlDbType.VarChar).Value = nuevaMaquinaria.Foto ?? "";

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al agregar la maquinaria: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void ActualizarMaquinaria(Maquinaria maquinariaActualizar)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    using (SqlCommand command = new SqlCommand("ActualizarMaquinaria", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@IdMaquinaria", SqlDbType.Int).Value = maquinariaActualizar.IdMaquinaria;
                        command.Parameters.Add("@NombreMaquinaria", SqlDbType.VarChar).Value = maquinariaActualizar.NombreMaquinaria;
                        command.Parameters.Add("@Foto", SqlDbType.VarChar).Value = maquinariaActualizar.Foto ?? "";
                        command.Parameters.Add("@IdEstadoMaquinaria", SqlDbType.Int).Value = maquinariaActualizar.IdEstadoMaquinaria;

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al actualizar la maquinaria: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static DataTable ObtenerMaquinariaId(int idMaquinaria)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    using (SqlCommand command = new SqlCommand("MaquinariaPorId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@IdMaquinaria", SqlDbType.Int).Value = idMaquinaria;

                        using (SqlDataAdapter ad = new SqlDataAdapter(command))
                        {
                            ad.Fill(dt);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al obtener la maquinaria: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static bool EliminarMaquinaria(int idMaquinaria)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    using (SqlCommand cmd = new SqlCommand("EliminarMaquinaria", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdMaquinaria", SqlDbType.Int).Value = idMaquinaria;

                        int filas = cmd.ExecuteNonQuery();
                        return filas > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al eliminar la maquinaria: " + ex.Message, ex);
            }
        }
        public static bool RestaurarMaquinaria(int idMaquinaria)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    using (SqlCommand cmd = new SqlCommand("RestaurarMaquinaria", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdMaquinaria", SqlDbType.Int).Value = idMaquinaria;

                        int filas = cmd.ExecuteNonQuery();
                        return filas > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al restaurar la maquinaria: " + ex.Message, ex);
            }
        }
    }
}
