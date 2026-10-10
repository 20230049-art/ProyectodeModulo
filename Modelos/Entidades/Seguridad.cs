using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modelos.Entidades
{
    public class Seguridad
    {
        public static string HashPassword(string passwordPlano)
        {
            return BCrypt.Net.BCrypt.HashPassword(passwordPlano);
        }

        public static bool VerificarPassword(string passwordPlano, string hashGuardado)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(passwordPlano, hashGuardado);
            }
            catch
            {
                return false;
            }
        }
    }
}
