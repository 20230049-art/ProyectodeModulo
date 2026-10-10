using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vista.GerenteMaquinaria
{
    public partial class frmActualizarMaquinaria : Form
    {
        private int idMaquinaria;
        private string rutaFotoSeleccionada = "";

        public frmActualizarMaquinaria(int idMaquinaria)
        {
            InitializeComponent();
        }
    }
}
