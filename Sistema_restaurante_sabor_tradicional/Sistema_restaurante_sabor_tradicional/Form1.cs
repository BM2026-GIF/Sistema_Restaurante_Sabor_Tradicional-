using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Sistema_restaurante_sabor_tradicional
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
        }
        private bool contrasenaVisible = false;
      
        private void picVerContrasena_Click(object sender, EventArgs e)
        {
            contrasenaVisible = !contrasenaVisible;

            if (contrasenaVisible)
            {
                txtContraseña.UseSystemPasswordChar = false;
                picVerContrasena.Image = Properties.Resources.ojo_abiertonew;
            }
            else
            {
                txtContraseña.UseSystemPasswordChar = true;
                picVerContrasena.Image = Properties.Resources.nuevo_ojo;
            }

            txtContraseña.Focus();
        }
        private void AbrirMenuPrincipal()
        {
            FrmAdministrador menuPrincipal = new FrmAdministrador();

            Hide();

            menuPrincipal.ShowDialog();

            Show();

            txtContraseña.Clear();
            txtUsuario.Focus();
        }
        private void LimpiarErrores()
        {
            lblErrorUsuario.Text = "";
            lblErrorUsuario.Visible = false;

            lblErrorContrasena.Text = "";
            lblErrorContrasena.Visible = false;
        }
        private bool ValidarCampos()
        {
            LimpiarErrores();

            bool datosCorrectos = true;

            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                lblErrorUsuario.Text = "El usuario no puede quedar vacío.";
                lblErrorUsuario.Visible = true;
                datosCorrectos = false;
            }
            else if (txtUsuario.Text.Trim().Length < 3)
            {
                lblErrorUsuario.Text = "El usuario debe tener al menos 3 caracteres.";
                lblErrorUsuario.Visible = true;
                datosCorrectos = false;
            }

            if (string.IsNullOrWhiteSpace(txtContraseña.Text))
            {
                lblErrorContrasena.Text = "La contraseña no puede quedar vacía.";
                lblErrorContrasena.Visible = true;
                datosCorrectos = false;
            }
            else if (txtContraseña.Text.Length < 4)
            {
                lblErrorContrasena.Text =
                    "La contraseña debe tener al menos 4 caracteres.";

                lblErrorContrasena.Visible = true;
                datosCorrectos = false;
            }

            return datosCorrectos;
        }

        private void btnIniciarsesion_Click(object sender, EventArgs e)
        {


            if (!ValidarCampos())
            {
                return;
            }

            string usuario = txtUsuario.Text.Trim();
            string contrasena = txtContraseña.Text;

            try
            {
                string conexion = "server=localhost; database=restaurante; user=root; password=Yesicaespinal2003;";

                using (MySqlConnection conn = new MySqlConnection(conexion))
                {
                    conn.Open();

                    string consulta = @"SELECT  id_usuario, nombre_usuario, rol, estado FROM usuario WHERE nombre_usuario = @nombreUsuario AND clave = @clave LIMIT 1;";

                    using (MySqlCommand comando = new MySqlCommand(consulta, conn))
                    {
                        comando.Parameters.AddWithValue("@nombreUsuario", usuario);
                        comando.Parameters.AddWithValue("@clave", contrasena);
                        using (MySqlDataReader lector = comando.ExecuteReader())
                        {
                            if (!lector.Read())
                            {
                                MessageBox.Show(
                                    "El usuario o la contraseña son incorrectos.",
                                    "Acceso denegado",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning
                                );

                                txtContraseña.Clear();
                                txtContraseña.Focus();
                                return;
                            }

                            bool usuarioActivo =
                                Convert.ToBoolean(lector["estado"]);

                            if (!usuarioActivo)
                            {
                                MessageBox.Show(
                                    "Este usuario se encuentra inactivo.\n" +
                                    "Comuníquese con el administrador.",
                                    "Usuario inactivo",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning
                                );

                                return;
                            }

                            Sesion.IdUsuario =
                                Convert.ToInt32(lector["id_usuario"]);

                            Sesion.NombreUsuario =
                                lector["nombre_usuario"].ToString() ?? "";

                            Sesion.Rol =
                                lector["rol"].ToString() ?? "";
                        }
                    }
                }

                MessageBox.Show(
                    $"Bienvenido, {Sesion.NombreUsuario}\n" +
                    $"Rol: {Sesion.Rol}",
                    "Inicio de sesión correcto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                AbrirMenuPrincipal();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "No se pudo consultar la base de datos.\n\n" +
                    ex.Message,
                    "Error de MySQL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error inesperado.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void txtUsuario_TextChanged(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                lblErrorUsuario.Text = "El usuario no puede quedar vacío.";
                lblErrorUsuario.ForeColor = Color.Red;
            }
            else if (usuario.Length < 3)
            {
                int faltantes = 3 - usuario.Length;

                lblErrorUsuario.Text =
                    $"Faltan {faltantes} carácter(es). Mínimo 3.";

                lblErrorUsuario.ForeColor = Color.Red;
            }
            else if (usuario.Contains(" "))
            {
                lblErrorUsuario.Text =
                    "El usuario no debe contener espacios.";

                lblErrorUsuario.ForeColor = Color.Red;
            }
            else
            {
                lblErrorUsuario.Text = "Usuario válido.";
                lblErrorUsuario.ForeColor = Color.Green;
            }

            lblErrorUsuario.Visible = true;
        }

        private void txtContraseña_TextChanged(object sender, EventArgs e)
        {
            string contrasena = txtContraseña.Text;

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                lblErrorContrasena.Text =
                    "La contraseña no puede quedar vacía.";

                lblErrorContrasena.ForeColor = Color.Red;
            }
            else if (contrasena.Length < 4)
            {
                int faltantes = 4 - contrasena.Length;

                lblErrorContrasena.Text =
                    $"Faltan {faltantes} carácter(es). Mínimo 4.";

                lblErrorContrasena.ForeColor = Color.Red;
            }
            else
            {
                lblErrorContrasena.Text = "Contraseña válida.";
                lblErrorContrasena.ForeColor = Color.Green;
            }

            lblErrorContrasena.Visible = true;
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            DialogResult respuesta = MessageBox.Show("¿Está seguro de que desea salir del sistema?", "Confirmar salida", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (respuesta == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
