using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modelos.Entidades
{
    public class Sesion
    {
        public static int IdPersona { get; set; } = 0;
        public static int IdCliente { get; set; } = 0;
        public static string NombreCompleto { get; set; } = "";
        public static string Correo { get; set; } = "";
        public static string Rol { get; set; } = "";
    }
}
