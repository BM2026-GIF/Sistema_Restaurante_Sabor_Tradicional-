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
using System.Windows.Forms.DataVisualization.Charting;
using MySql.Data.MySqlClient;
namespace Sistema_restaurante_sabor_tradicional
{
    public partial class FrmAdministrador : Form
    {

        private Chart chartVentas;
        public FrmAdministrador()
        {
            InitializeComponent();

            cmbPeriodoVentas.Items.Add("Últimos 7 días");
            cmbPeriodoVentas.Items.Add("Últimos 15 días");
            cmbPeriodoVentas.Items.Add("Últimos 30 días");
            cmbPeriodoVentas.SelectedIndex = 0;

            ConfigurarCard(cardUsuario);
            ConfigurarCard(cardCategorias);
            ConfigurarCard(cardPlatillos);
            ConfigurarCard(cardPedidos);
            ConfigurarCard(cardVentas);

            CrearGraficoVentas();
            ConfigurarGraficoVentas();
            //CargarDatosPruebaGrafico();
            CargarDatosCards();
            ConfigurarTablaPedidos();
            CargarPedidosRecientes();
            CargarVentas(7);
        }





        private void CrearGraficoVentas()
        {
            chartVentas = new Chart();

            chartVentas.Name = "chartVentas";
            chartVentas.Dock = DockStyle.Fill;
            chartVentas.BackColor = Color.White;

            pnlGraficoVentas.Controls.Clear();
            pnlGraficoVentas.Controls.Add(chartVentas);
        }
        private void ConfigurarGraficoVentas()
        {
            chartVentas.Series.Clear();
            chartVentas.ChartAreas.Clear();
            chartVentas.Legends.Clear();

            ChartArea area = new ChartArea("AreaVentas");

            area.BackColor = Color.White;

            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor =
                Color.FromArgb(230, 230, 230);

            area.AxisX.LineColor = Color.LightGray;
            area.AxisY.LineColor = Color.LightGray;

            area.AxisX.LabelStyle.ForeColor = Color.DimGray;
            area.AxisY.LabelStyle.ForeColor = Color.DimGray;

            area.AxisY.Minimum = 0;
            area.AxisY.LabelStyle.Format = "L {0:N0}";

            chartVentas.ChartAreas.Add(area);

            Series serieVentas = new Series("Ventas");

            serieVentas.ChartType = SeriesChartType.Spline;
            serieVentas.BorderWidth = 3;
            serieVentas.XValueType = ChartValueType.String;

            serieVentas.MarkerStyle = MarkerStyle.Circle;
            serieVentas.MarkerSize = 7;

            chartVentas.Series.Add(serieVentas);
        }

        private void CargarDatosPruebaGrafico()
        {
            chartVentas.Series["Ventas"].Points.Clear();

            chartVentas.Series["Ventas"].Points.AddXY("Lun", 800);
            chartVentas.Series["Ventas"].Points.AddXY("Mar", 1200);
            chartVentas.Series["Ventas"].Points.AddXY("Mié", 950);
            chartVentas.Series["Ventas"].Points.AddXY("Jue", 1500);
            chartVentas.Series["Ventas"].Points.AddXY("Vie", 1800);
            chartVentas.Series["Ventas"].Points.AddXY("Sáb", 1400);
            chartVentas.Series["Ventas"].Points.AddXY("Dom", 2100);
        }

        private void CargarVentas(int cantidadDias)
        {
            string conexion = "server=localhost; database=restaurante; user=root; password=Yesicaespinal2003;";
            try
            {
                chartVentas.Series["Ventas"].Points.Clear();

                DateTime fechaInicio = DateTime.Today.AddDays(-(cantidadDias - 1));
                DateTime fechaFinal = DateTime.Today;

                Dictionary<DateTime, decimal> ventasPorFecha =
                    new Dictionary<DateTime, decimal>();

                string consulta = @"
            SELECT
                DATE(fecha_pago) AS fecha,
                COALESCE(SUM(total), 0) AS total_ventas
            FROM factura
            WHERE DATE(fecha_pago) BETWEEN @fechaInicio AND @fechaFinal
            GROUP BY DATE(fecha_pago)
            ORDER BY DATE(fecha_pago);";

                Conexion conexionBD = new Conexion();

                using (MySqlConnection conn = new MySqlConnection(conexion))
                {
                    conn.Open();

                    using (MySqlCommand comando =
                           new MySqlCommand(consulta, conn))
                    {
                        comando.Parameters.AddWithValue(
                            "@fechaInicio",
                            fechaInicio.Date
                        );

                        comando.Parameters.AddWithValue(
                            "@fechaFinal",
                            fechaFinal.Date
                        );

                        using (MySqlDataReader lector =
                               comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                DateTime fecha =
                                    Convert.ToDateTime(
                                        lector["fecha"]
                                    ).Date;

                                decimal total =
                                    Convert.ToDecimal(
                                        lector["total_ventas"]
                                    );

                                ventasPorFecha[fecha] = total;
                            }
                        }
                    }
                }

                CultureInfo cultura =
                    new CultureInfo("es-HN");

                for (int i = 0; i < cantidadDias; i++)
                {
                    DateTime fecha = fechaInicio.AddDays(i);

                    decimal total = 0;

                    if (ventasPorFecha.ContainsKey(fecha))
                    {
                        total = ventasPorFecha[fecha];
                    }

                    string etiqueta;

                    if (cantidadDias == 7)
                    {
                        etiqueta = cultura.DateTimeFormat
                            .GetAbbreviatedDayName(fecha.DayOfWeek);

                        etiqueta =
                            char.ToUpper(etiqueta[0]) +
                            etiqueta.Substring(1);
                    }
                    else
                    {
                        etiqueta = fecha.ToString("dd/MM");
                    }

                    chartVentas.Series["Ventas"]
                        .Points.AddXY(etiqueta, total);
                }

                lblTituloVentasGrafico.Text =
                    $"Ventas de los últimos {cantidadDias} días";
            }
            catch (MySqlException ex)
            {
                AntdUI.Message.error(
                    this,
                    "No se pudieron cargar las ventas: " +
                    ex.Message
                );
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(
                    this,
                    "Ocurrió un error al cargar el gráfico: " +
                    ex.Message
                );
            }
        }

