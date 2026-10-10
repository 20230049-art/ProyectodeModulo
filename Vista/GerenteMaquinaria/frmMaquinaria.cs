using Modelos.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vista.GerenteInventario;

namespace Vista.GerenteMaquinaria
{
    public partial class frmMaquinaria : Form
    {
        private int filtroActual = 1;
        private int idMaquinariaSeleccionada;
        private string busquedaActual = "";
        public frmMaquinaria()
        {
            try
            {
                InitializeComponent();
                CargarMaquinariaPorFiltro(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al inicializar el formulario: " + ex.Message,
                        "ERROR-FORMULARIO-101", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarMaquinariaPorFiltro(int filtro)
        {
            try
            {
                filtroActual = filtro;
                idMaquinariaSeleccionada = -1; // resetear selección
                DataTable maquinaria;

                switch (filtro)
                {
                    case 2:
                        maquinaria = Maquinaria.MostrarMaquinariaActiva();
                        break;
                    case 3:
                        maquinaria = Maquinaria.MostrarMaquinariaInactiva();
                        break;
                    default:
                        maquinaria = Maquinaria.MostrarMaquinariaTotales();
                        break;
                }

                GenerarTarjetas(maquinaria);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la maquinaria: " + ex.Message,
                        "ERROR-CARGADATOS-008", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void GenerarTarjetas(DataTable maquinaria)
        {
            try
            {
                flowLayoutPanel1.Controls.Clear();

                if (maquinaria == null || maquinaria.Rows.Count == 0)
                {
                    Label lblVacio = new Label
                    {
                        Text = "No hay maquinaria para mostrar.",
                        Font = new Font("Book Antiqua", 15),
                        ForeColor = Color.FromArgb(100, 100, 100),
                        AutoSize = true,
                        Margin = new Padding(20)
                    };
                    flowLayoutPanel1.Controls.Add(lblVacio);
                    return;
                }

                foreach (DataRow fila in maquinaria.Rows)
                {
                    int id = Convert.ToInt32(fila["IdMaquinaria"]);
                    string nombre = fila["NombreMaquinaria"].ToString();
                    string foto = fila["Foto"]?.ToString() ?? "";

                    string estado = "";
                    if (maquinaria.Columns.Contains("EstadoActualMaquinaria"))
                    {
                        estado = fila["EstadoActualMaquinaria"].ToString();
                    }
                    else if (maquinaria.Columns.Contains("IdEstadoMaquinaria"))
                    {
                        int idEstado = Convert.ToInt32(fila["IdEstadoMaquinaria"]);
                        estado = ObtenerNombreEstado(idEstado);
                    }

                    Panel tarjeta = CrearTarjeta(id, nombre, estado, foto);
                    flowLayoutPanel1.Controls.Add(tarjeta);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar las tarjetas: " + ex.Message,
                        "ERROR-CARGADATOS-008", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel CrearTarjeta(int id, string nombre, string estado, string rutaFoto)
        {
            Panel tarjeta = new Panel
            {
                Width = 200,
                Height = 230,
                BackColor = Color.White,
                Margin = new Padding(10),
                Cursor = Cursors.Hand,
                Tag = id 
            };

            tarjeta.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, tarjeta.ClientRectangle,
                    Color.FromArgb(208, 112, 3), ButtonBorderStyle.Solid);
            };

            PictureBox pic = new PictureBox
            {
                Width = 180,
                Height = 130,
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(245, 228, 200),
                Tag = id
            };

            if (!string.IsNullOrWhiteSpace(rutaFoto) && File.Exists(rutaFoto))
            {
                try
                {
                    pic.Image = Image.FromFile(rutaFoto);
                }
                catch
                {
                }
            }

            Label lblNombre = new Label
            {
                Text = nombre,
                Font = new Font("Book Antiqua", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 6, 6),
                AutoSize = false,
                Width = 180,
                Height = 30,
                Location = new Point(10, 148),
                TextAlign = ContentAlignment.MiddleCenter,
                Tag = id
            };

            Label lblEstado = new Label
            {
                Text = estado,
                Font = new Font("Bookman Old Style", 10, FontStyle.Italic),
                ForeColor = ObtenerColorEstado(estado),
                AutoSize = false,
                Width = 180,
                Height = 25,
                Location = new Point(10, 180),
                TextAlign = ContentAlignment.MiddleCenter,
                Tag = id
            };

            tarjeta.Controls.Add(pic);
            tarjeta.Controls.Add(lblNombre);
            tarjeta.Controls.Add(lblEstado);

            EventHandler seleccionar = (s, e) => SeleccionarTarjeta(tarjeta, id);
            tarjeta.Click += seleccionar;
            pic.Click += seleccionar;
            lblNombre.Click += seleccionar;
            lblEstado.Click += seleccionar;

            EventHandler actualizar = (s, e) => AbrirActualizar(id);
            tarjeta.DoubleClick += actualizar;
            pic.DoubleClick += actualizar;
            lblNombre.DoubleClick += actualizar;
            lblEstado.DoubleClick += actualizar;

            return tarjeta;
        }

        private void SeleccionarTarjeta(Panel tarjeta, int id)
        {
            try
            {
                foreach (Control c in flowLayoutPanel1.Controls)
                {
                    if (c is Panel p)
                    {
                        p.BackColor = Color.White;
                    }
                }

                tarjeta.BackColor = Color.FromArgb(249, 190, 121); 
                idMaquinariaSeleccionada = id;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al seleccionar la tarjeta: " + ex.Message,
                        "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirActualizar(int idMaquinaria)
        {
            try
            {
                frmActualizarMaquinaria frmActualizar = new frmActualizarMaquinaria(idMaquinaria);
                frmActualizar.ShowDialog();
                CargarMaquinariaPorFiltro(filtroActual);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir el formulario de actualización: " + ex.Message,
                        "ERROR-CAMBIO-103", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ObtenerNombreEstado(int idEstado)
        {
            switch (idEstado)
            {
                case 1: return "Libre";
                case 2: return "Activa";
                case 3: return "Inactiva";
                default: return "Desconocido";
            }
        }

        private Color ObtenerColorEstado(string estado)
        {
            switch (estado)
            {
                case "Activa": return Color.FromArgb(0, 120, 0);    
                case "Inactiva": return Color.FromArgb(180, 0, 0);  
                case "Libre": return Color.FromArgb(0, 80, 160);    
                default: return Color.Black;
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

                frmAgregarMaquinaria abrir = new frmAgregarMaquinaria();
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
