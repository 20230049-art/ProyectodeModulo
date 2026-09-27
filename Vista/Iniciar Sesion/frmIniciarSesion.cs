using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vista.InicioGerente;

namespace Vista.Iniciar_Sesion
{
    public partial class frmIniciarSesion : Form
    {
        public frmIniciarSesion()
        {
            InitializeComponent();
        }

        //Cambio de formularios
        private Form activeForm = null;

        private void abrirForm(Form formularioAbrir)
        {
            if (activeForm != null && activeForm.GetType() == formularioAbrir.GetType())
            {
                return;
            }

            if (activeForm != null)
            {
                activeForm.Close();
                pnlVistaIniciarSesion.Controls.Remove(activeForm);
                activeForm.Dispose();
            }

            activeForm = formularioAbrir;
            formularioAbrir.TopLevel = false;
            formularioAbrir.FormBorderStyle = FormBorderStyle.None;
            formularioAbrir.Dock = DockStyle.Fill;

            pnlVistaIniciarSesion.Controls.Add(formularioAbrir);
            formularioAbrir.BringToFront();
            formularioAbrir.Show();
        }

        private void btnIniciarSesion_Click(object sender, EventArgs e)
        {
            abrirForm(new frmInicioGerente());
        }
    }
}
