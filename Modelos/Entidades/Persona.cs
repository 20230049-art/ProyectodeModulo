using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modelos.Entidades
{
    public class Persona
    {
        private int idPersona;
        private string nombrePersona;
        private string apellidoPersona;
        private string dUIPersona;
        private string contrasenaPersona;
        private string correoPersona;
        private string telefonoPersona;
        private int idRol;
        private bool eliminadoPersona;

        public int IdPersona { get => idPersona; set => idPersona = value; }
        public string NombrePersona { get => nombrePersona; set => nombrePersona = value; }
        public string ApellidoPersona { get => apellidoPersona; set => apellidoPersona = value; }
        public string DUIPersona { get => dUIPersona; set => dUIPersona = value; }
        public string ContrasenaPersona { get => contrasenaPersona; set => contrasenaPersona = value; }
        public string CorreoPersona { get => correoPersona; set => correoPersona = value; }
        public string TelefonoPersona { get => telefonoPersona; set => telefonoPersona = value; }
        public int IdRol { get => idRol; set => idRol = value; }
        public bool EliminadoPersona { get => eliminadoPersona; set => eliminadoPersona = value; }
    }
}
