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
    public class Persona
    {
        private int idPersona;
        private string nombrePersona;
        private string apellidoPersona;
        private string dUIPersona;
        private string contrasenaPersona;
        private string correoPersona;
        private string telefonoPersona;
        private int idRol;
        private string nombreRol;
        private bool eliminadoPersona;

        public int IdPersona { get => idPersona; set => idPersona = value; }
        public string NombrePersona { get => nombrePersona; set => nombrePersona = value; }
        public string ApellidoPersona { get => apellidoPersona; set => apellidoPersona = value; }
        public string DUIPersona { get => dUIPersona; set => dUIPersona = value; }
        public string ContrasenaPersona { get => contrasenaPersona; set => contrasenaPersona = value; }
        public string CorreoPersona { get => correoPersona; set => correoPersona = value; }
        public string TelefonoPersona { get => telefonoPersona; set => telefonoPersona = value; }
        public int IdRol { get => idRol; set => idRol = value; }
        public string NombreRol { get => nombreRol; set => nombreRol = value; }
        public bool EliminadoPersona { get => eliminadoPersona; set => eliminadoPersona = value; }

        public static DataTable MostrarUsuarios()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return dt;

                    string comando = "SELECT * FROM VistaTodosUsuarios;";
                    using (SqlDataAdapter ad = new SqlDataAdapter(comando, connection))
                    {
                        ad.Fill(dt);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error al obtener los usuarios: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static bool AgregarUsuario(Persona nuevo, string contrasenaPlano)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return false;

                    using (SqlCommand cmd = new SqlCommand("AgregarPersona", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@NombrePersona", SqlDbType.VarChar).Value = nuevo.NombrePersona;
                        cmd.Parameters.Add("@ApellidoPersona", SqlDbType.VarChar).Value = nuevo.ApellidoPersona;
                        cmd.Parameters.Add("@DUIPersona", SqlDbType.VarChar).Value = nuevo.DUIPersona;
                        cmd.Parameters.Add("@CorreoPersona", SqlDbType.VarChar).Value = nuevo.CorreoPersona;
                        cmd.Parameters.Add("@ContrasenaPersona", SqlDbType.VarChar).Value = Seguridad.HashPassword(contrasenaPlano);
                        cmd.Parameters.Add("@TelefonoPersona", SqlDbType.VarChar).Value = nuevo.TelefonoPersona;
                        cmd.Parameters.Add("@IdRol", SqlDbType.Int).Value = nuevo.IdRol;

                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error SQL al agregar el usuario:\n" + ex.Message,
                                "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar el usuario:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static Persona Login(string correo, string contrasenaPlano)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return null;

                    using (SqlCommand cmd = new SqlCommand("LoginUsuario", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@CorreoPersona", SqlDbType.VarChar).Value = correo;

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                string hashGuardado = dr["ContrasenaPersona"].ToString();

                                if (Seguridad.VerificarPassword(contrasenaPlano, hashGuardado))
                                {
                                    return new Persona
                                    {
                                        IdPersona = Convert.ToInt32(dr["IdPersona"]),
                                        NombrePersona = dr["NombrePersona"].ToString(),
                                        ApellidoPersona = dr["ApellidoPersona"].ToString(),
                                        CorreoPersona = dr["CorreoPersona"].ToString(),
                                        NombreRol = dr["NombreRol"].ToString()
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error SQL al iniciar sesión:\n" + ex.Message,
                                "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al iniciar sesión:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        public static bool EliminarUsuario(int idPersona)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return false;

                    using (SqlCommand cmd = new SqlCommand("EliminarPersona", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdPersona", SqlDbType.Int).Value = idPersona;

                        cmd.ExecuteNonQuery();
                        return EstaEliminado(idPersona);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error SQL al eliminar el usuario:\n" + ex.Message,
                                "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el usuario:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static bool EstaEliminado(int idPersona)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return false;

                    string query = "SELECT EliminadoPersona FROM Persona WHERE IdPersona = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = idPersona;

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

        public static bool RestaurarUsuario(int idPersona)
        {
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return false;

                    using (SqlCommand cmd = new SqlCommand("RestaurarPersona", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdPersona", SqlDbType.Int).Value = idPersona;

                        cmd.ExecuteNonQuery();
                        return !EstaEliminado(idPersona);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error SQL al restaurar el usuario:\n" + ex.Message,
                                "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al restaurar el usuario:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
