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
    public partial class frmAgregarMaquinaria : Form
    {
        private string rutaFoto = "";

        public frmAgregarMaquinaria()
        {
            InitializeComponent();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            
                try
                {
                    if (string.IsNullOrWhiteSpace(txtNombre.Text))
                    {
                        MessageBox.Show("Debe ingresar el nombre de la maquinaria.", "Campo vacío",
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DialogResult r = MessageBox.Show($"¿Agregar la maquinaria \"{txtNombre.Text.Trim()}\"?", "Confirmar",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (r != DialogResult.Yes)
                        return;

                    Maquinaria nueva = new Maquinaria();

                    nueva.NombreMaquinaria = txtNombre.Text.Trim();
                    nueva.Foto = rutaFoto;

                string resultado = Maquinaria.AgregarMaquinaria(nueva);

                    switch (resultado)
                    {
                        case "NUEVO":
                            MessageBox.Show("Maquinaria agregada correctamente.", "Operación exitosa",
                                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                            this.DialogResult = DialogResult.OK;
                            this.Close();
                            break;

                        case "ACTIVO":
                            MessageBox.Show("Ya existe una maquinaria activa con ese nombre.", "Duplicado",
                                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;

                        case "ELIMINADO":
                            MessageBox.Show("Ya existe una maquinaria eliminada con ese nombre.\n" + "Puede restaurarla desde el listado.",
                                            "Registro eliminado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;

                        case "ERROR":
                            break;

                        default:
                            MessageBox.Show("Resultado inesperado del servidor: " + resultado, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error inesperado al agregar:\n" + ex.Message, "ERROR-EXCEPCION-102",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        rutaFoto = ofd.FileName;

                        if (!File.Exists(rutaFoto))
                        {
                            MessageBox.Show("El archivo seleccionado no existe.", "Archivo no encontrado",
                                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            rutaFoto = "";
                            return;
                        }

                        string carpetaDestino = Path.Combine(Application.StartupPath, "Fotos");
                        if (!Directory.Exists(carpetaDestino))
                            Directory.CreateDirectory(carpetaDestino);

                        string nombreArchivo = Path.GetFileName(rutaFoto);
                        string rutaFinal = Path.Combine(carpetaDestino, nombreArchivo);
                        File.Copy(rutaFoto, rutaFinal, true);

                        rutaFoto = rutaFinal;  

                        if (picFoto != null)
                        {
                            picFoto.Image = Image.FromFile(rutaFoto);
                            picFoto.SizeMode = PictureBoxSizeMode.StretchImage;
                        }

                        MessageBox.Show("Foto cargada correctamente.", "Éxito",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("No tiene permisos para copiar el archivo en esa carpeta.\n\n" + ex.Message,
                                "Permisos denegados",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (IOException ex)
            {
                MessageBox.Show("Error al copiar la imagen (puede que esté en uso).\n\n" + ex.Message,
                                "Error de archivo",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado al subir la foto: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
