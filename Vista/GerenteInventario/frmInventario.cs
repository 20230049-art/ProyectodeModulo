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

namespace Vista.GerenteInventario
{
    public partial class frmInventario : Form
    {
        private int filtroActual = 1;
        private string busquedaActual = "";

        public frmInventario()
        {
            try
            {

                InitializeComponent();

                EstilizarGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al inicializar el formulario: " + ex.Message,
                        "ERROR-FORMULARIO-101", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void CargarInsumosPorFiltro(int filtro)
        {
            try
            {
                filtroActual = filtro;
                DataTable insumos;

                switch (filtro)
                {
                    case 2:
                        insumos = Insumos.MostrarInsumosDisponibles();
                        break;
                    case 3:
                        insumos = Insumos.MostrarInsumosAgotados();
                        break;
                    default:
                        insumos = Insumos.MostrarInsumos();
                        break;
                }

                dataGridView1.DataSource = null;

                if (insumos != null && insumos.Rows.Count > 0)
                {
                    dataGridView1.DataSource = insumos;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los insumos: " + ex.Message,
                        "ERROR-CARGADATOS-008", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmInventario_Load(object sender, EventArgs e)
        {
            AplicarFiltro(1);
        }

        private void AplicarFiltro(int filtro)
        {
            try
            {
                filtroActual = filtro;
                busquedaActual = "";
                textBox1.Text = "";

                DataTable insumos;

                if (!string.IsNullOrWhiteSpace(busquedaActual))
                {
                    insumos = Insumos.BuscarInsumo(busquedaActual);
                }
                else
                {
                    switch (filtro)
                    {
                        case 2: insumos = Insumos.MostrarInsumosDisponibles(); break;
                        case 3: insumos = Insumos.MostrarInsumosAgotados(); break;
                        default: insumos = Insumos.MostrarInsumos(); break;
                    }
                }

                AsignarInsumosGrid(insumos);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los insumos: " + ex.Message,
                        "ERROR-CARGADATOS-008", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AsignarInsumosGrid(DataTable insumos)
        {
            try
            {
                dataGridView1.DataSource = null;

                if (insumos == null || insumos.Rows.Count == 0)
                {
                    return;
                }

                dataGridView1.DataSource = insumos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al asignar los insumos al grid: " + ex.Message,
                        "ERROR-CARGADATOS-008", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                busquedaActual = textBox1.Text.Trim();

                if (string.IsNullOrWhiteSpace(busquedaActual))
                {
                    AplicarFiltro(filtroActual);
                    return;
                }

                DataTable insumosFiltrados = Insumos.BuscarInsumo(busquedaActual);
                AsignarInsumosGrid(insumosFiltrados);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar insumos: " + ex.Message,
                        "ERROR-NODATO-007", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                frmPatallaNegra fondo = new frmPatallaNegra();
                fondo.StartPosition = FormStartPosition.CenterParent;
                fondo.WindowState = FormWindowState.Maximized;
                fondo.Show();

                frmAgregarInsumo abrir = new frmAgregarInsumo();
                abrir.ShowDialog();

                fondo.Close();

                AplicarFiltro(filtroActual);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir el formulario de agregar: " + ex.Message,
                        "ERROR-CAMBIO-103", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show("Debe seleccionar un insumo del listado para eliminar.",
                            "ERROR-CAMVACIO-001", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (dataGridView1.CurrentRow.Cells["IdInsumos"].Value == null)
                {
                    MessageBox.Show("No se pudo obtener el ID del insumo seleccionado.",
                            "ERROR-CARGADATOS-008", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int idInsumo = Convert.ToInt32(dataGridView1.CurrentRow.Cells["IdInsumos"].Value);
                string nombre = dataGridView1.CurrentRow.Cells["NombreInsumo"].Value?.ToString() ?? "";

                DialogResult confirmacion = MessageBox.Show(
                    $"¿Está seguro que desea eliminar el siguiente insumo?\n\n" + $"Insumo: \"{nombre}\"\n\n" +
                    "El insumo no se borrará permanentemente, solo se marcará como eliminado.",  "Confirmar eliminación",
                    MessageBoxButtons.YesNo,  MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

                if (confirmacion != DialogResult.Yes) return;

                bool exito = Insumos.EliminarInsumo(idInsumo);

                if (!exito)
                {
                    MessageBox.Show("No se pudo eliminar el insumo de la base de datos.",
                            "ERROR-ELIMINAR-015", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Insumo eliminado correctamente.",
                        "PROCEDIMIENTO-EXITOSO", MessageBoxButtons.OK, MessageBoxIcon.Information);

                AplicarFiltro(filtroActual);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error de base de datos al eliminar: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el insumo: " + ex.Message,
                        "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

                DataGridViewRow fila = dataGridView1.Rows[e.RowIndex];
                if (fila.Cells["IdInsumos"].Value == null) return;

                int idInsumo = Convert.ToInt32(fila.Cells["IdInsumos"].Value);

                frmActualizarInsumo frmActualizar = new frmActualizarInsumo(idInsumo);
                frmActualizar.ShowDialog();

                AplicarFiltro(filtroActual);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir el formulario de actualización: " + ex.Message,
                        "ERROR-CAMBIO-103", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EstilizarGrid()
        {
            try
            {
                dataGridView1.EnableHeadersVisualStyles = false;
                dataGridView1.BorderStyle = BorderStyle.FixedSingle;
                dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dataGridView1.GridColor = Color.FromArgb(180, 210, 235);   
                dataGridView1.BackgroundColor = Color.FromArgb(235, 245, 255); 
                dataGridView1.RowHeadersVisible = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.MultiSelect = false;
                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.AllowUserToDeleteRows = false;
                dataGridView1.AllowUserToResizeRows = false;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 130, 190);  
                dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                dataGridView1.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
                dataGridView1.ColumnHeadersHeight = 38;
                dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

                dataGridView1.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                dataGridView1.DefaultCellStyle.ForeColor = Color.FromArgb(30, 60, 100);         
                dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(235, 245, 255);      
                dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(150, 200, 240); 
                dataGridView1.DefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 50, 90);
                dataGridView1.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
                dataGridView1.RowTemplate.Height = 34;

                dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(220, 235, 250);      
                dataGridView1.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(150, 200, 240);
                dataGridView1.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(30, 60, 100);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al estilizar el grid: " + ex.Message,
                        "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnActivo_Click(object sender, EventArgs e)
        {
            CargarInsumosPorFiltro(1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CargarInsumosPorFiltro(2);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CargarInsumosPorFiltro(3);
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;

                DataGridViewRow fila = dataGridView1.Rows[e.RowIndex];

                if (fila.Cells["IdInsumos"].Value == null)
                {
                    MessageBox.Show("No se pudo obtener el ID del insumo seleccionado.", "ERROR-CARGADATOS-008",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int idInsumo = Convert.ToInt32(fila.Cells["IdInsumos"].Value);
                string nombre = fila.Cells["NombreInsumo"].Value?.ToString() ?? "";

                frmPatallaNegra fondo = new frmPatallaNegra();
                fondo.StartPosition = FormStartPosition.CenterParent;
                fondo.WindowState = FormWindowState.Maximized;
                fondo.Show();

                frmActualizarInsumo frmActualizar = new frmActualizarInsumo(idInsumo);
                frmActualizar.ShowDialog();

                fondo.Close();

                AplicarFiltro(filtroActual);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir el formulario de actualización:\n" + ex.Message,
                                "ERROR-CAMBIO-103",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
