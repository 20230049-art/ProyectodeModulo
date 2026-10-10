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
        public frmPedidos()
        {
            InitializeComponent();
        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

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
    }
}
