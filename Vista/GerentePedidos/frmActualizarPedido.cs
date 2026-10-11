using Modelos;
using Modelos.Conexion_DB;
using Modelos.Entidades;
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
                CargarCombos();
                CargarPedido();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el formulario:\n" + ex.Message,
                                "ERROR-FORMULARIO-101", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CargarCombos()
        {
            try
            {

                cmbCliente.DataSource = Clientes.ObtenerClientesParaCombo();
                cmbCliente.DisplayMember = "NombreCompleto";
                cmbCliente.ValueMember = "IdCliente";
                cmbCliente.SelectedIndex = -1;

                DataTable dtMaq = new DataTable();
                using (var cn = Conexion.Conectar())
                {
                    if (cn != null)
                    {
                        using (var cmd = new SqlCommand(
                            "SELECT IdMaquinaria, NombreMaquinaria FROM Maquinaria WHERE EliminadoMaquinaria = 0 ORDER BY NombreMaquinaria", cn))
                        using (var da = new SqlDataAdapter(cmd))
                            da.Fill(dtMaq);
                    }
                }
                cmbMaquinaria.DataSource = dtMaq;
                cmbMaquinaria.DisplayMember = "NombreMaquinaria";
                cmbMaquinaria.ValueMember = "IdMaquinaria";
                cmbMaquinaria.SelectedIndex = -1;

                DataTable dtIns = new DataTable();
                using (var cn = Conexion.Conectar())
                {
                    if (cn != null)
                    {
                        using (var cmd = new SqlCommand(
                            "SELECT IdInsumos, NombreInsumo FROM Insumos WHERE EliminadoInsumos = 0 ORDER BY NombreInsumo", cn))
                        using (var da = new SqlDataAdapter(cmd))
                            da.Fill(dtIns);
                    }
                }
                cmbInsumo.DataSource = dtIns;
                cmbInsumo.DisplayMember = "NombreInsumo";
                cmbInsumo.ValueMember = "IdInsumos";
                cmbInsumo.SelectedIndex = -1;

                cmbEstado.Items.Clear();
                cmbEstado.Items.Add("En Proceso");
                cmbEstado.Items.Add("Finalizada");
                cmbEstado.Items.Add("Cancelada");
                cmbEstado.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los combos:\n" + ex.Message,
                    "ERROR-CARGA-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int idPersonaOriginal = 0;

        private void CargarPedido()
        {
            try
            {
                DataTable dt = Pedido.ObtenerPedidoId(idPedido);
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontró el pedido.", "IMPREXA",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }

                DataRow r = dt.Rows[0];

                if (r["IdPersona"] != DBNull.Value) idPersonaOriginal = Convert.ToInt32(r["IdPersona"]);
                txtDescripcion.Text = r["DescripcionPedido"].ToString();
                txtCantidad.Text = Convert.ToInt32(r["CantidadPedido"]).ToString();
                txtTotal.Text = Convert.ToDecimal(r["TotalPedido"]).ToString("N2");

                if (r["IdCliente"] != DBNull.Value)
                    cmbCliente.SelectedValue = Convert.ToInt32(r["IdCliente"]);
                if (r["IdMaquinaria"] != DBNull.Value)
                    cmbMaquinaria.SelectedValue = Convert.ToInt32(r["IdMaquinaria"]);
                if (r["IdInsumos"] != DBNull.Value)
                    cmbInsumo.SelectedValue = Convert.ToInt32(r["IdInsumos"]);
                if (r["IdEstadoPedido"] != DBNull.Value)
                {
                    int idEst = Convert.ToInt32(r["IdEstadoPedido"]);
                    switch (idEst)
                    {
                        case 1: cmbEstado.SelectedItem = "En Proceso"; break;
                        case 2: cmbEstado.SelectedItem = "Finalizada"; break;
                        case 3: cmbEstado.SelectedItem = "Cancelada"; break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el pedido:\n" + ex.Message,
                    "ERROR-CARGA-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (cmbCliente.SelectedIndex < 0 || cmbMaquinaria.SelectedIndex < 0 ||
                            cmbInsumo.SelectedIndex < 0 || cmbEstado.SelectedIndex < 0)
            {
                MessageBox.Show("Selecciona cliente, maquinaria, insumo y estado.",
                    "IMPREXA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
            {
                MessageBox.Show("Escribe una descripción del pedido.",
                    "IMPREXA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int cantidad;
            if (!int.TryParse(txtCantidad.Text, out cantidad) || cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser un número mayor a 0.",
                    "IMPREXA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal total;
            if (!decimal.TryParse(txtTotal.Text, out total))
            {
                MessageBox.Show("El total debe ser un número válido.",
                    "IMPREXA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var pedido = new Pedido
                {
                    IdPedido = idPedido,
                    DescripcionPedido = txtDescripcion.Text.Trim(),
                    CantidadPedido = cantidad,
                    TotalPedido = total,
                    IdMaquinaria = Convert.ToInt32(cmbMaquinaria.SelectedValue),
                    IdInsumos = Convert.ToInt32(cmbInsumo.SelectedValue),
                    IdPersona = idPersonaOriginal
                };

                bool ok = Pedido.ActualizarPedido(pedido);

                if (!ok)
                {
                    MessageBox.Show("No se pudieron guardar los cambios del pedido.",
                        "IMPREXA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int idEstadoNuevo;
                switch (cmbEstado.SelectedItem.ToString())
                {
                    case "En Proceso": idEstadoNuevo = 1; break;
                    case "Finalizada": idEstadoNuevo = 2; break;
                    case "Cancelada": idEstadoNuevo = 3; break;
                    default: idEstadoNuevo = 1; break;
                }

                bool okEstado = Pedido.CambiarEstado(idPedido, idEstadoNuevo);

                if (!okEstado)
                {
                    MessageBox.Show("Los datos se guardaron, pero no se pudo cambiar el estado.",
                        "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Pedido actualizado correctamente.",
                        "Operación exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar el pedido:\n" + ex.Message,
                    "ERROR-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