        private void ConfigurarTablaPedidos()
        {
            dgvPedidosRecientes.BackgroundColor = Color.White;
            dgvPedidosRecientes.BorderStyle = BorderStyle.None;

            dgvPedidosRecientes.RowHeadersVisible = false;
            dgvPedidosRecientes.ColumnHeadersVisible = false;

            dgvPedidosRecientes.AllowUserToAddRows = false;
            dgvPedidosRecientes.AllowUserToDeleteRows = false;
            dgvPedidosRecientes.AllowUserToResizeRows = false;
            dgvPedidosRecientes.ReadOnly = true;

            dgvPedidosRecientes.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvPedidosRecientes.MultiSelect = false;
            dgvPedidosRecientes.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgvPedidosRecientes.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;

            dgvPedidosRecientes.GridColor =
                Color.FromArgb(235, 235, 235);

            dgvPedidosRecientes.DefaultCellStyle.BackColor =
                Color.White;

            dgvPedidosRecientes.DefaultCellStyle.ForeColor =
                Color.FromArgb(30, 41, 59);

            dgvPedidosRecientes.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(245, 248, 255);

            dgvPedidosRecientes.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(30, 41, 59);

            dgvPedidosRecientes.DefaultCellStyle.Font =
                new Font("Segoe UI", 9);

            dgvPedidosRecientes.DefaultCellStyle.Padding =
                new Padding(5);

            dgvPedidosRecientes.RowTemplate.Height = 52;
        }

