using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_restaurante_sabor_tradicional
{
    internal class Conexion
    {

        private readonly string cadenaConexion = "Server=localhost;" +"Port=3306;" +"Database=restaurante;" +"Uid=root;" +"Pwd=Yesicaespinal2003;";

        public MySqlConnection ObtenerConexion()
        {
            return new MySqlConnection(cadenaConexion);
        }
    }



}

