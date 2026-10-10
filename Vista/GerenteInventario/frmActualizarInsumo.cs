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

namespace Vista.GerenteInventario
{
    public partial class frmActualizarInsumo : Form
    {
        private int idInsumo;
        private int idEstadoActual = 1;
        public frmActualizarInsumo(int idInsumo)
        {
            InitializeComponent();
            this.idInsumo = idInsumo;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult r = MessageBox.Show(  "¿Cancelar la actualización?",  "Confirmar cancelación",
                    MessageBoxButtons.YesNo,  MessageBoxIcon.Question);

                if (r == DialogResult.Yes)
                    this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cerrar el formulario:\n" + ex.Message,
                                "ERROR-EXCEPCION-102",  MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmActualizarInsumo_Load(object sender, EventArgs e)
        {
            try
            {
                CargarInsumo();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el formulario:\n" + ex.Message, "ERROR-FORMULARIO-101",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarInsumo()
        {
            try
            {
                DataTable dt = Insumos.ObtenerInsumoId(idInsumo);

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontró el insumo seleccionado.",  "Sin resultados",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }

                DataRow fila = dt.Rows[0];

                txtnombre.Text = fila["NombreInsumo"]?.ToString() ?? "";

                if (fila["CantidadInsumo"] != DBNull.Value)
                    txtCantidad.Text = fila["CantidadInsumo"].ToString();

                if (fila["PrecioInsumo"] != DBNull.Value)
                    txtPrecio.Text = Convert.ToDecimal(fila["PrecioInsumo"]).ToString("0.00");

                if (fila["IdEstadoInsumos"] != DBNull.Value)
                    idEstadoActual = Convert.ToInt32(fila["IdEstadoInsumos"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los datos del insumo:\n" + ex.Message,
                                "ERROR-CARGADATOS-008", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(txtnombre.Text))
            {
                MessageBox.Show("Debe ingresar el nombre del insumo.",  "Campo vacío",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return false;
            }

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad < 0)
            {
                MessageBox.Show("La cantidad debe ser un número entero mayor o igual a 0.",  "Validación",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCantidad.Focus();
                return false;
            }

            if (!decimal.TryParse(txtPrecio.Text, out decimal precio) || precio < 0)
            {
                MessageBox.Show("El precio debe ser un número válido mayor o igual a 0.",  "Validación",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecio.Focus();
                return false;
            }

            return true;
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Validar()) return;

                DialogResult r = MessageBox.Show(
                    $"¿Actualizar el insumo \"{txtnombre.Text.Trim()}\"?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (r != DialogResult.Yes) return;


                Insumos insumo = new Insumos
                {
                    IdInsumos = idInsumo,
                    NombreInsumo = txtnombre.Text.Trim(),
                    CantidadInsumo = int.Parse(txtCantidad.Text),
                    PrecioInsumo = decimal.Parse(txtPrecio.Text),
                    IdEstadoInsumo = idEstadoActual  
                };

                Insumos.ActualizarInsumo(insumo);

                MessageBox.Show("Insumo actualizado correctamente.", "Operación exitosa",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar el insumo:\n" + ex.Message, "ERROR-EXCEPCION-102",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
