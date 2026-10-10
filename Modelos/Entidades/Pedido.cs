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
    public class Pedido
    {
        private int idPedido;
        private DateTime fechaPedido;
        private DateTime? fechaEntrega;
        private string descripcionPedido;
        private int cantidadPedido;
        private decimal totalPedido;
        private int idCliente;
        private int idMaquinaria;
        private int idInsumos;
        private int idEstadoPedido;
        private int idPersona;
        private bool eliminadoPedido;

        public int IdPedido { get => idPedido; set => idPedido = value; }
        public DateTime FechaPedido { get => fechaPedido; set => fechaPedido = value; }
        public DateTime? FechaEntrega { get => fechaEntrega; set => fechaEntrega = value; }
        public string DescripcionPedido { get => descripcionPedido; set => descripcionPedido = value; }
        public int CantidadPedido { get => cantidadPedido; set => cantidadPedido = value; }
        public decimal TotalPedido { get => totalPedido; set => totalPedido = value; }
        public int IdCliente { get => idCliente; set => idCliente = value; }
        public int IdMaquinaria { get => idMaquinaria; set => idMaquinaria = value; }
        public int IdInsumos { get => idInsumos; set => idInsumos = value; }
        public int IdEstadoPedido { get => idEstadoPedido; set => idEstadoPedido = value; }
        public int IdPersona { get => idPersona; set => idPersona = value; }

        public bool EliminadoPedido { get => eliminadoPedido; set => eliminadoPedido = value; }
        public string NombreCliente { get => nombreCliente; set => nombreCliente = value; }
        public string ApellidoCliente { get => apellidoCliente; set => apellidoCliente = value; }
        public string NombreMaquinaria { get => nombreMaquinaria; set => nombreMaquinaria = value; }
        public string NombreInsumo { get => nombreInsumo; set => nombreInsumo = value; }
        public string NombreEstadoPedido { get => nombreEstadoPedido; set => nombreEstadoPedido = value; }
        public string Usuario { get => usuario; set => usuario = value; }

        private string nombreCliente;
        private string apellidoCliente;
        private string nombreMaquinaria;
        private string nombreInsumo;
        private string nombreEstadoPedido;
        private string usuario;

        public static DataTable MostrarPedidos()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return dt;

                    string comando = "SELECT * FROM VistaPedidos;";
                    using (SqlDataAdapter ad = new SqlDataAdapter(comando, connection))
                    {
                        ad.Fill(dt);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al obtener los pedidos: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static DataTable MostrarPedidosPorEstado(int idEstado)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return dt;

                    string comando = "SELECT * FROM VistaPedidos WHERE NombreEstadoPedido = @Estado;";

                    string estado = "";
                    switch (idEstado)
                    {
                        case 1: estado = "En Proceso"; break;
                        case 2: estado = "Finalizada"; break;
                        case 3: estado = "Cancelada"; break;
                    }

                    using (SqlCommand cmd = new SqlCommand(comando, connection))
                    {
                        cmd.Parameters.AddWithValue("@Estado", estado);

                        using (SqlDataAdapter ad = new SqlDataAdapter(cmd))
                        {
                            ad.Fill(dt);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al filtrar pedidos: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static DataTable BuscarPedido(string busqueda)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return dt;

                    using (SqlCommand cmd = new SqlCommand("BuscarPedido", connection))
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
                MessageBox.Show("Error al buscar pedidos: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static bool AgregarPedido(Pedido nuevo)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return false;

                    using (SqlCommand cmd = new SqlCommand("AgregarPedido", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@DescripcionPedido", SqlDbType.VarChar).Value = nuevo.DescripcionPedido;
                        cmd.Parameters.Add("@CantidadPedido", SqlDbType.Int).Value = nuevo.CantidadPedido;
                        cmd.Parameters.Add("@TotalPedido", SqlDbType.Money).Value = nuevo.TotalPedido;
                        cmd.Parameters.Add("@IdCliente", SqlDbType.Int).Value = nuevo.IdCliente;
                        cmd.Parameters.Add("@IdMaquinaria", SqlDbType.Int).Value = nuevo.IdMaquinaria;
                        cmd.Parameters.Add("@IdInsumos", SqlDbType.Int).Value = nuevo.IdInsumos;
                        cmd.Parameters.Add("@IdPersona", SqlDbType.Int).Value = nuevo.IdPersona;

                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error SQL al agregar el pedido:\n" + ex.Message,
                                "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar el pedido:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool ActualizarPedido(Pedido pedidoActualizar)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return false;

                    using (SqlCommand cmd = new SqlCommand("ActualizarPedido", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@IdPedido", SqlDbType.Int).Value = pedidoActualizar.IdPedido;
                        cmd.Parameters.Add("@DescripcionPedido", SqlDbType.VarChar).Value = pedidoActualizar.DescripcionPedido;
                        cmd.Parameters.Add("@CantidadPedido", SqlDbType.Int).Value = pedidoActualizar.CantidadPedido;
                        cmd.Parameters.Add("@TotalPedido", SqlDbType.Money).Value = pedidoActualizar.TotalPedido;
                        cmd.Parameters.Add("@IdMaquinaria", SqlDbType.Int).Value = pedidoActualizar.IdMaquinaria;
                        cmd.Parameters.Add("@IdInsumos", SqlDbType.Int).Value = pedidoActualizar.IdInsumos;
                        cmd.Parameters.Add("@IdPersona", SqlDbType.Int).Value = pedidoActualizar.IdPersona;

                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error SQL al actualizar el pedido:\n" + ex.Message,
                                "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar el pedido:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool CambiarEstado(int idPedido, int idEstado)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return false;

                    using (SqlCommand cmd = new SqlCommand("CambiarEstadoPedido", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdPedido", SqlDbType.Int).Value = idPedido;
                        cmd.Parameters.Add("@IdEstadoPedido", SqlDbType.Int).Value = idEstado;

                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error SQL al cambiar el estado:\n" + ex.Message,
                                "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cambiar el estado:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool EliminarPedido(int idPedido)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return false;

                    using (SqlCommand cmd = new SqlCommand("EliminarPedido", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdPedido", SqlDbType.Int).Value = idPedido;

                        cmd.ExecuteNonQuery();
                        return EstaEliminado(idPedido);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error SQL al eliminar el pedido:\n" + ex.Message,
                                "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el pedido:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static bool EstaEliminado(int idPedido)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return false;

                    string query = "SELECT EliminadoPedido FROM PedidosImpresion WHERE IdPedido = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = idPedido;

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

        public static bool RestaurarPedido(int idPedido)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return false;

                    using (SqlCommand cmd = new SqlCommand("RestaurarPedido", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdPedido", SqlDbType.Int).Value = idPedido;

                        cmd.ExecuteNonQuery();
                        return !EstaEliminado(idPedido);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error SQL al restaurar el pedido:\n" + ex.Message,
                                "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al restaurar el pedido:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static DataTable ObtenerPedidoId(int idPedido)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return dt;

                    using (SqlCommand cmd = new SqlCommand("PedidoPorId", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdPedido", SqlDbType.Int).Value = idPedido;

                        using (SqlDataAdapter ad = new SqlDataAdapter(cmd))
                        {
                            ad.Fill(dt);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al obtener el pedido: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }
    }
}

