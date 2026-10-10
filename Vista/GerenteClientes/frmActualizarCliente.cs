using Modelos.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vista.GerenteClientes
{
    public partial class frmActualizarCliente : Form
    {
        private int idCliente;

        public frmActualizarCliente(int idCliente)
        {
            InitializeComponent();
            this.idCliente = idCliente;
            CargarCliente();
        }

        private void CargarCliente()
        {
            try
            {
                DataTable dt = Clientes.ObtenerClienteId(idCliente);
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontró el cliente seleccionado.");
                    this.Close();
                    return;
                }
                DataRow fila = dt.Rows[0];
                txtNombre.Text = fila["NombreCliente"]?.ToString() ?? "";
                txtApellido.Text = fila["ApellidoCliente"]?.ToString() ?? "";
                mtxtDUI.Text = fila["DUICliente"]?.ToString() ?? "";
                mtxtTelefono.Text = fila["TelefonoCliente"]?.ToString() ?? "";
                txtCorreo.Text = fila["CorreoCliente"]?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los datos:\n" + ex.Message,
                    "ERROR-CARGADATOS-008", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Debe ingresar el nombre."); txtNombre.Focus(); return false;
            }
            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MessageBox.Show("Debe ingresar el apellido."); txtApellido.Focus(); return false;
            }
            if (string.IsNullOrWhiteSpace(mtxtTelefono.Text))
            {
                MessageBox.Show("Debe ingresar el teléfono."); mtxtTelefono.Focus(); return false;
            }
            if (string.IsNullOrWhiteSpace(txtCorreo.Text))
            {
                MessageBox.Show("Debe ingresar el correo."); txtCorreo.Focus(); return false;
            }
            if (!string.IsNullOrWhiteSpace(mtxtDUI.Text))
            {
                string dui = mtxtDUI.Text.Trim();
                if (dui.Length != 10 || dui[8] != '-')
                {
                    MessageBox.Show("El DUI debe tener el formato 00000000-0.");
                    mtxtDUI.Focus(); return false;
                }
            }
            return true;
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Validar()) return;

                DialogResult r = MessageBox.Show(
                    $"¿Actualizar al cliente \"{txtNombre.Text.Trim()} {txtApellido.Text.Trim()}\"?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (r != DialogResult.Yes) return;

                Clientes cliente = new Clientes
                {
                    IdCliente = idCliente,
                    NombreCliente = txtNombre.Text.Trim(),
                    ApellidoCliente = txtApellido.Text.Trim(),
                    DUICliente = string.IsNullOrWhiteSpace(mtxtDUI.Text) ? null : mtxtDUI.Text.Trim(),
                    TelefonoCliente = mtxtTelefono.Text.Trim(),
                    CorreoCliente = txtCorreo.Text.Trim()
                };

                if (Clientes.ActualizarCliente(cliente))
                {
                    MessageBox.Show("Cliente actualizado correctamente.",
                        "Operación exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar:\n" + ex.Message,
                    "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult r = MessageBox.Show("¿Cancelar la actualización?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.Yes) this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cerrar:\n" + ex.Message,
                    "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
