using Modelos.Conexion_DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modelos.Entidades
{
    public class Datos
    {
        public static bool ExisteGerente()
        {
            const string sql = @"
        SELECT COUNT(*)
        FROM Persona P
        INNER JOIN Rol R ON R.IdRol = P.IdRol
        WHERE R.NombreRol = 'Gerente' AND P.EliminadoPersona = 0";

            var result = Escalar(sql);
            return result != null && Convert.ToInt32(result) > 0;
        }

        public static int ContarUsuarios()
        {
            var result = Escalar("SELECT COUNT(*) FROM Persona WHERE EliminadoPersona = 0");
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public static int CrearPrimerGerente(string nombre, string apellido, string dui,
                                              string correo, string hash, string telefono)
        {
            using (var cn = Conexion.Conectar())
            {
                if (cn == null) return 0;
                const string sql = @"
            INSERT INTO Persona
                (NombrePersona, ApellidoPersona, DUIPersona, CorreoPersona,
                 ContrasenaPersona, TelefonoPersona, IdRol, EliminadoPersona)
            OUTPUT INSERTED.IdPersona
            VALUES
                (@n, @a, @d, @c, @p, @t, 1, 0)";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@n", nombre);
                    cmd.Parameters.AddWithValue("@a", apellido);
                    cmd.Parameters.AddWithValue("@d", dui);
                    cmd.Parameters.AddWithValue("@c", correo);
                    cmd.Parameters.AddWithValue("@p", hash);
                    cmd.Parameters.AddWithValue("@t", telefono);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public static DataTable Query(string sql, params SqlParameter[] pars)
        {
            var dt = new DataTable();
            using (var cn = Conexion.Conectar())
            {
                if (cn == null) return dt;
                using (var cmd = new SqlCommand(sql, cn))
                {
                    if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                    using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
                }
            }
            return dt;
        }

        public static DataTable SP(string nombre, params SqlParameter[] pars)
        {
            var dt = new DataTable();
            using (var cn = Conexion.Conectar())
            {
                if (cn == null) return dt;
                using (var cmd = new SqlCommand(nombre, cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                    using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
                }
            }
            return dt;
        }

        public static object Escalar(string sql, params SqlParameter[] pars)
        {
            using (var cn = Conexion.Conectar())
            {
                if (cn == null) return null;
                using (var cmd = new SqlCommand(sql, cn))
                {
                    if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                    return cmd.ExecuteScalar();
                }
            }
        }

        public static int Ejecutar(string sql, params SqlParameter[] pars)
        {
            using (var cn = Conexion.Conectar())
            {
                if (cn == null) return -1;
                using (var cmd = new SqlCommand(sql, cn))
                {
                    if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static int EjecutarSP(string nombre, params SqlParameter[] pars)
        {
            using (var cn = Conexion.Conectar())
            {
                if (cn == null) return -1;
                using (var cmd = new SqlCommand(nombre, cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public class PedidoVista
        {
            public int IdPedido;
            public DateTime FechaPedido;
            public DateTime? FechaEntrega;
            public string Descripcion;
            public int Cantidad;
            public decimal Total;
            public string Cliente;
            public string Maquinaria;
            public string Insumo;
            public string Estado;
            public string Usuario;
        }

        static PedidoVista LeerPedido(DataRow r)
        {
            return new PedidoVista
            {
                IdPedido = Convert.ToInt32(r["IdPedido"]),
                FechaPedido = Convert.ToDateTime(r["FechaPedido"]),
                FechaEntrega = r["FechaEntrega"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["FechaEntrega"]),
                Descripcion = r["DescripcionPedido"].ToString(),
                Cantidad = Convert.ToInt32(r["CantidadPedido"]),
                Total = Convert.ToDecimal(r["TotalPedido"]),
                Cliente = r["NombreCliente"] + " " + r["ApellidoCliente"],
                Maquinaria = r["NombreMaquinaria"] == DBNull.Value ? "" : r["NombreMaquinaria"].ToString(),
                Insumo = r["NombreInsumo"] == DBNull.Value ? "" : r["NombreInsumo"].ToString(),
                Estado = r["NombreEstadoPedido"].ToString(),
                Usuario = r["Usuario"] == DBNull.Value ? "" : r["Usuario"].ToString()
            };
        }

        public static void ResumenPedidos(out int total, out int proceso, out int finalizados, out int cancelados)
        {
            total = proceso = finalizados = cancelados = 0;
            object[] r = new object[4];
            string sql = @"
                SELECT
                    COUNT(*),
                    ISNULL(SUM(CASE WHEN IdEstadoPedido = 1 THEN 1 ELSE 0 END), 0),
                    ISNULL(SUM(CASE WHEN IdEstadoPedido = 2 THEN 1 ELSE 0 END), 0),
                    ISNULL(SUM(CASE WHEN IdEstadoPedido = 3 THEN 1 ELSE 0 END), 0)
                FROM PedidosImpresion
                WHERE EliminadoPedido = 0";
            var dt = Query(sql);
            if (dt.Rows.Count > 0)
            {
                total = Convert.ToInt32(dt.Rows[0][0]);
                proceso = Convert.ToInt32(dt.Rows[0][1]);
                finalizados = Convert.ToInt32(dt.Rows[0][2]);
                cancelados = Convert.ToInt32(dt.Rows[0][3]);
            }
        }

        public static List<PedidoVista> PedidosPorCliente(int idCliente)
        {
            var lista = new List<PedidoVista>();
            string sql = @"SELECT * FROM VistaPedidos 
                           WHERE IdPedido IN (
                               SELECT IdPedido FROM PedidosImpresion WHERE IdCliente = @id AND EliminadoPedido = 0
                           )
                           ORDER BY IdPedido DESC";
            var dt = Query(sql, new SqlParameter("@id", idCliente));
            foreach (DataRow r in dt.Rows) lista.Add(LeerPedido(r));
            return lista;
        }

        public static List<PedidoVista> PedidosPaginados(int pagina, int tamano, out int total)
        {
            total = Convert.ToInt32(Escalar("SELECT COUNT(*) FROM PedidosImpresion WHERE EliminadoPedido = 0") ?? 0);
            var lista = new List<PedidoVista>();
            string sql = @"SELECT * FROM VistaPedidos
                           ORDER BY IdPedido DESC
                           OFFSET @off ROWS FETCH NEXT @n ROWS ONLY";
            var dt = Query(sql,
                new SqlParameter("@off", (pagina - 1) * tamano),
                new SqlParameter("@n", tamano));
            foreach (DataRow r in dt.Rows) lista.Add(LeerPedido(r));
            return lista;
        }

        public static PedidoVista UltimoPedido()
        {
            var dt = Query("SELECT TOP 1 * FROM VistaPedidos ORDER BY IdPedido DESC");
            return dt.Rows.Count > 0 ? LeerPedido(dt.Rows[0]) : null;
        }

        public static int AgregarPedido(string desc, int cant, decimal total,
                                        int idCliente, int idMaquinaria, int idInsumos, int idPersona)
        {
            using (var cn = Conexion.Conectar())
            {
                if (cn == null) return 0;
                using (var cmd = new SqlCommand("AgregarPedido", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DescripcionPedido", desc);
                    cmd.Parameters.AddWithValue("@CantidadPedido", cant);
                    cmd.Parameters.AddWithValue("@TotalPedido", total);
                    cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                    cmd.Parameters.AddWithValue("@IdMaquinaria", idMaquinaria);
                    cmd.Parameters.AddWithValue("@IdInsumos", idInsumos);
                    cmd.Parameters.AddWithValue("@IdPersona", idPersona);

                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                            return Convert.ToInt32(r["IdPedido"]);
                    }
                }
            }
            return 0;
        }

        public static DataTable MaquinariaLibre()
            => Query("SELECT * FROM VistaMaquinariaLibre");

        public static DataTable MaquinariaActiva()
            => Query("SELECT * FROM VistaMaquinariaActiva");

        public static DataTable InsumosDisponibles()
            => Query(@"SELECT i.IdInsumos, i.NombreInsumo, i.PrecioInsumo, i.CantidadInsumo
                       FROM Insumos i
                       INNER JOIN EstadoInsumos e ON i.IdEstadoInsumos = e.IdEstadoInsumos
                       WHERE i.EliminadoInsumos = 0 AND i.CantidadInsumo > 0");

        public static DataTable Login(string correo)
            => SP("LoginUsuario", new SqlParameter("@CorreoPersona", correo));
        public static int AgregarCliente(string nombre, string apellido, string dui, string tel, string correo)
        {
            var dt = SP("AgregarCliente",
                new SqlParameter("@NombreCliente", nombre),
                new SqlParameter("@ApellidoCliente", apellido),
                new SqlParameter("@DUICliente", (object)dui ?? DBNull.Value),
                new SqlParameter("@TelefonoCliente", tel),
                new SqlParameter("@CorreoCliente", correo));
            if (dt.Rows.Count > 0 && dt.Rows[0]["Resultado"].ToString() != "ELIMINADO")
                return Convert.ToInt32(dt.Rows[0]["IdCliente"]);
            return 0;
        }
    }
}
