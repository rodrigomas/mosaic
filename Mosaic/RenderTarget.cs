using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Mosaic
{
    public class RenderTarget : IDisposable
    {
        int width;
        int height;

        uint uiFrameBuffer;
        uint uiRenderTexture;
        uint uiRenderBuffer;

        bool hasDepth = true;

        private bool Disposed = false;
        private int mID = 0;
        private static int NumberFBOs = 0;

        public int Code
        {
            get { return mID; }
        }

        public int ID
        {
            get { return (int)uiRenderTexture; }
        }

        public int Height
        {
            get { return height; }
        }

        public int Width
        {
            get { return width; }
        }

        public RenderTarget(int w, int h, bool useDepth)
        {
            width = w;
            height = h;
            hasDepth = useDepth;
            mID = NumberFBOs++;

            Initialize();
        }

        public void SetCurrentTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, uiRenderTexture);
        }

        public void SetTarget()
        {
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, uiFrameBuffer);

            if (hasDepth)
            {
                GL.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, uiRenderBuffer);
            }
        }

        public void UnsetTarget()
        {
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);

            if (hasDepth)
            {
                GL.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, 0);
            }
        }

        private void Initialize()
        {
            GL.GenFramebuffers(1, out uiFrameBuffer);
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, uiFrameBuffer);

            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt);

            GL.GenTextures(1, out uiRenderTexture);
            GL.BindTexture(TextureTarget.Texture2D, uiRenderTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, System.IntPtr.Zero);

            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, uiRenderTexture, 0);

            //status = GL.CheckFramebufferStatusEXT(GL._FRAMEBUFFER_EXT);

            status = GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt);

            if (hasDepth)
            {
                GL.GenRenderbuffers(1, out uiRenderBuffer);
                GL.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, uiRenderBuffer);
                GL.RenderbufferStorage(RenderbufferTarget.RenderbufferExt, RenderbufferStorage.DepthComponent24, width, height);
                GL.FramebufferRenderbuffer(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, RenderbufferTarget.RenderbufferExt, uiRenderBuffer);
            }

            status = GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt);

            bool t = status == FramebufferErrorCode.FramebufferCompleteExt;

            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;

                if (uiFrameBuffer != 0)
                    GL.DeleteFramebuffers(1, ref uiFrameBuffer);

                if (uiRenderTexture != 0)
                    GL.DeleteTextures(1, ref uiRenderTexture);

                if (uiRenderBuffer != 0)
                    GL.DeleteRenderbuffers(1, ref uiRenderBuffer);
            }
        }

        public System.Drawing.Image GetImage()
        {
            if (OpenTK.Graphics.GraphicsContext.CurrentContext == null)
                throw new OpenTK.Graphics.GraphicsContextMissingException();

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(Width, Height);

            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0,0,Width,Height), 
                    System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            SetCurrentTexture();

            GL.GetTexImage(OpenTK.Graphics.OpenGL.TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, OpenTK.Graphics.OpenGL.PixelType.UnsignedByte, data.Scan0);

            //GL.ReadPixels(0, 0, this.ClientSize.Width, this.ClientSize.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);

            bmp.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

            UnsetTarget();

            return bmp;
        }
    }
}
