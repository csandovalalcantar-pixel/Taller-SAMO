using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SAMO_WinForms
{
    public class Database
    {
        public string Server { get; private set; }
        public string TallerDb { get; private set; }
        public string RefDb { get; private set; }
        private string masterConnection;

        public Database()
        {
            Server = ConfigurationManager.AppSettings["Server"] ?? "localhost";
            TallerDb = ConfigurationManager.AppSettings["TallerDb"] ?? "Taller_Automotriz";
            RefDb = ConfigurationManager.AppSettings["RefaccionesDb"] ?? "Refaccionaria";
            bool trusted = (ConfigurationManager.AppSettings["TrustedConnection"] ?? "true").ToLower() == "true";
            var b = new SqlConnectionStringBuilder { DataSource = Server, InitialCatalog = "master", TrustServerCertificate = true, ConnectTimeout = 8 };
            if (trusted) b.IntegratedSecurity = true;
            else { b.UserID = ConfigurationManager.AppSettings["User"] ?? ""; b.Password = ConfigurationManager.AppSettings["Password"] ?? ""; }
            masterConnection = b.ConnectionString;
            using (var c = new SqlConnection(masterConnection))
            {
                c.Open();
                using (var cmd = new SqlCommand("SELECT DB_ID(@t), DB_ID(@r)", c))
                {
                    cmd.Parameters.AddWithValue("@t", TallerDb); cmd.Parameters.AddWithValue("@r", RefDb);
                    using (var r = cmd.ExecuteReader())
                    {
                        r.Read();
                        if (r.IsDBNull(0)) throw new Exception("No existe la base de datos " + TallerDb + " en " + Server);
                        if (r.IsDBNull(1)) throw new Exception("No existe la base de datos " + RefDb + " en " + Server);
                    }
                }
            }
        }

        private string Conn(string db) { var b = new SqlConnectionStringBuilder(masterConnection); b.InitialCatalog = db; return b.ConnectionString; }
        public DataTable Query(string sql, bool taller, params object[] values)
        {
            using (var c = new SqlConnection(Conn(taller ? TallerDb : RefDb)))
            using (var cmd = new SqlCommand(sql, c))
            using (var da = new SqlDataAdapter(cmd))
            {
                Add(cmd, values); var dt = new DataTable(); da.Fill(dt); return dt;
            }
        }
        public int Exec(string sql, bool taller, params object[] values)
        {
            using (var c = new SqlConnection(Conn(taller ? TallerDb : RefDb)))
            using (var cmd = new SqlCommand(sql, c))
            {
                Add(cmd, values); c.Open(); return cmd.ExecuteNonQuery();
            }
        }
        public object Scalar(string sql, bool taller, params object[] values)
        {
            using (var c = new SqlConnection(Conn(taller ? TallerDb : RefDb)))
            using (var cmd = new SqlCommand(sql, c))
            {
                Add(cmd, values); c.Open(); return cmd.ExecuteScalar();
            }
        }
        public int InsertId(string sql, bool taller, params object[] values)
        {
            object x = Scalar(sql + "; SELECT CAST(SCOPE_IDENTITY() AS INT);", taller, values);
            return Convert.ToInt32(x);
        }
        private void Add(SqlCommand cmd, object[] values)
        {
            for (int i = 0; i < values.Length; i++) cmd.Parameters.AddWithValue("@p" + i, values[i] ?? DBNull.Value);
            // Los SQL de esta aplicación usan parámetros posicionales '?'; convertirlos al orden @p0...
            string sql = cmd.CommandText;
            for (int i = 0; i < values.Length; i++) sql = sql.Replace("?", "@p" + i);
            cmd.CommandText = sql;
        }
    }
}
