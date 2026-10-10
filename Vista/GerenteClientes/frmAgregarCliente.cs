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
    public partial class frmAgregarCliente : Form
    {
        public frmAgregarCliente()
        {
            InitializeComponent();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtNombre.Text) ||
                    !string.IsNullOrWhiteSpace(txtApellido.Text) ||
                    !string.IsNullOrWhiteSpace(mtxtTelefono.Text) ||
                    !string.IsNullOrWhiteSpace(txtCorreo.Text))
                {
                    DialogResult r = MessageBox.Show( "¿Cancelar y descartar los datos ingresados?",  "Confirmar cancelación",
                        MessageBoxButtons.YesNo,  MessageBoxIcon.Question);

                    if (r != DialogResult.Yes) return;
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cerrar el formulario:\n" + ex.Message,  "ERROR",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Debe ingresar el nombre del cliente.", "Campo vacío",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MessageBox.Show("Debe ingresar el apellido del cliente.", "Campo vacío",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(mtxtTelefono.Text))
            {
                MessageBox.Show("Debe ingresar el teléfono del cliente.", "Campo vacío",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCorreo.Text))
            {
                MessageBox.Show("Debe ingresar el correo del cliente.", "Campo vacío",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(mtxtDUI.Text))
            {
                string dui = mtxtDUI.Text.Trim();
                if (dui.Length != 10 || dui[8] != '-')
                {
                    MessageBox.Show("El DUI debe tener el formato 00000000-0.", "Formato inválido",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    mtxtDUI.Focus();
                    return false;
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
                    $"¿Agregar al cliente \"{txtNombre.Text.Trim()} {txtApellido.Text.Trim()}\"?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (r != DialogResult.Yes) return;

                Clientes nuevo = new Clientes
                {
                    NombreCliente = txtNombre.Text.Trim(),
                    ApellidoCliente = txtApellido.Text.Trim(),
                    DUICliente = string.IsNullOrWhiteSpace(mtxtDUI.Text) ? null : mtxtDUI.Text.Trim(),
                    TelefonoCliente = mtxtTelefono.Text.Trim(),
                    CorreoCliente = txtCorreo.Text.Trim()
                };

                string resultado = Clientes.AgregarCliente(nuevo);

                switch (resultado)
                {
                    case "NUEVO":
                        MessageBox.Show("Cliente agregado correctamente.", "Operación exitosa",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        break;

                    case "ACTIVO":
                        MessageBox.Show("Ya existe un cliente ACTIVO con ese DUI o correo.", "Duplicado",
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;

                    case "ELIMINADO":
                        MessageBox.Show("Ya existe un cliente ELIMINADO con ese DUI o correo.\n" + "Puede restaurarlo desde el listado.",
                                        "Registro eliminado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;

                    case "ERROR":
                        MessageBox.Show("Ocurrió un error al agregar el cliente.","Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;

                    default:
                        MessageBox.Show("Resultado inesperado del servidor: " + resultado, "Aviso",
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar el cliente:\n" + ex.Message,  "ERROR",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
