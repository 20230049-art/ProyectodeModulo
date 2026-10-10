using Modelos;
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
using static System.Collections.Specialized.BitVector32;

namespace Vista.GerentePedidos
{
    public partial class AgregarPedido : Form
    {
        private decimal precioUnitario = 0;

        public AgregarPedido()
        {
            InitializeComponent();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult r = MessageBox.Show(
                    "¿Cancelar y descartar el pedido?",
                    "Confirmar cancelación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (r == DialogResult.Yes)
                    this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cerrar: " + ex.Message);
            }
        }

        private void AgregarPedido_Load(object sender, EventArgs e)
        {
            try
            {
                CargarClientes();
                CargarMaquinaria();
                CargarInsumos();
                txtCantidad.Text = "1";
                txtCantidad.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el formulario:\n" + ex.Message,
                                "ERROR-FORMULARIO-101", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarClientes()
        {
            try
            {
                DataTable dt = Clientes.ObtenerClientesParaCombo();
                cmbCliente.DataSource = dt;
                cmbCliente.DisplayMember = "NombreCompleto";
                cmbCliente.ValueMember = "IdCliente";
                cmbCliente.SelectedIndex = -1;
                cmbCliente.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar clientes: " + ex.Message);
            }
        }

        private void CargarMaquinaria()
        {
            try
            {
                DataTable dt = Maquinaria.ObtenerMaquinariaParaCombo();
                cmbMaquinaria.DataSource = dt;
                cmbMaquinaria.DisplayMember = "NombreMaquinaria";
                cmbMaquinaria.ValueMember = "IdMaquinaria";
                cmbMaquinaria.SelectedIndex = -1;
                cmbMaquinaria.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar maquinaria: " + ex.Message);
            }
        }

        private void CargarInsumos()
        {
            try
            {
                DataTable dt = Insumos.ObtenerInsumosParaCombo();
                cmbInsumo.DataSource = dt;
                cmbInsumo.DisplayMember = "NombreInsumo";
                cmbInsumo.ValueMember = "IdInsumos";
                cmbInsumo.SelectedIndex = -1;
                cmbInsumo.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar insumos: " + ex.Message);
            }
        }

        private void cmbInsumo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbInsumo.SelectedItem is DataRowView fila)
                {
                    precioUnitario = Convert.ToDecimal(fila["PrecioInsumo"]);
                    CalcularTotal();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al seleccionar insumo: " + ex.Message);
            }
        }

        private void txtCantidad_TextChanged(object sender, EventArgs e)
        {
            CalcularTotal();
        }

        private void CalcularTotal()
        {
            try
            {
                if (int.TryParse(txtCantidad.Text, out int cantidad) && cantidad > 0)
                {
                    decimal total = cantidad * precioUnitario;
                    txtTotal.Text = total.ToString("C2");  
                }
                else
                {
                    txtTotal.Text = "$0.00";
                }
            }
            catch
            {
                txtTotal.Text = "$0.00";
            }
        }

        private bool Validar()
        {
            if (cmbCliente.SelectedIndex < 0)
            {
                MessageBox.Show("Debe seleccionar un cliente.", "Campo vacío",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCliente.Focus();
                return false;
            }

            if (cmbMaquinaria.SelectedIndex < 0)
            {
                MessageBox.Show("Debe seleccionar una maquinaria.", "Campo vacío",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbMaquinaria.Focus();
                return false;
            }

            if (cmbInsumo.SelectedIndex < 0)
            {
                MessageBox.Show("Debe seleccionar un insumo.", "Campo vacío",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbInsumo.Focus();
                return false;
            }

            if (!int.TryParse(txtCantidad.Text, out int cant) || cant <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a 0.", "Validación",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCantidad.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
            {
                MessageBox.Show("Debe ingresar una descripción.", "Campo vacío",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDescripcion.Focus();
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
                    "¿Registrar este pedido?\n\n" +
                    "Se creará con estado \"En Proceso\".",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (r != DialogResult.Yes) return;

                int cantidad = int.Parse(txtCantidad.Text);
                decimal total = cantidad * precioUnitario;

                Pedido nuevo = new Pedido
                {
                    DescripcionPedido = txtDescripcion.Text.Trim(),
                    CantidadPedido = cantidad,
                    TotalPedido = total,
                    IdCliente = Convert.ToInt32(cmbCliente.SelectedValue),
                    IdMaquinaria = Convert.ToInt32(cmbMaquinaria.SelectedValue),
                    IdInsumos = Convert.ToInt32(cmbInsumo.SelectedValue),
                    IdPersona = Logiin.UsuarioActual?.IdPersona ?? 1
                };

                bool exito = Pedido.AgregarPedido(nuevo);

                if (exito)
                {
                    MessageBox.Show("Pedido registrado correctamente.",
                                    "Operación exitosa",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo registrar el pedido.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el pedido:\n" + ex.Message,
                                "ERROR-EXCEPCION-102",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
