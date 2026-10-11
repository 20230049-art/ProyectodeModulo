using Modelos;
using Modelos.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vista.InicioGerente;
using static System.Collections.Specialized.BitVector32;

namespace Vista.Iniciar_Sesion
{
    public partial class frmIniciarSesion : Form
    {
        private readonly string archivoRecordar =
            Path.Combine(Application.StartupPath, "recordar.txt");

        public frmIniciarSesion()
        {
            InitializeComponent();

        }

        //Cambio de formularios
        private Form activeForm = null;

        private void abrirForm(Form formularioAbrir)
        {
            if (activeForm != null && activeForm.GetType() == formularioAbrir.GetType())
            {
                return;
            }

            if (activeForm != null)
            {
                activeForm.Close();
                pnlVistaIniciarSesion.Controls.Remove(activeForm);
                activeForm.Dispose();
            }

            activeForm = formularioAbrir;
            formularioAbrir.TopLevel = false;
            formularioAbrir.FormBorderStyle = FormBorderStyle.None;
            formularioAbrir.Dock = DockStyle.Fill;

            pnlVistaIniciarSesion.Controls.Add(formularioAbrir);
            formularioAbrir.BringToFront();
            formularioAbrir.Show();
        }

        private void btnIniciarSesion_Click(object sender, EventArgs e)
        {
            try
            {
                string correo = txtCorreo.Text.Trim();
                string contrasena = txtContrasena.Text;

                if (string.IsNullOrWhiteSpace(correo))
                {
                    MessageBox.Show("Debe ingresar el usuario/correo.", "Campo vacío",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCorreo.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(contrasena))
                {
                    MessageBox.Show("Debe ingresar la contraseña.", "Campo vacío",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtContrasena.Focus();
                    return;
                }

                Persona usuario = Persona.Login(correo, contrasena);

                if (usuario == null)
                {
                    MessageBox.Show("Usuario o contraseña incorrectos.",
                                    "Acceso denegado",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtContrasena.Clear();
                    txtContrasena.Focus();
                    return;
                }

                Logiin.UsuarioActual = usuario;
                GuardarCorreoRecordado(correo);

                switch (usuario.NombreRol)
                {
                    case "Gerente":
                        abrirForm(new Vista.InicioGerente.frmInicioGerente());
                        break;

                    case "Empleado":
                        abrirForm(new Vista.frmOperador.FormOperador());  
                        break;

                    case "Cliente":
                        abrirForm(new Vista.frmInicio.FormInicio());      
                        break;

                    default:
                        MessageBox.Show("Rol no reconocido: " + usuario.NombreRol,
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Logiin.CerrarSesion();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al iniciar sesión:\n" + ex.Message,
                                "ERROR-EXCEPCION-102",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmIniciarSesion_Load(object sender, EventArgs e)
        {
            try
            {
                CargarCorreoRecordado();
                txtCorreo.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el formulario:\n" + ex.Message,
                                "ERROR-FORMULARIO-101",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarCorreoRecordado()
        {
            try
            {
                if (File.Exists(archivoRecordar))
                {
                    string correo = File.ReadAllText(archivoRecordar).Trim();
                    if (!string.IsNullOrEmpty(correo))
                    {
                        txtCorreo.Text = correo;
                        chkRecordarme.Checked = true;
                        txtContrasena.Focus();
                    }
                }
            }
            catch { }
        }

        private void GuardarCorreoRecordado(string correo)
        {
            try
            {
                if (chkRecordarme.Checked)
                    File.WriteAllText(archivoRecordar, correo);
                else if (File.Exists(archivoRecordar))
                    File.Delete(archivoRecordar);
            }
            catch { }
        }

        private void txtCorreo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtContrasena.Focus();
                e.SuppressKeyPress = true;
            }
        }

        private void txtContrasena_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnIniciarSesion_Click(sender, e);
                e.SuppressKeyPress = true;
            }
        }

        private void lnkOlvidaste_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(
                "Por favor contacte al administrador del sistema para restablecer su contraseña.\n\n" +
                "Correo: admin@imprexa.com",
                "Recuperar contraseña",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
