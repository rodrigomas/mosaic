using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using MySql.Data.MySqlClient;
using System.Data;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace Mosaic
{
    class DBManager : IDisposable
    {
        //private MySqlConnection Conn = null;


        static Dictionary<int, List<String>> Data = new Dictionary<int, List<string>>();

        static bool isOpen = false;

        public DBManager()
        {
            /*Conn = new MySqlConnection("server=192.168.0.18;user id=root;password=123456;database=mosaic");

            try
            {
                Conn.Open();
            }
            catch
            {

            }*/

            if (File.Exists("database.db") && !isOpen)
            {
                Data.Clear();
                using (StreamReader sr = new StreamReader("database.db"))
                {
                    String Line = sr.ReadLine();

                    while (Line != null)
                    {
                        String[] dts = Line.Split('&');

                        int key = int.Parse(dts[0]);
                        Data.Add(key, new List<String>());

                        for (int i = 1; i < dts.Length; i++)
                        {
                            Data[key].Add(dts[i]);
                            FileList.Add(dts[i]);
                        }

                        Line = sr.ReadLine();
                    }
                }

                isOpen = true;
            }
        }

        static List<String> FileList = new List<string>();

        public static void Save()
        {
            using (StreamWriter fs = new StreamWriter("database.db"))
            {
                foreach (var d in Data)
                {
                    fs.Write(d.Key);
                    foreach (string s in d.Value)
                    {
                        fs.Write("&" + s);
                    }

                    fs.WriteLine();
                }
            }
        }

        public void Dispose()
        {
            /*if (Conn != null && Conn.State == System.Data.ConnectionState.Open)
            {
                Conn.Close();
            }*/
            

           // String ser = JsonConvert.SerializeObject(Data);

           // File.WriteAllText("database.json", ser);
        }

        internal bool HasDB(string dbName)
        {
            /*int a = 0;

            return HasDB(dbName, out a);*/

            return true;
        }

        internal bool HasDB(string dbName, out int id)
        {
            id  = 1;

            return true;
            /*id = 0;

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
            }                   */     
        }

        internal bool CreateDB(string dbName)
        {

            return true;
            /*if(HasDB(dbName)) return true;

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
            }  */       
        }

        internal bool DBHasFile(string SHA1, string dbName)
        {
            /*int a = 0;

            return DBHasFile(SHA1, dbName, out a);*/

            return true;
        }

        internal bool DBHasFile(string SHA1, string dbName, out int id)
        {
            id = 1;
            /*
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
            }             */

            return true;
        }

        internal bool InsertFile(string path, string SHA1, System.Drawing.Color[] color, Color center, int cnt, byte[] img, string dbName)
        {
            ////////////

            foreach (Color c in color)
            {
                if (Data.ContainsKey(c.ToArgb()))
                {
                    Data[c.ToArgb()].Add(path);
                }
                else
                {
                    Data[c.ToArgb()] = new List<string>();
                    Data[c.ToArgb()].Add(path);
                } 
            }            

            return true;

            /*int dbid = 0;

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
            }    */     
        }

        private System.Globalization.CultureInfo Info = new System.Globalization.CultureInfo("en-us");

        private SortedList<String, double> _Images = new SortedList<string, double>();

        internal void ResetImages()
        {
            _Images.Clear();
            ListImage.Clear();
        }

        internal List<String> GetLibraries()
        {
            List<String> dic = new List<String>();

            dic.Add("BASE");

            return dic;

            //String SQL = String.Format(Info, "SELECT l.LibraryName FROM mosaic.library l ORDER BY l.LibraryName");

            //try
            //{
            //    using (MySqlCommand cmd = new MySqlCommand(SQL, Conn))
            //    {
            //        using (MySqlDataReader dr = cmd.ExecuteReader())
            //        {
            //            while (dr.Read())
            //            {
            //                dic.Add(dr["LibraryName"].ToString());
            //            }

            //            return dic;
            //        }
            //    }
            //}
            //catch
            //{
            //    return dic;
            //}                        
        }

        Dictionary<int, List<String>> ListImage = new Dictionary<int, List<string>>();

        List<String> RListImage = new List<string>();

        internal System.Drawing.Image GetImage(string dbName, System.Drawing.Color c, double dist, out string FileName, out int Cnt)
        {
            //int dbid = 0;

            FileName = null;
            Cnt = 0;

            int color = c.ToArgb();

           /*for (int i = 0; i < Data.Values.Count; i++)
            {
                var cd = Data.Values.ElementAt(i);

                if( cd.Count == 0) continue;

                FileName = cd[0];

                foreach(String ff in cd)
                {
                    if (!RListImage.Contains(ff))
                    {
                        RListImage.Add(ff);
                        FileName = ff;
                        return null;
                    }{
                }                
            }

            return null;*/
            
            if (Data.ContainsKey(color))
            {
                FileName = Data[color][0];
                Cnt = 1;
                /*
                if (!ListImage.ContainsKey(color))
                {
                    lock (ListImage)
                    {
                        if (!ListImage.ContainsKey(color))
                            ListImage.Add(color, new List<String>());
                    }

                    ListImage[color].Add(FileName);

                    return null;
                }

                for (int i = 0; i < Data[color].Count; i++)
                {
                    if (!ListImage[color].Contains(Data[color][i]))
                    {
                        ListImage[color].Add(Data[color][i]);

                        FileName = Data[color][i];

                        return null;
                    }
                }

                return null;*/
            }

            for (int i = 0; i < Data.Keys.Count; i++)
            {
                Color colorr = Color.FromArgb(Data.Keys.ElementAt(i));

                float dr = (c.R - colorr.R) / 255.0f;
                float dg = (c.G - colorr.G) / 255.0f;
                float db = (c.B - colorr.B) / 255.0f;

                double d = Math.Sqrt(dr * dr + dg * dg + db * db);

                if (d < dist)
                {
                    FileName = Data[colorr.ToArgb()][MainRandom.Next(0, Data[colorr.ToArgb()].Count)];
                    Cnt = 1;

                    if (!ListImage.ContainsKey(colorr.ToArgb()))
                    {
                        lock (ListImage)
                        {
                            if (!ListImage.ContainsKey(colorr.ToArgb()))
                                ListImage.Add(colorr.ToArgb(), new List<String>());
                        }
                    }

                    for (int j = 0; j < Data[colorr.ToArgb()].Count; j++)
                    {
                        if(!ListImage[colorr.ToArgb()].Contains(Data[colorr.ToArgb()][j]))
                        {
                            ListImage[colorr.ToArgb()].Add(Data[colorr.ToArgb()][j]);
                            FileName = Data[colorr.ToArgb()][j];

                            return null;
                        }                        
                    }

                    return null;
                }
            }

            return null;

            /*

            int d = (int)Math.Max(10, (int)dist);

            int j = 0;

            int [] vec = new int[2*d] ;

            int ni = -1;
            int pi = 1;
            for(int i = 0 ; i < 2*d; i++)
            {
                if(i % 2 == 0) 
                {
                    vec[i] = pi++;
                } else 
                {
                    vec[i] = ni--;
                }
            }

            while (j < 2*d)
            {
                if (Data.ContainsKey(color))
                {
                    FileName = Data[color][0];
                    Cnt = 1;

                    if (!ListImage.ContainsKey(color))
                    {
                        ListImage.Add(color, new List<String>());
                        ListImage[color].Add(FileName);

                        return null;
                    }

                    for (int i = 0; i < Data[color].Count; i++)
                    {
                        if (!ListImage[color].Contains(Data[color][i]))
                        {
                            ListImage[color].Add(Data[color][i]);

                            FileName = Data[color][i];

                            return null;
                        }
                    }

                    return null;
                }

                float hue = c.GetHue();
                float sat = c.GetSaturation();
                float value = c.GetBrightness();

                color = ColorFromAhsb(255, hue, sat, Math.Max(0, Math.Min(1, value + 0.025f * vec[j]))).ToArgb();

                j++;
            }

            return null;*/

            /*if (!HasDB(dbName, out dbid)) return null;

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
            }    */               
        }

        public static Color ColorFromAhsb(int a, float h, float s, float b)
        {

            float fMax, fMid, fMin;
            int iSextant, iMax, iMid, iMin;

            if (0.5 < b)
            {
                fMax = b - (b * s) + s;
                fMin = b + (b * s) - s;
            }
            else
            {
                fMax = b + (b * s);
                fMin = b - (b * s);
            }

            iSextant = (int)Math.Floor(h / 60f);
            if (300f <= h)
            {
                h -= 360f;
            }
            h /= 60f;
            h -= 2f * (float)Math.Floor(((iSextant + 1f) % 6f) / 2f);
            if (0 == iSextant % 2)
            {
                fMid = h * (fMax - fMin) + fMin;
            }
            else
            {
                fMid = fMin - h * (fMax - fMin);
            }

            iMax = Convert.ToInt32(fMax * 255);
            iMid = Convert.ToInt32(fMid * 255);
            iMin = Convert.ToInt32(fMin * 255);

            switch (iSextant)
            {
                case 1:
                    return Color.FromArgb(a, iMid, iMax, iMin);
                case 2:
                    return Color.FromArgb(a, iMin, iMax, iMid);
                case 3:
                    return Color.FromArgb(a, iMin, iMid, iMax);
                case 4:
                    return Color.FromArgb(a, iMid, iMin, iMax);
                case 5:
                    return Color.FromArgb(a, iMax, iMin, iMid);
                default:
                    return Color.FromArgb(a, iMax, iMid, iMin);
            }
        }

        static Random MainRandom = new Random();

        static int idx = 3;

        internal void GetImageRND(string dbName, Color c, out string FileName, out int Cnt)
        {
            Random MainRandom2 = new Random(idx++ + (int)(DateTime.Now - new DateTime(2012, 1, 1)).TotalMilliseconds);

            FileName = null;
            Cnt = 0;

            int j = MainRandom2.Next(0, FileList.Count);

            FileName = FileList[j];

            return;
        }
    }
}
