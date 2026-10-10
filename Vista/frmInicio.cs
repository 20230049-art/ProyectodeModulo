using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static Vista.frmInicio.Db;

namespace Vista
{
    public class frmInicio : Form
    {
        // ASTRID CRUZ 
        public static class Db
        {
            public static string ConnectionString =
         @"Server=localhost\SQLEXPRESS;Database=Imprexa;Trusted_Connection=True;TrustServerCertificate=True;";

            public class Pedido
            {
                public int Id;
                public string Producto;
                public int Cantidad;
                public DateTime Fecha;
                public string Estado;
                public string Cliente;
                public string Maquinaria;
                public string Insumo;
                public string Usuario;
            }

            static Pedido Leer(SqlDataReader r)
            {
                return new Pedido
                {
                    Id = Convert.ToInt32(r["IdPedido"]),
                    Fecha = Convert.ToDateTime(r["FechaPedido"]),
                    Producto = r["DescripcionPedido"].ToString(),
                    Cantidad = Convert.ToInt32(r["CantidadPedido"]),
                    Cliente = r["NombreCliente"].ToString() + " " + r["ApellidoCliente"].ToString(),
                    Maquinaria = r["NombreMaquinaria"] == DBNull.Value ? "" : r["NombreMaquinaria"].ToString(),
                    Insumo = r["NombreInsumo"] == DBNull.Value ? "" : r["NombreInsumo"].ToString(),
                    Estado = r["NombreEstadoPedido"].ToString(),
                    Usuario = r["Usuario"] == DBNull.Value ? "" : r["Usuario"].ToString()
                };
            }

            public static void Resumen(out int total, out int proceso, out int entregados)
            {
                total = 0; proceso = 0; entregados = 0;
                const string sql = @"
            SELECT 
                COUNT(*),
                ISNULL(SUM(CASE WHEN IdEstadoPedido = 1 THEN 1 ELSE 0 END), 0),
                ISNULL(SUM(CASE WHEN IdEstadoPedido = 2 THEN 1 ELSE 0 END), 0)
            FROM PedidosImpresion
            WHERE EliminadoPedido = 0";
                using (var cn = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            total = r.GetInt32(0);
                            proceso = r.GetInt32(1);
                            entregados = r.GetInt32(2);
                        }
                    }
                }
            }

            // Último pedido (usa la vista VistaPedidos)
            public static Pedido Ultimo()
            {
                const string sql = "SELECT TOP 1 * FROM VistaPedidos ORDER BY IdPedido DESC";
                using (var cn = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? Leer(r) : null;
                }
            }