        private void CargarPedidosRecientes()
        {
            try
            {
                dgvPedidosRecientes.Rows.Clear();

                string consulta = @"
            SELECT
                id_pedido,
                numero_mesa,
                fecha,
                hora,
                estado,
                total
            FROM pedido
            ORDER BY fecha DESC, hora DESC
            LIMIT 5;";

                Conexion conexionBD = new Conexion();

                using (MySqlConnection conexion =
                       conexionBD.ObtenerConexion())
                {
                    conexion.Open();

                    using (MySqlCommand comando =
                          new MySqlCommand(consulta, conexion))
                    {
                        using (MySqlDataReader lector =
                               comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                int idPedido = Convert.ToInt32(lector["id_pedido"]);

                                int numeroMesa = Convert.ToInt32(lector["numero_mesa"]);

                                DateTime fecha = Convert.ToDateTime(lector["fecha"]);

                                TimeSpan hora = (TimeSpan)lector["hora"];

                                string estado = lector["estado"].ToString();

                                decimal total = Convert.ToDecimal(lector["total"]);

                                string horaTexto = DateTime.Today.Add(hora).ToString("hh:mm tt");

                                string mesaHora =
                                    $"Mesa {numeroMesa}\n" + $"{fecha:dd/MM/yyyy} - {horaTexto}";

                                dgvPedidosRecientes.Rows.Add(
                                    $"#{idPedido:D5}", mesaHora, estado, $"L {total:N2}", "›");

                                dgvPedidosRecientes.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                                dgvPedidosRecientes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                            }
                        }
                    }
                }

                AplicarEstiloEstados();
            }
            catch (MySqlException ex)
            {
                AntdUI.Message.error(
                    this,
                    "No se pudieron cargar los pedidos recientes: " +
                    ex.Message
                );
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(
                    this,
                    "Ocurrió un error al cargar los pedidos: " +
                    ex.Message
                );
            }
        }

        private void AplicarEstiloEstados()
        {
            foreach (DataGridViewRow fila
                     in dgvPedidosRecientes.Rows)
            {
                string estado =
                    fila.Cells["colEstado"].Value?
                    .ToString() ?? "";

                DataGridViewCell celdaEstado =
                    fila.Cells["colEstado"];

                celdaEstado.Style.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;

                switch (estado)
                {
                    case "Pendiente":
                        celdaEstado.Style.BackColor =
                            Color.FromArgb(255, 247, 230);

                        celdaEstado.Style.ForeColor =
                            Color.DarkOrange;
                        break;

                    case "En preparación":
                        celdaEstado.Style.BackColor =
                            Color.FromArgb(230, 244, 255);

                        celdaEstado.Style.ForeColor =
                            Color.RoyalBlue;
                        break;

                    case "Entregado":
                        celdaEstado.Style.BackColor =
                            Color.FromArgb(230, 255, 241);

                        celdaEstado.Style.ForeColor =
                            Color.SeaGreen;
                        break;
                }
            }
        }


        private void CargarDatosCards()
        {
            try
            {
                Conexion conexionBD = new Conexion();

                using (MySqlConnection conexion =
                       conexionBD.ObtenerConexion())
                {
                    conexion.Open();

                    lblTotalUsuarios.Text = ObtenerConteo(
                        conexion,
                        "SELECT COUNT(*) FROM usuario WHERE estado = 1;"
                    );

                    lblTotalCategorias.Text = ObtenerConteo(
                        conexion,
                        "SELECT COUNT(*) FROM categoria;"
                    );

                    lblTotalPlatillo.Text = ObtenerConteo(
                        conexion,
                        "SELECT COUNT(*) FROM platillo WHERE disponibilidad = 1;"
                    );

                    lblTotalPedidos.Text = ObtenerConteo(
                        conexion,
                        "SELECT COUNT(*) FROM pedido WHERE fecha = CURDATE();"
                    );

                    decimal ventasHoy = ObtenerTotalDecimal(
                        conexion,
                        @"SELECT COALESCE(SUM(total), 0)
                  FROM factura
                  WHERE DATE(fecha_pago) = CURDATE();"
                    );

                    lblTotalVentas.Text = $"L {ventasHoy:N2}";
                }
            }
            catch (MySqlException ex)
            {
                AntdUI.Message.error(
                    this,
                    "No se pudieron cargar los datos del panel: " +
                    ex.Message
                );
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(
                    this,
                    "Ocurrió un error al cargar los cards: " +
                    ex.Message
                );
            }
        }

        private string ObtenerConteo(
    MySqlConnection conexion,
    string consulta)
        {
            using (MySqlCommand comando =
                   new MySqlCommand(consulta, conexion))
            {
                object resultado = comando.ExecuteScalar();

                return resultado == null ||
                       resultado == DBNull.Value
                    ? "0"
                    : resultado.ToString();
            }
        }

        private decimal ObtenerTotalDecimal(
    MySqlConnection conexion,
    string consulta)
        {
            using (MySqlCommand comando =
                   new MySqlCommand(consulta, conexion))
            {
                object resultado = comando.ExecuteScalar();

                if (resultado == null ||
                    resultado == DBNull.Value)
                {
                    return 0;
                }

                return Convert.ToDecimal(resultado);
            }
        }


        private void FrmMenuPrincipal_Load(object sender, EventArgs e)
        {

        }
        private void ConfigurarCard(AntdUI.Panel card)
        {
            card.BackColor = Color.White;
            card.BorderWidth = 1;
            card.BorderColor = Color.FromArgb(220, 220, 220);
            card.Radius = 15;


        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            lblFechaActual.Text = DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy");

            lblHoraActual.Text = DateTime.Now.ToString("dddd, hh:mm:ss tt");

        }


        private void cardVentas_Click(object sender, EventArgs e)
        {

        }

        private void btnPanelControl_MouseEnter(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void btnPanelControl_MouseLeave(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.Transparent;
                boton.ForeColor = Color.White;
            }
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnlContenido_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cardCategorias_Click(object sender, EventArgs e)
        {

        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            DialogResult respuesta = MessageBox.Show("¿Está seguro de que desea salir del sistema?", "Confirmar salida", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (respuesta == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void btnPanelControl_Click(object sender, EventArgs e)
        {

        }

        private void btnUsuario_MouseEnter(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void btnUsuario_MouseLeave(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.Transparent;
                boton.ForeColor = Color.White;
            }
        }

        private void btnCategorias_MouseEnter(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void btnCategorias_MouseLeave(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.Transparent;
                boton.ForeColor = Color.White;
            }
        }

        private void btnPlatillos_MouseEnter(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void btnPlatillos_MouseLeave(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void btnMeseros_MouseEnter(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void btnMeseros_MouseLeave(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void btnPedidos_MouseEnter(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void btnPedidos_MouseLeave(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void btnFacturacion_MouseEnter(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void btnFacturacion_MouseLeave(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void btnReportes_MouseEnter(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void btnReportes_MouseLeave(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void btnSalir_MouseEnter(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void btnSalir_MouseLeave(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button boton)
            {
                boton.BackColor = Color.DarkRed;
                boton.ForeColor = Color.White;
            }
        }

        private void cmbPeriodoVentas_SelectedIndexChanged(object sender, AntdUI.IntEventArgs e)
        {

            if (chartVentas == null ||
                chartVentas.Series.Count == 0)
            {
                return;
            }

            switch (cmbPeriodoVentas.SelectedIndex)
            {
                case 0:
                    CargarVentas(7);
                    break;

                case 1:
                    CargarVentas(15);
                    break;

                case 2:
                    CargarVentas(30);
                    break;
            }
        }
    }
}
