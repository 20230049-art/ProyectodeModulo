using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vista.GerenteClientes;
using Vista.GerenteInventario;
using Vista.GerenteMaquinaria;
using Vista.GerentePedidos;
using Vista.GerenteReportes;
using Vista.GerenteUsuario;
using Vista.Iniciar_Sesion;

namespace Vista.InicioGerente
{
    public partial class frmInicioGerente : Form
    {
        public frmInicioGerente()
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
                pnlVistaMenu.Controls.Remove(activeForm);
                activeForm.Dispose();
            }

            activeForm = formularioAbrir;
            formularioAbrir.TopLevel = false;
            formularioAbrir.FormBorderStyle = FormBorderStyle.None;
            formularioAbrir.Dock = DockStyle.Fill;

            pnlVistaMenu.Controls.Add(formularioAbrir);
            formularioAbrir.BringToFront();
            formularioAbrir.Show();
        }

        private void btnInicio_Click(object sender, EventArgs e)
        {
            abrirForm(new frmInicioGerenteSinMenu());
        }

        private void btnPedidos_Click(object sender, EventArgs e)
        {
            abrirForm(new frmPedidos());
        }

        private void btnMaquinaria_Click(object sender, EventArgs e)
        {
            abrirForm(new frmMaquinaria());
        }

        private void btnInventario_Click(object sender, EventArgs e)
        {
            abrirForm(new frmInventario());
        }

        private void btnCliente_Click(object sender, EventArgs e)
        {
            abrirForm(new frmClientescs());
        }

        private void btnUsuario_Click(object sender, EventArgs e)
        {
            abrirForm(new frmUsuario());
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            abrirForm(new frmReportes());
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show("¿Estás seguro de que deseas cerrar sesión?", "Cerrar sesión",
             MessageBoxButtons.YesNo, MessageBoxIcon.Question
    );

            if (resultado == DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