            // Paginación (usa la vista VistaPedidos)
            public static List<Pedido> Pagina(int pagina, int tamano)
            {
                var lista = new List<Pedido>();
                const string sql = @"
            SELECT *
            FROM VistaPedidos
            ORDER BY IdPedido DESC
            OFFSET @off ROWS FETCH NEXT @n ROWS ONLY";
                using (var cn = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@off", (pagina - 1) * tamano);
                    cmd.Parameters.AddWithValue("@n", tamano);
                    cn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) lista.Add(Leer(r));
                }
                return lista;
            }

            // Insertar usando el procedimiento AgregarPedido (ya lo tienes en tu SQL)
            public static int Insertar(string descripcion, int cantidad, decimal total,
                                        int idCliente, int idMaquinaria, int idInsumos, int idPersona)
            {
                using (var cn = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand("AgregarPedido", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DescripcionPedido", descripcion);
                    cmd.Parameters.AddWithValue("@CantidadPedido", cantidad);
                    cmd.Parameters.AddWithValue("@TotalPedido", total);
                    cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                    cmd.Parameters.AddWithValue("@IdMaquinaria", idMaquinaria);
                    cmd.Parameters.AddWithValue("@IdInsumos", idInsumos);
                    cmd.Parameters.AddWithValue("@IdPersona", idPersona);

                    cn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                            return Convert.ToInt32(r["IdPedido"]);
                    }
                }
                return 0;
            }
        }

        // ===================== CONTROLES PERSONALIZADOS =====================
        public class RoundedPanel : Panel
        {
            public int Radius { get; set; } = 14;
            public Color FillColor { get; set; } = Color.White;
            public Color BorderColor { get; set; } = Color.FromArgb(225, 233, 242);
            public Color? GradientEnd { get; set; }

            public RoundedPanel()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                         ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
            }

            public static GraphicsPath Round(Rectangle r, int radius)
            {
                int d = Math.Max(1, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
                var p = new GraphicsPath();
                p.AddArc(r.X, r.Y, d, d, 180, 90);
                p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                p.CloseFigure();
                return p;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, Width - 1, Height - 1);
                if (r.Width > 2 && r.Height > 2)
                {
                    using (var path = Round(r, Radius))
                    {
                        if (GradientEnd.HasValue)
                            using (var br = new LinearGradientBrush(r, FillColor, GradientEnd.Value, 20f))
                                g.FillPath(br, path);
                        else
                            using (var br = new SolidBrush(FillColor))
                                g.FillPath(br, path);
                        using (var pen = new Pen(BorderColor))
                            g.DrawPath(pen, path);
                    }
                }
                base.OnPaint(e);
            }
        }

        public class RoundedButton : Control
        {
            public int Radius { get; set; } = 12;
            public Color FillColor { get; set; } = Color.Transparent;
            public Color HoverColor { get; set; } = Color.FromArgb(30, 255, 255, 255);
            public Color BorderColor { get; set; } = Color.Transparent;
            public string Icon { get; set; } = "";
            public Font IconFont { get; set; } = new Font("Segoe UI Emoji", 13f);
            public bool CenterText { get; set; }
            private bool _hover;

            public RoundedButton()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                         ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
                Cursor = Cursors.Hand;
            }

            protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
            protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, Width - 1, Height - 1);
                var col = (_hover && Enabled) ? HoverColor : FillColor;
                var fore = Enabled ? ForeColor : Color.FromArgb(175, 185, 200);
                using (var path = RoundedPanel.Round(r, Radius))
                {
                    if (col.A > 0) using (var br = new SolidBrush(col)) g.FillPath(br, path);
                    if (BorderColor.A > 0) using (var pen = new Pen(BorderColor)) g.DrawPath(pen, path);
                }

                var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                if (!string.IsNullOrEmpty(Icon))
                {
                    TextRenderer.DrawText(g, Icon, IconFont, new Rectangle(14, 0, 36, Height), fore,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                    TextRenderer.DrawText(g, Text, Font, new Rectangle(54, 0, Width - 60, Height), fore,
                        flags | TextFormatFlags.Left);
                }
                else
                {
                    TextRenderer.DrawText(g, Text, Font, new Rectangle(8, 0, Width - 16, Height), fore,
                        flags | (CenterText ? TextFormatFlags.HorizontalCenter : TextFormatFlags.Left));
                }
            }
        }

        // ===================== FORMULARIO PRINCIPAL =====================
        public class FormInicio : Form
        {
            // Paleta
            static readonly Color Navy = Color.FromArgb(10, 37, 64);
            static readonly Color Teal = Color.FromArgb(18, 120, 150);
            static readonly Color Orange = Color.FromArgb(255, 138, 43);
            static readonly Color Dark = Color.FromArgb(16, 35, 60);
            static readonly Color Gray = Color.FromArgb(110, 125, 145);
            static readonly Color PageBg = Color.FromArgb(246, 250, 253);
            static readonly Color LineCol = Color.FromArgb(225, 233, 242);

            // Estado / datos
            Pedido ultimoPedido;
            int totalPedidos, enProceso;
            int paginaActual = 1, tamanoPagina = 5, totalPaginas = 1;
            string ultimoError;
            bool avisoMostrado;
            Timer timerActualizar;

            // UI
            Panel sidebar, host;
            Panel[] pages;
            RoundedButton[] menu = new RoundedButton[4];
            Label lblTotal, lblProceso, lblTerminados;
            Label lblUPTitulo, lblUPCant, lblUPFecha, pillLabel;
            Label lblDb;
            RoundedPanel pill;
            DataGridView grid;
            ComboBox cboProducto, cboPapel, cboAcabado, cboTamano;
            NumericUpDown numCantidad;
            TextBox txtDesc;
            Label lblError;
            Label lblInfoPag, lblPagina;
            RoundedButton btnPrimera, btnAnterior, btnSiguiente, btnUltima;

            public FormInicio()
            {
                Text = "IMPREXA - Gestión inteligente de impresión";
                ClientSize = new Size(1280, 820);
                MinimumSize = new Size(1100, 780);
                StartPosition = FormStartPosition.CenterScreen;
                BackColor = PageBg;
                Font = F(10);
                DoubleBuffered = true;

                var content = new Panel { Dock = DockStyle.Fill, BackColor = PageBg };
                host = new Panel { Dock = DockStyle.Fill, BackColor = PageBg };

                pages = new[] { BuildInicio(), BuildCotizar(), BuildPedidos() };
                foreach (var p in pages) { p.Dock = DockStyle.Fill; host.Controls.Add(p); }

                content.Controls.Add(host);
                content.Controls.Add(BuildTopbar());
                Controls.Add(content);

                BuildSidebar();
                Controls.Add(sidebar);

                ShowPage(0);

                // Se conecta a la base de datos al abrir y se actualiza sola cada 30 segundos
                timerActualizar = new Timer { Interval = 30000 };
                timerActualizar.Tick += (s, e) => RefreshData();
                Shown += (s, e) => { RefreshData(); timerActualizar.Start(); };
                FormClosed += (s, e) => timerActualizar.Dispose();
            }

            // ---------- Helpers ----------
            static Font F(float size, FontStyle st = FontStyle.Regular) { return new Font("Segoe UI", size, st); }

            Label L(string text, float size, Color color, FontStyle st, int x, int y, int w = 0, string font = "Segoe UI")
            {
                var l = new Label
                {
                    Text = text,
                    Font = new Font(font, size, st),
                    ForeColor = color,
                    BackColor = Color.Transparent,
                    Location = new Point(x, y)
                };
                if (w > 0) { l.AutoSize = false; l.Width = w; l.Height = (int)(size * 2.4f); }
                else l.AutoSize = true;
                return l;
            }

            void Clickable(Control c, EventHandler h)
            {
                c.Cursor = Cursors.Hand;
                c.Click += h;
                foreach (Control ch in c.Controls) Clickable(ch, h);
            }

            // ---------- Barra lateral ----------
            void BuildSidebar()
            {
                sidebar = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Navy };
                sidebar.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    int W = sidebar.Width, H = sidebar.Height;
                    using (var b = new SolidBrush(Color.FromArgb(150, 30, 160, 200)))
                        g.FillPolygon(b, new[] { new Point(30, H), new Point(W, H - 190), new Point(W, H - 120), new Point(120, H) });
                    using (var b = new SolidBrush(Orange))
                        g.FillPolygon(b, new[] { new Point(130, H), new Point(W, H - 115), new Point(W, H - 60), new Point(190, H) });
                };

                sidebar.Controls.Add(L("🖨", 26, Color.White, FontStyle.Regular, 16, 20, 0, "Segoe UI Emoji"));
                sidebar.Controls.Add(L("IMPREXA", 21, Color.White, FontStyle.Bold, 64, 18));
                sidebar.Controls.Add(L("Gestión inteligente de impresión", 7.5f, Color.FromArgb(190, 210, 230), FontStyle.Regular, 68, 54));

                string[] names = { "Inicio", "Solicitar cotización", "Mis pedidos", "Cerrar sesión" };
                string[] icons = { "🏠", "📄", "📦", "🚪" };
                for (int i = 0; i < 4; i++)
                {
                    int idx = i;
                    var b = new RoundedButton
                    {
                        Text = names[i],
                        Icon = icons[i],
                        Size = new Size(226, 50),
                        Location = new Point(12, 115 + i * 58),
                        ForeColor = Color.White,
                        Font = F(11)
                    };
                    b.Click += (s, e) => { if (idx == 3) Logout(); else ShowPage(idx); };
                    menu[i] = b;
                    sidebar.Controls.Add(b);
                }

                var l1 = L("Tus ideas", 15, Color.White, FontStyle.Regular, 22, 0);
                var l2 = L("en buenas", 15, Color.White, FontStyle.Regular, 22, 0);
                var l3 = L("manos.", 20, Color.FromArgb(40, 190, 230), FontStyle.Italic, 22, 0, 0, "Segoe Script");
                sidebar.Controls.AddRange(new Control[] { l1, l2, l3 });
                EventHandler place = (s, e) =>
                {
                    int y = sidebar.Height - 215;
                    l1.Top = y; l2.Top = y + 30; l3.Top = y + 58;
                };
                sidebar.Resize += place;
                place(null, null);
            }

            void Logout()
            {
                if (MessageBox.Show("¿Deseas cerrar sesión?", "IMPREXA", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Close();
            }

            // ---------- Barra superior ----------
            Panel BuildTopbar()
            {
                var top = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White };
                top.Paint += (s, e) =>
                {
                    using (var pen = new Pen(LineCol))
                        e.Graphics.DrawLine(pen, 0, top.Height - 1, top.Width, top.Height - 1);
                };

                // Estado de la conexión a la base de datos
                lblDb = L("●  Conectando...", 9.5f, Gray, FontStyle.Regular, 24, 29);
                lblDb.Cursor = Cursors.Hand;
                lblDb.Click += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(ultimoError))
                        MessageBox.Show("No se pudo conectar a SQL Server:\n\n" + ultimoError +
                            "\n\nRevisa la cadena de conexión (Db.ConnectionString) y que el servidor esté encendido.",
                            "Conexión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else
                        RefreshData();
                };
                top.Controls.Add(lblDb);

                var user = new Panel { Dock = DockStyle.Right, Width = 310, BackColor = Color.White };

                var bell = L("🔔", 16, Dark, FontStyle.Regular, 24, 22, 0, "Segoe UI Emoji");
                bell.Cursor = Cursors.Hand;
                bell.Click += (s, e) =>
                    MessageBox.Show("Tienes " + enProceso + " pedido(s) en producción.", "Notificaciones", MessageBoxButtons.OK, MessageBoxIcon.Information);
                user.Controls.Add(bell);

                var dot = new RoundedPanel { Size = new Size(10, 10), Location = new Point(46, 20), Radius = 5, FillColor = Color.Red, BorderColor = Color.Red };
                user.Controls.Add(dot);
                dot.BringToFront();

                var avatar = new RoundedPanel { Size = new Size(46, 46), Location = new Point(96, 17), Radius = 23, FillColor = Color.FromArgb(11, 60, 90), BorderColor = Color.FromArgb(11, 60, 90) };
                avatar.Controls.Add(new Label { Text = "AC", ForeColor = Color.White, Font = F(11, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
                user.Controls.Add(avatar);

                user.Controls.Add(L("Astrid Cruz", 10.5f, Dark, FontStyle.Bold, 152, 19));
                user.Controls.Add(L("Cliente", 9, Gray, FontStyle.Regular, 152, 41));
                user.Controls.Add(L("▾", 11, Gray, FontStyle.Regular, 270, 26));

                top.Controls.Add(user);
                return top;
            }

            // ---------- Navegación ----------
            void ShowPage(int i)
            {
                for (int k = 0; k < pages.Length; k++) pages[k].Visible = (k == i);
                for (int k = 0; k < 3; k++)
                {
                    bool active = k == i;
                    menu[k].FillColor = active ? Teal : Color.Transparent;
                    menu[k].HoverColor = active ? Teal : Color.FromArgb(30, 255, 255, 255);
                    menu[k].Invalidate();
                }
                if (i == 2 && IsHandleCreated) RefreshData();
            }

            // ---------- PÁGINA: INICIO ----------
            Panel BuildInicio()
            {
                var page = new Panel { BackColor = PageBg };
                var t = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(22),
                    ColumnCount = 1,
                    RowCount = 3,
                    BackColor = Color.Transparent
                };
                t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                t.RowStyles.Add(new RowStyle(SizeType.Absolute, 205));
                t.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));
                t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                // ----- Banner -----
                var banner = new RoundedPanel
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(8),
                    Radius = 16,
                    FillColor = Color.FromArgb(240, 247, 252),
                    GradientEnd = Color.FromArgb(226, 240, 250),
                    BorderColor = Color.FromArgb(226, 240, 250)
                };
                banner.Controls.Add(L("Hola, Astrid 👋", 26, Dark, FontStyle.Bold, 34, 28, 0, "Segoe UI"));
                banner.Controls.Add(L("Aquí puedes gestionar tus solicitudes y dar seguimiento", 12.5f, Color.FromArgb(70, 95, 125), FontStyle.Regular, 34, 88));
                banner.Controls.Add(L("a tus impresiones.", 12.5f, Color.FromArgb(70, 95, 125), FontStyle.Regular, 34, 114));
                banner.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    var r = new Rectangle(0, 0, banner.Width - 1, banner.Height - 1);
                    if (r.Width < 500) return;
                    using (var clip = RoundedPanel.Round(r, 16))
                    {
                        g.SetClip(clip);
                        int x = (int)(r.Width * 0.58);
                        using (var navy = new LinearGradientBrush(new Rectangle(x - 10, 0, r.Width - x + 10, r.Height), Color.FromArgb(40, 90, 130), Navy, 0f))
                            g.FillPolygon(navy, new[] { new Point(x + 90, 0), new Point(r.Right, 0), new Point(r.Right, r.Bottom), new Point(x - 10, r.Bottom) });
                        using (var orBrush = new SolidBrush(Orange))
                            g.FillPolygon(orBrush, new[] { new Point(r.Right - 110, r.Bottom), new Point(r.Right, r.Bottom - 100), new Point(r.Right, r.Bottom - 55), new Point(r.Right - 60, r.Bottom) });

                        var st = g.Save();
                        g.TranslateTransform(x + (r.Width - x) * 0.45f, r.Height / 2f);
                        g.RotateTransform(-12);
                        var cardRect = new Rectangle(-120, -70, 240, 140);
                        using (var wb = new SolidBrush(Color.White))
                        using (var cp = RoundedPanel.Round(cardRect, 8))
                            g.FillPath(wb, cp);
                        using (var f = new Font("Segoe UI", 20, FontStyle.Bold | FontStyle.Italic))
                        using (var tb = new SolidBrush(Navy))
                        using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                            g.DrawString("IMPREXA", f, tb, new RectangleF(-120, -30, 240, 60), sf);
                        using (var f2 = new Font("Segoe UI", 7))
                        using (var gb = new SolidBrush(Gray))
                        using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                            g.DrawString("Gestión inteligente de impresión", f2, gb, new RectangleF(-120, 20, 240, 20), sf);
                        g.Restore(st);
                        g.ResetClip();
                    }
                };
                t.Controls.Add(banner, 0, 0);

                // ----- Tarjetas de resumen -----
                var stats = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = new Padding(0), ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent };
                for (int i = 0; i < 3; i++) stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
                stats.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                stats.Controls.Add(MakeStat("Mis pedidos", "Total de pedidos realizados", "📄", Color.FromArgb(40, 150, 200), Color.FromArgb(212, 238, 250), out lblTotal, 2), 0, 0);
                stats.Controls.Add(MakeStat("En proceso", "Pedidos en producción", "⚙", Color.FromArgb(240, 130, 20), Color.FromArgb(255, 232, 205), out lblProceso, 2), 1, 0);
                stats.Controls.Add(MakeStat("Terminados", "Pedidos entregados", "✔", Color.FromArgb(22, 150, 90), Color.FromArgb(214, 243, 226), out lblTerminados, 2), 2, 0);
                t.Controls.Add(stats, 0, 1);

                // ----- Fila inferior -----
                var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = new Padding(0), ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
                bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
                bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
                bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                // Último pedido
                var cardUp = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(8) };
                cardUp.Controls.Add(L("📦", 16, Dark, FontStyle.Regular, 18, 14, 0, "Segoe UI Emoji"));
                cardUp.Controls.Add(L("Último pedido", 15, Dark, FontStyle.Bold, 62, 16));
                var linkTodos = L("Ver todos ›", 9.5f, Teal, FontStyle.Bold, 0, 22);
                linkTodos.Cursor = Cursors.Hand;
                linkTodos.Click += (s, e) => ShowPage(2);
                cardUp.Controls.Add(linkTodos);
                var sep = new Panel { BackColor = LineCol, Height = 1, Location = new Point(20, 58), Width = 400 };
                cardUp.Controls.Add(sep);

                var thumb = new RoundedPanel { Size = new Size(200, 150), Location = new Point(24, 76), Radius = 10, FillColor = Navy, BorderColor = Navy };
                thumb.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    Color[] cols = { Color.FromArgb(40, 190, 230), Orange, Color.FromArgb(250, 200, 40), Color.FromArgb(220, 60, 90) };
                    for (int i = 0; i < cols.Length; i++)
                        using (var b = new SolidBrush(cols[i]))
                            g.FillRectangle(b, 60 + i * 20, 118, 18, 22);
                    using (var f = new Font("Segoe UI", 13, FontStyle.Bold | FontStyle.Italic))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString("IMPREXA", f, Brushes.White, new RectangleF(0, 20, 200, 80), sf);
                };
                cardUp.Controls.Add(thumb);

                lblUPTitulo = L("", 12, Dark, FontStyle.Bold, 244, 78);
                lblUPCant = L("", 10.5f, Gray, FontStyle.Regular, 244, 112, 0, "Segoe UI Emoji");
                lblUPFecha = L("", 10.5f, Gray, FontStyle.Regular, 244, 144, 0, "Segoe UI Emoji");
                cardUp.Controls.AddRange(new Control[] { lblUPTitulo, lblUPCant, lblUPFecha });

                pill = new RoundedPanel { Size = new Size(160, 32), Location = new Point(244, 180), Radius = 16, Visible = false };
                pillLabel = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI Emoji", 9.5f), BackColor = Color.Transparent };
                pill.Controls.Add(pillLabel);
                cardUp.Controls.Add(pill);

                var btnDet = new RoundedButton
                {
                    Text = "Ver detalles  ›",
                    Size = new Size(150, 42),
                    Radius = 21,
                    FillColor = Color.White,
                    HoverColor = Color.FromArgb(235, 246, 252),
                    BorderColor = Color.FromArgb(200, 225, 240),
                    ForeColor = Teal,
                    Font = F(10, FontStyle.Bold),
                    CenterText = true
                };
                btnDet.Click += (s, e) =>
                {
                    var p = ultimoPedido;
                    if (p == null) { MessageBox.Show("Aún no tienes pedidos.", "IMPREXA", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                    MessageBox.Show(
                        "Pedido #" + p.Id.ToString("000") + "\n\nProducto: " + p.Producto +
                        "\nCantidad: " + p.Cantidad.ToString("N0") + " unidades" +
                        "\nFecha: " + p.Fecha.ToString("dd/MM/yyyy") +
                        "\nEstado: " + p.Estado,
                        "Detalle del pedido", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                cardUp.Controls.Add(btnDet);

                cardUp.Resize += (s, e) =>
                {
                    linkTodos.Left = cardUp.Width - linkTodos.Width - 24;
                    sep.Width = Math.Max(10, cardUp.Width - 40);
                    btnDet.Location = new Point(cardUp.Width - btnDet.Width - 24, cardUp.Height - btnDet.Height - 20);
                };
                bottom.Controls.Add(cardUp, 0, 0);

                // CTA
                var cta = new RoundedPanel
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(8),
                    FillColor = Color.FromArgb(242, 248, 253),
                    GradientEnd = Color.FromArgb(222, 238, 250)
                };
                cta.Controls.Add(L("📝", 32, Dark, FontStyle.Regular, 22, 14, 0, "Segoe UI Emoji"));
                cta.Controls.Add(L("¿Necesitas imprimir algo?", 17, Dark, FontStyle.Bold, 24, 84, 300));
                cta.Controls.Add(L("Solicita una cotización y cuéntanos qué necesitas.", 10, Gray, FontStyle.Regular, 24, 122, 260));
                var btnCta = new RoundedButton
                {
                    Text = "+   Solicitar cotización   ›",
                    Size = new Size(250, 48),
                    Location = new Point(24, 176),
                    Radius = 24,
                    FillColor = Navy,
                    HoverColor = Teal,
                    ForeColor = Color.White,
                    Font = F(10.5f, FontStyle.Bold),
                    CenterText = true
                };
                btnCta.Click += (s, e) => ShowPage(1);
                cta.Controls.Add(btnCta);
                bottom.Controls.Add(cta, 1, 0);

                t.Controls.Add(bottom, 0, 2);
                page.Controls.Add(t);
                return page;
            }

            RoundedPanel MakeStat(string title, string sub, string glyph, Color iconFg, Color iconBg, out Label value, int targetPage)
            {
                var c = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(8) };
                var ic = new RoundedPanel { Size = new Size(68, 68), Location = new Point(20, 22), Radius = 34, FillColor = iconBg, BorderColor = iconBg };
                ic.Controls.Add(new Label
                {
                    Text = glyph,
                    Font = new Font("Segoe UI Emoji", 20),
                    ForeColor = iconFg,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                });
                c.Controls.Add(ic);
                c.Controls.Add(L(title, 12, Dark, FontStyle.Bold, 106, 24));
                value = L("0", 28, Dark, FontStyle.Bold, 102, 48);
                c.Controls.Add(value);
                c.Controls.Add(L(sub, 9.5f, Gray, FontStyle.Regular, 106, 108));
                var chev = L("›", 20, Teal, FontStyle.Bold, 0, 14);
                c.Controls.Add(chev);
                c.Resize += (s, e) => chev.Left = c.Width - 40;
                Clickable(c, (s, e) => ShowPage(targetPage));
                return c;
            }

            // ---------- PÁGINA: SOLICITAR COTIZACIÓN ----------
            Panel BuildCotizar()
            {
                var page = new Panel { BackColor = PageBg, AutoScroll = true };
                var card = new RoundedPanel { Location = new Point(30, 30), Size = new Size(730, 560) };
                int x = 32;

                card.Controls.Add(L("Solicitar cotización", 20, Dark, FontStyle.Bold, x, 22));
                card.Controls.Add(L("Cuéntanos qué necesitas imprimir y registraremos tu solicitud.", 10.5f, Gray, FontStyle.Regular, x, 62));

                card.Controls.Add(L("Producto", 10, Dark, FontStyle.Bold, x, 108));
                cboProducto = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(x, 134), Width = 320, Font = F(11) };
                cboProducto.Items.AddRange(new object[] { "Tarjetas de presentación", "Volantes promocionales", "Folletos trípticos", "Etiquetas adhesivas", "Banners", "Calendarios", "Otro" });
                cboProducto.SelectedIndex = 0;
                card.Controls.Add(cboProducto);

                card.Controls.Add(L("Cantidad (unidades)", 10, Dark, FontStyle.Bold, x + 350, 108));
                numCantidad = new NumericUpDown { Minimum = 1, Maximum = 1000000, Value = 100, Location = new Point(x + 350, 134), Width = 320, Font = F(11) };
                card.Controls.Add(numCantidad);

                card.Controls.Add(L("Tipo de papel", 10, Dark, FontStyle.Bold, x, 188));
                cboPapel = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(x, 214), Width = 320, Font = F(11) };
                cboPapel.Items.AddRange(new object[] { "Couché 150 g", "Bond 75 g", "Opalina", "Cartulina", "Adhesivo" });
                cboPapel.SelectedIndex = 0;
                card.Controls.Add(cboPapel);

                card.Controls.Add(L("Acabado", 10, Dark, FontStyle.Bold, x + 350, 188));
                cboAcabado = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(x + 350, 214), Width = 320, Font = F(11) };
                cboAcabado.Items.AddRange(new object[] { "Mate", "Brillante", "Sin acabado" });
                cboAcabado.SelectedIndex = 0;
                card.Controls.Add(cboAcabado);

                card.Controls.Add(L("Descripción del pedido", 10, Dark, FontStyle.Bold, x, 268));
                txtDesc = new TextBox { Multiline = true, Location = new Point(x, 294), Size = new Size(670, 120), Font = F(11), ScrollBars = ScrollBars.Vertical, MaxLength = 500 };
                card.Controls.Add(txtDesc);

                lblError = L("", 10, Color.FromArgb(200, 40, 40), FontStyle.Regular, x, 428);
                card.Controls.Add(lblError);

                var btn = new RoundedButton
                {
                    Text = "Enviar solicitud  ›",
                    Size = new Size(230, 48),
                    Location = new Point(x, 470),
                    Radius = 24,
                    FillColor = Navy,
                    HoverColor = Teal,
                    ForeColor = Color.White,
                    Font = F(11, FontStyle.Bold),
                    CenterText = true
                };
                btn.Click += EnviarCotizacion;
                card.Controls.Add(btn);

                page.Controls.Add(card);
                return page;
            }

            void EnviarCotizacion(object sender, EventArgs e)
            {
                if (string.IsNullOrWhiteSpace(txtDesc.Text))
                {
                    lblError.Text = "Describe brevemente lo que necesitas imprimir.";
                    return;
                }
                lblError.Text = "";

                // Armar la descripción con producto + papel + acabado
                string descripcion = cboProducto.SelectedItem + " | Papel: " + cboPapel.SelectedItem +
                                     " | Acabado: " + cboAcabado.SelectedItem + " | " + txtDesc.Text.Trim();

                try
                {
                    // IDs fijos de prueba (debes tener estos registros en la BD)
                    int idCliente = 1;   // Cliente genérico
                    int idMaquinaria = 1;   // Impresora 1
                    int idInsumos = 1;   // Papel Bond
                    int idPersona = 1;   // Empleado de prueba
                    decimal total = 0;   // Aún no se calcula

                    int nuevoId = Db.Insertar(descripcion, (int)numCantidad.Value, total,
                                               idCliente, idMaquinaria, idInsumos, idPersona);

                    MessageBox.Show("Tu solicitud #" + nuevoId.ToString("000") + " fue registrada correctamente.",
                        "Solicitud enviada", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtDesc.Clear();
                    numCantidad.Value = 100;
                    cboProducto.SelectedIndex = 0;
                    paginaActual = 1;
                    ShowPage(2);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo guardar la solicitud:\n\n" + ex.Message,
                        "Error de base de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // ---------- PÁGINA: MIS PEDIDOS (con paginación) ----------
            Panel BuildPedidos()
            {
                var page = new Panel { BackColor = PageBg, Padding = new Padding(30) };

                grid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    RowHeadersVisible = false,
                    AllowUserToAddRows = false,
                    AllowUserToResizeRows = false,
                    ReadOnly = true,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    EnableHeadersVisualStyles = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                    GridColor = LineCol,
                    ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                    ColumnHeadersHeight = 44,
                    Font = F(10.5f)
                };
                grid.ColumnHeadersDefaultCellStyle.BackColor = Navy;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                grid.ColumnHeadersDefaultCellStyle.Font = F(10.5f, FontStyle.Bold);
                grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Navy;
                grid.DefaultCellStyle.ForeColor = Dark;
                grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 244, 250);
                grid.DefaultCellStyle.SelectionForeColor = Dark;
                grid.RowTemplate.Height = 46;

                grid.Columns.Add("id", "N.º");
                grid.Columns.Add("producto", "Producto");
                grid.Columns.Add("cantidad", "Cantidad");
                grid.Columns.Add("fecha", "Fecha");
                grid.Columns.Add("estado", "Estado");
                grid.Columns[0].FillWeight = 12;
                grid.Columns[1].FillWeight = 40;

                grid.CellFormatting += (s, e) =>
                {
                    if (e.RowIndex >= 0 && e.ColumnIndex == 4 && e.Value != null)
                    {
                        string est = e.Value.ToString();
                        e.CellStyle.Font = F(10.5f, FontStyle.Bold);
                        e.CellStyle.ForeColor = est == "Entregado" ? Color.FromArgb(22, 150, 90)
                                              : est == "Pendiente" ? Color.FromArgb(20, 120, 170)
                                              : Color.FromArgb(240, 120, 20);
                    }
                };

                var title = new Label { Text = "Mis pedidos", Dock = DockStyle.Top, Height = 64, Font = F(20, FontStyle.Bold), ForeColor = Dark };

                // El orden importa: primero el grid (Fill), luego el pie (Bottom) y al final el título (Top)
                page.Controls.Add(grid);
                page.Controls.Add(BuildPaginacion());
                page.Controls.Add(title);
                return page;
            }

            RoundedButton NavBtn(string txt, Action accion)
            {
                var b = new RoundedButton
                {
                    Text = txt,
                    Size = new Size(42, 36),
                    Radius = 10,
                    FillColor = Color.White,
                    HoverColor = Color.FromArgb(235, 246, 252),
                    BorderColor = Color.FromArgb(200, 225, 240),
                    ForeColor = Teal,
                    Font = F(12, FontStyle.Bold),
                    CenterText = true,
                    Margin = new Padding(3, 0, 3, 0)
                };
                b.Click += (s, e) => { if (b.Enabled) accion(); };
                return b;
            }

            Panel BuildPaginacion()
            {
                var footer = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = PageBg };

                lblInfoPag = new Label
                {
                    Dock = DockStyle.Left,
                    Width = 340,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = F(10),
                    ForeColor = Gray
                };

                var flow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Right,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    WrapContents = false,
                    FlowDirection = FlowDirection.LeftToRight,
                    BackColor = PageBg,
                    Padding = new Padding(0, 14, 0, 0)
                };

                var lblFilas = new Label { Text = "Filas:", AutoSize = true, Font = F(10), ForeColor = Gray, Margin = new Padding(0, 9, 6, 0) };
                cboTamano = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 60, Font = F(10.5f), Margin = new Padding(0, 5, 24, 0) };
                cboTamano.Items.AddRange(new object[] { 5, 10, 20 });
                cboTamano.SelectedIndex = 0;
                cboTamano.SelectedIndexChanged += (s, e) =>
                {
                    tamanoPagina = (int)cboTamano.SelectedItem;
                    paginaActual = 1;
                    RefreshData();
                };

                btnPrimera = NavBtn("«", () => { paginaActual = 1; RefreshData(); });
                btnAnterior = NavBtn("‹", () => { paginaActual--; RefreshData(); });
                lblPagina = new Label
                {
                    AutoSize = false,
                    Width = 140,
                    Height = 36,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = F(10.5f, FontStyle.Bold),
                    ForeColor = Dark,
                    Margin = new Padding(6, 0, 6, 0)
                };
                btnSiguiente = NavBtn("›", () => { paginaActual++; RefreshData(); });
                btnUltima = NavBtn("»", () => { paginaActual = totalPaginas; RefreshData(); });

                flow.Controls.AddRange(new Control[] { lblFilas, cboTamano, btnPrimera, btnAnterior, lblPagina, btnSiguiente, btnUltima });

                footer.Controls.Add(flow);
                footer.Controls.Add(lblInfoPag);
                return footer;
            }

            // ---------- Cargar datos desde SQL Server ----------
            void RefreshData()
            {
                try
                {
                    int total, proceso, entregados;
                    Db.Resumen(out total, out proceso, out entregados);

                    totalPedidos = total;
                    enProceso = proceso;
                    lblTotal.Text = total.ToString();
                    lblProceso.Text = proceso.ToString();
                    lblTerminados.Text = entregados.ToString();

                    totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
                    if (paginaActual > totalPaginas) paginaActual = totalPaginas;
                    if (paginaActual < 1) paginaActual = 1;

                    ultimoPedido = Db.Ultimo();
                    MostrarUltimo();
                    CargarPagina();

                    SetConexion(true, null);
                }
                catch (Exception ex)
                {
                    SetConexion(false, ex.Message);
                }
            }

            void CargarPagina()
            {
                var lista = Db.Pagina(paginaActual, tamanoPagina);

                grid.Rows.Clear();
                foreach (var p in lista)
                    grid.Rows.Add("#" + p.Id.ToString("000"), p.Producto,
                                  p.Cantidad.ToString("N0"), p.Fecha.ToString("dd/MM/yyyy"), p.Estado);
                grid.ClearSelection();

                int desde = totalPedidos == 0 ? 0 : (paginaActual - 1) * tamanoPagina + 1;
                int hasta = Math.Min(paginaActual * tamanoPagina, totalPedidos);
                lblInfoPag.Text = "Mostrando " + desde + "–" + hasta + " de " + totalPedidos + " pedidos";
                lblPagina.Text = "Página " + paginaActual + " de " + totalPaginas;

                btnPrimera.Enabled = btnAnterior.Enabled = paginaActual > 1;
                btnSiguiente.Enabled = btnUltima.Enabled = paginaActual < totalPaginas;
            }

            void MostrarUltimo()
            {
                var u = ultimoPedido;
                if (u == null)
                {
                    lblUPTitulo.Text = "Aún no tienes pedidos";
                    lblUPCant.Text = "";
                    lblUPFecha.Text = "";
                    pill.Visible = false;
                    return;
                }
                lblUPTitulo.Text = "#" + u.Id.ToString("000") + "  —  " + u.Producto;
                lblUPCant.Text = "👥  " + u.Cantidad.ToString("N0") + " unidades";
                lblUPFecha.Text = "📅  " + u.Fecha.ToString("dd/MM/yyyy");
                SetPill(u.Estado);
                pill.Visible = true;
            }

            void SetConexion(bool ok, string error)
            {
                ultimoError = error;
                lblDb.Text = ok ? "●  Conectado a SQL Server" : "●  Sin conexión a la base de datos (clic para ver el error)";
                lblDb.ForeColor = ok ? Color.FromArgb(22, 150, 90) : Color.FromArgb(200, 40, 40);

                if (!ok && !avisoMostrado)
                {
                    avisoMostrado = true;
                    MessageBox.Show("No se pudo conectar a SQL Server:\n\n" + error +
                        "\n\nRevisa la cadena de conexión (Db.ConnectionString) y que hayas ejecutado el script Imprexa.sql.",
                        "Conexión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                if (ok) avisoMostrado = false;
            }

            void SetPill(string estado)
            {
                Color bg, fg; string ic;
                switch (estado)
                {
                    case "Finalizada": bg = Color.FromArgb(214, 243, 226); fg = Color.FromArgb(22, 150, 90); ic = "✔"; break;
                    case "En Proceso": bg = Color.FromArgb(255, 236, 214); fg = Color.FromArgb(240, 120, 20); ic = "⚙"; break;
                    case "Cancelada": bg = Color.FromArgb(255, 220, 220); fg = Color.FromArgb(200, 40, 40); ic = "✖"; break;
                    default: bg = Color.FromArgb(219, 238, 250); fg = Color.FromArgb(20, 120, 170); ic = "🕒"; break;
                }
                pill.FillColor = bg; pill.BorderColor = bg;
                pillLabel.ForeColor = fg;
                pillLabel.Text = ic + "  " + estado;
                pill.Invalidate();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // frmInicio
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "frmInicio";
            this.Load += new System.EventHandler(this.frmInicio_Load);
            this.ResumeLayout(false);

        }

        private void frmInicio_Load(object sender, EventArgs e)
        {

        }
    }
}
