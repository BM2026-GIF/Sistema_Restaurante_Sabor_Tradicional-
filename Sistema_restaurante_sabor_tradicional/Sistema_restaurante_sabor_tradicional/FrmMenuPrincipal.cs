using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace Sistema_restaurante_sabor_tradicional
{
    public partial class FrmAdministrador : Form
    {
        public FrmAdministrador()
        {
            InitializeComponent();
        }

        private void FrmMenuPrincipal_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblFechaActual.Text = DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy");

            lblHoraActual.Text = DateTime.Now.ToString("dddd, hh:mm:ss tt");

        }

        private void cardVentas_Click(object sender, EventArgs e)
        {

        }
    }
}
