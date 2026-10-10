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

namespace Vista.GerenteReportes
{
    public partial class frmReportes : Form
    {
        public frmReportes()
        {
            InitializeComponent();
        }

        private void frmReportes_Load(object sender, EventArgs e)
        {
            try
            {
                ReportesPdf.Inicializar();

                cmbMes.Items.Clear();
                cmbMes.Items.AddRange(new object[] {
                    "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                    "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
                });
                cmbMes.SelectedIndex = DateTime.Now.Month - 1;

                cmbAnio.Items.Clear();
                for (int a = DateTime.Now.Year - 2; a <= DateTime.Now.Year + 1; a++)
                    cmbAnio.Items.Add(a);
                cmbAnio.SelectedItem = DateTime.Now.Year;

                cmbMes.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbAnio.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el formulario:\n" + ex.Message,
                                "ERROR-FORMULARIO-101", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClientesTotales_Click(object sender, EventArgs e)
        {
            string ruta = PedirRuta("Reporte_ClientesTotales");
            if (ruta == null) return;
            ReportesPdf.ClientesTotales(ruta);
        }

        private void btnPedidosTotales_Click(object sender, EventArgs e)
        {
            string ruta = PedirRuta("Reporte_PedidosTotales");
            if (ruta == null) return;

            int mes = cmbMes.SelectedIndex + 1;
            int anio = Convert.ToInt32(cmbAnio.SelectedItem);

            ReportesPdf.PedidosTotales(ruta, mes, anio);
        }

        private void btnMaquinariaTotal_Click(object sender, EventArgs e)
        {
            string ruta = PedirRuta("Reporte_MaquinariaTotal");
            if (ruta == null) return;
            ReportesPdf.MaquinariaTotal(ruta);
        }

        private void btnInventarioTotal_Click(object sender, EventArgs e)
        {
            string ruta = PedirRuta("Reporte_InventarioTotal");
            if (ruta == null) return;
            ReportesPdf.InventarioTotal(ruta);
        }

        private void btnPedidosEnProceso_Click(object sender, EventArgs e)
        {
            string ruta = PedirRuta("Reporte_PedidosEnProceso");
            if (ruta == null) return;

            int mes = cmbMes.SelectedIndex + 1;
            int anio = Convert.ToInt32(cmbAnio.SelectedItem);

            ReportesPdf.PedidosEnProceso(ruta, mes, anio);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string ruta = PedirRuta("Reporte_MaquinariaLibre");
            if (ruta == null) return;
            ReportesPdf.MaquinariaLibre(ruta);
        }

        private string PedirRuta(string nombreArchivo)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Archivo PDF (*.pdf)|*.pdf";
                sfd.FileName = $"{nombreArchivo}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                sfd.Title = "Guardar reporte como...";

                if (sfd.ShowDialog() == DialogResult.OK)
                    return sfd.FileName;

                return null;
            }
        }
    }
}
