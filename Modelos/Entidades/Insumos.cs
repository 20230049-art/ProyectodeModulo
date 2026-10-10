using Modelos.Conexion_DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Modelos.Conexion_DB.Conexion;

namespace Modelos.Entidades
{
    public  class Insumos
    {
        private int idInsumos;
        private string nombreInsumo;
        private decimal precioInsumo;
        private int cantidadInsumo;
        private int idEstadoInsumo;
        private string estadoActualInsumo;
        private bool eliminadoInsumo;

        public int IdInsumos { get => idInsumos; set => idInsumos = value; }
        public string NombreInsumo { get => nombreInsumo; set => nombreInsumo = value; }
        public decimal PrecioInsumo { get => precioInsumo; set => precioInsumo = value; }
        public int CantidadInsumo { get => cantidadInsumo; set => cantidadInsumo = value; }
        public int IdEstadoInsumo { get => idEstadoInsumo; set => idEstadoInsumo = value; }
        public string EstadoActualInsumo { get => estadoActualInsumo; set => estadoActualInsumo = value; }
        public bool EliminadoInsumo { get => eliminadoInsumo; set => eliminadoInsumo = value; }

        public static DataTable MostrarInsumos()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    string comando = "SELECT * FROM VistaInsumosGERENTE;";
                    using (SqlDataAdapter ad = new SqlDataAdapter(comando, connection))
                    {
                        ad.Fill(dt);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al obtener los insumos: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static DataTable MostrarInsumosDisponibles()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    string comando = "SELECT * FROM VistaInsumosGERENTE WHERE IdEstadoInsumos = 1;";
                    using (SqlDataAdapter ad = new SqlDataAdapter(comando, connection))
                    {
                        ad.Fill(dt);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al obtener los insumos disponibles: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static DataTable MostrarInsumosAgotados()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    string comando = "SELECT * FROM VistaInsumosGERENTE WHERE IdEstadoInsumos = 2;";
                    using (SqlDataAdapter ad = new SqlDataAdapter(comando, connection))
                    {
                        ad.Fill(dt);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al obtener los insumos agotados: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static DataTable BuscarInsumo(string busqueda)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    using (SqlCommand cmd = new SqlCommand("BuscarInsumo", connection))
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
                MessageBox.Show("Error al buscar insumos: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static void ActualizarInsumo(Insumos insumoActualizar)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    using (SqlCommand command = new SqlCommand("ActualizarInsumo", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@IdInsumos", SqlDbType.Int).Value = insumoActualizar.IdInsumos;
                        command.Parameters.Add("@NombreInsumo", SqlDbType.VarChar).Value = insumoActualizar.NombreInsumo;
                        command.Parameters.Add("@PrecioInsumo", SqlDbType.Money).Value = insumoActualizar.PrecioInsumo;
                        command.Parameters.Add("@CantidadInsumo", SqlDbType.Int).Value = insumoActualizar.CantidadInsumo;
                        command.Parameters.Add("@IdEstadoInsumos", SqlDbType.Int).Value = insumoActualizar.IdEstadoInsumo;

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al actualizar el insumo: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static DataTable ObtenerInsumoId(int idInsumo)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    using (SqlCommand command = new SqlCommand("InsumosPorId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@IdInsumos", SqlDbType.Int).Value = idInsumo;

                        using (SqlDataAdapter ad = new SqlDataAdapter(command))
                        {
                            ad.Fill(dt);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al obtener el insumo: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static bool EliminarInsumo(int idInsumo)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    using (SqlCommand cmd = new SqlCommand("EliminarInsumos", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdInsumos", SqlDbType.Int).Value = idInsumo;

                        int filas = cmd.ExecuteNonQuery();
                        return filas > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al eliminar el insumo: " + ex.Message, ex);
            }
        }

        public static bool RestaurarInsumo(int idInsumo)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    using (SqlCommand cmd = new SqlCommand("RestaurarInsumos", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdInsumos", SqlDbType.Int).Value = idInsumo;

                        int filas = cmd.ExecuteNonQuery();
                        return filas > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al restaurar el insumo: " + ex.Message, ex);
            }
        }

    }
}
