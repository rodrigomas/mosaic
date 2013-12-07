using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;

namespace Mosaic
{
    public partial class MainForm : Form
    {

        class ItemData
        {
            public DateTime CreatedTime { get; set; }

            public Image Img { get; set; }

            public Image [] Frames { get; set; }

            public int Cnt { get; set; }

            public Texture Tex { get; set; }

            public Color color { get; set; }

            public String FileName { get; set; }
        }

        class FrameData
        {
            public OpenTK.Vector3 Position;
            public float Time;
            public float Alpha;
            public float Blend;
        }


        class AnimationSequency
        {
            public ItemData Primary { get; set; }

            public ItemData Target { get; set; }

            public PointF TargetPosition  { get; set; }

            public ItemData[,] ImageArray { get; set; }

            public List<FrameData> Frames { get; set; }

        }

        List<AnimationSequency> _Animations = new List<AnimationSequency>();

        List<ItemData> _DisplayImages = new List<ItemData>();

        private Texture LastImage = null;

        DateTime Start = DateTime.Now;
        DateTime Reset = DateTime.Now;
        DateTime Delta = DateTime.Now;

        private double TransitionTime = 5000;

        private double TransitionOffset = 0;

        private bool GLContextLoaded = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Renderer_Paint(object sender, PaintEventArgs e)
        {
            double cnt = (DateTime.Now - Start).TotalMilliseconds;

            double delta = (DateTime.Now - Delta).TotalMilliseconds;

            Delta = DateTime.Now;

            if (!GLContextLoaded) return;

            Renderer.MakeCurrent();

            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.PushAttrib(AttribMask.EnableBit);

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.AlphaTest);
            GL.Enable(EnableCap.Blend);

            GL.MatrixMode(MatrixMode.Projection);

            GL.PushMatrix();

            GL.LoadIdentity();

            GL.Ortho(0, Renderer.Width, 0, Renderer.Height, -1, 1);

            GL.Viewport(0, 0, Renderer.Width, Renderer.Height);

            GL.MatrixMode(MatrixMode.Modelview);

            GL.PushMatrix();

            GL.LoadIdentity();

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);

            double basefactor = 1.0f;

            TransitionOffset += delta;

            double factor = (TransitionOffset) / TransitionTime + 0.1;

            if (LastImage != null)
            {
                GL.ActiveTexture(TextureUnit.Texture0);

                LastImage.SetCurrent();

                if (LastImage.Width > LastImage.Height)
                {
                    if (Width < LastImage.Width)
                    {
                        basefactor = Width / (float)LastImage.Width;
                    }
                    else if (Height < LastImage.Height)
                    {
                        basefactor = Height / (float)LastImage.Height;
                    }
                }
                else
                {
                    if (Height < LastImage.Height)
                    {
                        basefactor = Height / (float)LastImage.Height;

                    }
                    else if (Width < LastImage.Width)
                    {
                        basefactor = Width / (float)LastImage.Width;
                    }
                }

                {
                    GL.Color4(1.0f, 1.0f, 1.0f, 1.0f - factor);

                    factor = factor * basefactor;

                    GL.PushMatrix();

                    GL.Translate(-LastImage.Width * basefactor * 0.5, -LastImage.Height * basefactor * 0.5, 0);

//                    GL.Translate((Width - LastImage.Width * basefactor) / 2.0 + P0.X, (Height - LastImage.Height * basefactor) / 2.0 + P0.Y, 0);

                    GL.Scale(factor, factor, factor);

                    GL.Begin(BeginMode.Quads);

                    GL.TexCoord2(0, 0);
                    GL.Vertex2(0, 0);

                    GL.TexCoord2(1, 0);
                    GL.Vertex2(0 + LastImage.Width, 0);

                    GL.TexCoord2(1, 1);
                    GL.Vertex2(LastImage.Width, LastImage.Height);

                    GL.TexCoord2(0, 1);
                    GL.Vertex2(0, LastImage.Height);

                    GL.End();

                    GL.PopMatrix();
                }

                //if (Math.Abs(TransitionOffset) > TransitionTime)
                //{
                //    CurrentID = (CurrentID + 1) % _DisplayImages.Count;

                //    Image img = _DisplayImages[CurrentID].Img;

                //    LastImage.UpdateTexture(img as Bitmap, false, true);

                //    TransitionOffset = 0;

                //    P0.X = Rand.Next(Width / 4);
                //    P0.Y = Rand.Next(Height / 4);

                //    //if (TransitionOffset > 0)
                //    //{

                //    //}
                //    //else
                //    //{

                //    //}
                //}
            }
            //else
            //{
            //    if (_DisplayImages.Count != 0)
            //    {
            //        CurrentID = 0;

