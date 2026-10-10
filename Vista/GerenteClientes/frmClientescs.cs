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
        private int idClienteSeleccionado = -1;
        private Panel panelSeleccionado = null;

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
                idClienteSeleccionado = -1;
                panelSeleccionado = null;

                if (clientes == null || clientes.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontraron clientes que coincidan con la búsqueda.",
                            "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (DataRow fila in clientes.Rows)
                {
                    Panel panelCliente = MostrarClientes(fila);

                    if (panelCliente == null) continue;

                    DataRow filaLocal = fila;
                    int idClienteLocal = Convert.ToInt32(filaLocal["IdCliente"]);

                    EventHandler seleccionar = (s, e) =>
                    {
                        try
                        {
                            if (panelSeleccionado != null)
                                panelSeleccionado.BackColor = Color.White;

                            panelCliente.BackColor = Color.FromArgb(200, 230, 255);
                            panelSeleccionado = panelCliente;
                            idClienteSeleccionado = idClienteLocal;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error al seleccionar el cliente: " + ex.Message,
                                    "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };

                    EventHandler abrirActualizar = (s, e) =>
                    {
                        try
                        {
                            frmPatallaNegra fondo = new frmPatallaNegra();
                            fondo.StartPosition = FormStartPosition.CenterParent;
                            fondo.WindowState = FormWindowState.Maximized;
                            fondo.Show();

                            frmActualizarCliente frmActualizar = new frmActualizarCliente(idClienteLocal);
                            fondo.Close();

                            DialogResult resultado = frmActualizar.ShowDialog();


                            if (resultado == DialogResult.OK)
                            {
                                DataTable recargados = Clientes.MostrarClient();
                                CargarClientesEnPantalla(recargados);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error al abrir el formulario de actualización:\n" + ex.Message,
                                    "ERROR-CAMBIO-103", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };

                    panelCliente.Click += seleccionar;
                    panelCliente.DoubleClick += abrirActualizar;

                    foreach (Control abajo in panelCliente.Controls)
                    {
                        abajo.Click += seleccionar;
                        abajo.DoubleClick += abrirActualizar;
                    }

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
                panelCliente.BackColor = pnlContenedor.BackColor;
                panelCliente.Margin = new Padding(8);
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

                Label lblTelefono = new Label();

                lblTelefono.Text = "Teléfono: " + fila["TelefonoCliente"].ToString();
                lblTelefono.Font = new Font("Book Antiqua", 21, FontStyle.Regular);
                lblTelefono.Location = new Point(425, 28);
                lblTelefono.AutoSize = true;

                Label lblCorreo = new Label();
                lblCorreo.Text = "Correo: " + fila["CorreoCliente"].ToString();
                lblCorreo.Font = new Font("Book Antiqua", 21, FontStyle.Regular);
                lblCorreo.Location = new Point(425, 70);
                lblCorreo.AutoSize = true;

                Label lblDocumento = new Label();
                lblDocumento.Text = "DUI: " + fila["DUICliente"].ToString();
                lblDocumento.Font = new Font("Book Antiqua", 21, FontStyle.Regular);
                lblDocumento.Location = new Point(779, 54);
                lblDocumento.AutoSize = true;

                Panel pnlDecoracion = new Panel();
                pnlDecoracion.Size = new Size(1, 98);
                pnlDecoracion.Location = new Point(410, 17);
                pnlDecoracion.BackColor = Color.Black;

                Panel pnlDecoracion1 = new Panel();
                pnlDecoracion1.Size = new Size(1, 98);
                pnlDecoracion1.Location = new Point(762, 17);
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

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                if (idClienteSeleccionado <= 0)
                {
                    MessageBox.Show("Debe seleccionar un cliente de la lista para eliminar.", "Sin selección",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult r = MessageBox.Show(
                    "¿Está seguro de eliminar al cliente seleccionado?\n\n" +  "Podrá restaurarlo después si lo necesita.",
                    "Confirmar eliminación", MessageBoxButtons.YesNo,  MessageBoxIcon.Question);

                if (r != DialogResult.Yes) return;

                bool eliminado = Clientes.EliminarCliente(idClienteSeleccionado);

                if (eliminado)
                {
                    MessageBox.Show("Cliente eliminado correctamente.", "Operación exitosa",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                    idClienteSeleccionado = -1;
                    panelSeleccionado = null;

                    DataTable clientes = Clientes.MostrarClient();
                    CargarClientesEnPantalla(clientes);
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el cliente.\n" + "Puede que ya esté eliminado o no exista.",
                                    "Aviso",MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el cliente:\n" + ex.Message,
                                "ERROR",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
