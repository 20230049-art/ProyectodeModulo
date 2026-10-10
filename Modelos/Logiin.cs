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
        public static Persona UsuarioActual { get; set; }

        public static bool EsGerente => UsuarioActual?.NombreRol == "Gerente";
        public static bool EsEmpleado => UsuarioActual?.NombreRol == "Empleado";
        public static bool EsCliente => UsuarioActual?.NombreRol == "Cliente";
        public static bool EstaLogueado => UsuarioActual != null;

        public static void CerrarSesion()
        {
            UsuarioActual = null;
        }
    }
}
