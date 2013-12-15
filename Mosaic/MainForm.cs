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
using OpenTK;
using System.Drawing.Imaging;

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

            public Image Img { get; set; }

            public Texture Tex { get; set; }
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

        private float H0;

        private int CurrentIdx = 0;
        private int CurrentFrame = 0;

        private bool Rendering = false;

        private RenderTarget _Target = null;

        float H = 200;

        float TempBlend = 0;

        Texture TempTex = null;

        private Effect BlendEffect = null;

        private void Renderer_Paint(object sender, PaintEventArgs e)
        {
            //if (H < 0) H = 200;

            Renderer.MakeCurrent();
            
           /* int w = 640;
            int h = 480;

            H = (int)ImgDuration.Value;

            TempBlend = Math.Max(0.0f, Math.Min(1.0f, H / 500.0f)); 

            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.PushAttrib(AttribMask.EnableBit);

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.AlphaTest);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            float aspect = w / (float)(h);            

            Matrix4 Perspective = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), aspect, 1f, 1000.0f);
            GL.LoadMatrix(ref Perspective);
            
            //double pw = 2 * 1.0f * Math.Tan(Math.PI / 8);            

            //float W = H / 1.0f * ((float)pw);

            //GL.Frustum(0, w, 0, h, -1, 10000);
            //GL.Ortho(0, w, 0, h, -1, 100);

            GL.Viewport(0, 0, w, h);            

            GL.MatrixMode(MatrixMode.Modelview);

            GL.LoadIdentity();

            GL.Enable(EnableCap.Texture2D);

            //Matrix4 LookAt = Matrix4.LookAt(frame.Position.X, frame.Position.Y, frame.Position.Z, frame.Position.X, frame.Position.Y + 1.0f, frame.Position.Z, 0, 0, 1);
            //Matrix4 LookAt = Matrix4.LookAt(0, 0, -H * 640 / W, 0, 0, 0, 0, 1, 0);
            Matrix4 LookAt = Matrix4.LookAt(0, 0, -H, 0, 0, 0, 0, 1, 0);

            GL.LoadMatrix(ref LookAt);

            GL.ActiveTexture(TextureUnit.Texture0);            

            //GL.Scale(W / 640, W * aspect / 480, 1);
            GL.Translate(-w / 2.0f, -h / 2.0f, 0);
           //
            if (_DisplayImages.Count != 0)
            {
                BlendEffect.Begin();
                BlendEffect.setValue("ImageMap", 0);
                BlendEffect.setValue("BlendFactor", TempBlend);                

                Bitmap bmp = (_DisplayImages[0].Img as Bitmap);                

                BitmapData BmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                unsafe
                {
                    byte* p = (byte*)BmpData.Scan0.ToPointer();

                    for (int i = 0; i < bmp.Width; i++)
                    {
                        for (int j = 0; j < bmp.Height; j++)
                        {
                            byte* cp = p + j * BmpData.Stride + 4 * i;

                            Color c = Color.FromArgb(cp[2], cp[1], cp[0]);

                            //ItemData IT = seq.ImageArray[x, y];

                            /*if (IT.FileName != "NO_IMAGE")
                            {
                                IT.Img = Image.FromFile(IT.FileName);

                                //IT.Frames = FillFrames(IT.FileName, IT.Cnt);
                            }

                            IT.Tex = Texture.LoadTexture(IT.Img as Bitmap, true, false, true);

                            //  IT.color = Utils.GetColorMap(IT.Img as Bitmap); 

                            if (IT.Cnt > 1)
                            {
                                IT.Tex.UpdateTexture(IT.Frames[CurrentFrame % IT.Cnt] as Bitmap, true, true);
                            }

                            IT.Tex.SetCurrent();* /

                            TempTex.SetCurrent();

                            GL.Color4(c);

                            GL.PushMatrix();

                            //GL.Translate(i, j, 0);                            
                            GL.Translate(i, j * aspect, 0);
                            GL.Scale(1, aspect, 1);

                            GL.Begin(BeginMode.Quads);
                            GL.TexCoord2(0, 0);
                            GL.Vertex2(0, 0);

                            GL.TexCoord2(1, 0);
                            GL.Vertex2(1, 0);

                            GL.TexCoord2(1, 1);
                            GL.Vertex2(1, 1);

                            GL.TexCoord2(0, 1);
                            GL.Vertex2(0, 1);
                            GL.End();

                            GL.PopMatrix();


                            //if (IT.FileName != "NO_IMAGE")
                            //{
                            //    IT.Img.Dispose();
                            //    DisposeAll(IT.Frames);
                            //}

                            //IT.Tex.Dispose();
                        }
                    }
                }

                bmp.UnlockBits(BmpData);

                BlendEffect.End();
            }

            Renderer.SwapBuffers();
            */

            double cnt = (DateTime.Now - Start).TotalMilliseconds;

            double delta = (DateTime.Now - Delta).TotalMilliseconds;

            Delta = DateTime.Now;

            if (!GLContextLoaded) return;

            Renderer.MakeCurrent();

            if (Rendering && CurrentIdx < _Animations.Count && CurrentFrame < _Animations[CurrentIdx].Frames.Count)
            {
                LastImage = _Animations[CurrentIdx].Frames[CurrentFrame].Tex;
            }

            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.PushAttrib(AttribMask.EnableBit);

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.AlphaTest);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);           
           
            
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Renderer.Width, 0, Renderer.Height, -1, 1);
            GL.Viewport(0, 0, Renderer.Width, Renderer.Height);
                
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Disable(EnableCap.Texture2D);

            GL.Enable(EnableCap.Texture2D);
            GL.ActiveTexture(TextureUnit.Texture0);

            if (LastImage != null)
            {
                LastImage.SetCurrent();
            }

            GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);

            GL.PushMatrix();

            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(0, 0);
            GL.Vertex2(0, 0);

            GL.TexCoord2(1, 0);
            GL.Vertex2(0 + _Target.Width, 0);

            GL.TexCoord2(1, 1);
            GL.Vertex2(_Target.Width, _Target.Height);

            GL.TexCoord2(0, 1);
            GL.Vertex2(0, _Target.Height);

            GL.End();

            GL.PopMatrix();
            GL.PopAttrib();

            Renderer.SwapBuffers();
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
            Renderer.MakeCurrent();

            if (TempTex == null)
            {
                TempTex = Texture.LoadTexture(Properties.Resources.NO_IMAGE.GetThumbnailImage((int)renderWidth.Value, (int)renderHeight.Value, null, IntPtr.Zero) as Bitmap, true, true, true);
            }

            if (BlendEffect == null)
            {
                BlendEffect = Effect.FromFile("Colorize.glsl");
            }

            _Target = new RenderTarget((int)renderWidth.Value, (int)renderHeight.Value, true);
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

                ImgList.Images.RemoveByKey(dt.FileName);

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
                    if (ImgList.Images.ContainsKey(dialog.FileName))
                        return;

                    ListViewItem it = ListImage.Items.Add(Path.GetFileName(dialog.FileName));

                    Bitmap bmp = Image.FromFile(dialog.FileName) as Bitmap;

                    if (bmp.Width != (int)renderWidth.Value || bmp.Height != (int)renderHeight.Value)
                    {
                        bmp = Rescale(bmp, (int)renderWidth.Value, (int)renderHeight.Value);
                    }

                    ItemData data = new ItemData()
                    {
                        FileName = dialog.FileName,
                        CreatedTime = DateTime.UtcNow,
                        Img = bmp,
                        Tex = null
                    };

                    ImgList.Images.Add(data.FileName, bmp.GetThumbnailImage(ImgList.ImageSize.Width, ImgList.ImageSize.Height, null, IntPtr.Zero));

                    it.ImageKey = data.FileName;                 

                    data.Tex = Texture.LoadTexture(data.Img as Bitmap, true, false, true);

                    data.color = Utils.GetColorMap(data.Img as Bitmap);

                    it.Tag = data;

                    _DisplayImages.Add(data);
                }
            }
        }

        private Bitmap Rescale(Bitmap bmp, int w, int h)
        {
            Bitmap bmp2 = new Bitmap(w,h);

            using (Graphics g = Graphics.FromImage(bmp2))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.DrawImage(bmp, 0, 0, w, h);
            }

            return bmp2;
        }

        private bool VideoRead = false;

        private void CreateDB_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    VideoRead = chkVideo.Checked;

                    String Folderf = dialog.SelectedPath;

                    ProgressDialog pb = new ProgressDialog();

                    String dbName = DBSet.Text;
                    bool Recursive = chkRecursive.Checked;

                    BackgroundWorker worker = new BackgroundWorker() { WorkerReportsProgress = true };

                    worker.ProgressChanged += (s, ev) =>
                        {
                            if(pb.Visible == false)
                                pb.Show();

                            pb.Progress.Value = Math.Min(100, ev.ProgressPercentage);
                            pb.Text = ev.UserState.ToString();
                            pb.ProgressLabel.Text = ev.UserState.ToString();
                        };

                    worker.RunWorkerCompleted += (s, r) =>
                        {
                            pb.Dispose();
                            worker.Dispose();

                            MainForm_Load(s, r);
                        };

                    worker.DoWork += (s, ev) =>
                        {
                            worker.ReportProgress(0, "Computing files...");

                            Queue<DirectoryInfo> Dirs = new Queue<DirectoryInfo>();

                            int cnt = 0;

                            DirectoryInfo Info = new DirectoryInfo(Folderf);

                            Dirs.Enqueue(Info);

                            while (Dirs.Count != 0)
                            {
                                DirectoryInfo dir = Dirs.Dequeue();

                                if (Recursive)
                                {
                                    foreach (DirectoryInfo dir2 in dir.GetDirectories())
                                    {
                                        Dirs.Enqueue(dir2);
                                    }
                                }

                                cnt += dir.GetFiles("*.jpg").Length;
                            }

                            if(cnt == 0) return;

                            int worked = 0;

                            worker.ReportProgress(0, String.Format("Processing {0} Files ...", cnt));

                            Dirs.Clear();
                            Dirs.Enqueue(Info);
                            while (Dirs.Count != 0)
                            {
                                DirectoryInfo dir = Dirs.Dequeue();

                                if (Recursive)
                                {
                                    foreach (DirectoryInfo dir2 in dir.GetDirectories())
                                    {
                                        Dirs.Enqueue(dir2);
                                    }
                                }

                                foreach (FileInfo file in dir.GetFiles("*.jpg"))
                                {
                                    int pr = (int)( (worked * 100.0) / cnt);

                                    worker.ReportProgress(pr, String.Format("Processing {1}/{0} ({2}%) - {3}", cnt, worked, pr, file.Name));

                                    String SHA1 = "";
                                    using (Stream st = file.OpenRead())
                                    {
                                        SHA1 = Utils.GetSHA1Hash(st);
                                    }

                                    Color[] color;

                                    Color center;

                                    using (Image bmp = Image.FromFile(file.FullName))
                                    {
                                        color = Utils.GetColorMapArray(bmp as Bitmap);

                                        center = (bmp as Bitmap).GetPixel(bmp.Width / 2, bmp.Height / 2);

                                        //SaveInDB(file, SHA1, color, center, Utils.ImageToByte(bmp), dbName);
                                        SaveInDB(file, SHA1, color, center, null, dbName);
                                    }

                                    worked++;                                    
                                }    
                                
                            }

                            DBManager.Save();
                        };                    

                    worker.RunWorkerAsync();
                }
            }
        }

        private int CountFrames(String fullName)
        {
            String FileNameNoExt = Path.GetFileNameWithoutExtension(fullName);
            String FileDir = Path.GetDirectoryName(fullName);
            String FileNameExt = Path.GetExtension(fullName);

            int cnt = 1;

            if (!VideoRead)
            {
                return (File.Exists(fullName)) ? 1 : 0;
            }

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

            string[] files = System.IO.Directory.GetFiles(FileDir);

            int start = -1;
            int end = -1;

            for (int i = 0; i < files.Length; i++)
            {
                if (Path.GetExtension(files[i]) != ".jpg") continue;
                
                String fName = Path.GetFileNameWithoutExtension(files[i]);

                if (!fName.StartsWith(FileDigits)) continue;

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

        private void SaveInDB(FileInfo file, string SHA1, Color[] color, Color center, byte[] img, string dbName)
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

            string[] files = System.IO.Directory.GetFiles(FileDir);

            SortedList<int, Image> Files = new SortedList<int, Image>();

            for (int i = 0; i < files.Length && i < cnt; i++)
            {
                if (Path.GetExtension(files[i]) != ".jpg") continue;

                String fName = Path.GetFileNameWithoutExtension(files[i]);

                if (!fName.StartsWith(FileDigits)) continue;

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

        String LastInput = "";

        private void BuildAnimation_Click(object sender, EventArgs e)
        {
            System.Globalization.CultureInfo Info = new System.Globalization.CultureInfo("en-us");

            float rate = float.Parse(FrameRate.Text.Replace(',', '.'), Info);

            String dbName = DBSet.Text;

            int ImageDuration = (int)ImgDuration.Value;

            int w = (int)renderWidth.Value;
            int h = (int)renderHeight.Value;

            using (DBManager manager = new DBManager())
            {
                Random Rnd = new Random();

                manager.ResetImages();

                _Animations.Clear();

                for (int s = 0; s < _DisplayImages.Count - 1; s++)
                {
                    // SELECT FILES
                    ItemData dt = _DisplayImages[s];

                    Bitmap bmp = (dt.Img as Bitmap).GetThumbnailImage(w, h, null, IntPtr.Zero) as Bitmap;

                    AnimationSequency seq = new AnimationSequency()
                    {
                        Frames = new List<FrameData>(),
                        ImageArray = new ItemData[bmp.Width, bmp.Height],
                        Primary = dt,
                        Target = _DisplayImages[s + 1]
                    };

                    Color color = seq.Target.color;

                    List<PointF> CanPositions = new List<PointF>();

                    BitmapData BmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    unsafe
                    {
                        byte* p = (byte*)BmpData.Scan0.ToPointer();

                        for (int i = 0; i < bmp.Width; i++)
                        {
                            for (int j = 0; j < bmp.Height; j++)
                            {
                                byte* cp = p + j * BmpData.Stride + 3 * i;

                                Color c = Color.FromArgb(cp[0], cp[1], cp[2]);

                                float dr = (c.R - color.R) / 255.0f;
                                float dg = (c.G - color.G) / 255.0f;
                                float db = (c.B - color.B) / 255.0f;

                                double d = Math.Sqrt(dr * dr + dg * dg + db * db);

                                if (d < 0.12)
                                {
                                    CanPositions.Add(new PointF(i, j));
                                }
                            }
                        }
                    }

                    bmp.UnlockBits(BmpData);

                    if (File.Exists("cache.tmp") && LastInput == seq.Primary.FileName)
                    {
                        seq.ImageArray = LoadSelection();
                        LastInput = seq.Primary.FileName;
                    }

                    PointF pos = CanPositions[Rnd.Next(CanPositions.Count)];
                    seq.TargetPosition = pos;
                    seq.ImageArray[(int)pos.X, (int)pos.Y] = _DisplayImages[s + 1];

                    BmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    ItemData nullitem = new ItemData()
                                    {
                                        FileName = "NO_IMAGE",
                                        CreatedTime = DateTime.UtcNow,
                                        Img = Properties.Resources.NO_IMAGE.GetThumbnailImage(w, h, null, IntPtr.Zero),
                                        Tex = null,
                                        Cnt = 1,
                                        Frames = null
                                    };

                    bool random = chkRandom.Checked;

                    if (!File.Exists("cache.tmp") || LastInput != seq.Primary.FileName)
                    {
                        unsafe
                        {
                            int ww = bmp.Width;
                            int hh = bmp.Height;

                            byte* p = (byte*)BmpData.Scan0.ToPointer();

                            // Choosing Images
                            //for (int i = 0; i < bmp.Width; i++)
                            Parallel.For(0, ww, (i) =>
                            {
                                for (int j = 0; j < hh; j++)
                                {
                                    if (i == (int)pos.X && j == (int)pos.Y) continue;

                                    //Color c = bmp.GetPixel(i, j);
                                    byte* cp = p + j * BmpData.Stride + 3 * i;

                                    Color c = Color.FromArgb(cp[0], cp[1], cp[2]);

                                    String FileName = "";

                                    int cnt = 0;

                                    ItemData data = null;

                                    if (random)
                                    {
                                        manager.GetImageRND(dbName, c, out FileName, out cnt);
                                    }
                                    else
                                    {
                                        manager.GetImage(dbName, c, 0.25, out FileName, out cnt);

                                        if (String.IsNullOrEmpty(FileName))
                                        {
                                            manager.GetImageRND(dbName, c, out FileName, out cnt);
                                        }
                                    }                                    

                                    // Bitmap idf = Image.FromFile(FileName) as Bitmap;

                                    //  Image LImage = (idf).GetThumbnailImage(w, h, null, IntPtr.Zero);

                                    data = new ItemData()
                                    {
                                        FileName = FileName,
                                        CreatedTime = DateTime.UtcNow,
                                        Img = null,
                                        Tex = null,
                                        Cnt = cnt,
                                        Frames = null
                                    };

                                    //  idf.Dispose();

                                    //data.Frames = FillFrames(FileName, cnt);


                                    //   data.Tex = Texture.LoadTexture(data.Img as Bitmap, true, false, true);

                                    //  data.color = Utils.GetColorMap(data.Img as Bitmap);                                

                                    seq.ImageArray[i, j] = data;
                                }
                            });

                        }
                        bmp.UnlockBits(BmpData);
                    }

                    SaveSelection(seq.ImageArray);

                    int nFrames = (int)Math.Ceiling(rate * ImageDuration);

                    //float IDT = 1.0f / (rate * ImageDuration);

                    //float DH = 500;
   

                    RectangleF View = new RectangleF(0, 0, w, h);

                    Parallel.For(0, nFrames, (f) =>
                    //for (int f = 0; f < nFrames; f++)
                    {
                        ///float t = f * IDT;

                        //                        float x = seq.TargetPosition.X * fsmooth(t);
                        //                      float z = DH * (1 - fsmooth(t));
                        //                    float y = seq.TargetPosition.Y * fsmooth(t);

                        ///float a = DH * asmooth(t);
                        //float b = DH * bsmooth(t);

                        /*FrameData data = new FrameData()
                        {
                             Time = t,
                             Position = new OpenTK.Vector3(0, 0, 0),
                             Alpha = a,
                             Blend = 0
                        };*/

                        //Image img = Render(seq, f, data, w, h);

                        //data.Img = img;


                        ColorMatrix matrix = new ColorMatrix();

                        //create image attributes  
                        ImageAttributes attributes = new ImageAttributes();   

                        AForge.Imaging.Filters.HueModifier Filter = new AForge.Imaging.Filters.HueModifier();

                        float a = f / (float)(nFrames);

                        float aspect = w / (float)(h);

                        Bitmap img = new Bitmap(w, h);

                        Bitmap prim = null;

                        using (Image ximg = Image.FromFile(seq.Primary.FileName))
                        {
                            prim = ximg.GetThumbnailImage(w, h, null, IntPtr.Zero) as Bitmap;
                        }

                        using (Graphics g = Graphics.FromImage(img))
                        {
                            if (f == 0)
                            {
                                g.DrawImage(prim, 0, 0, w, h);
                            }
                            else if (f == nFrames - 1)
                            {
                                using (Image ximg = Image.FromFile(seq.Target.FileName))
                                {
                                    using (Image img2 = ximg.GetThumbnailImage(w, h, null, IntPtr.Zero))
                                    {
                                        g.DrawImage(img2, 0, 0, w, h);
                                    }
                                }

                            }
                            else
                            {
                                //Parallel.For(0, w, (x) =>
                                for (int x = 0; x < w; x++)
                                {
                                    if (x % 10 == 0) GC.Collect();

                                    for (int y = 0; y < h; y++)
                                    {
                                        float tx = pos.X * a * w - w / 2 * (1 - a);
                                        float ty = pos.Y * a * h - h / 2 * (1 - a);

                                        // DEFINE SCALE
                                        float scalex = a;
                                        float scaley = a;

                                        // CULLING
                                        if (!IsVisible(x, y, w, h, tx, ty, a, View))
                                            continue;

                                        Color c = prim.GetPixel(x, y);

                                        ItemData IT = seq.ImageArray[x, y];

                                        float X, Y;

                                        X = x * scalex * w - tx;
                                        Y = y * scaley * h - ty;

                                        if (IT.FileName != "NO_IMAGE")
                                        {
                                            g.FillRectangle(new SolidBrush(c), new RectangleF(X, Y, (w * scalex), (h * scaley)));

                                            using (Image bmp2x = Image.FromFile(IT.FileName))
                                            {
                                                using (IT.Img = bmp2x.GetThumbnailImage(w, h, null, IntPtr.Zero))
                                                {
                                                    //IT.Frames = FillFrames(IT.FileName, IT.Cnt);          
                                                    Bitmap bmp2 = (bmp2x as Bitmap);

                                                    if (f < (nFrames * 0.25f))
                                                    {
                                                        matrix.Matrix33 = f / (nFrames * 0.25f);
                                                        attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                                                       /* Filter.Hue = (int)c.GetHue();

                                                        if (f < (nFrames * 0.10f))
                                                        {
                                                            bmp2 = Filter.Apply(bmp2x as Bitmap);
                                                            g.DrawImage(bmp2, new Rectangle((int)X, (int)Y, (int)(w * scalex), (int)(h * scaley)), 0, 0, w, h, GraphicsUnit.Pixel, attributes);
                                                            bmp2.Dispose();
                                                        }
                                                        else*/
                                                        {
                                                            g.DrawImage(bmp2, new Rectangle((int)X, (int)Y, (int)(w * scalex), (int)(h * scaley)), 0, 0, w, h, GraphicsUnit.Pixel, attributes);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        g.DrawImage(bmp2x, new RectangleF(X, Y, (w * scalex), (h * scaley)), new RectangleF(0, 0, w, h), GraphicsUnit.Pixel);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            g.FillRectangle(new SolidBrush(c), new RectangleF(X, Y, (w * scalex), (h * scaley)));
                                        }
                                    }
                                }//);
                            }
                        }

                        //  data.Tex = Texture.LoadTexture(img as Bitmap, true, false, true);

                        //seq.Frames.Add(data);

                        img.Save(Path.Combine(OutputFolder, String.Format("Export_{0}_{1}.png", s, f)));
                    });

                    _Animations.Add(seq);
                }
            }
        }

        private void SaveSelection(ItemData[,] itemData)
        {
            using (StreamWriter wr = new StreamWriter("cache.tmp"))
            {
                wr.WriteLine(itemData.GetLength(0).ToString() + "," + itemData.GetLength(1).ToString());

                for (int i = 0; i < itemData.GetLength(0); i++)
                {
                    for (int j = 0; j < itemData.GetLength(1); j++)
                    {
                        wr.Write(itemData[i, j].FileName + ";");
                    }

                    wr.WriteLine();
                }
            }
        }

        private ItemData[,] LoadSelection()
        {
            ItemData[,] itemData = null;


            using (StreamReader wr = new StreamReader("cache.tmp"))
            {

                String dt = wr.ReadLine();

                String[] pts = dt.Split(',');
                
                itemData = new ItemData[int.Parse(pts[0]), int.Parse(pts[1])];

                for (int i = 0; i < itemData.GetLength(0); i++)
                {
                    dt = wr.ReadLine();
                    pts = dt.Split(';');

                    for (int j = 0; j < itemData.GetLength(1); j++)
                    {
                        itemData[i, j] = new ItemData();
                        itemData[i, j].FileName = pts[j];
                    }
                }
            }

            return itemData;
        }        

        private bool IsVisible(int x, int y, int w, int h, float tx, float ty, float a, RectangleF View)
        {
            RectangleF f0 = new RectangleF(x * w * a - tx, y * h * a - ty, (w * a), (h * a));

            return View.IntersectsWith(f0);
        }

        private int THUMB_WIDTH = 512;
        private int THUMB_HEIGHT = 512;

        private Texture RenderTexture = null;

        private Image Render(AnimationSequency seq, int f, FrameData frame, int w, int h)
        {
            Renderer.MakeCurrent();

            _Target.SetTarget();

            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);            

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.PushAttrib(AttribMask.EnableBit);

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.AlphaTest);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);            

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            float aspect = w / (float)(h);

            Matrix4 Perspective = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), aspect, 1f, 1000.0f);
            GL.LoadMatrix(ref Perspective);

            GL.Viewport(0, 0, w, h);

            GL.MatrixMode(MatrixMode.Modelview);

            GL.LoadIdentity();

            GL.Enable(EnableCap.Texture2D);

            //Matrix4 LookAt = Matrix4.LookAt(frame.Position.X, frame.Position.Y, frame.Position.Z, frame.Position.X, frame.Position.Y + 1.0f, frame.Position.Z, 0, 0, 1);
            Matrix4 LookAt = Matrix4.LookAt(0, frame.Position.Y, 0, 0, 0, 0, 0, 1, 0);
            //Matrix4 LookAt = Matrix4.LookAt(0, 0, -10, 0, 0, 0, 0, 1, 0);

            GL.LoadMatrix(ref LookAt);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Translate(-w / 2.0f, -h / 2.0f, 0);

            BlendEffect.Begin();
            BlendEffect.setValue("ImageMap", 0);
            BlendEffect.setValue("BlendFactor", frame.Blend); 

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Color c = (seq.Primary.Img as Bitmap).GetPixel(x, y);

                    ItemData IT = seq.ImageArray[x, y];

                    if (IT.FileName != "NO_IMAGE")
                    {
                        IT.Img = Image.FromFile(IT.FileName).GetThumbnailImage(w, h, null, IntPtr.Zero);

                        //IT.Frames = FillFrames(IT.FileName, IT.Cnt);
                    }

                    if( RenderTexture == null )
                    {
                        RenderTexture = Texture.LoadTexture(IT.Img as Bitmap, false, false, true);
                    } else 
                    {
                        RenderTexture.UpdateTexture(IT.Img as Bitmap, false, false);
                    }

                    //IT.Tex = Texture.LoadTexture(IT.Img as Bitmap, true, false, true);

                    //  IT.color = Utils.GetColorMap(IT.Img as Bitmap); 

                    //if (IT.Cnt > 1)
                    //{
                    //    IT.Tex.UpdateTexture(IT.Frames[CurrentFrame % IT.Cnt] as Bitmap, true, true);
                    //}

                    //IT.Tex.SetCurrent();

                    RenderTexture.SetCurrent();

                    GL.Color4(c);

                    GL.PushMatrix();

                    //GL.Translate(x, y * aspect, 0);
                    //GL.Translate(x, y, 0);
                    GL.Translate(x, y * aspect, 0);
                    GL.Scale(1, aspect, 1);

                    GL.Begin(BeginMode.Quads);

                    GL.TexCoord2(0, 0);
                    GL.Vertex2(0, 0);

                    GL.TexCoord2(1, 0);
                    GL.Vertex2(1, 0);

                    GL.TexCoord2(1, 1);
                    GL.Vertex2(1, 1);

                    GL.TexCoord2(0, 1);
                    GL.Vertex2(0, 1);

                    GL.End();

                    GL.PopMatrix();

                    if (IT.FileName != "NO_IMAGE")
                    {
                        IT.Img.Dispose();
                        DisposeAll(IT.Frames);
                    }

                    //IT.Tex.Dispose();
                }
            }

            Renderer.SwapBuffers();

            _Target.UnsetTarget();

            return _Target.GetImage();
        }

        private void DisposeAll(Image[] image)
        {
            if (image != null)
            {
                foreach (Image img in image)
                {
                    img.Dispose();
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

        private void ListImage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ListImage.SelectedItems.Count > 0)
            {
                ItemData data = ListImage.SelectedItems[0].Tag as ItemData;

                LastImage = data.Tex;
            }
            else
            {
                LastImage = null;
            }
        }
    }
}
