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
using Vista.GerenteMaquinaria;

namespace Vista.GerentePedidos
{
    public partial class frmPedidos : Form
    {
        private int filtroActual = 1;   // 1=Todos, 2=En Proceso, 3=Finalizada, 4=Cancelada
        private int idPedidoSeleccionado = -1;
        private Panel panelSeleccionado = null;

        public frmPedidos()
        {
            InitializeComponent();
            CargarPedidosPorFiltro(1);
        }
        private void CargarPedidosPorFiltro(int filtro)
        {
            try
            {
                filtroActual = filtro;
                idPedidoSeleccionado = -1;
                panelSeleccionado = null;

                DataTable pedidos;

                switch (filtro)
                {
                    case 2:
                        pedidos = Pedido.MostrarPedidosPorEstado(1);  // En Proceso
                        break;
                    case 3:
                        pedidos = Pedido.MostrarPedidosPorEstado(2);  // Finalizada
                        break;
                    case 4:
                        pedidos = Pedido.MostrarPedidosPorEstado(3);  // Cancelada
                        break;
                    default:
                        pedidos = Pedido.MostrarPedidos();
                        break;
                }

                GenerarTarjetas(pedidos);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los pedidos: " + ex.Message,
                        "ERROR-CARGADATOS-008", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                frmPatallaNegra fondo = new frmPatallaNegra();
                fondo.StartPosition = FormStartPosition.CenterParent;
                fondo.WindowState = FormWindowState.Maximized;
                fondo.Show();

                AgregarPedido abrir = new AgregarPedido();
                abrir.ShowDialog();

                fondo.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir el formulario de agregar: " + ex.Message,
                        "ERROR-CAMBIO-103", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerarTarjetas(DataTable pedidos)
        {
            try
            {
                flpPedidos.Controls.Clear();

                if (pedidos == null || pedidos.Rows.Count == 0)
                {
                    Label lblVacio = new Label
                    {
                        Text = "No hay pedidos para mostrar.",
                        Font = new Font("Segoe UI", 14, FontStyle.Regular),
                        ForeColor = Color.FromArgb(100, 100, 100),
                        AutoSize = true,
                        Margin = new Padding(20)
                    };
                    flpPedidos.Controls.Add(lblVacio);
                    return;
                }

                foreach (DataRow fila in pedidos.Rows)
                {
                    int id = Convert.ToInt32(fila["IdPedido"]);
                    Panel tarjeta = CrearTarjeta(id, fila);
                    if (tarjeta != null)
                        flpPedidos.Controls.Add(tarjeta);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar tarjetas: " + ex.Message);
            }
        }

        private Panel CrearTarjeta(int id, DataRow fila)
        {
            try
            {
                string cliente = $"{fila["NombreCliente"]} {fila["ApellidoCliente"]}";
                string maquinaria = fila["NombreMaquinaria"].ToString();
                string insumo = fila["NombreInsumo"].ToString();
                string estado = fila["NombreEstadoPedido"].ToString();
                decimal total = Convert.ToDecimal(fila["TotalPedido"]);
                int cantidad = Convert.ToInt32(fila["CantidadPedido"]);
                DateTime fecha = Convert.ToDateTime(fila["FechaPedido"]);

                Panel tarjeta = new Panel
                {
                    Width = 260,
                    Height = 260,
                    BackColor = Color.White,
                    Margin = new Padding(12),
                    Cursor = Cursors.Hand,
                    Tag = id
                };

                tarjeta.Paint += (s, e) =>
                {
                    ControlPaint.DrawBorder(e.Graphics, tarjeta.ClientRectangle,
                        Color.FromArgb(100, 180, 230), ButtonBorderStyle.Solid);
                };

                Label lblId = new Label
                {
                    Text = $"Pedido #{id}",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 60, 100),
                    Location = new Point(10, 10),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };

                Label lblCliente = new Label
                {
                    Text = $"Cliente: {cliente}",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Location = new Point(10, 45),
                    Size = new Size(240, 20),
                    AutoEllipsis = true,
                    BackColor = Color.Transparent
                };

                Label lblMaquinaria = new Label
                {
                    Text = $"Máquina: {maquinaria}",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Location = new Point(10, 70),
                    Size = new Size(240, 20),
                    AutoEllipsis = true,
                    BackColor = Color.Transparent
                };

                Label lblInsumo = new Label
                {
                    Text = $"Insumo: {insumo}",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Location = new Point(10, 95),
                    Size = new Size(240, 20),
                    AutoEllipsis = true,
                    BackColor = Color.Transparent
                };

                Label lblCantidad = new Label
                {
                    Text = $"Cantidad: {cantidad}",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Location = new Point(10, 120),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };

                Label lblFecha = new Label
                {
                    Text = $"Fecha: {fecha:dd/MM/yyyy}",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(120, 120, 120),
                    Location = new Point(10, 145),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };

                Label lblTotal = new Label
                {
                    Text = $"Total: {total:C2}",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 120, 60),
                    Location = new Point(10, 175),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };

                Label lblEstado = new Label
                {
                    Text = estado,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = ObtenerColorEstado(estado),
                    Location = new Point(10, 215),
                    Size = new Size(240, 25),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };

                tarjeta.Controls.Add(lblId);
                tarjeta.Controls.Add(lblCliente);
                tarjeta.Controls.Add(lblMaquinaria);
                tarjeta.Controls.Add(lblInsumo);
                tarjeta.Controls.Add(lblCantidad);
                tarjeta.Controls.Add(lblFecha);
                tarjeta.Controls.Add(lblTotal);
                tarjeta.Controls.Add(lblEstado);

                // Eventos
                EventHandler seleccionar = (s, e) =>
                {
                    if (panelSeleccionado != null)
                        panelSeleccionado.BackColor = Color.White;

                    tarjeta.BackColor = Color.FromArgb(200, 230, 255);
                    panelSeleccionado = tarjeta;
                    idPedidoSeleccionado = id;
                };

                EventHandler actualizar = (s, e) =>
                {
                    frmActualizarPedido frm = new frmActualizarPedido(id);
                    DialogResult resultado = frm.ShowDialog();
                    if (resultado == DialogResult.OK)
                        CargarPedidosPorFiltro(filtroActual);
                };

                tarjeta.Click += seleccionar;
                tarjeta.DoubleClick += actualizar;

                foreach (Control c in tarjeta.Controls)
                {
                    c.Click += seleccionar;
                    c.DoubleClick += actualizar;
                }

                return tarjeta;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear tarjeta: " + ex.Message);
                return null;
            }
        }

        private Color ObtenerColorEstado(string estado)
        {
            switch (estado)
            {
                case "En Proceso": return Color.FromArgb(200, 120, 0);
                default: return Color.Black;
            }
        }
    }
}
