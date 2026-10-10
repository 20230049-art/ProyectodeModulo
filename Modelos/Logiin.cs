using Modelos.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modelos
{
    public class Logiin
    {
        private static Persona UsuarioActual;

        public static Persona UsuarioActual1 { get => UsuarioActual; set => UsuarioActual = value; }

        public static bool EsGerente => UsuarioActual?.IdRol == 1;
        public static bool EsEmpleado => UsuarioActual?.IdRol == 2;
        public static bool EsCliente => UsuarioActual?.IdRol == 3;

        public static bool EstaLogueado => UsuarioActual != null;

        public static void CerrarSesion()
        {
            UsuarioActual = null;
        }
    }
}
