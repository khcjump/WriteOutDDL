using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Data;
using System.Data.Common;
//using Microsoft.Practices.EnterpriseLibrary.Data;
//using System.Data.Common;
using System.Data.SqlClient;
using Dapper;


namespace WriteOutDDL
{
    internal class Routine
    {
        public string ROUTINE_NAME { get; set; }
        public string ROUTINE_TYPE { get; set; }
        public string BODY { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string WORKNAME;
            string WORKFOLDER = Environment.CurrentDirectory + @"\OUTPUT_" + DateTime.Now.ToString("yyyyMMddHHmm");

            if (args.Length == 1)
            {
                WORKNAME = args[0];

                try
                {

                    System.IO.Directory.CreateDirectory(Environment.CurrentDirectory + "\\" + WORKNAME);
                    WORKFOLDER = Environment.CurrentDirectory + @"\" + WORKNAME;
                }
                catch
                {
                    //有異常就維持原本的
                }
            }

            Console.WriteLine("Generation DDL SQL to : " + WORKFOLDER);

            string path = WORKFOLDER;
            if (!File.Exists(path)) { Directory.CreateDirectory(path); }

            //Database db = DatabaseFactory.CreateDatabase();



            using (DbConnection cn = new SqlConnection(connectionString: ""))
            {
                DataTable t = new DataTable();
                DbCommand cmd;
                #region Procedure,Function

                cn.Open();
                string sql = @"
SELECT ROUTINE_NAME, ROUTINE_TYPE, 
OBJECT_DEFINITION(OBJECT_ID(ROUTINE_SCHEMA + '.' + ROUTINE_NAME)) AS BODY
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_BODY = 'SQL' order by 2 ";
                var routines = cn.Query<Routine>(sql: sql);

                foreach (var row in routines)
                {
                    string body = row.BODY;
                    string routineName = row.ROUTINE_NAME;
                    //可加入自訂的篩選條件
                    if (routineName.StartsWith("dt_")) continue;
                    Console.WriteLine(row.ROUTINE_TYPE + ":" + routineName);
                    File.WriteAllText(
                        Path.Combine(
                            path, string.Format("{0}-{1}.sql", row.ROUTINE_TYPE, routineName)),
                            row.BODY);
                }

                #endregion

                #region View

                sql = @"
SELECT TABLE_NAME AS ROUTINE_NAME, 'VIEW' as ROUTINE_TYPE, VIEW_DEFINITION AS BODY
FROM INFORMATION_SCHEMA.VIEWS
";
                routines = cn.Query<Routine>(sql: sql);

                foreach (var row in routines)
                {
                    string body = row.BODY;
                    string routineName = row.ROUTINE_NAME;

                    //可加入自訂的篩選條件
                    if (routineName.StartsWith("dt_")) continue;
                    Console.WriteLine(" View :" + routineName);
                    File.WriteAllText(
                        Path.Combine(
                            path, string.Format("{0}-{1}.sql", row.ROUTINE_TYPE, routineName)),
                            row.BODY);
                }

                #endregion
            }
        }
    }
}
