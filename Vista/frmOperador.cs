using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vista
{
    public class frmOperador : Form
    {
        public class PageData
        {
            public int Total;
            public List<object[]> Filas = new List<object[]>();
        }
        /// <summary>
        /// Fernando Antonio Martinez Beltran 
        /// </summary>
        public static class Db
        {
            public static string ConnectionString =
                @"Server=localhost\SQLEXPRESS;Database=Imprexa;Trusted_Connection=True;TrustServerCertificate=True;";

            public class Resumen
            {
                public int Total, Pendiente, EnProduccion, Terminado, Entregado;
                public int MaqOperativas, MaqTotal, InsumosBajos;
            }

            public static Resumen ObtenerResumen()
            {
                var res = new Resumen();
                const string sql = @"
                SELECT Estado, COUNT(*) FROM dbo.Pedidos GROUP BY Estado;
                SELECT COUNT(*), ISNULL(SUM(CASE WHEN Estado = N'Operativa' THEN 1 ELSE 0 END), 0) FROM dbo.Maquinas;
                SELECT COUNT(*) FROM dbo.Insumos WHERE Cantidad <= StockMinimo;";
                using (var cn = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string est = r.GetString(0);
                            int n = r.GetInt32(1);
                            res.Total += n;
                            if (est == "Pendiente") res.Pendiente = n;
                            else if (est == "En producción") res.EnProduccion = n;
                            else if (est == "Terminado") res.Terminado = n;
                            else if (est == "Entregado") res.Entregado = n;
                        }
                        r.NextResult();
                        if (r.Read()) { res.MaqTotal = r.GetInt32(0); res.MaqOperativas = r.GetInt32(1); }
                        r.NextResult();
                        if (r.Read()) res.InsumosBajos = r.GetInt32(0);
                    }
                }
                return res;
            }

            static List<object[]> Leer(string sql, Action<SqlCommand> pars)
            {
                var lista = new List<object[]>();
                using (var cn = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    if (pars != null) pars(cmd);
                    cn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) { var o = new object[r.FieldCount]; r.GetValues(o); lista.Add(o); }
                }
                return lista;
            }

            // Id, Cliente, Trabajo, Cantidad, Fecha, Estado
            public static List<object[]> Recientes(int n)
            {
                const string sql = @"
                SELECT TOP (@n) p.Id, ISNULL(c.Nombre, N'—'), p.Producto, p.Cantidad, p.Fecha, p.Estado
                FROM dbo.Pedidos p LEFT JOIN dbo.Clientes c ON c.Id = p.ClienteId
                ORDER BY p.Fecha DESC, p.Id DESC";
                return Leer(sql, cmd => cmd.Parameters.AddWithValue("@n", n));
            }

            // Nombre, Cantidad
            public static List<object[]> InsumosBajos(int n)
            {
                const string sql = @"
                SELECT TOP (@n) Nombre, Cantidad FROM dbo.Insumos
                WHERE Cantidad <= StockMinimo
                ORDER BY (Cantidad * 1.0 / StockMinimo), Nombre";
                return Leer(sql, cmd => cmd.Parameters.AddWithValue("@n", n));
            }

            // Nombre, Estado
            public static List<object[]> MaquinasLista(int n)
            {
                const string sql = "SELECT TOP (@n) Nombre, Estado FROM dbo.Maquinas ORDER BY Id";
                return Leer(sql, cmd => cmd.Parameters.AddWithValue("@n", n));
            }

            // Paginación en el servidor (OFFSET / FETCH): solo viaja la página que se está viendo
            static PageData Consultar(string from, string select, string where, string orderBy,
                                      int pagina, int tamano, Action<SqlCommand> pars)
            {
                var data = new PageData();
                using (var cn = new SqlConnection(ConnectionString))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM " + from + where, cn))
                    {
                        if (pars != null) pars(cmd);
                        data.Total = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    string sql = "SELECT " + select + " FROM " + from + where + " ORDER BY " + orderBy +
                                 " OFFSET @off ROWS FETCH NEXT @n ROWS ONLY";
                    using (var cmd = new SqlCommand(sql, cn))
                    {
                        if (pars != null) pars(cmd);
                        cmd.Parameters.AddWithValue("@off", (pagina - 1) * tamano);
                        cmd.Parameters.AddWithValue("@n", tamano);
                        using (var r = cmd.ExecuteReader())
                            while (r.Read()) { var o = new object[r.FieldCount]; r.GetValues(o); data.Filas.Add(o); }
                    }
                }
                return data;
            }

            static void Params(SqlCommand cmd, string estado, string q)
            {
                if (!string.IsNullOrEmpty(estado)) cmd.Parameters.AddWithValue("@estado", estado);
                if (q.Length > 0) cmd.Parameters.AddWithValue("@q", "%" + q + "%");
            }

            public static PageData Pedidos(int pagina, int tamano, string busqueda, string estado)
            {
                string q = (busqueda ?? "").Trim().TrimStart('#');
                int id;
                if (!int.TryParse(q, out id)) id = -1;
                string where = " WHERE 1 = 1";
                if (!string.IsNullOrEmpty(estado)) where += " AND p.Estado = @estado";
                if (q.Length > 0) where += " AND (c.Nombre LIKE @q OR p.Producto LIKE @q OR p.Id = @id)";
                return Consultar("dbo.Pedidos p LEFT JOIN dbo.Clientes c ON c.Id = p.ClienteId",
                    "p.Id, ISNULL(c.Nombre, N'—'), p.Producto, p.Cantidad, p.Fecha, p.Estado",
                    where, "p.Id DESC", pagina, tamano, cmd =>
                    {
                        Params(cmd, estado, q);
                        if (q.Length > 0) cmd.Parameters.AddWithValue("@id", id);
                    });
            }

            public static PageData Maquinas(int pagina, int tamano, string busqueda, string estado)
            {
                string q = (busqueda ?? "").Trim();
                string where = " WHERE 1 = 1";
                if (!string.IsNullOrEmpty(estado)) where += " AND m.Estado = @estado";
                if (q.Length > 0) where += " AND (m.Nombre LIKE @q OR m.Tipo LIKE @q)";
                return Consultar("dbo.Maquinas m", "m.Id, m.Nombre, m.Tipo, m.Estado",
                    where, "m.Id", pagina, tamano, cmd => Params(cmd, estado, q));
            }

            public static PageData Insumos(int pagina, int tamano, string busqueda, string estado)
            {
                string q = (busqueda ?? "").Trim();
                const string est = "CASE WHEN i.Cantidad <= i.StockMinimo THEN N'Bajo' ELSE N'Normal' END";
                string where = " WHERE 1 = 1";
                if (!string.IsNullOrEmpty(estado)) where += " AND (" + est + ") = @estado";
                if (q.Length > 0) where += " AND i.Nombre LIKE @q";
                return Consultar("dbo.Insumos i", "i.Id, i.Nombre, i.Cantidad, i.StockMinimo, " + est,
                    where, "CASE WHEN i.Cantidad <= i.StockMinimo THEN 0 ELSE 1 END, i.Nombre",
                    pagina, tamano, cmd => Params(cmd, estado, q));
            }

            public static PageData Clientes(int pagina, int tamano, string busqueda, string estado)
            {
                string q = (busqueda ?? "").Trim();
                string where = " WHERE 1 = 1";
                if (q.Length > 0) where += " AND (c.Nombre LIKE @q OR c.Correo LIKE @q)";
                return Consultar("dbo.Clientes c",
                    "c.Id, c.Nombre, ISNULL(c.Telefono, N'—'), ISNULL(c.Correo, N'—'), (SELECT COUNT(*) FROM dbo.Pedidos p WHERE p.ClienteId = c.Id)",
                    where, "c.Nombre", pagina, tamano, cmd => Params(cmd, null, q));
            }

            public static void CambiarEstado(string tabla, int id, string estado)
            {
                if (tabla != "Pedidos" && tabla != "Maquinas") throw new ArgumentException("Tabla no permitida");
                using (var cn = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand("UPDATE dbo." + tabla + " SET Estado = @e WHERE Id = @id", cn))
                {
                    cmd.Parameters.AddWithValue("@e", estado);
                    cmd.Parameters.AddWithValue("@id", id);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // =====================================================================
        //  ESTILO, ICONOS Y UTILIDADES
        // =====================================================================
        public static class Estilo
        {
            public static readonly float Scale = ObtenerEscala();
            static float ObtenerEscala()
            {
                try { using (var g = Graphics.FromHwnd(IntPtr.Zero)) return g.DpiX / 96f; }
                catch { return 1f; }
            }
            public static int S(float v) { return (int)Math.Round(v * Scale); }

            public static readonly Color Navy = Color.FromArgb(9, 36, 63);
            public static readonly Color Teal = Color.FromArgb(14, 112, 150);
            public static readonly Color Cyan = Color.FromArgb(40, 190, 230);
            public static readonly Color Orange = Color.FromArgb(255, 138, 43);
            public static readonly Color Dark = Color.FromArgb(16, 35, 60);
            public static readonly Color Gray = Color.FromArgb(110, 125, 145);
            public static readonly Color PageBg = Color.FromArgb(247, 250, 253);
            public static readonly Color LineCol = Color.FromArgb(226, 234, 243);
            public static readonly Font FPill = new Font("Segoe UI", 8.5f);

            public static Font F(float size, FontStyle st = FontStyle.Regular) { return new Font("Segoe UI", size, st); }

            public static Label Lbl(string text, float size, Color color, FontStyle st, int x, int y, string font = "Segoe UI")
            {
                return new Label
                {
                    Text = text,
                    Font = new Font(font, size, st),
                    ForeColor = color,
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    Location = new Point(x, y)
                };
            }

            public static void DoubleBuffer(Control c)
            {
                typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(c, true, null);
            }

            // ---- Colores de las "pastillas" de estado ----
            public static void Pill(string estado, out Color bg, out Color fg, out string icon)
            {
                switch (estado)
                {
                    case "En producción": bg = Color.FromArgb(255, 237, 216); fg = Color.FromArgb(235, 115, 15); icon = "gear"; break;
                    case "Terminado": bg = Color.FromArgb(214, 243, 226); fg = Color.FromArgb(20, 140, 85); icon = "check"; break;
                    case "Entregado": bg = Color.FromArgb(220, 236, 253); fg = Color.FromArgb(37, 99, 235); icon = "ring"; break;
                    case "Pendiente": bg = Color.FromArgb(230, 238, 247); fg = Color.FromArgb(80, 100, 130); icon = "gear"; break;
                    case "Operativa": bg = Color.FromArgb(214, 243, 226); fg = Color.FromArgb(20, 140, 85); icon = "dot"; break;
                    case "Mantenimiento": bg = Color.FromArgb(255, 237, 216); fg = Color.FromArgb(235, 115, 15); icon = "dot"; break;
                    case "Fuera de servicio": bg = Color.FromArgb(254, 226, 226); fg = Color.FromArgb(210, 45, 45); icon = "dot"; break;
                    case "Bajo": bg = Color.FromArgb(254, 226, 226); fg = Color.FromArgb(210, 45, 45); icon = "gear"; break;
                    case "Normal": bg = Color.FromArgb(214, 243, 226); fg = Color.FromArgb(20, 140, 85); icon = "check"; break;
                    default: bg = Color.FromArgb(230, 238, 247); fg = Color.FromArgb(80, 100, 130); icon = "dot"; break;
                }
            }

            public static Size PillSize(string texto)
            {
                var t = TextRenderer.MeasureText(texto, FPill, new Size(1000, 100), TextFormatFlags.NoPadding);
                return new Size(t.Width + S(38), Math.Max(S(24), t.Height + S(8)));
            }

            public static void DrawPill(Graphics g, Point loc, string estado)
            {
                Color bg, fg; string ic;
                Pill(estado, out bg, out fg, out ic);
                var sz = PillSize(estado);
                var r = new Rectangle(loc.X, loc.Y, sz.Width, sz.Height);
                var old = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedPanel.Round(r, r.Height / 2))
                using (var b = new SolidBrush(bg))
                    g.FillPath(b, path);
                Icons.Draw(g, ic, new RectangleF(r.X + S(10), r.Y + (r.Height - S(14)) / 2f, S(14), S(14)), fg, 2.4f);
                TextRenderer.DrawText(g, estado, FPill, new Rectangle(r.X + S(28), r.Y, r.Width - S(28), r.Height), fg,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.Left);
                g.SmoothingMode = old;
            }

            // ---- Tablas (DataGridView) con el mismo estilo ----
            public static void StyleGrid(DataGridView g)
            {
                g.BackgroundColor = Color.White;
                g.BorderStyle = BorderStyle.None;
                g.RowHeadersVisible = false;
                g.AllowUserToAddRows = false;
                g.AllowUserToDeleteRows = false;
                g.AllowUserToResizeRows = false;
                g.AllowUserToResizeColumns = false;
                g.ReadOnly = true;
                g.MultiSelect = false;
                g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                g.EnableHeadersVisualStyles = false;
                g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                g.GridColor = LineCol;
                g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                g.ColumnHeadersHeight = 42;
                g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                g.Font = F(9.5f);
                g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 246, 252);
                g.ColumnHeadersDefaultCellStyle.ForeColor = Dark;
                g.ColumnHeadersDefaultCellStyle.Font = F(9.5f, FontStyle.Bold);
                g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 246, 252);
                g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Dark;
                g.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
                g.DefaultCellStyle.ForeColor = Color.FromArgb(45, 60, 85);
                g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(236, 246, 252);
                g.DefaultCellStyle.SelectionForeColor = Dark;
                g.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
                g.RowTemplate.Height = S(54);
                DoubleBuffer(g);
            }

            public static void CrearColumnas(DataGridView g, string[] headers, float[] pesos, bool accion)
            {
                for (int i = 0; i < headers.Length; i++)
                {
                    g.Columns.Add(new DataGridViewTextBoxColumn
                    {
                        HeaderText = headers[i],
                        FillWeight = pesos[i],
                        MinimumWidth = 60,
                        SortMode = DataGridViewColumnSortMode.NotSortable
                    });
                }
                if (accion)
                {
                    var c = new DataGridViewTextBoxColumn
                    {
                        HeaderText = "",
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                        Width = 56,
                        MinimumWidth = 56,
                        SortMode = DataGridViewColumnSortMode.NotSortable
                    };
                    c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    c.DefaultCellStyle.Font = F(12, FontStyle.Bold);
                    c.DefaultCellStyle.ForeColor = Gray;
                    c.DefaultCellStyle.Padding = new Padding(0);
                    g.Columns.Add(c);
                }
            }

            static void GridPill(DataGridViewCellPaintingEventArgs e, int col)
            {
                if (e.RowIndex < 0 || e.ColumnIndex != col || e.Value == null) return;
                e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
                string est = Convert.ToString(e.Value);
                var sz = PillSize(est);
                DrawPill(e.Graphics, new Point(e.CellBounds.X + S(12), e.CellBounds.Y + (e.CellBounds.Height - sz.Height) / 2), est);
                e.Handled = true;
            }

            public static void WireGrid(DataGridView g, int pillCol, int accionCol, Action<int, string> onAccion)
            {
                g.CellPainting += (s, e) => { if (pillCol >= 0) GridPill(e, pillCol); };
                g.CellMouseEnter += (s, e) =>
                {
                    if (accionCol >= 0 && e.RowIndex >= 0 && e.ColumnIndex == accionCol) g.Cursor = Cursors.Hand;
                };
                g.CellMouseLeave += (s, e) => { g.Cursor = Cursors.Default; };
                g.CellClick += (s, e) =>
                {
                    if (accionCol < 0 || e.RowIndex < 0 || e.ColumnIndex != accionCol) return;
                    var row = g.Rows[e.RowIndex];
                    if (row.Tag == null) return;
                    string actual = pillCol >= 0 ? Convert.ToString(row.Cells[pillCol].Value) : "";
                    onAccion(Convert.ToInt32(row.Tag), actual);
                };
            }
        }

        // Iconos dibujados con código (no necesitan imágenes ni fuentes especiales)
        public static class Icons
        {
            public static void Draw(Graphics g, string name, RectangleF r, Color c, float stroke)
            {
                var st = g.Save();
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TranslateTransform(r.X, r.Y);
                float s = Math.Min(r.Width, r.Height) / 24f;
                g.ScaleTransform(s, s);
                using (var pen = new Pen(c, stroke))
                using (var br = new SolidBrush(c))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    pen.LineJoin = LineJoin.Round;
                    switch (name)
                    {
                        case "home":
                            g.DrawLines(pen, new[] { new PointF(3, 11), new PointF(12, 3), new PointF(21, 11) });
                            g.DrawLines(pen, new[] { new PointF(5.5f, 9.5f), new PointF(5.5f, 20.5f), new PointF(10, 20.5f), new PointF(10, 14), new PointF(14, 14), new PointF(14, 20.5f), new PointF(18.5f, 20.5f), new PointF(18.5f, 9.5f) });
                            break;
                        case "doc":
                            g.DrawPolygon(pen, new[] { new PointF(6, 3), new PointF(14, 3), new PointF(19, 8), new PointF(19, 21), new PointF(6, 21) });
                            g.DrawLines(pen, new[] { new PointF(14, 3), new PointF(14, 8), new PointF(19, 8) });
                            g.DrawLine(pen, 9, 13, 16, 13);
                            g.DrawLine(pen, 9, 17, 16, 17);
                            break;
                        case "printer":
                            g.DrawRectangle(pen, 7f, 3f, 10f, 6f);
                            g.DrawRectangle(pen, 3f, 9f, 18f, 8f);
                            g.DrawRectangle(pen, 7f, 14f, 10f, 7f);
                            g.FillEllipse(br, 17f, 11f, 1.8f, 1.8f);
                            break;
                        case "box":
                            g.DrawPolygon(pen, new[] { new PointF(12, 2.5f), new PointF(20.5f, 7), new PointF(20.5f, 17), new PointF(12, 21.5f), new PointF(3.5f, 17), new PointF(3.5f, 7) });
                            g.DrawLines(pen, new[] { new PointF(3.5f, 7), new PointF(12, 12), new PointF(20.5f, 7) });
                            g.DrawLine(pen, 12, 12, 12, 21.5f);
                            break;
                        case "user":
                            g.DrawEllipse(pen, 8f, 3.5f, 8f, 8f);
                            g.DrawArc(pen, 4f, 14f, 16f, 14f, 180f, 180f);
                            break;
                        case "clipboard":
                            g.DrawRectangle(pen, 5f, 5f, 14f, 16f);
                            g.DrawRectangle(pen, 9f, 2.5f, 6f, 4f);
                            g.DrawLine(pen, 8.5f, 12, 15.5f, 12);
                            g.DrawLine(pen, 8.5f, 16, 15.5f, 16);
                            break;
                        case "gear":
                            g.DrawEllipse(pen, 6.5f, 6.5f, 11f, 11f);
                            g.DrawEllipse(pen, 9.5f, 9.5f, 5f, 5f);
                            for (int i = 0; i < 8; i++)
                            {
                                double a = i * Math.PI / 4;
                                g.DrawLine(pen, (float)(12 + 5.5 * Math.Cos(a)), (float)(12 + 5.5 * Math.Sin(a)),
                                                (float)(12 + 9.5 * Math.Cos(a)), (float)(12 + 9.5 * Math.Sin(a)));
                            }
                            break;
                        case "bell":
                            using (var gp = new GraphicsPath())
                            {
                                gp.AddArc(6f, 3f, 12f, 13f, 180f, 180f);
                                gp.AddLine(18f, 9.5f, 18f, 14.5f);
                                gp.AddLine(18f, 14.5f, 20f, 17.5f);
                                gp.AddLine(20f, 17.5f, 4f, 17.5f);
                                gp.AddLine(4f, 17.5f, 6f, 14.5f);
                                gp.CloseFigure();
                                g.FillPath(br, gp);
                                g.DrawPath(pen, gp);
                            }
                            g.FillEllipse(br, 9.5f, 18.5f, 5f, 3.5f);
                            break;
                        case "calendar":
                            g.DrawRectangle(pen, 3.5f, 5f, 17f, 15.5f);
                            g.DrawLine(pen, 3.5f, 10, 20.5f, 10);
                            g.DrawLine(pen, 8, 3, 8, 6.5f);
                            g.DrawLine(pen, 16, 3, 16, 6.5f);
                            g.FillEllipse(br, 7f, 13f, 2f, 2f);
                            g.FillEllipse(br, 11f, 13f, 2f, 2f);
                            g.FillEllipse(br, 15f, 13f, 2f, 2f);
                            g.FillEllipse(br, 7f, 16.5f, 2f, 2f);
                            g.FillEllipse(br, 11f, 16.5f, 2f, 2f);
                            break;
                        case "search":
                            g.DrawEllipse(pen, 4f, 4f, 12f, 12f);
                            g.DrawLine(pen, 14, 14, 20, 20);
                            break;
                        case "drop":
                            using (var gp = new GraphicsPath())
                            {
                                gp.AddBezier(12f, 3f, 8f, 9f, 5f, 12f, 5f, 15f);
                                gp.AddArc(5f, 8f, 14f, 14f, 180f, -180f);
                                gp.AddBezier(19f, 15f, 19f, 12f, 16f, 9f, 12f, 3f);
                                gp.CloseFigure();
                                g.DrawPath(pen, gp);
                            }
                            break;
                        case "warning":
                            var tri = new[] { new PointF(12, 3.5f), new PointF(21.5f, 20), new PointF(2.5f, 20) };
                            g.FillPolygon(br, tri);
                            g.DrawPolygon(pen, tri);
                            using (var wp = new Pen(Color.White, stroke))
                            using (var wb = new SolidBrush(Color.White))
                            {
                                wp.StartCap = LineCap.Round;
                                wp.EndCap = LineCap.Round;
                                g.DrawLine(wp, 12, 9.5f, 12, 14f);
                                g.FillEllipse(wb, 11f, 15.8f, 2f, 2f);
                            }
                            break;
                        case "chart":
                            g.FillRectangle(br, 3.5f, 12.5f, 5f, 8.5f);
                            g.FillRectangle(br, 10f, 4f, 5f, 17f);
                            g.FillRectangle(br, 16.5f, 9f, 4.5f, 12f);
                            break;
                        case "check":
                            g.DrawLines(pen, new[] { new PointF(5, 12.5f), new PointF(10, 17.5f), new PointF(19, 7) });
                            break;
                        case "ring":
                            g.DrawEllipse(pen, 4.5f, 4.5f, 15f, 15f);
                            break;
                        case "dot":
                            g.FillEllipse(br, 7f, 7f, 10f, 10f);
                            break;
                        case "chevR":
                            g.DrawLines(pen, new[] { new PointF(9, 5), new PointF(16, 12), new PointF(9, 19) });
                            break;
                        case "chevD":
                            g.DrawLines(pen, new[] { new PointF(5, 9), new PointF(12, 16), new PointF(19, 9) });
                            break;
                    }
                }
                g.Restore(st);
            }
        }

        // Cómo se muestran las filas de cada consulta en las tablas
        public static class Formatos
        {
            public static object[] Pedido(object[] r)
            {
                return new object[]
                {
                "#" + Convert.ToInt32(r[0]).ToString("000"), r[1], r[2],
                Convert.ToInt32(r[3]).ToString("N0"), Convert.ToDateTime(r[4]).ToString("dd/MM/yyyy"), r[5], "···"
                };
            }
            public static object[] Maquina(object[] r) { return new object[] { r[1], r[2], r[3], "···" }; }
            public static object[] Insumo(object[] r)
            {
                return new object[] { r[1], Convert.ToInt32(r[2]).ToString("N0"), Convert.ToInt32(r[3]).ToString("N0"), r[4] };
            }
            public static object[] Cliente(object[] r) { return new object[] { r[1], r[2], r[3], r[4] }; }
        }

        // Menú "···" para cambiar el estado de un pedido o una máquina (se guarda en SQL Server)
        public static class Acciones
        {
            public static void MenuEstado(string tabla, int id, string actual, string[] estados, Action despues)
            {
                var menu = new ContextMenuStrip();
                menu.Font = Estilo.F(10);
                menu.Items.Add(new ToolStripMenuItem("Cambiar estado a:") { Enabled = false });
                foreach (var est in estados)
                {
                    string nuevo = est;
                    var item = new ToolStripMenuItem(nuevo) { Checked = (nuevo == actual) };
                    item.Click += (s, e) =>
                    {
                        try
                        {
                            Db.CambiarEstado(tabla, id, nuevo);
                            if (despues != null) despues();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("No se pudo cambiar el estado:\n\n" + ex.Message, "Error de base de datos",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };
                    menu.Items.Add(item);
                }
                menu.Show(Cursor.Position);
            }
        }

        // =====================================================================
        //  CONTROLES PERSONALIZADOS
        // =====================================================================
        public class RoundedPanel : Panel
        {
            public int Radius { get; set; } = 14;
            public Color FillColor { get; set; } = Color.White;
            public Color BorderColor { get; set; } = Color.FromArgb(226, 234, 243);
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
                    using (var path = Round(r, Estilo.S(Radius)))
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
            public Color HoverColor { get; set; } = Color.FromArgb(28, 255, 255, 255);
            public Color BorderColor { get; set; } = Color.Transparent;
            public string Icon { get; set; } = "";     // nombre del icono (ver clase Icons)
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
                using (var path = RoundedPanel.Round(r, Estilo.S(Radius)))
                {
                    if (col.A > 0) using (var br = new SolidBrush(col)) g.FillPath(br, path);
                    if (BorderColor.A > 0) using (var pen = new Pen(BorderColor)) g.DrawPath(pen, path);
                }

                var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                if (!string.IsNullOrEmpty(Icon))
                {
                    int ic = Estilo.S(22);
                    Icons.Draw(g, Icon, new RectangleF(Estilo.S(18), (Height - ic) / 2f, ic, ic), fore, 1.8f);
                    TextRenderer.DrawText(g, Text, Font, new Rectangle(Estilo.S(58), 0, Width - Estilo.S(64), Height), fore,
                        flags | TextFormatFlags.Left);
                }
                else
                {
                    TextRenderer.DrawText(g, Text, Font, new Rectangle(Estilo.S(8), 0, Width - Estilo.S(16), Height), fore,
                        flags | (CenterText ? TextFormatFlags.HorizontalCenter : TextFormatFlags.Left));
                }
            }
        }

        // Un icono suelto (ocupa todo el tamaño del control)
        public class IconBox : Control
        {
            public string IconName = "doc";
            public Color IconColor = Estilo.Dark;
            public float Stroke = 1.8f;
            public bool Badge;

            public IconBox()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                         ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Icons.Draw(e.Graphics, IconName, new RectangleF(0, 0, Width, Height), IconColor, Stroke);
                if (Badge)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    float d = Width * 0.32f;
                    using (var b = new SolidBrush(Color.FromArgb(235, 60, 60)))
                        e.Graphics.FillEllipse(b, Width - d - 1, 1, d, d);
                }
            }
        }

        // Tarjeta de resumen (Total de pedidos, En producción, etc.)
        public class StatCard : Control
        {
            public string Title = "", Value = "0", Sub = "", IconName = "doc";
            public Color IconFg = Estilo.Teal;
            public Color IconBg = Color.FromArgb(210, 235, 250);
            public Color Tint = Color.FromArgb(236, 246, 253);
            public Color BorderCol = Color.FromArgb(214, 232, 246);
            bool hover;
            static readonly Font FTitle = new Font("Segoe UI", 11.5f, FontStyle.Bold);
            static readonly Font FValue = new Font("Segoe UI", 22f, FontStyle.Bold);
            static readonly Font FSub = new Font("Segoe UI", 9.5f);

            public StatCard()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                         ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
                Cursor = Cursors.Hand;
            }

            public void SetValue(string v) { if (Value != v) { Value = v; Invalidate(); } }
            protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                if (Width < 20 || Height < 20) return;
                var r = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = RoundedPanel.Round(r, Estilo.S(14)))
                {
                    using (var br = new LinearGradientBrush(r, Color.White, Tint, 35f)) g.FillPath(br, path);
                    using (var pen = new Pen(hover ? IconFg : BorderCol)) g.DrawPath(pen, path);
                }
                int d = Estilo.S(48), x0 = Estilo.S(16), y0 = Estilo.S(12);
                using (var b = new SolidBrush(IconBg)) g.FillEllipse(b, x0, y0, d, d);
                Icons.Draw(g, IconName, new RectangleF(x0 + d * 0.26f, y0 + d * 0.26f, d * 0.48f, d * 0.48f), IconFg, 1.8f);
                TextRenderer.DrawText(g, Title, FTitle,
                    new Rectangle(x0 + d + Estilo.S(12), y0, Math.Max(10, Width - x0 - d - Estilo.S(60)), d), Estilo.Dark,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
                TextRenderer.DrawText(g, Value, FValue, new Point(x0, Estilo.S(60)), Estilo.Dark, TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, Sub, FSub, new Point(x0 + 2, Estilo.S(100)), Estilo.Gray, TextFormatFlags.NoPadding);
                Icons.Draw(g, "chevR", new RectangleF(Width - Estilo.S(32), Estilo.S(22), Estilo.S(16), Estilo.S(16)), Estilo.Teal, 2.4f);
            }
        }

        // Gráfico de dona "Pedidos por estado"
        public class DonutChart : Control
        {
            public int[] Values = new int[0];
            public Color[] Colors = new Color[0];
            public int Total;
            static readonly Font FBig = new Font("Segoe UI", 20f, FontStyle.Bold);
            static readonly Font FSmall = new Font("Segoe UI", 10f);

            public DonutChart()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                         ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
            }

            public void SetData(int[] values, Color[] colors, int total)
            {
                Values = values; Colors = colors; Total = total;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int size = Math.Min(Width, Height) - Estilo.S(6);
                if (size < 40) return;
                float thick = size * 0.20f;
                float cx = Width / 2f, cy = Height / 2f;
                var rect = new RectangleF(cx - size / 2f + thick / 2f, cy - size / 2f + thick / 2f, size - thick, size - thick);

                int sum = 0;
                foreach (var v in Values) sum += v;
                if (sum <= 0)
                {
                    using (var pen = new Pen(Estilo.LineCol, thick)) g.DrawEllipse(pen, rect);
                }
                else
                {
                    float start = -90f, gap = 2.5f;
                    for (int i = 0; i < Values.Length; i++)
                    {
                        if (Values[i] <= 0) continue;
                        float sweep = 360f * Values[i] / sum;
                        using (var pen = new Pen(Colors[i], thick))
                        {
                            pen.StartCap = LineCap.Flat;
                            pen.EndCap = LineCap.Flat;
                            g.DrawArc(pen, rect, start + gap / 2f, Math.Max(0.5f, sweep - gap));
                        }
                        start += sweep;
                    }
                }
                var f = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;
                TextRenderer.DrawText(g, Total.ToString(), FBig,
                    new Rectangle((int)cx - Estilo.S(50), (int)cy - Estilo.S(26), Estilo.S(100), Estilo.S(32)), Estilo.Dark, f);
                TextRenderer.DrawText(g, "Total", FSmall,
                    new Rectangle((int)cx - Estilo.S(50), (int)cy + Estilo.S(4), Estilo.S(100), Estilo.S(20)), Estilo.Gray, f);
            }
        }

        // Leyenda de la dona (punto de color + nombre + cantidad)
        public class LegendList : Control
        {
            public string[] Names = new string[0];
            public Color[] Colors = new Color[0];
            public int[] Values = new int[0];
            static readonly Font F1 = new Font("Segoe UI", 9.5f);

            public LegendList()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                         ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int rh = Estilo.S(32), n = Names.Length;
                int y = Math.Max(0, (Height - n * rh) / 2);
                for (int i = 0; i < n; i++)
                {
                    using (var b = new SolidBrush(Colors[i]))
                        g.FillEllipse(b, Estilo.S(4), y + (rh - Estilo.S(12)) / 2, Estilo.S(12), Estilo.S(12));
                    var fl = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;
                    TextRenderer.DrawText(g, Names[i], F1, new Rectangle(Estilo.S(26), y, Width - Estilo.S(60), rh), Estilo.Dark, fl);
                    int val = i < Values.Length ? Values[i] : 0;
                    TextRenderer.DrawText(g, val.ToString(), F1, new Rectangle(0, y, Width - Estilo.S(8), rh), Estilo.Dark, fl | TextFormatFlags.Right);
                    y += rh;
                }
            }
        }

        // Lista corta "Insumos con bajo stock"
        public class MiniList : Control
        {
            public List<object[]> Filas = new List<object[]>();   // Nombre, Cantidad
            static readonly Font FH = new Font("Segoe UI", 9f, FontStyle.Bold);
            static readonly Font FN = new Font("Segoe UI", 9.5f);
            static readonly Font FQ = new Font("Segoe UI", 10f, FontStyle.Bold);

            public MiniList()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                         ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int w = Width, hh = Estilo.S(30), rh = Estilo.S(38);
                int colQ = (int)(w * 0.52);
                var fl = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;
                using (var br = new SolidBrush(Color.FromArgb(240, 246, 252))) g.FillRectangle(br, 0, 0, w, hh);
                TextRenderer.DrawText(g, "Insumo", FH, new Rectangle(Estilo.S(12), 0, colQ, hh), Estilo.Dark, fl);
                TextRenderer.DrawText(g, "Cantidad", FH, new Rectangle(colQ, 0, w - colQ, hh), Estilo.Dark, fl);

                int y = hh;
                foreach (var f in Filas)
                {
                    string nombre = Convert.ToString(f[0]);
                    int cant = Convert.ToInt32(f[1]);
                    using (var pen = new Pen(Estilo.LineCol)) g.DrawLine(pen, 0, y + rh - 1, w, y + rh - 1);
                    string ic = nombre.StartsWith("Tinta", StringComparison.OrdinalIgnoreCase) ? "drop" : "doc";
                    int isz = Estilo.S(20);
                    Icons.Draw(g, ic, new RectangleF(Estilo.S(12), y + (rh - isz) / 2f, isz, isz), Color.FromArgb(30, 100, 150), 1.8f);
                    TextRenderer.DrawText(g, nombre, FN, new Rectangle(Estilo.S(42), y, colQ - Estilo.S(44), rh), Estilo.Dark,
                        fl | TextFormatFlags.EndEllipsis);
                    TextRenderer.DrawText(g, cant.ToString(), FQ, new Rectangle(colQ, y, Estilo.S(60), rh), Estilo.Dark, fl);
                    var ps = Estilo.PillSize("Bajo");
                    Estilo.DrawPill(g, new Point(w - ps.Width - Estilo.S(12), y + (rh - ps.Height) / 2), "Bajo");
                    y += rh;
                }
            }
        }

        // Tarjeta pequeña de máquina (Offset 01, Digital 01...)
        public class MachineCard : Control
        {
            public string Nombre = "", Estado = "";
            static readonly Font FN = new Font("Segoe UI", 9f, FontStyle.Bold);
            static readonly Font FS = new Font("Segoe UI", 8f);

            public MachineCard()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                         ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                if (Width < 20 || Height < 20) return;
                var r = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = RoundedPanel.Round(r, Estilo.S(10)))
                {
                    using (var b = new SolidBrush(Color.White)) g.FillPath(b, path);
                    using (var pen = new Pen(Estilo.LineCol)) g.DrawPath(pen, path);
                }
                int isz = Estilo.S(24);
                Icons.Draw(g, "printer", new RectangleF((Width - isz) / 2f, Estilo.S(8), isz, isz), Estilo.Dark, 1.6f);
                TextRenderer.DrawText(g, Nombre, FN, new Rectangle(0, Estilo.S(36), Width, Estilo.S(18)), Estilo.Dark,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);

                Color bg, fg; string ic;
                Estilo.Pill(Estado, out bg, out fg, out ic);
                int tw = TextRenderer.MeasureText(Estado, FS, new Size(500, 50), TextFormatFlags.NoPadding).Width;
                int x = (Width - (tw + Estilo.S(12))) / 2;
                using (var b = new SolidBrush(fg)) g.FillEllipse(b, x, Estilo.S(62), Estilo.S(7), Estilo.S(7));
                TextRenderer.DrawText(g, Estado, FS, new Rectangle(x + Estilo.S(12), Estilo.S(57), tw + Estilo.S(4), Estilo.S(17)), Estilo.Dark,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }

        // Fecha y hora de la esquina superior derecha
        public class FechaBox : Control
        {
            public string L1 = "", L2 = "";
            static readonly Font F1 = new Font("Segoe UI", 11f);
            static readonly Font F2 = new Font("Segoe UI", 10f);

            public FechaBox()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                         ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
            }

            public void Set(string a, string b)
            {
                if (L1 != a || L2 != b) { L1 = a; L2 = b; Invalidate(); }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int w1 = TextRenderer.MeasureText(L1, F1, new Size(1000, 100), TextFormatFlags.NoPadding).Width;
                int w2 = TextRenderer.MeasureText(L2, F2, new Size(1000, 100), TextFormatFlags.NoPadding).Width;
                int tw = Math.Max(w1, w2);
                int block = Estilo.S(44) + tw;
                int x = Math.Max(0, Width - block - Estilo.S(4));
                int cy = Height / 2;
                Icons.Draw(g, "calendar", new RectangleF(x, cy - Estilo.S(16), Estilo.S(32), Estilo.S(32)), Color.FromArgb(70, 95, 125), 1.6f);
                TextRenderer.DrawText(g, L1, F1, new Point(x + Estilo.S(44), cy - Estilo.S(22)), Color.FromArgb(40, 60, 90), TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, L2, F2, new Point(x + Estilo.S(44), cy + Estilo.S(2)), Estilo.Gray, TextFormatFlags.NoPadding);
            }
        }

        // =====================================================================
        //  PÁGINA CON TABLA PAGINADA (Pedidos, Maquinaria, Insumos, Clientes)
        // =====================================================================
        public class PagedPage : Panel
        {
            public DataGridView Grid;
            public Action<Exception> OnError;
            public Action OnOk;
            public Action OnCambio;
            public string Busqueda = "";

            readonly Func<int, int, string, string, PageData> loader;
            readonly Func<object[], object[]> formato;
            ComboBox cboEstado, cboTamano;
            Label lblInfo, lblPagina;
            RoundedButton bPrim, bAnt, bSig, bUlt;
            int pagina = 1, tamano = 10, totalPaginas = 1, total;

            public PagedPage(string titulo, string subtitulo, string[] headers, float[] pesos, string[] filtroEstados,
                             int pillCol, string tablaAccion, string[] estadosAccion,
                             Func<int, int, string, string, PageData> loader, Func<object[], object[]> formato)
            {
                this.loader = loader;
                this.formato = formato;
                BackColor = Estilo.PageBg;
                Padding = new Padding(26, 14, 26, 12);

                // ----- Encabezado -----
                var head = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = Color.Transparent };
                head.Controls.Add(Estilo.Lbl(titulo, 20, Estilo.Dark, FontStyle.Bold, 2, 6));
                head.Controls.Add(Estilo.Lbl(subtitulo, 10.5f, Estilo.Gray, FontStyle.Regular, 4, 46));
                if (filtroEstados != null)
                {
                    var lblF = Estilo.Lbl("Estado:", 10, Estilo.Gray, FontStyle.Regular, 0, 0);
                    cboEstado = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 170, Font = Estilo.F(10.5f) };
                    cboEstado.Items.Add("Todos");
                    foreach (var s in filtroEstados) cboEstado.Items.Add(s);
                    cboEstado.SelectedIndex = 0;
                    cboEstado.SelectedIndexChanged += (s, e) => { pagina = 1; Reload(); };
                    head.Controls.Add(lblF);
                    head.Controls.Add(cboEstado);
                    head.Resize += (s, e) =>
                    {
                        cboEstado.Left = head.Width - cboEstado.Width - 2;
                        cboEstado.Top = Estilo.S(18);
                        lblF.Left = cboEstado.Left - lblF.Width - Estilo.S(8);
                        lblF.Top = cboEstado.Top + Estilo.S(4);
                    };
                }

                // ----- Tabla -----
                var card = new RoundedPanel { Dock = DockStyle.Fill, Padding = new Padding(10) };
                Grid = new DataGridView { Dock = DockStyle.Fill };
                Estilo.StyleGrid(Grid);
                Estilo.CrearColumnas(Grid, headers, pesos, tablaAccion != null);
                int accionCol = tablaAccion != null ? headers.Length : -1;
                Estilo.WireGrid(Grid, pillCol, accionCol, (id, actual) =>
                    Acciones.MenuEstado(tablaAccion, id, actual, estadosAccion, () =>
                    {
                        Reload();
                        if (OnCambio != null) OnCambio();
                    }));
                card.Controls.Add(Grid);

                // El orden importa: primero el centro (Fill), luego el pie (Bottom) y al final el encabezado (Top)
                Controls.Add(card);
                Controls.Add(BuildFooter());
                Controls.Add(head);
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
                    ForeColor = Estilo.Teal,
                    Font = Estilo.F(12, FontStyle.Bold),
                    CenterText = true,
                    Margin = new Padding(3, 0, 3, 0)
                };
                b.Click += (s, e) => { if (b.Enabled) accion(); };
                return b;
            }

            Panel BuildFooter()
            {
                var footer = new Panel { Dock = DockStyle.Bottom, Height = 62, BackColor = Color.Transparent };
                lblInfo = new Label
                {
                    Dock = DockStyle.Left,
                    Width = 360,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = Estilo.F(10),
                    ForeColor = Estilo.Gray
                };

                var flow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Right,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    WrapContents = false,
                    FlowDirection = FlowDirection.LeftToRight,
                    BackColor = Color.Transparent,
                    Padding = new Padding(0, 14, 0, 0)
                };

                var lblFilas = new Label { Text = "Filas:", AutoSize = true, Font = Estilo.F(10), ForeColor = Estilo.Gray, Margin = new Padding(0, 9, 6, 0) };
                cboTamano = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 60, Font = Estilo.F(10.5f), Margin = new Padding(0, 5, 24, 0) };
                cboTamano.Items.AddRange(new object[] { 5, 10, 20 });
                cboTamano.SelectedIndex = 1;
                cboTamano.SelectedIndexChanged += (s, e) =>
                {
                    tamano = (int)cboTamano.SelectedItem;
                    pagina = 1;
                    Reload();
                };

                bPrim = NavBtn("«", () => { pagina = 1; Reload(); });
                bAnt = NavBtn("‹", () => { pagina--; Reload(); });
                lblPagina = new Label
                {
                    AutoSize = false,
                    Width = 140,
                    Height = 36,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = Estilo.F(10.5f, FontStyle.Bold),
                    ForeColor = Estilo.Dark,
                    Margin = new Padding(6, 0, 6, 0)
                };
                bSig = NavBtn("›", () => { pagina++; Reload(); });
                bUlt = NavBtn("»", () => { pagina = totalPaginas; Reload(); });

                flow.Controls.AddRange(new Control[] { lblFilas, cboTamano, bPrim, bAnt, lblPagina, bSig, bUlt });
                footer.Controls.Add(flow);
                footer.Controls.Add(lblInfo);
                return footer;
            }

            string EstadoFiltro()
            {
                if (cboEstado == null || cboEstado.SelectedIndex <= 0) return null;
                return cboEstado.SelectedItem.ToString();
            }

            // Cambia el texto de búsqueda sin recargar (la recarga la hace quien muestre la página)
            public void SetBusqueda(string q) { Busqueda = q ?? ""; pagina = 1; }

            public void Reload()
            {
                try
                {
                    var data = loader(pagina, tamano, Busqueda, EstadoFiltro());
                    total = data.Total;
                    totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamano));
                    if (pagina > totalPaginas)
                    {
                        pagina = totalPaginas;
                        data = loader(pagina, tamano, Busqueda, EstadoFiltro());
                    }
                    if (pagina < 1) pagina = 1;

                    Grid.Rows.Clear();
                    foreach (var f in data.Filas)
                    {
                        int i = Grid.Rows.Add(formato(f));
                        Grid.Rows[i].Tag = f[0];
                    }
                    Grid.ClearSelection();

                    int desde = total == 0 ? 0 : (pagina - 1) * tamano + 1;
                    int hasta = Math.Min(pagina * tamano, total);
                    lblInfo.Text = "Mostrando " + desde + "–" + hasta + " de " + total + " registros";
                    lblPagina.Text = "Página " + pagina + " de " + totalPaginas;
                    bPrim.Enabled = bAnt.Enabled = pagina > 1;
                    bSig.Enabled = bUlt.Enabled = pagina < totalPaginas;

                    if (OnOk != null) OnOk();
                }
                catch (Exception ex)
                {
                    if (OnError != null) OnError(ex);
                }
            }
        }

        // =====================================================================
        //  FORMULARIO PRINCIPAL
        // =====================================================================
        public class FormAdmin : Form
        {
            static readonly string[] EstadosPedido = { "Pendiente", "En producción", "Terminado", "Entregado" };
            static readonly string[] EstadosMaquina = { "Operativa", "Mantenimiento", "Fuera de servicio" };
            static readonly Color[] ColoresEstado =
            {
            Color.FromArgb(167, 139, 250), Color.FromArgb(251, 146, 60),
            Color.FromArgb(74, 190, 130), Color.FromArgb(59, 130, 246)
        };
            const string Placeholder = "Buscar en el sistema...";

            static readonly Font FLogo = new Font("Segoe UI", 21f, FontStyle.Bold);
            static readonly Font FSubLogo = new Font("Segoe UI", 7.5f);
            static readonly Font FSlogan = new Font("Segoe UI", 15f);
            static readonly Font FScript = new Font("Segoe Script", 17f, FontStyle.Italic);

            Panel sidebar, host, topbar, userPanel;
            RoundedButton[] menu = new RoundedButton[7];
            Panel[] pages = new Panel[7];
            int current;

            StatCard cardTotal, cardProd, cardMaq, cardIns;
            DonutChart donut;
            LegendList legend;
            DataGridView gridRec;
            MiniList miniIns;
            MachineCard[] maqCards = new MachineCard[4];
            FechaBox fechaBox;
            IconBox bell;
            Label lblDb;
            TextBox txtBuscar;
            bool phOn = true;
            PagedPage pgPedidos, pgMaquinas, pgInsumos, pgClientes;

            System.Windows.Forms.Timer timer;
            int tick;
            int pendientes, insumosBajos;
            string ultimoError;
            bool avisoMostrado;

            public FormAdmin()
            {
                AutoScaleDimensions = new SizeF(96F, 96F);
                AutoScaleMode = AutoScaleMode.Dpi;

                Text = "IMPREXA - Panel de administración";
                var wa = Screen.PrimaryScreen.WorkingArea;
                ClientSize = new Size((int)Math.Min(1366.0, wa.Width / Estilo.Scale - 40),
                                      (int)Math.Min(930.0, wa.Height / Estilo.Scale - 90));
                MinimumSize = new Size(1100, 620);
                StartPosition = FormStartPosition.CenterScreen;
                BackColor = Estilo.PageBg;
                Font = Estilo.F(10);
                DoubleBuffered = true;

                // ----- Páginas -----
                pages[0] = BuildInicio();
                pages[1] = pgPedidos = new PagedPage("Pedidos de impresión", "Consulta y gestiona todos los pedidos de la imprenta.",
                    new[] { "#", "Cliente", "Trabajo", "Cantidad", "Fecha", "Estado" },
                    new[] { 9f, 22f, 26f, 12f, 14f, 17f }, EstadosPedido, 5, "Pedidos", EstadosPedido,
                    Db.Pedidos, Formatos.Pedido);
                pages[2] = pgMaquinas = new PagedPage("Maquinaria", "Estado de las máquinas de producción.",
                    new[] { "Máquina", "Tipo", "Estado" },
                    new[] { 30f, 34f, 24f }, EstadosMaquina, 2, "Maquinas", EstadosMaquina,
                    Db.Maquinas, Formatos.Maquina);
                pages[3] = pgInsumos = new PagedPage("Insumos", "Control de papel, tintas y materiales.",
                    new[] { "Insumo", "Cantidad", "Stock mínimo", "Estado" },
                    new[] { 34f, 16f, 18f, 20f }, new[] { "Bajo", "Normal" }, 3, null, null,
                    Db.Insumos, Formatos.Insumo);
                pages[4] = pgClientes = new PagedPage("Clientes", "Clientes registrados y cantidad de pedidos.",
                    new[] { "Cliente", "Teléfono", "Correo", "Pedidos" },
                    new[] { 30f, 18f, 34f, 10f }, null, -1, null, null,
                    Db.Clientes, Formatos.Cliente);
                pages[5] = BuildPlaceholder("Reportes", "Aquí podrás generar reportes de producción, ventas e inventario.");
                pages[6] = BuildPlaceholder("Usuarios y roles", "Aquí podrás administrar los usuarios del sistema y sus permisos.");

                foreach (var pp in new[] { pgPedidos, pgMaquinas, pgInsumos, pgClientes })
                {
                    pp.OnError = ex => SetConexion(false, ex.Message);
                    pp.OnOk = () => SetConexion(true, null);
                    pp.OnCambio = RefreshAll;
                }

                host = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.PageBg };
                foreach (var p in pages) { p.Dock = DockStyle.Fill; p.Visible = false; host.Controls.Add(p); }

                var content = new Panel { Dock = DockStyle.Fill, BackColor = Estilo.PageBg };
                content.Controls.Add(host);
                content.Controls.Add(BuildTopbar());
                Controls.Add(content);

                BuildSidebar();
                Controls.Add(sidebar);

                ShowPage(0);

                // Se conecta al abrir; después se actualiza sola cada 30 segundos
                timer = new System.Windows.Forms.Timer { Interval = 1000 };
                timer.Tick += (s, e) =>
                {
                    tick++;
                    ActualizarReloj();
                    if (tick % 30 == 0) RefreshTodo();
                };
                Shown += (s, e) => { ActualizarReloj(); RefreshAll(); timer.Start(); };
                FormClosed += (s, e) => timer.Dispose();
            }

            // ---------- Utilidades ----------
            void Clickable(Control c, EventHandler h)
            {
                c.Cursor = Cursors.Hand;
                c.Click += h;
                foreach (Control ch in c.Controls) Clickable(ch, h);
            }

            Label Link(RoundedPanel card, string texto, int y, EventHandler click)
            {
                var l = Estilo.Lbl(texto, 9.5f, Estilo.Teal, FontStyle.Bold, 0, y);
                l.Cursor = Cursors.Hand;
                l.Click += click;
                card.Controls.Add(l);
                EventHandler place = (s, e) => l.Left = card.Width - l.Width - Estilo.S(18);
                card.Resize += place;
                l.SizeChanged += place;
                return l;
            }

            void ActualizarReloj()
            {
                var ci = new CultureInfo("es-ES");
                var now = DateTime.Now;
                int h = now.Hour % 12;
                if (h == 0) h = 12;
                string hora = h + ":" + now.Minute.ToString("00") + " " + (now.Hour < 12 ? "a. m." : "p. m.");
                fechaBox.Set(now.ToString("d 'de' MMMM 'de' yyyy", ci), hora);
            }

            // ---------- Barra lateral ----------
            void BuildSidebar()
            {
                sidebar = new Panel { Dock = DockStyle.Left, Width = 262, BackColor = Estilo.Navy };
                sidebar.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    int W = sidebar.Width, H = sidebar.Height;

                    // Franjas decorativas
                    using (var b = new SolidBrush(Color.FromArgb(160, 30, 150, 200)))
                        g.FillPolygon(b, new[] { new Point(Estilo.S(30), H), new Point(W, H - Estilo.S(210)), new Point(W, H - Estilo.S(135)), new Point(Estilo.S(120), H) });
                    using (var b = new SolidBrush(Estilo.Orange))
                        g.FillPolygon(b, new[] { new Point(Estilo.S(120), H), new Point(W, H - Estilo.S(128)), new Point(W, H - Estilo.S(90)), new Point(Estilo.S(165), H) });

                    // Logo
                    Icons.Draw(g, "doc", new RectangleF(Estilo.S(18), Estilo.S(16), Estilo.S(46), Estilo.S(46)), Color.White, 1.5f);
                    using (var b = new SolidBrush(Estilo.Navy))
                        g.FillEllipse(b, Estilo.S(8), Estilo.S(38), Estilo.S(30), Estilo.S(30));
                    Icons.Draw(g, "gear", new RectangleF(Estilo.S(8), Estilo.S(38), Estilo.S(30), Estilo.S(30)), Estilo.Cyan, 2f);
                    int x0 = Estilo.S(74);
                    var fl = TextFormatFlags.NoPadding;
                    int wA = TextRenderer.MeasureText(g, "IMPRE", FLogo, new Size(1000, 100), fl).Width;
                    TextRenderer.DrawText(g, "IMPRE", FLogo, new Point(x0, Estilo.S(14)), Color.White, fl);
                    TextRenderer.DrawText(g, "XA", FLogo, new Point(x0 + wA, Estilo.S(14)), Estilo.Cyan, fl);
                    TextRenderer.DrawText(g, "Gestión inteligente de impresión", FSubLogo, new Point(x0 + Estilo.S(2), Estilo.S(52)),
                        Color.FromArgb(190, 210, 230), fl);

                    // Separador antes de "Usuarios y roles"
                    using (var pen = new Pen(Color.FromArgb(45, 255, 255, 255)))
                        g.DrawLine(pen, Estilo.S(24), Estilo.S(428), W - Estilo.S(24), Estilo.S(428));

                    // Eslogan
                    int y = H - Estilo.S(250);
                    TextRenderer.DrawText(g, "Tu impresión,", FSlogan, new Point(Estilo.S(26), y), Color.White, fl);
                    int wN = TextRenderer.MeasureText(g, "nuestro", FSlogan, new Size(1000, 100), fl).Width + Estilo.S(6);
                    TextRenderer.DrawText(g, "nuestro", FSlogan, new Point(Estilo.S(26), y + Estilo.S(28)), Color.White, fl);
                    TextRenderer.DrawText(g, "compromiso.", FScript, new Point(Estilo.S(26) + wN, y + Estilo.S(24)), Estilo.Cyan, fl);
                };

                string[] names = { "Inicio", "Pedidos de impresión", "Maquinaria", "Insumos", "Clientes", "Reportes", "Usuarios y roles" };
                string[] icons = { "home", "doc", "printer", "box", "user", "clipboard", "gear" };
                for (int i = 0; i < 7; i++)
                {
                    int idx = i;
                    var b = new RoundedButton
                    {
                        Text = names[i],
                        Icon = icons[i],
                        Size = new Size(236, 46),
                        Location = new Point(13, i < 6 ? 118 + i * 50 : 444),
                        ForeColor = Color.White,
                        Font = Estilo.F(11)
                    };
                    b.Click += (s, e) => ShowPage(idx);
                    menu[i] = b;
                    sidebar.Controls.Add(b);
                }
            }

            // ---------- Barra superior ----------
            Panel BuildTopbar()
            {
                topbar = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = Color.White };
                topbar.Paint += (s, e) =>
                {
                    using (var pen = new Pen(Estilo.LineCol))
                        e.Graphics.DrawLine(pen, 0, topbar.Height - 1, topbar.Width, topbar.Height - 1);
                };

                // Buscador
                var fillBusca = Color.FromArgb(247, 250, 253);
                var box = new RoundedPanel
                {
                    Location = new Point(26, 20),
                    Size = new Size(560, 44),
                    Radius = 10,
                    FillColor = fillBusca,
                    BorderColor = Estilo.LineCol
                };
                box.Controls.Add(new IconBox { IconName = "search", IconColor = Estilo.Gray, Stroke = 1.9f, Location = new Point(14, 12), Size = new Size(20, 20) });
                txtBuscar = new TextBox
                {
                    BorderStyle = BorderStyle.None,
                    Font = Estilo.F(10.5f),
                    BackColor = fillBusca,
                    ForeColor = Estilo.Gray,
                    Location = new Point(46, 12),
                    Width = 490,
                    Text = Placeholder
                };
                txtBuscar.Enter += (s, e) =>
                {
                    if (phOn) { txtBuscar.Text = ""; txtBuscar.ForeColor = Estilo.Dark; phOn = false; }
                };
                txtBuscar.Leave += (s, e) =>
                {
                    if (txtBuscar.Text.Trim().Length == 0) { phOn = true; txtBuscar.Text = Placeholder; txtBuscar.ForeColor = Estilo.Gray; }
                };
                txtBuscar.KeyDown += (s, e) =>
                {
                    if (e.KeyCode != Keys.Enter) return;
                    e.SuppressKeyPress = true;
                    pgPedidos.SetBusqueda(phOn ? "" : txtBuscar.Text.Trim());
                    ShowPage(1);
                };
                box.Controls.Add(txtBuscar);
                box.Click += (s, e) => txtBuscar.Focus();
                topbar.Controls.Add(box);

                // Estado de la conexión
                lblDb = Estilo.Lbl("●  Conectando...", 9, Estilo.Gray, FontStyle.Regular, 0, 0);
                lblDb.Cursor = Cursors.Hand;
                lblDb.Click += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(ultimoError))
                        MessageBox.Show("No se pudo conectar a SQL Server:\n\n" + ultimoError +
                            "\n\nRevisa la cadena de conexión (Db.ConnectionString) y que hayas ejecutado el script ImprexaAdmin.sql.",
                            "Conexión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else
                        RefreshTodo();
                };
                topbar.Controls.Add(lblDb);

                // Zona del usuario
                userPanel = new Panel { Dock = DockStyle.Right, Width = 300, BackColor = Color.White };
                bell = new IconBox { IconName = "bell", IconColor = Estilo.Dark, Stroke = 1.6f, Location = new Point(14, 26), Size = new Size(30, 30), Cursor = Cursors.Hand };
                bell.Click += (s, e) =>
                    MessageBox.Show("Tienes " + insumosBajos + " insumo(s) con stock bajo y " + pendientes + " pedido(s) pendiente(s).",
                        "Notificaciones", MessageBoxButtons.OK, MessageBoxIcon.Information);
                userPanel.Controls.Add(bell);

                var avatar = new RoundedPanel
                {
                    Size = new Size(46, 46),
                    Location = new Point(78, 19),
                    Radius = 23,
                    FillColor = Color.FromArgb(11, 60, 90),
                    BorderColor = Color.FromArgb(11, 60, 90)
                };
                avatar.Controls.Add(new Label
                {
                    Text = "AD",
                    ForeColor = Color.White,
                    Font = Estilo.F(11, FontStyle.Bold),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                });
                userPanel.Controls.Add(avatar);
                userPanel.Controls.Add(Estilo.Lbl("Administrador", 10.5f, Estilo.Dark, FontStyle.Bold, 134, 21));
                userPanel.Controls.Add(Estilo.Lbl("Admin", 9, Estilo.Gray, FontStyle.Regular, 134, 43));
                userPanel.Controls.Add(new IconBox { IconName = "chevD", IconColor = Estilo.Gray, Stroke = 2.2f, Location = new Point(262, 34), Size = new Size(16, 16) });

                // Clic en avatar / nombre / flecha abre el menú del usuario
                foreach (Control c in userPanel.Controls)
                    if (c != bell) Clickable(c, (s, e) => MostrarMenuUsuario());
                topbar.Controls.Add(userPanel);

                EventHandler place = (s, e) =>
                {
                    lblDb.Left = topbar.Width - userPanel.Width - lblDb.Width - Estilo.S(10);
                    lblDb.Top = (topbar.Height - lblDb.Height) / 2;
                };
                topbar.Resize += place;
                lblDb.SizeChanged += place;
                return topbar;
            }

            void MostrarMenuUsuario()
            {
                var m = new ContextMenuStrip { Font = Estilo.F(10) };
                m.Items.Add("Actualizar datos", null, (s, e) => RefreshTodo());
                m.Items.Add(new ToolStripSeparator());
                m.Items.Add("Cerrar sesión", null, (s, e) =>
                {
                    if (MessageBox.Show("¿Deseas cerrar sesión?", "IMPREXA", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        Close();
                });
                m.Show(Cursor.Position);
            }

            // ---------- Navegación ----------
            void ShowPage(int i)
            {
                current = i;
                for (int k = 0; k < pages.Length; k++) pages[k].Visible = (k == i);
                for (int k = 0; k < menu.Length; k++)
                {
                    bool active = k == i;
                    menu[k].FillColor = active ? Estilo.Teal : Color.Transparent;
                    menu[k].HoverColor = active ? Estilo.Teal : Color.FromArgb(28, 255, 255, 255);
                    menu[k].Invalidate();
                }
                if (IsHandleCreated) RefreshTodo();
            }

            void RefreshTodo()
            {
                if (current == 0) { RefreshAll(); return; }
                var pp = pages[current] as PagedPage;
                if (pp != null) pp.Reload();
                RefreshAll();
            }

            // ---------- PÁGINA: INICIO ----------
            Panel BuildInicio()
            {
                var page = new Panel { BackColor = Estilo.PageBg, AutoScroll = true };
                var root = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    Height = 852,
                    ColumnCount = 1,
                    RowCount = 3,
                    Padding = new Padding(26, 12, 26, 14),
                    BackColor = Color.Transparent
                };
                root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92f));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150f));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 580f));

                // ----- Saludo + fecha -----
                var head = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Margin = Padding.Empty };
                fechaBox = new FechaBox { Dock = DockStyle.Right, Width = 290 };
                head.Controls.Add(fechaBox);
                var greet = Estilo.Lbl("¡Hola, Administrador!", 22, Estilo.Dark, FontStyle.Bold, 2, 0);
                var wave = Estilo.Lbl("👋", 20, Color.FromArgb(240, 165, 30), FontStyle.Regular, 0, 2, "Segoe UI Emoji");
                EventHandler placeWave = (s, e) => { wave.Left = greet.Right + 2; wave.Top = greet.Top + 2; };
                greet.SizeChanged += placeWave;
                greet.LocationChanged += placeWave;
                head.Controls.Add(greet);
                head.Controls.Add(wave);
                var gris = Color.FromArgb(100, 125, 155);
                head.Controls.Add(Estilo.Lbl("Aquí puedes visualizar el estado general de la imprenta y gestionar", 11.5f, gris, FontStyle.Regular, 4, 48));
                head.Controls.Add(Estilo.Lbl("todos los módulos del sistema.", 11.5f, gris, FontStyle.Regular, 4, 70));
                root.Controls.Add(head, 0, 0);

                // ----- Tarjetas de resumen -----
                var stats = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty, ColumnCount = 4, RowCount = 1, BackColor = Color.Transparent };
                for (int i = 0; i < 4; i++) stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
                stats.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

                cardTotal = new StatCard
                {
                    Title = "Total de pedidos",
                    Sub = "Pedidos registrados",
                    IconName = "doc",
                    IconFg = Color.FromArgb(30, 150, 200),
                    IconBg = Color.FromArgb(205, 234, 249),
                    Tint = Color.FromArgb(232, 245, 253),
                    BorderCol = Color.FromArgb(210, 231, 246),
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 6, 14, 6)
                };
                cardProd = new StatCard
                {
                    Title = "En producción",
                    Sub = "Pedidos activos",
                    IconName = "printer",
                    IconFg = Color.FromArgb(240, 120, 20),
                    IconBg = Color.FromArgb(255, 226, 199),
                    Tint = Color.FromArgb(255, 243, 232),
                    BorderCol = Color.FromArgb(252, 225, 202),
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 6, 14, 6)
                };
                cardMaq = new StatCard
                {
                    Title = "Máquinas operativas",
                    Sub = "Estado de maquinaria",
                    IconName = "gear",
                    IconFg = Color.FromArgb(22, 150, 100),
                    IconBg = Color.FromArgb(203, 238, 221),
                    Tint = Color.FromArgb(232, 249, 241),
                    BorderCol = Color.FromArgb(207, 236, 222),
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 6, 14, 6)
                };
                cardIns = new StatCard
                {
                    Title = "Insumos bajos",
                    Sub = "Requieren revisión",
                    IconName = "box",
                    IconFg = Color.FromArgb(120, 90, 200),
                    IconBg = Color.FromArgb(227, 219, 250),
                    Tint = Color.FromArgb(242, 238, 252),
                    BorderCol = Color.FromArgb(226, 218, 246),
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 6, 0, 6)
                };
                cardTotal.Click += (s, e) => ShowPage(1);
                cardProd.Click += (s, e) => ShowPage(1);
                cardMaq.Click += (s, e) => ShowPage(2);
                cardIns.Click += (s, e) => ShowPage(3);
                stats.Controls.Add(cardTotal, 0, 0);
                stats.Controls.Add(cardProd, 1, 0);
                stats.Controls.Add(cardMaq, 2, 0);
                stats.Controls.Add(cardIns, 3, 0);
                root.Controls.Add(stats, 0, 1);

                // ----- Zona principal: izquierda (pedidos + banner) / derecha (gráfico, insumos, máquinas) -----
                var main = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
                main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63f));
                main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37f));
                main.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

                var left = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 14, 0), ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent };
                left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                left.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                left.RowStyles.Add(new RowStyle(SizeType.Absolute, 130f));
                left.Controls.Add(BuildRecientes(), 0, 0);
                left.Controls.Add(BuildBanner(), 0, 1);

                var right = new TableLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty, ColumnCount = 1, RowCount = 3, BackColor = Color.Transparent };
                right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                right.RowStyles.Add(new RowStyle(SizeType.Absolute, 218f));
                right.RowStyles.Add(new RowStyle(SizeType.Absolute, 212f));
                right.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                right.Controls.Add(BuildDonutCard(), 0, 0);
                right.Controls.Add(BuildInsumosCard(), 0, 1);
                right.Controls.Add(BuildMaquinasCard(), 0, 2);

                main.Controls.Add(left, 0, 0);
                main.Controls.Add(right, 1, 0);
                root.Controls.Add(main, 0, 2);

                page.Controls.Add(root);
                return page;
            }

            RoundedPanel BuildRecientes()
            {
                var card = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 12), Padding = new Padding(10, 66, 10, 12) };
                card.Controls.Add(new IconBox { IconName = "clipboard", IconColor = Estilo.Dark, Stroke = 1.7f, Location = new Point(18, 16), Size = new Size(30, 30) });
                card.Controls.Add(Estilo.Lbl("Pedidos recientes", 14, Estilo.Dark, FontStyle.Bold, 58, 18));
                Link(card, "Ver todos  ›", 23, (s, e) => ShowPage(1));

                gridRec = new DataGridView { Dock = DockStyle.Fill };
                Estilo.StyleGrid(gridRec);
                Estilo.CrearColumnas(gridRec, new[] { "#", "Cliente", "Trabajo", "Cantidad", "Fecha", "Estado" },
                    new[] { 9f, 20f, 26f, 12f, 14f, 19f }, true);
                Estilo.WireGrid(gridRec, 5, 6, (id, actual) =>
                    Acciones.MenuEstado("Pedidos", id, actual, EstadosPedido, RefreshTodo));
                card.Controls.Add(gridRec);
                return card;
            }

            RoundedPanel BuildBanner()
            {
                var card = new RoundedPanel
                {
                    Dock = DockStyle.Fill,
                    Margin = Padding.Empty,
                    FillColor = Color.FromArgb(240, 247, 252),
                    GradientEnd = Color.FromArgb(222, 238, 250),
                    BorderColor = Color.FromArgb(214, 232, 246)
                };
                card.Controls.Add(new IconBox { IconName = "printer", IconColor = Estilo.Teal, Stroke = 1.3f, Location = new Point(30, 24), Size = new Size(82, 82) });
                card.Controls.Add(Estilo.Lbl("Gestiona todo desde un solo lugar", 13, Estilo.Dark, FontStyle.Bold, 134, 26));
                card.Controls.Add(Estilo.Lbl("Crea nuevos pedidos, controla la producción y mantén", 9.5f, Estilo.Gray, FontStyle.Regular, 135, 58));
                card.Controls.Add(Estilo.Lbl("el control de tus recursos.", 9.5f, Estilo.Gray, FontStyle.Regular, 135, 78));

                var btn = new RoundedButton
                {
                    Text = "Ir a pedidos   ›",
                    Size = new Size(180, 46),
                    Radius = 23,
                    FillColor = Estilo.Navy,
                    HoverColor = Estilo.Teal,
                    ForeColor = Color.White,
                    Font = Estilo.F(10.5f, FontStyle.Bold),
                    CenterText = true
                };
                btn.Click += (s, e) => ShowPage(1);
                card.Controls.Add(btn);
                card.Resize += (s, e) =>
                {
                    btn.Left = card.Width - btn.Width - Estilo.S(30);
                    btn.Top = (card.Height - btn.Height) / 2;
                };
                return card;
            }

            RoundedPanel BuildDonutCard()
            {
                var card = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 12), Padding = new Padding(14, 54, 14, 10) };
                card.Controls.Add(new IconBox { IconName = "chart", IconColor = Estilo.Dark, Location = new Point(18, 15), Size = new Size(26, 26) });
                card.Controls.Add(Estilo.Lbl("Pedidos por estado", 12.5f, Estilo.Dark, FontStyle.Bold, 54, 17));

                var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
                t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46f));
                t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54f));
                t.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                donut = new DonutChart { Dock = DockStyle.Fill, Margin = new Padding(4) };
                legend = new LegendList { Dock = DockStyle.Fill, Margin = new Padding(8, 0, 0, 0) };
                legend.Names = EstadosPedido;
                legend.Colors = ColoresEstado;
                t.Controls.Add(donut, 0, 0);
                t.Controls.Add(legend, 1, 0);
                card.Controls.Add(t);
                return card;
            }

            RoundedPanel BuildInsumosCard()
            {
                var card = new RoundedPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 12), Padding = new Padding(10, 50, 10, 10) };
                card.Controls.Add(new IconBox { IconName = "warning", IconColor = Estilo.Dark, Stroke = 1.6f, Location = new Point(18, 13), Size = new Size(26, 26) });
                card.Controls.Add(Estilo.Lbl("Insumos con bajo stock", 11.5f, Estilo.Dark, FontStyle.Bold, 54, 16));
                Link(card, "Ver todos  ›", 18, (s, e) => ShowPage(3));
                miniIns = new MiniList { Dock = DockStyle.Fill };
                card.Controls.Add(miniIns);
                return card;
            }

            RoundedPanel BuildMaquinasCard()
            {
                var card = new RoundedPanel { Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = new Padding(12, 48, 12, 12) };
                card.Controls.Add(new IconBox { IconName = "gear", IconColor = Estilo.Dark, Stroke = 1.7f, Location = new Point(18, 13), Size = new Size(24, 24) });
                card.Controls.Add(Estilo.Lbl("Máquinas en uso", 11.5f, Estilo.Dark, FontStyle.Bold, 50, 15));
                Link(card, "Ver todas  ›", 18, (s, e) => ShowPage(2));

                var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Color.Transparent };
                for (int i = 0; i < 4; i++) t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
                t.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                for (int i = 0; i < 4; i++)
                {
                    maqCards[i] = new MachineCard { Dock = DockStyle.Fill, Margin = new Padding(3, 0, 3, 0), Visible = false };
                    maqCards[i].Click += (s, e) => ShowPage(2);
                    maqCards[i].Cursor = Cursors.Hand;
                    t.Controls.Add(maqCards[i], i, 0);
                }
                card.Controls.Add(t);
                return card;
            }

            Panel BuildPlaceholder(string titulo, string texto)
            {
                var p = new Panel { BackColor = Estilo.PageBg, Padding = new Padding(26, 14, 26, 14) };
                var card = new RoundedPanel { Dock = DockStyle.Fill };
                card.Controls.Add(Estilo.Lbl(titulo, 20, Estilo.Dark, FontStyle.Bold, 30, 28));
                card.Controls.Add(Estilo.Lbl(texto, 11, Estilo.Gray, FontStyle.Regular, 32, 74));
                card.Controls.Add(Estilo.Lbl("Módulo en construcción", 10, Estilo.Teal, FontStyle.Bold, 32, 108));
                p.Controls.Add(card);
                return p;
            }

            // ---------- Cargar el dashboard desde SQL Server ----------
            void RefreshAll()
            {
                try
                {
                    var r = Db.ObtenerResumen();
                    cardTotal.SetValue(r.Total.ToString());
                    cardProd.SetValue(r.EnProduccion.ToString());
                    cardMaq.SetValue(r.MaqOperativas + " / " + r.MaqTotal);
                    cardIns.SetValue(r.InsumosBajos.ToString());

                    var valores = new[] { r.Pendiente, r.EnProduccion, r.Terminado, r.Entregado };
                    donut.SetData(valores, ColoresEstado, r.Total);
                    legend.Values = valores;
                    legend.Invalidate();

                    pendientes = r.Pendiente;
                    insumosBajos = r.InsumosBajos;
                    bell.Badge = (pendientes + insumosBajos) > 0;
                    bell.Invalidate();

                    gridRec.Rows.Clear();
                    foreach (var f in Db.Recientes(5))
                    {
                        int i = gridRec.Rows.Add(Formatos.Pedido(f));
                        gridRec.Rows[i].Tag = f[0];
                    }
                    gridRec.ClearSelection();

                    miniIns.Filas = Db.InsumosBajos(3);
                    miniIns.Invalidate();

                    var maq = Db.MaquinasLista(maqCards.Length);
                    for (int i = 0; i < maqCards.Length; i++)
                    {
                        if (i < maq.Count)
                        {
                            maqCards[i].Nombre = Convert.ToString(maq[i][0]);
                            maqCards[i].Estado = Convert.ToString(maq[i][1]);
                            maqCards[i].Visible = true;
                        }
                        else maqCards[i].Visible = false;
                        maqCards[i].Invalidate();
                    }

                    SetConexion(true, null);
                }
                catch (Exception ex)
                {
                    SetConexion(false, ex.Message);
                }
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
                        "\n\nRevisa la cadena de conexión (Db.ConnectionString) y que hayas ejecutado el script ImprexaAdmin.sql.",
                        "Conexión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                if (ok) avisoMostrado = false;
            }
        }
    }
}
