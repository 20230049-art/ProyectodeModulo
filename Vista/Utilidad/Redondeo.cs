using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vista.Utilidad
{
    public class Redondeo
    {
        public static void RedondearFig(Control control, int radio)
        {
            GraphicsPath forma = new GraphicsPath();

            forma.AddArc(0, 0, radio, radio, 180, 90);
            forma.AddArc(control.Width - radio, 0, radio, radio, 270, 90);
            forma.AddArc(control.Width - radio, control.Height - radio, radio, radio, 0, 90);
            forma.AddArc(0, control.Height - radio, radio, radio, 90, 90);

            forma.CloseFigure();

            control.Region = new Region(forma);
        }

        public static void RedondearFormulario(Form formulario, int radio)
        {
            GraphicsPath path = new GraphicsPath();

            path.AddArc(0, 0, radio, radio, 180, 90);
            path.AddArc(formulario.Width - radio, 0, radio, radio, 270, 90);
            path.AddArc(formulario.Width - radio, formulario.Height - radio, radio, radio, 0, 90);
            path.AddArc(0, formulario.Height - radio, radio, radio, 90, 90);

            path.CloseFigure();

            formulario.Region = new Region(path);
        }
    }
}
