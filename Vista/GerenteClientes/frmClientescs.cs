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
using Vista.GerenteInventario;

namespace Vista.GerenteClientes
{
    public partial class frmClientescs : Form
    {
        public frmClientescs()
        {
            InitializeComponent();
            DataTable clientes = Clientes.MostrarClient();
           CargarClientesEnPantalla(clientes);
        }

        private void CargarClientesEnPantalla(DataTable clientes)
        {
            try
            {
                flpClientes.Controls.Clear();

                if (clientes == null || clientes.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontraron clientes que coincidan con la búsqueda.",
                            "ERROR-NODATO-007", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (DataRow fila in clientes.Rows)
                {
                    Panel panelCliente = MostrarClientes(fila);

                    if (panelCliente == null)
                    {
                        continue;
                    }

                    DataRow filaLocal = fila;

                    panelCliente.Click += (s, e) =>
                    {
                        try
                        {
                            //if (modoEliminar)
                            //{
                            //    ConfirmarEliminar(filaLocal);
                            //}
                            //else
                            //{
                            //    AbrirfrmActualizar_Click(s, e);
                            //}
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error al procesar el clic del cliente: " + ex.Message,
                                    "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };

                    flpClientes.Controls.Add(panelCliente);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al mostrar los clientes en pantalla: " + ex.Message,
                        "ERROR-CARGADATOS-008", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel MostrarClientes(DataRow fila)
        {
            try
            {
                if (fila == null)
                {
                    return null;
                }

                Panel panelCliente = new Panel();

                panelCliente.Width = pnlContenedor.Width;
                panelCliente.Height = pnlContenedor.Height;
                panelCliente.BorderStyle = pnlContenedor.BorderStyle;
                panelCliente.Tag = fila["IdCliente"];

                Label lblNombre = new Label();

                lblNombre.Text = "Nombre: " + fila["NombreCliente"].ToString();
                lblNombre.Font = new Font("Book Antiqua", 21, FontStyle.Regular);
                lblNombre.Location = new Point(17, 28);
                lblNombre.AutoSize = true;

                Label lblApellido = new Label();

                lblApellido.Text = "Apellido: " + fila["ApellidoCliente"].ToString();
                lblApellido.Font = new Font("Book Antiqua", 21, FontStyle.Regular);
                lblApellido.Location = new Point(17, 69);
                lblApellido.AutoSize = true;

                Label lblDocumento = new Label();
                lblDocumento.Text = "DUI: " + fila["DUICliente"].ToString();
                lblDocumento.Font = new Font("Book Antiqua", 21, FontStyle.Regular);
                lblDocumento.Location = new Point(429, 28);
                lblDocumento.AutoSize = true;

                Label lblTelefono = new Label();

                lblTelefono.Text = "Teléfono: " + fila["TelefonoCliente"].ToString();
                lblTelefono.Font = new Font("Book Antiqua", 21, FontStyle.Regular);
                lblTelefono.Location = new Point(781, 69);
                lblTelefono.AutoSize = true;

                Label lblCorreo = new Label();
                lblCorreo.Text = "Correo: " + fila["CorreoCliente"].ToString();
                lblCorreo.Font = new Font("Book Antiqua", 21, FontStyle.Regular);
                lblCorreo.Location = new Point(781, 28);
                lblCorreo.AutoSize = true;

                Panel pnlDecoracion = new Panel();
                pnlDecoracion.Size = new Size(1, 98);
                pnlDecoracion.Location = new Point(416, 17);
                pnlDecoracion.BackColor = Color.Black;

                Panel pnlDecoracion1 = new Panel();
                pnlDecoracion1.Size = new Size(1, 98);
                pnlDecoracion1.Location = new Point(759, 17);
                pnlDecoracion1.BackColor = Color.Black;

                panelCliente.Controls.Add(lblNombre);
                panelCliente.Controls.Add(lblApellido);
                panelCliente.Controls.Add(lblDocumento);
                panelCliente.Controls.Add(lblTelefono);
                panelCliente.Controls.Add(lblCorreo);
                panelCliente.Controls.Add(pnlDecoracion);
                panelCliente.Controls.Add(pnlDecoracion1);

                return panelCliente;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("Una columna esperada no existe en los datos del cliente: " + ex.Message,
                        "ERROR-CARGADATOS-008", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear el panel del cliente: " + ex.Message,
                        "ERROR-CARGADATOS-008", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                frmPatallaNegra fondo = new frmPatallaNegra();
                fondo.StartPosition = FormStartPosition.CenterParent;
                fondo.WindowState = FormWindowState.Maximized;
                fondo.Show();

                frmAgregarCliente abrir = new frmAgregarCliente();
                abrir.ShowDialog();

                fondo.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir el formulario de agregar: " + ex.Message,
                        "ERROR-CAMBIO-103", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
