using Modelos.Conexion_DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        public static (string Resultado, int IdCliente) AgregarCliente(Clientes nuevoCliente)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return ("ERROR", 0);

                    using (SqlCommand cmd = new SqlCommand("AgregarCliente", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@NombreCliente", SqlDbType.VarChar).Value = nuevoCliente.NombreCliente;
                        cmd.Parameters.Add("@ApellidoCliente", SqlDbType.VarChar).Value = nuevoCliente.ApellidoCliente;
                        cmd.Parameters.Add("@DUICliente", SqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(nuevoCliente.DUICliente) ? (object)DBNull.Value : nuevoCliente.DUICliente;
                        cmd.Parameters.Add("@TelefonoCliente", SqlDbType.VarChar).Value = nuevoCliente.TelefonoCliente;
                        cmd.Parameters.Add("@CorreoCliente", SqlDbType.VarChar).Value = nuevoCliente.CorreoCliente;

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                string resultado = dr["Resultado"]?.ToString() ?? "ERROR";
                                int idCliente = dr["IdCliente"] != DBNull.Value ? Convert.ToInt32(dr["IdCliente"]) : 0;

                                return (resultado, idCliente);
                            }
                        }

                        return ("ERROR", 0);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error SQL al agregar el cliente:\n" + ex.Message,
                                "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return ("ERROR", 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar el cliente:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return ("ERROR", 0);
            }
        }
        public static DataTable ObtenerClienteId(int idCliente)
            {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    using (SqlCommand cmd = new SqlCommand("ClientePorId", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdCliente", SqlDbType.Int).Value = idCliente;

                        using (SqlDataAdapter ad = new SqlDataAdapter(cmd))
                        {
                            ad.Fill(dt);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al obtener el cliente: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static bool ActualizarCliente(Clientes clienteActualizar)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return false;

                    using (SqlCommand cmd = new SqlCommand("ActualizarCliente", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@IdCliente", SqlDbType.Int).Value = clienteActualizar.IdCliente;
                        cmd.Parameters.Add("@NombreCliente", SqlDbType.VarChar).Value = clienteActualizar.NombreCliente;
                        cmd.Parameters.Add("@ApellidoCliente", SqlDbType.VarChar).Value = clienteActualizar.ApellidoCliente;
                        cmd.Parameters.Add("@DUICliente", SqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(clienteActualizar.DUICliente) ? (object)DBNull.Value : clienteActualizar.DUICliente;
                        cmd.Parameters.Add("@TelefonoCliente", SqlDbType.VarChar).Value = clienteActualizar.TelefonoCliente;
                        cmd.Parameters.Add("@CorreoCliente", SqlDbType.VarChar).Value = clienteActualizar.CorreoCliente;

                        object resultado = cmd.ExecuteScalar();

                        return resultado != null && resultado != DBNull.Value && Convert.ToInt32(resultado) > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error SQL al actualizar el cliente:\n" + ex.Message,
                                "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar el cliente:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool EliminarCliente(int idCliente)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return false;

                    using (SqlCommand cmd = new SqlCommand("EliminarCliente", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdCliente", SqlDbType.Int).Value = idCliente;

                        cmd.ExecuteNonQuery();
                        return EstaEliminado(idCliente);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error SQL al eliminar el cliente:\n" + ex.Message,
                                "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el cliente:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static bool EstaEliminado(int idCliente)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return false;

                    string query = "SELECT EliminadoCliente FROM Cliente WHERE IdCliente = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = idCliente;

                        object resultado = cmd.ExecuteScalar();
                        if (resultado == null || resultado == DBNull.Value) return false;

                        return Convert.ToBoolean(resultado);
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool RestaurarCliente(int idCliente)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return false;

                    using (SqlCommand cmd = new SqlCommand("RestaurarCliente", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdCliente", SqlDbType.Int).Value = idCliente;

                        cmd.ExecuteNonQuery();
                        return !EstaEliminado(idCliente);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error SQL al restaurar el cliente:\n" + ex.Message,
                                "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al restaurar el cliente:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static DataTable ObtenerClientesParaCombo()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return dt;

                    string comando = "SELECT IdCliente, NombreCliente + ' ' + ApellidoCliente AS NombreCompleto " +
                                     "FROM Cliente WHERE EliminadoCliente = 0 ORDER BY NombreCliente;";

                    using (SqlDataAdapter ad = new SqlDataAdapter(comando, connection))
                    {
                        ad.Fill(dt);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al obtener clientes: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }
    }
}
