using Modelos.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vista.GerenteClientes;
using Vista.Iniciar_Sesion;
using Vista.InicioGerente;
using static Vista.frmInicio;

namespace Vista
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (!Datos.ExisteGerente())
            {
                using (var frm = new frmPrimerUsuario())
                {
                    if (frm.ShowDialog() != DialogResult.OK)
                        return;   
                }

                Application.Run(new frmInicioGerente());
                return;
            }

            using (var login = new frmIniciarSesion())
            {
                if (login.ShowDialog() != DialogResult.OK)
                    return;
            }

            switch ((Sesion.Rol ?? "").Trim())
            {
                case "Gerente":
                    Application.Run(new frmInicioGerente());
                    break;

                case "Empleado":
                    Application.Run(new frmOperador.FormOperador());
                    break;

                case "Cliente":
                    Application.Run(new frmInicio.FormInicio());
                    break;

                default:
                    MessageBox.Show("Rol no reconocido: '" + Sesion.Rol + "'",
                        "IMPREXA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }
    }
}
