using Modelos;
using Modelos.Conexion_DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vista.Iniciar_Sesion
{
    public partial class frmPrimerUsuario : Form
    {
        public frmPrimerUsuario()
        {
            InitializeComponent();
        }

        private void btnFinalizarConfiguracion_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
               string.IsNullOrWhiteSpace(txtApellidos.Text) ||
               string.IsNullOrWhiteSpace(txtCorreo.Text) ||
               string.IsNullOrWhiteSpace(txtDUI.Text) ||
               string.IsNullOrWhiteSpace(txtTelefono.Text) ||
               string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                MessageBox.Show("Completa todos los campos.", "IMPREXA",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtContrasena.Text.Length < 6)
            {
                MessageBox.Show("La contraseña debe tener al menos 6 caracteres.", "IMPREXA",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtDUI.Text.Trim().Length != 10 || txtDUI.Text.Trim()[8] != '-')
            {
                MessageBox.Show("El DUI debe tener el formato 00000000-0.", "IMPREXA",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtTelefono.Text.Trim().Length != 9 || txtTelefono.Text.Trim()[4] != '-')
            {
                MessageBox.Show("El teléfono debe tener el formato 0000-0000.", "IMPREXA",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string hash = BCrypt.Net.BCrypt.HashPassword(txtContrasena.Text);

                using (var cn = Conexion.Conectar())
                {
                    if (cn == null)
                    {
                        MessageBox.Show("No se pudo conectar a la base de datos.", "IMPREXA",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    using (var check = new SqlCommand(
                        @"SELECT COUNT(*) FROM Persona P
                          INNER JOIN Rol R ON R.IdRol = P.IdRol
                          WHERE R.NombreRol = 'Gerente' AND P.EliminadoPersona = 0", cn))
                    {
                        int existe = Convert.ToInt32(check.ExecuteScalar());
                        if (existe > 0)
                        {
                            MessageBox.Show("Ya existe un gerente registrado. Inicia sesión normalmente.",
                                "IMPREXA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DialogResult = DialogResult.OK;
                            Close();
                            return;
                        }
                    }

                    const string sql = @"
                        INSERT INTO Persona
                            (NombrePersona, ApellidoPersona, DUIPersona, CorreoPersona,
                             ContrasenaPersona, TelefonoPersona, IdRol, EliminadoPersona)
                        OUTPUT INSERTED.IdPersona
                        VALUES
                            (@n, @a, @d, @c, @p, @t, 1, 0)";

                    int idNuevo;
                    using (var cmd = new SqlCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@n", txtNombre.Text.Trim());
                        cmd.Parameters.AddWithValue("@a", txtApellidos.Text.Trim());
                        cmd.Parameters.AddWithValue("@d", txtDUI.Text.Trim());
                        cmd.Parameters.AddWithValue("@c", txtCorreo.Text.Trim());
                        cmd.Parameters.AddWithValue("@p", hash);
                        cmd.Parameters.AddWithValue("@t", txtTelefono.Text.Trim());

                        idNuevo = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    Modelos.Entidades.Sesion.IdPersona = idNuevo;
                    Modelos.Entidades.Sesion.NombreCompleto =
                        txtNombre.Text.Trim() + " " + txtApellidos.Text.Trim();
                    Modelos.Entidades.Sesion.Correo = txtCorreo.Text.Trim();
                    Modelos.Entidades.Sesion.Rol = "Gerente";
                }

                MessageBox.Show("¡Administrador creado!\n\nBienvenido, " +
                    Modelos.Entidades.Sesion.NombreCompleto,
                    "IMPREXA", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (SqlException ex)
            {
                string msg = ex.Number == 2627 || ex.Number == 2601
                    ? "Ya existe un usuario con ese correo, DUI o teléfono."
                    : "Error al crear el usuario:\n\n" + ex.Message;
                MessageBox.Show(msg, "IMPREXA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado:\n\n" + ex.Message,
                    "IMPREXA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
