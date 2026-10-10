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
using Vista.Utilidad;

namespace Vista.GerenteInventario
{
    public partial class frmAgregarInsumo : Form
    {
        public frmAgregarInsumo()
        {
            InitializeComponent();

            Redondeo.RedondearFig(btnAgregar, 7);
            Redondeo.RedondearFig(btnCerrar, 7);
            Redondeo.RedondearFig(panel1, 11);
            Redondeo.RedondearFig(panel2, 11);

        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Debe ingresar el nombre del insumo.", "Campo vacío",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return false;
            }

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad < 0)
            {
                MessageBox.Show("La cantidad debe ser un número entero mayor o igual a 0.", "Validación",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCantidad.Focus();
                return false;
            }

            if (!decimal.TryParse(txtPrecio.Text, out decimal precio) || precio < 0)
            {
                MessageBox.Show("El precio debe ser un número válido mayor o igual a 0.",
                                "Validación",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecio.Focus();
                return false;
            }

            return true;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtNombre.Text) ||
                    !string.IsNullOrWhiteSpace(txtCantidad.Text) ||
                    !string.IsNullOrWhiteSpace(txtPrecio.Text))
                {
                    DialogResult r = MessageBox.Show(
                        "¿Cancelar y descartar los datos ingresados?","Confirmar cancelación",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (r != DialogResult.Yes) return;
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cerrar el formulario:\n" + ex.Message, "ERROR-EXCEPCION-102",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Validar()) return;

                DialogResult r = MessageBox.Show(
                    $"¿Agregar el insumo \"{txtNombre.Text.Trim()}\"?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (r != DialogResult.Yes) return;

                Insumos insumo = new Insumos
                {
                    NombreInsumo = txtNombre.Text.Trim(),
                    CantidadInsumo = int.Parse(txtCantidad.Text),
                    PrecioInsumo = decimal.Parse(txtPrecio.Text),
                    IdEstadoInsumo = 1  
                };

                bool exito = Insumos.AgregarInsumo(insumo);

                if (exito)
                {
                    MessageBox.Show("Insumo agregado correctamente.",  "Operación exitosa",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo agregar el insumo.",  "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar el insumo:\n" + ex.Message,  "ERROR",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
