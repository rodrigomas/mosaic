using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System.Drawing;

namespace Mosaic
{
    class DBManager : IDisposable
    {
        private MySqlConnection Conn = null;

        public DBManager()
        {
            Conn = new  MySqlConnection("server=localhost;user id=root;password=123456;database=mosaic");
            Conn.Open();
        }

        public void Dispose()
        {
            if (Conn != null && Conn.State == System.Data.ConnectionState.Open)
            {
                Conn.Close();
            }
        }

        internal bool HasDB(string dbName)
        {
            int a = 0;

            return HasDB(dbName, out a);
        }

        internal bool HasDB(string dbName, out int id)
        {
            id = 0;

            String SQL = String.Format(Info, "SELECT l.lid FROM mosaic.library l WHERE l.LibraryName = '{0}'", dbName);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(SQL, Conn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            id = int.Parse(dr[0].ToString());
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }                        
        }

        internal bool CreateDB(string dbName)
        {
            if(HasDB(dbName)) return true;

            String SQL = String.Format(Info, "INSERT INTO mosaic.library (LibraryName, CreationDate) VALUES ('{0}', NOW())", dbName);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(SQL, Conn))
                {
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        return true;
                    }
                    else return false;
                }
            }
            catch
            {
                return false;
            }         
        }

        internal bool DBHasFile(string SHA1, string dbName)
        {
            int a = 0;

            return DBHasFile(SHA1, dbName, out a);
        }

        internal bool DBHasFile(string SHA1, string dbName, out int id)
        {
            id = 0;

            String SQL = String.Format(Info, "SELECT i.iid, l.lid FROM mosaic.image i INNER JOIN mosaic.library l ON i.ImageLibrary = l.lid WHERE l.LibraryName = '{0}' AND i.ImageSHA = '{1}'", dbName, SHA1);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(SQL, Conn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            id = int.Parse(dr["lid"].ToString());
                            return true;
                        }
                        else
                        {
                            dr.Close();

                            HasDB(dbName, out id);

                            return false;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }                    
        }

        internal bool InsertFile(string path, string SHA1, System.Drawing.Color color, Color center, int cnt, byte[] img, string dbName)
        {
            int dbid = 0;

            if (DBHasFile(SHA1, dbName, out dbid)) return true;

            if (dbid == 0) return false;

            String SQL = String.Format(Info, "INSERT INTO `mosaic`.`image` (`ImageSHA`, `ImagePath`, `ImageR`, `ImageG`, `ImageB`, `ImageI`, " +
                "`Img`, `ImageCreation`, `ImageLibrary`, `ImageCR`, `ImageCG`, `ImageCB`, `ImageCnt`) " +
                " VALUES ('{0}', @imgpath, {2}, {3}, {4}, {5}, @imgblob, NOW(), {6}, {7}, {8}, {9}, {10})",
                SHA1, path, color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, Utils.Luminance(color), dbid, center.R / 255.0f, center.G / 255.0f, center.B / 255.0f, cnt);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(SQL, Conn))
                {
                    cmd.Parameters.AddWithValue("@imgpath", path);
                    cmd.Parameters.AddWithValue("@imgblob", img);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        return true;
                    }

                    else return false;
                }
            }
            catch
            {
                return false;
            }         
        }

        private System.Globalization.CultureInfo Info = new System.Globalization.CultureInfo("en-us");

        private SortedList<String, double> _Images = new SortedList<string, double>();

        internal void ResetImages()
        {
            _Images.Clear();
        }

        internal List<String> GetLibraries()
        {
            List<String> dic = new List<String>();

            String SQL = String.Format(Info, "SELECT l.LibraryName FROM mosaic.library l ORDER BY l.LibraryName");

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(SQL, Conn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            dic.Add(dr["LibraryName"].ToString());
                        }

                        return dic;
                    }
                }
            }
            catch
            {
                return dic;
            }                        
        }

        internal System.Drawing.Image GetImage(string dbName, System.Drawing.Color c, double dist, out string FileName, out int Cnt)
        {
            int dbid = 0;

            FileName = null;
            Cnt = 0;

            if (!HasDB(dbName, out dbid)) return null;

            double R = c.R / 255.0;
            double G = c.G / 255.0;
            double B = c.B / 255.0;
            double L = Utils.Luminance(c);

            String SQL = String.Format(Info, "CALL GetImages({0},{1},{2},{3},{4},{5});", R, G, B, L, dist, dbid);

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(SQL, Conn))
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        byte[] resp = null;
                        double d = 0;
                        Cnt = 0;
                        String SHA = "";

                        while (dr.Read())
                        {
                            SHA = dr["ImageSHA"] as String;

                            if( resp == null )
                            {
                                resp = dr["Img"]  as byte[];
                                FileName = dr["ImagePath"] as String;
                                d = (float)dr["ImageDistance"];
                                Cnt = (int)dr["ImageCnt"];
                            }

                            if (!_Images.ContainsKey(SHA))
                            {
                                resp = dr["Img"] as byte[];
                                FileName = dr["ImagePath"] as String;
                                d = (float)dr["ImageDistance"];
                                Cnt = (int)dr["ImageCnt"];

                                _Images.Add(SHA, d);

                                return Utils.ByteToImage(resp);
                            }
                            else continue;
                        }

                        if (resp == null)
                        {
                            return null;
                        }
                        else
                        {
                            return Utils.ByteToImage(resp);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }                   
        }
    }
}
