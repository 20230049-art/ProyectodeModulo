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

namespace Vista.GerenteUsuario
{
    public partial class frmUsuario : Form
    {
        private int idUsuarioSeleccionado = -1;
        private Panel panelSeleccionado = null;

        public frmUsuario()
        {
            InitializeComponent();
            CargarRoles();
            CargarUsuarios();
            EstilizarGrid();
        }

        private void CargarRoles()
        {
            try
            {
                cmbRol.Items.Clear();
                cmbRol.Items.Add("Gerente");    
                cmbRol.Items.Add("Empleado");   
                cmbRol.Items.Add("Cliente");   
                cmbRol.SelectedIndex = 1;       
                cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los roles:\n" + ex.Message,
                                "ERROR-CARGAROLES-101", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarUsuarios()
        {
            try
            {
                DataTable usuarios = Persona.MostrarUsuarios();

                dgvUsuarios.DataSource = null;

                if (usuarios != null && usuarios.Rows.Count > 0)
                {
                    dgvUsuarios.DataSource = usuarios;
                    ConfigurarColumnas();
                }

                idUsuarioSeleccionado = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los usuarios:\n" + ex.Message,
                                "ERROR-CARGADATOS-008",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarColumnas()
        {
            try
            {
                if (dgvUsuarios.Columns.Count == 0) return;

                if (dgvUsuarios.Columns.Contains("IdPersona"))
                    dgvUsuarios.Columns["IdPersona"].HeaderText = "ID";
                if (dgvUsuarios.Columns.Contains("NombrePersona"))
                    dgvUsuarios.Columns["NombrePersona"].HeaderText = "Nombre";
                if (dgvUsuarios.Columns.Contains("ApellidoPersona"))
                    dgvUsuarios.Columns["ApellidoPersona"].HeaderText = "Apellido";
                if (dgvUsuarios.Columns.Contains("DUIPersona"))
                    dgvUsuarios.Columns["DUIPersona"].HeaderText = "Usuario";
                if (dgvUsuarios.Columns.Contains("CorreoPersona"))
                    dgvUsuarios.Columns["CorreoPersona"].HeaderText = "Correo";
                if (dgvUsuarios.Columns.Contains("TelefonoPersona"))
                    dgvUsuarios.Columns["TelefonoPersona"].HeaderText = "Teléfono";
                if (dgvUsuarios.Columns.Contains("NombreRol"))
                    dgvUsuarios.Columns["NombreRol"].HeaderText = "Rol";

                if (dgvUsuarios.Columns.Contains("IdPersona"))
                    dgvUsuarios.Columns["IdPersona"].ReadOnly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al configurar columnas:\n" + ex.Message,
                                "ERROR-CARGADATOS-008",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EstilizarGrid()
        {
            try
            {
                dgvUsuarios.EnableHeadersVisualStyles = false;
                dgvUsuarios.BorderStyle = BorderStyle.FixedSingle;
                dgvUsuarios.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgvUsuarios.GridColor = Color.FromArgb(180, 210, 235);
                dgvUsuarios.BackgroundColor = Color.FromArgb(235, 245, 255);
                dgvUsuarios.RowHeadersVisible = false;
                dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvUsuarios.MultiSelect = false;
                dgvUsuarios.ReadOnly = true;
                dgvUsuarios.AllowUserToAddRows = false;
                dgvUsuarios.AllowUserToDeleteRows = false;
                dgvUsuarios.AllowUserToResizeRows = false;
                dgvUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                dgvUsuarios.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 130, 190);
                dgvUsuarios.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvUsuarios.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                dgvUsuarios.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                dgvUsuarios.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
                dgvUsuarios.ColumnHeadersHeight = 38;
                dgvUsuarios.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

                dgvUsuarios.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                dgvUsuarios.DefaultCellStyle.ForeColor = Color.FromArgb(30, 60, 100);
                dgvUsuarios.DefaultCellStyle.BackColor = Color.FromArgb(235, 245, 255);
                dgvUsuarios.DefaultCellStyle.SelectionBackColor = Color.FromArgb(150, 200, 240);
                dgvUsuarios.DefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 50, 90);
                dgvUsuarios.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
                dgvUsuarios.RowTemplate.Height = 34;

                dgvUsuarios.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(220, 235, 250);
                dgvUsuarios.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(150, 200, 240);
                dgvUsuarios.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(30, 60, 100);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al estilizar el grid:\n" + ex.Message,
                                "ERROR-EXCEPCION-102", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvUsuarios_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;

                DataGridViewRow fila = dgvUsuarios.Rows[e.RowIndex];

                if (fila.Cells["IdPersona"].Value == null) return;

                idUsuarioSeleccionado = Convert.ToInt32(fila.Cells["IdPersona"].Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al seleccionar el usuario:\n" + ex.Message,
                                "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MessageBox.Show("Debe ingresar el nombre.", "Campo vacío",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNombre.Focus(); return;
                }

                if (string.IsNullOrWhiteSpace(txtApellidos.Text))
                {
                    MessageBox.Show("Debe ingresar los apellidos.", "Campo vacío",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtApellidos.Focus(); return;
                }

                if (string.IsNullOrWhiteSpace(mtxtDUI.Text))
                {
                    MessageBox.Show("Debe ingresar el DUI.", "Campo vacío",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    mtxtDUI.Focus(); return;
                }

                string dui = mtxtDUI.Text.Trim();

                if (dui.Length != 10 || dui[8] != '-' || !dui.Replace("-", "").All(char.IsDigit))
                {
                    MessageBox.Show("El DUI debe tener el formato 00000000-0.",
                                    "Formato inválido",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    mtxtDUI.Focus(); return;
                }

                if (string.IsNullOrWhiteSpace(txtCorreo.Text))
                {
                    MessageBox.Show("Debe ingresar el correo.", "Campo vacío",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCorreo.Focus(); return;
                }

                if (string.IsNullOrWhiteSpace(mtxtTelefono.Text))
                {
                    MessageBox.Show("Debe ingresar el teléfono.", "Campo vacío",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    mtxtTelefono.Focus(); return;
                }

                if (string.IsNullOrWhiteSpace(txtContrasena.Text))
                {
                    MessageBox.Show("Debe ingresar la contraseña.", "Campo vacío",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtContrasena.Focus(); return;
                }

                if (cmbRol.SelectedIndex < 0)
                {
                    MessageBox.Show("Debe seleccionar un rol.", "Campo vacío",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbRol.Focus(); return;
                }

                DialogResult r = MessageBox.Show(
                    $"¿Crear al usuario \"{txtNombre.Text.Trim()} {txtApellidos.Text.Trim()}\"?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (r != DialogResult.Yes) return;

                Persona nuevo = new Persona
                {
                    NombrePersona = txtNombre.Text.Trim(),
                    ApellidoPersona = txtApellidos.Text.Trim(),
                    DUIPersona = dui,
                    CorreoPersona = txtCorreo.Text.Trim(),
                    TelefonoPersona = mtxtTelefono.Text.Trim(),
                    IdRol = cmbRol.SelectedIndex + 1
                };

                bool exito = Persona.AgregarUsuario(nuevo, txtContrasena.Text);

                if (exito)
                {
                    MessageBox.Show("Usuario creado correctamente.",
                                    "Operación exitosa",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarFormulario();
                    CargarUsuarios();
                }
                else
                {
                    MessageBox.Show("No se pudo crear el usuario.\n" +
                                    "Verifique que el correo, teléfono y DUI no estén duplicados.",
                                    "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el usuario:\n" + ex.Message,
                                "ERROR-EXCEPCION-102",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarFormulario()
        {
            txtNombre.Clear();
            txtApellidos.Clear();
            txtCorreo.Clear();
            mtxtDUI.Clear();
            mtxtTelefono.Clear();
            txtContrasena.Clear();
            cmbRol.SelectedIndex = 1;
            txtNombre.Focus();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (idUsuarioSeleccionado <= 0)
                {
                    MessageBox.Show("Debe seleccionar un usuario de la lista.",
                                    "Sin selección", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult r = MessageBox.Show(
                    "¿Está seguro de eliminar al usuario seleccionado?\n\n" +
                    "Podrá restaurarlo después si lo necesita.", "Confirmar eliminación",
                    MessageBoxButtons.YesNo,MessageBoxIcon.Question);

                if (r != DialogResult.Yes) return;

                bool eliminado = Persona.EliminarUsuario(idUsuarioSeleccionado);

                if (eliminado)
                {
                    MessageBox.Show("Usuario eliminado correctamente.",
                                    "Operación exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    idUsuarioSeleccionado = -1;
                    CargarUsuarios();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el usuario.",
                                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el usuario:\n" + ex.Message,
                                "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPaginaAnterior_Click(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        private void btnPaginaSiguiente_Click(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }
    }
}
