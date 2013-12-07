using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Mosaic
{
    /// <summary>
    /// Classe de Representação de uma textura 2D RGBA
    /// </summary>
    public class Texture : IDisposable
    {

        #region Atributos Privados
        /// <summary>
        /// ID da textura na placa gráfica
        /// </summary>
        private int mID = 0;

        /// <summary>
        /// Largura da Textura
        /// </summary>
        private int mWidth = 0;

        /// <summary>
        /// Altura da Textura
        /// </summary>
        private int mHeight = 0;

        /// <summary>
        /// Nome da Textura
        /// </summary>
        private String mName = "";

        /// <summary>
        /// Flag informativa para o sistema de gerência de memória sobre o estado de ocupação da memória de vídeo.
        /// </summary>
        private bool Disposed = false;
        #endregion

        #region Propriedades
        /// <summary>
        /// Nome da Textura
        /// </summary>
        public String Name
        {
            get { return mName; }
            set { mName = value; }
        }

        /// <summary>
        /// ID da textura na placa gráfica
        /// </summary>
        public int Id
        {
            get { return mID; }
        }

        /// <summary>
        /// Largura da Textura
        /// </summary>
        public int Width
        {
            get { return mWidth; }
        }

        /// <summary>
        /// Altura da Textura
        /// </summary>
        public int Height
        {
            get { return mHeight; }
        }
        #endregion

        #region Construtor
        /// <summary>
        /// Contrutor privado
        /// </summary>
        private Texture()
        {

        }
        #endregion

        #region Funções Membro
        /// <summary>
        /// Descarrega a Textura
        /// </summary>
        public void Dispose()
        {
            // Verificando se já foi descarregado
            if (!Disposed)
            {
                // Atualizando estado de descarga
                Disposed = true;

                // Verificando validade da textura
                if (mID != 0)
                {
                    // Removendo
                    GL.DeleteTextures(1, ref mID);
                }
            }
        }

        /// <summary>
        /// Habilita a textura
        /// </summary>
        public void SetCurrent()
        {
            GL.BindTexture(TextureTarget.Texture2D, mID);
        }
        #endregion

        internal void ForceID(int id)
        {
            mID = id;
        }

        #region Funções Estáticas

        /// <summary>
        /// Carrega uma Textura
        /// </summary>
        /// <param name="bmp">Bitmap da Textura</param>
        /// <param name="useMipMaps">Habilita o uso de pirâmide mipmap</param>
        /// <returns>A textura já carregada na placa de vídeo</returns>
        /// <remarks>A textura é adicionada no ResourceManager e caso a mesma já tenha sido alocada, a versão alocada é retornada.</remarks>
        /// <seealso cref="ResourceManager"/>
        public static Texture LoadTexture(System.Drawing.Bitmap bmp, bool useMipMaps)
        {
            // Obtem código hash
            int hash = bmp.GetHashCode();

            // Busca no ResourceManager pela textura
            IDisposable s = ResourceManager.Instance.GetResource(hash.ToString());

            // Se não existir
            if (s == null)
            {
                // Carrega e adiciona
                Texture t = LoadTexture(bmp, useMipMaps, true);
                ResourceManager.Instance.AddResource(hash.ToString(), t);
                return t;
            }
            else // Se existir
            {
                // Retorna
                return s as Texture;
            }
        }

        /// <summary>
        /// Carrega uma Textura
        /// </summary>
        /// <param name="bmp">Bitmap da Textura</param>
        /// <param name="useMipMaps">Habilita o uso de pirâmide mipmap</param>
        /// <param name="repeat">Habilita repetição da textura</param>
        /// <returns>A textura já carregada na placa de vídeo</returns>
        public static Texture LoadTexture(System.Drawing.Bitmap bmp, bool useMipMaps, bool repeat, bool flipY)
        {
            // Cria a textura
            Texture tex = new Texture();

            // Atualiza os atributos
            tex.mWidth = bmp.Width;
            tex.mHeight = bmp.Height;

            // Gira a imagem para ficar no padrão OpenGL (Y positivo no sentido 0,1,0)
            if (flipY)
            {
                bmp.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);
            }
            else
            {
                bmp.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipNone);
            }

            // Criando retangulo de cópia
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);

            // Travando dados
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Salvando o estado do OpenGL
            GL.PushAttrib(AttribMask.TextureBit);

            // Gerando Textura na Placa
            GL.GenTextures(1, out tex.mID);

            // Amarrando Textura
            GL.BindTexture(TextureTarget.Texture2D, tex.mID);

            // Se não usar mipmap, usa filtro linear
            if (!useMipMaps)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            else // Se usar mipmap, cria pirâmide
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                //Glu.gluBuild2DMipmaps(EnableCap.Texture2D, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            // Se for repetir a textura, habilita
            if (repeat)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }

            // Restaurando estado OpenGL
            GL.PopAttrib();

            // Destravando Dados
            bmp.UnlockBits(bmpData);

            // Retornando Textura
            return tex;
        }

        public void UpdateTexture(System.Drawing.Bitmap bmp, bool flipY, bool useMipMaps)
        {
            // Gira a imagem para ficar no padrão OpenGL (Y positivo no sentido 0,1,0)
            if (flipY)
            {
                bmp.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);
            }
            else
            {
                bmp.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipNone);
            }

            // Criando retangulo de cópia
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);

            // Travando dados
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Salvando o estado do OpenGL
            GL.PushAttrib(AttribMask.TextureBit);

            // Amarrando Textura
            GL.BindTexture(TextureTarget.Texture2D, mID);

            if (!useMipMaps)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            else // Se usar mipmap, cria pirâmide
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                //Glu.gluBuild2DMipmaps(EnableCap.Texture2D, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            // Restaurando estado OpenGL
            GL.PopAttrib();

            // Destravando Dados
            bmp.UnlockBits(bmpData);

            mWidth = bmp.Width;
            mHeight = bmp.Height;
        }

        /// <summary>
        /// Carrega uma Textura
        /// </summary>
        /// <param name="bmp">Bitmap da Textura</param>
        /// <param name="useMipMaps">Habilita o uso de pirâmide mipmap</param>
        /// <param name="repeat">Habilita repetição da textura</param>
        /// <returns>A textura já carregada na placa de vídeo</returns>
        private static Texture LoadTexture(System.Drawing.Bitmap bmp, bool useMipMaps, bool repeat)
        {
            return LoadTexture(bmp, useMipMaps, repeat, false);
        }

        /// <summary>
        /// Carrega uma Textura
        /// </summary>
        /// <param name="FileName">Caminho do arquivo</param>
        /// <param name="useMipMaps">Habilita o uso de pirâmide mipmap</param>
        /// <returns>A textura já carregada na placa de vídeo, ou null caso o arquivo não exista.</returns>
        /// <remarks>A textura é adicionada no ResourceManager e caso a mesma já tenha sido alocada, a versão alocada é retornada.</remarks>
        /// <seealso cref="ResourceManager"/>
        public static Texture LoadTexture(string FileName, bool useMipMaps)
        {
            return LoadTexture(FileName, useMipMaps, true, false);
        }

        /// <summary>
        /// Carrega uma Textura
        /// </summary>
        /// <param name="FileName">Caminho do arquivo</param>
        /// <param name="useMipMaps">Habilita o uso de pirâmide mipmap</param>
        /// <returns>A textura já carregada na placa de vídeo, ou null caso o arquivo não exista.</returns>
        /// <remarks>A textura pode ser adicionada no ResourceManager e caso a mesma já tenha sido alocada, a versão alocada é retornada.</remarks>
        /// <seealso cref="ResourceManager"/>
        public static Texture LoadTexture(string FileName, bool useMipMaps, bool repeat, bool flipY)
        {
            // Se o arquivo não existir retorna null
            if (!System.IO.File.Exists(FileName))
            {
                return null;
            }

            // busca pelo arquivo no ResourceManager
            IDisposable s = ResourceManager.Instance.GetResource(FileName);

            // Se não existir 
            if (s == null)
            {
                // Carrega e adiciona
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(FileName);

                Texture ret = LoadTexture(bmp, useMipMaps, repeat, flipY);

                if (bmp != null)
                {
                    bmp.Dispose();
                }

                ResourceManager.Instance.AddResource(FileName, ret);
                return ret;
            }
            else // Se existir 
            {
                // Retorna
                return s as Texture;
            }
        }

        /// <summary>
        /// Carrega uma Textura
        /// </summary>
        /// <param name="FileName">Caminho do arquivo</param>
        /// <param name="useMipMaps">Habilita o uso de pirâmide mipmap</param>
        /// <returns>A textura já carregada na placa de vídeo, ou null caso o arquivo não exista.</returns>
        /// <remarks>A textura pode ser adicionada no ResourceManager e caso a mesma já tenha sido alocada, a versão alocada é retornada.</remarks>
        /// <seealso cref="ResourceManager"/>
        public static Texture LoadTextureNoResource(string FileName, bool useMipMaps, bool repeat, bool flipY)
        {
            // Se o arquivo não existir retorna null
            if (!System.IO.File.Exists(FileName))
            {
                return null;
            }

            // Carrega e adiciona
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(FileName);

            Texture ret = LoadTexture(bmp, useMipMaps, repeat, flipY);

            if (bmp != null)
            {
                bmp.Dispose();
            }

            return ret;
        }
        #endregion


        internal static Texture LoadTexture(short[] img, int Width, int Height, bool useMipMaps, bool repeat)
        {
            // Cria a textura
            Texture tex = new Texture();

            // Atualiza os atributos
            tex.mWidth = Width;
            tex.mHeight = Height;

            // Salvando o estado do OpenGL
            GL.PushAttrib(AttribMask.TextureBit);

            // Gerando Textura na Placa
            GL.GenTextures(1, out tex.mID);

            // Amarrando Textura
            GL.BindTexture(TextureTarget.Texture2D, tex.mID);

            // Se não usar mipmap, usa filtro linear
            if (!useMipMaps)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Short, img);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            else // Se usar mipmap, cria pirâmide
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Short, img);
                //Glu.gluBuild2DMipmaps(EnableCap.Texture2D, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            // Se for repetir a textura, habilita
            if (repeat)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }

            // Restaurando estado OpenGL
            GL.PopAttrib();

            // Retornando Textura
            return tex;
        }

        internal void UpdateTexture(short[] img, int Width, int Height, bool useMipMaps)
        {
            // Salvando o estado do OpenGL
            GL.PushAttrib(AttribMask.TextureBit);

            // Amarrando Textura
            GL.BindTexture(TextureTarget.Texture2D, mID);

            if (!useMipMaps)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Short, img);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            else // Se usar mipmap, cria pirâmide
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Short, img);
                //Glu.gluBuild2DMipmaps(EnableCap.Texture2D, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            // Restaurando estado OpenGL
            GL.PopAttrib();
        }

        internal static Texture LoadTexture(byte[] img, int Width, int Height, bool useMipMaps, bool repeat)
        {
            // Cria a textura
            Texture tex = new Texture();

            // Atualiza os atributos
            tex.mWidth = Width;
            tex.mHeight = Height;

            // Salvando o estado do OpenGL
            GL.PushAttrib(AttribMask.TextureBit);

            // Gerando Textura na Placa
            GL.GenTextures(1, out tex.mID);

            // Amarrando Textura
            GL.BindTexture(TextureTarget.Texture2D, tex.mID);

            // Se não usar mipmap, usa filtro linear
            if (!useMipMaps)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, img);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            else // Se usar mipmap, cria pirâmide
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, img);
                //Glu.gluBuild2DMipmaps(EnableCap.Texture2D, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            // Se for repetir a textura, habilita
            if (repeat)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }

            // Restaurando estado OpenGL
            GL.PopAttrib();

            // Retornando Textura
            return tex;
        }

        internal void UpdateTexture(byte[] img, int Width, int Height, bool useMipMaps)
        {
            // Salvando o estado do OpenGL
            GL.PushAttrib(AttribMask.TextureBit);

            // Amarrando Textura
            GL.BindTexture(TextureTarget.Texture2D, mID);

            if (!useMipMaps)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, img);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            else // Se usar mipmap, cria pirâmide
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, img);
                //Glu.gluBuild2DMipmaps(EnableCap.Texture2D, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            // Restaurando estado OpenGL
            GL.PopAttrib();
        }
    }
}
