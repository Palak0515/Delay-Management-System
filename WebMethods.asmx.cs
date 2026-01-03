using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace Delay_Management_System
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ScriptService] // ✅ Allows JavaScript (AJAX) to call the web methods
    public class WebMethods1 : WebService
    {
        public class DSDelay
        {
            public string DS_CAST_NO { get; set; }
            public string ACT_DS_START { get; set; }
            public string ACT_DS_END { get; set; }
            public string CYCLE_TIME { get; set; }
            public string STANDARD_TIME { get; set; }
            public string DELAY { get; set; }
            public string DS_AGENCY { get; set; }
            public string DS_REASONS { get; set; }
            public string REMARKS { get; set; }
            public string INSERT_TIME { get; set; }
        }

        [WebMethod]
        public List<DSDelay> Get_DS_CastNo()
        {
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;
            List<DSDelay> list = new List<DSDelay>();

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT DISTINCT DS_CAST_NO FROM tb_ds_delay_data ORDER BY DS_CAST_NO ASC";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new DSDelay
                        {
                            DS_CAST_NO = reader["DS_CAST_NO"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        [WebMethod]
        public List<DSDelay> Get_DSDelay_Data(string CastNo)
        {
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;
            List<DSDelay> list = new List<DSDelay>();

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = @"SELECT DS_CAST_NO, DS_ACT_DES_TMT_START_1, DS_ACT_DES_TMT_END_1, CYCLE_TIME, STANDARD_TIME, DELAY, AGENCY, REASONS, REMARKS, INSERT_TIME 
                                 FROM tb_ds_delay_data 
                                 WHERE DS_CAST_NO = @CastNo";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CastNo", CastNo);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new DSDelay
                            {
                                DS_CAST_NO = reader["DS_CAST_NO"].ToString(),
                                ACT_DS_START = Convert.ToDateTime(reader["DS_ACT_DES_TMT_START_1"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                ACT_DS_END = Convert.ToDateTime(reader["DS_ACT_DES_TMT_END_1"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                CYCLE_TIME = reader["CYCLE_TIME"].ToString(),
                                STANDARD_TIME = reader["STANDARD_TIME"].ToString(),
                                DELAY = reader["DELAY"].ToString(),
                                DS_AGENCY = reader["AGENCY"].ToString(),
                                DS_REASONS = reader["REASONS"].ToString(),
                                REMARKS = reader["REMARKS"].ToString(),
                                INSERT_TIME = reader["INSERT_TIME"].ToString()
                            });
                        }
                    }
                }
            }

            return list;
        }

        [WebMethod]
        public List<DSDelay> Get_ds_Delay_Download_Data(string start_time, string end_time)
        {
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;
            List<DSDelay> list = new List<DSDelay>();

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = @"SELECT DS_CAST_NO, DS_ACT_DES_TMT_START_1, DS_ACT_DES_TMT_END_1, CYCLE_TIME, STANDARD_TIME, DELAY, AGENCY, REASONS, REMARKS, INSERT_TIME 
                                 FROM tb_ds_delay_data 
                                 WHERE DS_ACT_DES_TMT_START_1 BETWEEN STR_TO_DATE(@start_time, '%Y-%m-%d %H:%i:%s') AND STR_TO_DATE(@end_time, '%Y-%m-%d %H:%i:%s')";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@start_time", start_time);
                    cmd.Parameters.AddWithValue("@end_time", end_time);
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new DSDelay
                                {
                                    DS_CAST_NO = reader["DS_CAST_NO"].ToString(),
                                    ACT_DS_START = Convert.ToDateTime(reader["DS_ACT_DES_TMT_START_1"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                    ACT_DS_END = Convert.ToDateTime(reader["DS_ACT_DES_TMT_END_1"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                    CYCLE_TIME = reader["CYCLE_TIME"].ToString(),
                                    STANDARD_TIME = reader["STANDARD_TIME"].ToString(),
                                    DELAY = reader["DELAY"].ToString(),
                                    DS_AGENCY = reader["AGENCY"].ToString(),
                                    DS_REASONS = reader["REASONS"].ToString(),
                                    REMARKS = reader["REMARKS"].ToString(),
                                    INSERT_TIME = reader["INSERT_TIME"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            return list;
        }


        [WebMethod]
        public string Insert_Data(string CastNo, string ActStart, string EndTime, string CycleTime, string StdTime, string Delay, string Agency, string Reason, string Remarks)
        {
            try
            {
                // ✅ Try parsing ActStart and EndTime
                if (!DateTime.TryParse(ActStart, out DateTime parsedActStart))
                    return "error: Invalid ActStart format";

                if (!DateTime.TryParse(EndTime, out DateTime parsedEndTime))
                    return "error: Invalid EndTime format";

                // ✅ Optional: Format for logging/debugging (not for DB insertion)
                string formattedActStart = parsedActStart.ToString("yyyy-MM-dd");
                string formattedEndTime = parsedEndTime.ToString("yyyy-MM-dd");

                string connStr = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    int count = 0;
                    conn.Open();
                    string query_1 = @"SELECT COUNT(*) FROM tb_ds_delay_data WHERE DS_CAST_NO='" + CastNo + "'";
                    MySqlCommand cmd1 = new MySqlCommand(query_1, conn);
                    cmd1.CommandText = query_1;
                    MySqlDataAdapter adp = new MySqlDataAdapter(cmd1);
                    DataSet Obj_DataSet_Log = new DataSet();
                    adp.Fill(Obj_DataSet_Log, "Tracker");

                    if (Obj_DataSet_Log.Tables["Tracker"].Rows.Count > 0)
                    {
                        count = Convert.ToInt32(Obj_DataSet_Log.Tables["Tracker"].Rows[0][0].ToString());
                    }

                    if (count == 0)
                    {
                        string query = @"INSERT INTO tb_ds_delay_data 
                    (DS_CAST_NO, DS_ACT_DES_TMT_START_1, DS_ACT_DES_TMT_END_1, CYCLE_TIME, STANDARD_TIME, DELAY, AGENCY, REASONS, REMARKS, INSERT_TIME)
                    VALUES (@CastNo, @ActStart, @EndTime, @CycleTime, @StdTime, @Delay, @Agency, @Reason, @Remarks, NOW())";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@CastNo", CastNo);
                            cmd.Parameters.AddWithValue("@ActStart", parsedActStart); // safe DateTime
                            cmd.Parameters.AddWithValue("@EndTime", parsedEndTime);   // safe DateTime
                            cmd.Parameters.AddWithValue("@CycleTime", CycleTime);
                            cmd.Parameters.AddWithValue("@StdTime", StdTime);
                            cmd.Parameters.AddWithValue("@Delay", Delay);
                            cmd.Parameters.AddWithValue("@Agency", Agency);
                            cmd.Parameters.AddWithValue("@Reason", Reason);
                            cmd.Parameters.AddWithValue("@Remarks", Remarks);

                            int result = cmd.ExecuteNonQuery();
                            return result > 0 ? "success" : "fail";
                        }
                    }
                    else
                    {
                        string query = @"update tb_ds_delay_data set
                    AGENCY = @Agency, REASONS = @Reason, REMARKS = @Remarks, INSERT_TIME= sysdate() 
                    where DS_CAST_NO = @CastNo";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@CastNo", CastNo);
                            cmd.Parameters.AddWithValue("@Agency", Agency);
                            cmd.Parameters.AddWithValue("@Reason", Reason);
                            cmd.Parameters.AddWithValue("@Remarks", Remarks);

                            int result = cmd.ExecuteNonQuery();
                            return result > 0 ? "success" : "fail";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
        }

        [WebMethod]
        public List<DSDelay> Get_DS_Agency()
        {
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;
            List<DSDelay> list = new List<DSDelay>();

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = @"SELECT DISTINCT AGENCY FROM Agency_Reason_Table ORDER BY AGENCY ASC";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new DSDelay
                                {
                                    DS_AGENCY = reader["AGENCY"].ToString(),
                                });
                            }
                        }
                    }
                }
            return list;
        }

        [WebMethod]
        public List<DSDelay> Get_DS_AgencyReason(string agencyReason)
        {
            string connStr = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;
            List<DSDelay> list = new List<DSDelay>();

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = @"SELECT DISTINCT REASONS FROM Agency_Reason_Table where agency = @agency ORDER BY REASONS ASC";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@agency", agencyReason);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new DSDelay
                            {
                                DS_REASONS = reader["REASONS"].ToString(),
                            });
                        }
                    }
                }
            }
            return list;
        }

    }
}

