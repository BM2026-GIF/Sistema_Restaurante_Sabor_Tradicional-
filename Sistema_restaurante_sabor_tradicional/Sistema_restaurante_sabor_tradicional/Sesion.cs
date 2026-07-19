using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_restaurante_sabor_tradicional
{
 
        public static class Sesion
        {
            public static int IdUsuario { get; set; }

            public static string NombreUsuario { get; set; } = string.Empty;

            public static string Rol { get; set; } = string.Empty;

            public static void CerrarSesion()
            {
                IdUsuario = 0;
                NombreUsuario = string.Empty;
                Rol = string.Empty;
            }
        }
   }

