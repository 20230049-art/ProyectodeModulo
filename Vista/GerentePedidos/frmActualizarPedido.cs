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
    public partial class frmActualizarPedido : Form
    {
        private int idPedido;
        private decimal precioUnitario = 0;
        public frmActualizarPedido(int idPedido)
        {
            InitializeComponent();
            this.idPedido = idPedido;
        }

        private void frmActualizarPedido_Load(object sender, EventArgs e)
        {
            try
            {
                CargarMaquinaria();
                CargarInsumos();
                CargarEstados();
                CargarPedido();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el formulario:\n" + ex.Message,
                                "ERROR-FORMULARIO-101", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                cmbInsumo.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar insumos: " + ex.Message);
            }
        }

        private void CargarEstados()
        {
            try
            {
                cmbEstado.Items.Clear();
                cmbEstado.Items.Add("En Proceso");  
                cmbEstado.Items.Add("Finalizada");  
                cmbEstado.Items.Add("Cancelada");   
                cmbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar estados: " + ex.Message);
            }
        }

        private void CargarPedido()
        {
            try
            {
                DataTable dt = Pedido.ObtenerPedidoId(idPedido);
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontró el pedido seleccionado.");
                    this.Close();
                    return;
                }

                DataRow fila = dt.Rows[0];

                lblClienteValor.Text = fila["IdCliente"]?.ToString() ?? "";
                txtDescripcion.Text = fila["DescripcionPedido"]?.ToString() ?? "";
                txtCantidad.Text = fila["CantidadPedido"]?.ToString() ?? "1";

                if (fila["IdMaquinaria"] != DBNull.Value)
                    cmbMaquinaria.SelectedValue = Convert.ToInt32(fila["IdMaquinaria"]);

                if (fila["IdInsumos"] != DBNull.Value)
                    cmbInsumo.SelectedValue = Convert.ToInt32(fila["IdInsumos"]);

                if (fila["IdEstadoPedido"] != DBNull.Value)
                {
                    int idEstado = Convert.ToInt32(fila["IdEstadoPedido"]);
                    cmbEstado.SelectedIndex = idEstado - 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar pedido: " + ex.Message);
            }
        }

        private void cmbInsumo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbInsumo.SelectedItem is DataRowView fila)
            {
                precioUnitario = Convert.ToDecimal(fila["PrecioInsumo"]);
                CalcularTotal();
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
                if (int.TryParse(txtCantidad.Text, out int cant) && cant > 0)
                    txtTotal.Text = (cant * precioUnitario).ToString("C2");
                else
                    txtTotal.Text = "$0.00";
            }
            catch
            {
                txtTotal.Text = "$0.00";
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbMaquinaria.SelectedIndex < 0 || cmbInsumo.SelectedIndex < 0 ||
                    !int.TryParse(txtCantidad.Text, out int cant) || cant <= 0)
                {
                    MessageBox.Show("Complete todos los campos correctamente.");
                    return;
                }

                DialogResult r = MessageBox.Show("¿Actualizar este pedido?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;

                int nuevoEstado = cmbEstado.SelectedIndex + 1;

                // Primero actualizar datos
                Pedido p = new Pedido
                {
                    IdPedido = idPedido,
                    DescripcionPedido = txtDescripcion.Text.Trim(),
                    CantidadPedido = cant,
                    TotalPedido = cant * precioUnitario,
                    IdMaquinaria = Convert.ToInt32(cmbMaquinaria.SelectedValue),
                    IdInsumos = Convert.ToInt32(cmbInsumo.SelectedValue),
                    IdPersona = Logiin.UsuarioActual?.IdPersona ?? 1
                };

                Pedido.ActualizarPedido(p);

                // Luego cambiar el estado si es diferente
                Pedido.CambiarEstado(idPedido, nuevoEstado);

                MessageBox.Show("Pedido actualizado correctamente.",
                                "Operación exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
