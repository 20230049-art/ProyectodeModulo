using Modelos.Conexion_DB;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Modelos.Entidades
{
    public class ReportesPdf
    {
        public static void Inicializar()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.UseSystemFonts = true;
        }

        public static void ClientesTotales(string rutaGuardar)
        {
            DataTable dt = ObtenerDatos(
                "SELECT IdCliente, NombreCliente, ApellidoCliente, DUICliente, " +
                "TelefonoCliente, CorreoCliente FROM VistaClienteGerente;");

            GenerarPDF(dt, "Reporte de Clientes Totales", rutaGuardar,
                "Clientes activos en el sistema");
        }

        public static void PedidosTotales(string rutaGuardar, int mes, int anio)
        {
            DataTable dt = ObtenerDatos(
                $"SELECT IdPedido, FechaPedido, NombreCliente, ApellidoCliente, " +
                $"NombreMaquinaria, NombreInsumo, CantidadPedido, TotalPedido, NombreEstadoPedido " +
                $"FROM VistaPedidos " +
                $"WHERE MONTH(FechaPedido) = {mes} AND YEAR(FechaPedido) = {anio};");

            GenerarPDF(dt, $"Reporte de Pedidos Totales - {ObtenerNombreMes(mes)} {anio}",
                rutaGuardar, "Pedidos registrados en el mes seleccionado");
        }

        public static void MaquinariaTotal(string rutaGuardar)
        {
            DataTable dt = ObtenerDatos(
                "SELECT M.IdMaquinaria, M.NombreMaquinaria, E.EstadoActualMaquinaria " +
                "FROM Maquinaria M " +
                "INNER JOIN EstadoMaquinaria E ON M.IdEstadoMaquinaria = E.IdEstadoMaquinaria " +
                "WHERE M.EliminadoMaquinaria = 0;");

            GenerarPDF(dt, "Reporte de Maquinaria Total", rutaGuardar,
                "Toda la maquinaria registrada");
        }

        public static void InventarioTotal(string rutaGuardar)
        {
            DataTable dt = ObtenerDatos(
                "SELECT IdInsumos, NombreInsumo, PrecioInsumo, CantidadInsumo, " +
                "(PrecioInsumo * CantidadInsumo) AS TotalInventario " +
                "FROM VistaInsumosGERENTE;");

            GenerarPDF(dt, "Reporte de Inventario Total", rutaGuardar,
                "Todos los insumos disponibles");
        }

        public static void PedidosEnProceso(string rutaGuardar, int mes, int anio)
        {
            DataTable dt = ObtenerDatos(
                $"SELECT IdPedido, FechaPedido, NombreCliente, ApellidoCliente, " +
                $"NombreMaquinaria, NombreInsumo, CantidadPedido, TotalPedido, NombreEstadoPedido " +
                $"FROM VistaPedidos " +
                $"WHERE NombreEstadoPedido = 'En Proceso' " +
                $"AND MONTH(FechaPedido) = {mes} AND YEAR(FechaPedido) = {anio};");

            GenerarPDF(dt, $"Reporte de Pedidos en Proceso - {ObtenerNombreMes(mes)} {anio}",
                rutaGuardar, "Pedidos que aún no han sido finalizados");
        }

        public static void MaquinariaLibre(string rutaGuardar)
        {
            DataTable dt = ObtenerDatos("SELECT * FROM VistaMaquinariaLibre;");

            GenerarPDF(dt, "Reporte de Maquinaria Libre", rutaGuardar,
                "Maquinaria disponible para asignar");
        }

        private static void GenerarPDF(DataTable dt, string titulo, string subtitulo, string nombreBase)
        {
            try
            {
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("No hay datos para generar este reporte.",
                                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string carpetaDestino = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Imprexa Reportes");

                if (!Directory.Exists(carpetaDestino))
                    Directory.CreateDirectory(carpetaDestino);

                string nombreArchivo = $"{nombreBase}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1.5f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("IMPREXA")
                                .FontSize(22).Bold().FontColor(Colors.Blue.Darken2);

                            col.Item().Text(titulo)
                                .FontSize(16).SemiBold().FontColor(Colors.Black);

                            col.Item().Text(subtitulo)
                                .FontSize(11).Italic().FontColor(Colors.Grey.Darken1);

                            col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(9).FontColor(Colors.Grey.Medium);
                        });

                        page.Content().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                for (int i = 0; i < dt.Columns.Count; i++)
                                    columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                foreach (DataColumn col in dt.Columns)
                                {
                                    header.Cell()
                                        .Background(Colors.Blue.Darken2)
                                        .Padding(5)
                                        .Text(col.ColumnName)
                                        .FontColor(Colors.White)
                                        .FontSize(10).Bold();
                                }
                            });

                            int fila = 0;
                            foreach (DataRow row in dt.Rows)
                            {
                                var bgColor = (fila % 2 == 0) ? Colors.White : Colors.Grey.Lighten4;
                                foreach (var item in row.ItemArray)
                                {
                                    table.Cell()
                                        .Background(bgColor)
                                        .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                        .Padding(4)
                                        .Text(item?.ToString() ?? "")
                                        .FontSize(9);
                                }
                                fila++;
                            }
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                    });
                }).GeneratePdf(rutaCompleta);

                System.Diagnostics.Process.Start(rutaCompleta);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el PDF:\n" + ex.Message,
                                "ERROR-PDF-103", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static DataTable ObtenerDatos(string consulta)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = Conexion.Conectar())
                {
                    if (connection == null) return dt;
                    using (SqlDataAdapter ad = new SqlDataAdapter(consulta, connection))
                        ad.Fill(dt);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error SQL al obtener datos: " + ex.Message,
                        "ERROR-SQL-100", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        private static string ObtenerNombreMes(int mes)
        {
            string[] meses = { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                               "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
            return (mes >= 1 && mes <= 12) ? meses[mes] : "";
        }
    }
}
