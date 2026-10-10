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

namespace Vista.GerenteMaquinaria
{
    public partial class frmActualizarMaquinaria : Form
    {
        private int idMaquinaria;
        private int idEstadoActual = 1;         
        private string rutaFotoSeleccionada = "";

        public frmActualizarMaquinaria(int idMaquinaria)
        {
            InitializeComponent();
            this.idMaquinaria = idMaquinaria;   
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult r = MessageBox.Show( "¿Cancelar la actualización?", "Confirmar cancelación",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (r == DialogResult.Yes)
                    this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cerrar el formulario:\n" + ex.Message,  "ERROR-EXCEPCION-102",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmActualizarMaquinaria_Load(object sender, EventArgs e)
        {
            try
            {
                CargarMaquinaria();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el formulario:\n" + ex.Message,  "ERROR-FORMULARIO-101",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarMaquinaria()
        {
            try
            {
                DataTable dt = Maquinaria.ObtenerMaquinariaId(idMaquinaria);

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontró la maquinaria seleccionada.", "Sin resultados",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }

                DataRow fila = dt.Rows[0];

                txtNombre.Text = fila["NombreMaquinaria"]?.ToString() ?? "";

                if (fila["IdEstadoMaquinaria"] != DBNull.Value)
                    idEstadoActual = Convert.ToInt32(fila["IdEstadoMaquinaria"]);

                rutaFotoSeleccionada = fila["Foto"]?.ToString() ?? "";

                if (!string.IsNullOrWhiteSpace(rutaFotoSeleccionada))
                    MostrarImagen(rutaFotoSeleccionada);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los datos:\n" + ex.Message,  "ERROR-CARGADATOS-008",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarImagen(string ruta)
        {
            try
            {
                string rutaCompleta = ruta;
                if (!Path.IsPathRooted(ruta))
                    rutaCompleta = Path.Combine(Application.StartupPath, ruta);

                if (File.Exists(rutaCompleta))
                {
                    byte[] bytes = File.ReadAllBytes(rutaCompleta);
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        picFoto.Image = Image.FromStream(ms);
                    }
                    picFoto.SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    picFoto.Image = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al mostrar la imagen:\n" + ex.Message,  "ERROR-IMAGEN-103",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                picFoto.Image = null;
            }
        }

        private void btnSubir_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = "Seleccione una imagen";
                    ofd.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string rutaOrigen = ofd.FileName;

                        if (!File.Exists(rutaOrigen))
                        {
                            MessageBox.Show("El archivo seleccionado no existe.",
                                            "Archivo no encontrado",
                                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        string carpetaDestino = Path.Combine(Application.StartupPath, "Fotos");
                        if (!Directory.Exists(carpetaDestino))
                            Directory.CreateDirectory(carpetaDestino);

                        string nombreArchivo = Path.GetFileName(rutaOrigen);
                        string rutaFinal = Path.Combine(carpetaDestino, nombreArchivo);
                        File.Copy(rutaOrigen, rutaFinal, true);

                        rutaFotoSeleccionada = rutaFinal;

                        MostrarImagen(rutaFotoSeleccionada);

                        MessageBox.Show("Foto cargada correctamente.", "Éxito",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("No tiene permisos para copiar el archivo.\n\n" + ex.Message,  "Permisos denegados",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (IOException ex)
            {
                MessageBox.Show("Error al copiar la imagen (puede que esté en uso).\n\n" + ex.Message,  "Error de archivo",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado al subir la foto:\n" + ex.Message, "ERROR-EXCEPCION-102",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MessageBox.Show("Debe ingresar el nombre de la maquinaria.", "Campo vacío",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNombre.Focus();
                    return;
                }

                DialogResult r = MessageBox.Show(
                    $"¿Actualizar la maquinaria \"{txtNombre.Text.Trim()}\"?",  "Confirmar",
                    MessageBoxButtons.YesNo,  MessageBoxIcon.Question);

                if (r != DialogResult.Yes) return;

                Maquinaria m = new Maquinaria
                {
                    IdMaquinaria = idMaquinaria,
                    NombreMaquinaria = txtNombre.Text.Trim(),
                    IdEstadoMaquinaria = idEstadoActual,   
                    Foto = rutaFotoSeleccionada ?? ""
                };

                Maquinaria.ActualizarMaquinaria(m);

                MessageBox.Show("Maquinaria actualizada correctamente.", "Operación exitosa",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar la maquinaria:\n" + ex.Message, "ERROR-EXCEPCION-102",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