            //        Image img = _DisplayImages[CurrentID].Img;

            //        LastImage = Texture.LoadTexture(img as Bitmap, true, false, true);
            //    }
            //}

            ///            
            Renderer.SwapBuffers();

            //if ((DateTime.Now - Reset).TotalMinutes > 20)
            //{
            //    Reset = DateTime.Now;

            //    if (_DisplayImages.Count > 30)
            //    {
            //        Clean();
            //    }
            //}
        }

        private void Clean()
        {
            foreach (ItemData it in _DisplayImages)
            {
                it.Img.Dispose();
                it.Tex.Dispose();

                _DisplayImages.Remove(it);
            }
        }

        private void Renderer_Load(object sender, EventArgs e)
        {
            Renderer.Context.LoadAll();
            GLContextLoaded = true;
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (!GLContextLoaded) return;

            Renderer.Refresh();
        }

        private void RemImage_Click(object sender, EventArgs e)
        {
            if (ListImage.SelectedItems.Count != 0)
            {
                ItemData dt = ListImage.SelectedItems[0].Tag as ItemData;

                dt.Img.Dispose();
                dt.Tex.Dispose();

                _DisplayImages.Remove(dt);
                ListImage.Items.Remove(ListImage.SelectedItems[0]);

                // DO
            }
        }

        private void UpImage_Click(object sender, EventArgs e)
        {
            if (ListImage.SelectedItems.Count != 0)
            {
                int idx = ListImage.SelectedIndices[0];

                if (idx == 0) return;

                ItemData dt = ListImage.SelectedItems[0].Tag as ItemData;
                ListViewItem it = ListImage.SelectedItems[0];

                ListImage.Items[idx] = ListImage.Items[idx - 1];
                ListImage.Items[idx - 1] = it;

                _DisplayImages[idx] = _DisplayImages[idx - 1];
                _DisplayImages[idx - 1] = dt;
                
                // DO
            }
        }

        private void DownImage_Click(object sender, EventArgs e)
        {
            if (ListImage.SelectedItems.Count != 0)
            {
                int idx = ListImage.SelectedIndices[0];

                if (idx == ListImage.Items.Count - 1) return;

                ItemData dt = ListImage.SelectedItems[0].Tag as ItemData;
                ListViewItem it = ListImage.SelectedItems[0];

                ListImage.Items[idx] = ListImage.Items[idx + 1];
                ListImage.Items[idx + 1] = it;

                _DisplayImages[idx] = _DisplayImages[idx + 1];
                _DisplayImages[idx + 1] = dt;

                // DO
            }
        }

        private String OutputFolder = "";

        private void SelOutFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    OutputFolder = dialog.SelectedPath;

                    OutFolder.Text = OutputFolder;
                }
            }
        }

        private void AddImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ListViewItem it = ListImage.Items.Add(Path.GetFileName(dialog.FileName));

                    ItemData data = new ItemData()
                    {
                        FileName = dialog.FileName,
                        CreatedTime = DateTime.UtcNow,
                        Img = Image.FromFile(dialog.FileName),
                        Tex = null
                    };

                    data.Tex = Texture.LoadTexture(data.Img as Bitmap, true, false, true);

                    data.color = Utils.GetColorMap(data.Img as Bitmap);

                    it.Tag = data;

                    _DisplayImages.Add(data);
                }
            }
        }

        private void CreateDB_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    String Folderf = dialog.SelectedPath;

                    DirectoryInfo Info = new DirectoryInfo(Folderf);

                    foreach (FileInfo file in Info.GetFiles("*.jpg"))
                    {
                        String SHA1 = "";
                        using( Stream st = file.OpenRead())
                        {
                            SHA1 = Utils.GetSHA1Hash(st);
                        }

                        Color color;

                        Color center;

                        using(Image bmp = Image.FromFile(file.FullName))
                        {
                            color = Utils.GetColorMap(bmp as Bitmap);

                            center = (bmp as Bitmap).GetPixel(bmp.Width / 2, bmp.Height / 2);

                            SaveInDB(file, SHA1, color, center, Utils.ImageToByte(bmp), DBSet.Text);
                        }                        
                    }                   
                }
            }
        }

        private int CountFrames(String fullName)
        {
            String FileNameNoExt = Path.GetFileNameWithoutExtension(fullName);
            String FileDir = Path.GetDirectoryName(fullName);
            String FileNameExt = Path.GetExtension(fullName);

            int cnt = 1;

            int lastIndex = FileNameNoExt.Length - 1;
            for (int i = lastIndex; i >= 1; i--)
            {
                if (!char.IsNumber(FileNameNoExt[i]))
                {
                    lastIndex = i - 1;
                    break;
                }
            }

            int digits = (FileNameNoExt.Length - 1) - lastIndex;

            String FileDigits = FileNameNoExt.Substring(0, FileNameNoExt.Length - digits);

            String FilePathDigits = Path.Combine(FileDir, FileDigits);

            string [] files = System.IO.Directory.GetFiles(FilePathDigits + "*");

            int start = -1;
            int end = -1;

            for (int i = 0; i < files.Length; i++)
            {
                String fName = Path.GetFileNameWithoutExtension(files[i]);

                if (fName.Length < FileDigits.Length) continue;

                fName = fName.Remove(FileDigits.Length - 1);

                int frame = 0;

                if (int.TryParse(fName, out frame))
                {
                    if (start == -1)
                        start = frame;

                    if (start > frame)
                        start = frame;

                    if (end == -1)
                        end = frame;

                    if (end < frame)
                        end = frame;

                    cnt++;
                }
            }

            return cnt;
        }

        private void SaveInDB(FileInfo file, string SHA1, Color color, Color center, byte[] img, string dbName)
        {
            using (DBManager manager = new DBManager())
            {
                if (!manager.HasDB(dbName))
                {
                    manager.CreateDB(dbName);
                }

                int cnt = CountFrames(file.FullName);

                manager.InsertFile(file.FullName, SHA1, color, center, cnt, img, dbName);
            }
        }

        private Image[] FillFrames(string fullName, int cnt)
        {
            String FileNameNoExt = Path.GetFileNameWithoutExtension(fullName);
            String FileDir = Path.GetDirectoryName(fullName);
            String FileNameExt = Path.GetExtension(fullName);

            int lastIndex = FileNameNoExt.Length - 1;
            for (int i = lastIndex; i >= 1; i--)
            {
                if (!char.IsNumber(FileNameNoExt[i]))
                {
                    lastIndex = i - 1;
                    break;
                }
            }

            int digits = (FileNameNoExt.Length - 1) - lastIndex;

            String FileDigits = FileNameNoExt.Substring(0, FileNameNoExt.Length - digits);

            String FilePathDigits = Path.Combine(FileDir, FileDigits);

            string[] files = System.IO.Directory.GetFiles(FilePathDigits + "*");

            SortedList<int, Image> Files = new SortedList<int, Image>();

            for (int i = 0; i < files.Length && i < cnt; i++)
            {
                String fName = Path.GetFileNameWithoutExtension(files[i]);

                if (fName.Length < FileDigits.Length) continue;

                fName = fName.Remove(FileDigits.Length - 1);

                int frame = 0;

                if (int.TryParse(fName, out frame))
                {
                    Files.Add(frame, Image.FromFile(files[i]));
                }
            }

            return Files.Values.ToArray();
        }

        private void BuildAnimation_Click(object sender, EventArgs e)
        {
            System.Globalization.CultureInfo Info = new System.Globalization.CultureInfo("en-us");

            float rate = float.Parse(FrameRate.Text.Replace(',', '.'), Info);

            using (DBManager manager = new DBManager())
            {
                Random Rnd = new Random();

                _Animations.Clear();

                for (int s = 0; s < _DisplayImages.Count - 1; s++)
                {
                    // SELECT FILES
                    ItemData dt = _DisplayImages[s];

                    Bitmap bmp = dt.Img as Bitmap;

                    AnimationSequency seq = new AnimationSequency()
                    {
                        Frames = new List<FrameData>(),
                        ImageArray = new ItemData[bmp.Width, bmp.Height],
                        Primary = dt,
                        Target = _DisplayImages[s + 1]
                    };

                    Color color = seq.Target.color;

                    List<PointF> CanPositions = new List<PointF>();

                    for (int i = 0; i < bmp.Width; i++)
                    {
                        for (int j = 0; j < bmp.Height; j++)
                        {
                            Color c = bmp.GetPixel(i, j);

                            if (c.R == color.R && c.G == color.G && c.B == color.B)
                            {
                                CanPositions.Add(new PointF(i, j));
                            }
                        }
                    }

                    PointF pos = CanPositions[Rnd.Next(CanPositions.Count)];
                    seq.TargetPosition = pos;
                    seq.ImageArray[(int)pos.X, (int)pos.Y] = _DisplayImages[s + 1];

                    // Choosing Images
                    for (int i = 0; i < bmp.Width; i++)
                    {
                        for (int j = 0; j < bmp.Height; j++)
                        {
                            if (i == (int)pos.X && j == (int)pos.Y) continue;

                            Color c = bmp.GetPixel(i, j);

                            String FileName = "";

                            int cnt = 0;

                            Image img = manager.GetImage(DBSet.Text, c, 0.01, out FileName, out cnt);

                            ItemData data = new ItemData()
                            {
                                FileName = FileName,
                                CreatedTime = DateTime.UtcNow,
                                Img = Image.FromFile(FileName),
                                Tex = null,
                                Cnt = cnt,
                                Frames = null
                            };

                            data.Tex = Texture.LoadTexture(data.Img as Bitmap, true, false, true);

                            data.color = Utils.GetColorMap(data.Img as Bitmap);

                            data.Frames = FillFrames(FileName, cnt);

                            seq.ImageArray[i, j] = data;
                        }
                    }

                    int nFrames = (int)Math.Ceiling(rate * (int)ImgDuration.Value);

                    float IDT = 1.0f / (rate * (int)ImgDuration.Value);

                    float DH = 100;

                    for (int f = 0; f < nFrames; f++)
                    {
                        float t = f * IDT;

                        float x = seq.TargetPosition.X * fsmooth(t);
                        float z = seq.TargetPosition.Y * fsmooth(t);
                        float y = DH * fsmooth(t);

                        float a = DH * asmooth(t);
                        float b = DH * bsmooth(t);

                        FrameData data = new FrameData()
                        {
                             Time = t,
                             Position = new OpenTK.Vector3(x, y, z),
                             Alpha = a,
                             Blend = b
                        };

                        seq.Frames.Add(data);
                    }

                    _Animations.Add(seq);
                }
            }
        }

        private float bsmooth(float t)
        {
            return Math.Min(1, t / ((int)ImgDuration.Value / 10));
        }

        private float asmooth(float t)
        {
            return Math.Min(1, t / ((int)ImgDuration.Value / 5));
        }

        private float fsmooth(float t)
        {
            return Math.Min(1, t / ((int)ImgDuration.Value / 1));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            DBSet.Items.Clear();

            using (DBManager manager = new DBManager())
            {
                List<string> list = manager.GetLibraries();

                foreach (String str in list)
                {
                    DBSet.Items.Add(str);
                }

                if (DBSet.Items.Count > 0)
                {
                    DBSet.SelectedIndex = 0;
                }
            }
        }
    }
}
