using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Windows.Forms;

namespace Mosaic
{
    class Effect : IDisposable
    {
        private int iProgram = 0;

        private bool Disposed = false;

        private Dictionary<String, int> mShaderKeys = new Dictionary<string, int>();

        private Effect()
        {

        }

        public void Begin()
        {
            GL.UseProgram(iProgram);
        }

        public void End()
        {
            GL.UseProgram(0);
        }

        public static Effect FromFile(string ShaderFile)
        {

            if (!System.IO.File.Exists(ShaderFile))
            {
                return null;
            }

            IDisposable s = ResourceManager.Instance.GetResource(ShaderFile);

            if (s == null)
            {
                Effect ef = new Effect();

                ef.iProgram = LoadShader(ShaderFile);

                if (ef.iProgram < 0)
                {
                    return null;
                }

                ResourceManager.Instance.AddResource(ShaderFile, ef);
                return ef;
            }
            else
            {
                return s as Effect;
            }


        }

        private int getUniform(String Name)
        {
            if (mShaderKeys.ContainsKey(Name))
            {
                return mShaderKeys[Name];
            }
            else
            {
                int uni = GL.GetUniformLocation(iProgram, Name);
                mShaderKeys.Add(Name, uni);
                return uni;
            }
        }

        public void setValue(String Name, Vector3 val)
        {
            int var = getUniform(Name);

            GL.Uniform3(var, val.X, val.Y, val.Z);
        }

        public void setValue(String Name, float[] val)
        {
            int var = getUniform(Name);

            switch (val.Length)
            {
                case 1:
                    GL.Uniform1(var, 1, val);
                    break;
                case 2:
                    GL.Uniform2(var, 1, val);
                    break;
                case 3:
                    GL.Uniform3(var, 1, val);
                    break;
                case 4:
                    GL.Uniform4(var, 1, val);
                    break;
                default:
                    GL.UniformMatrix4(var, 1, false, val);
                    break;
            }
        }

        public void setValue(String Name, float[] val, int n)
        {
            int var = getUniform(Name);

            int type = val.Length / n;

            switch (type)
            {
                case 1:
                    GL.Uniform1(var, n, val);
                    break;

                case 2:
                    GL.Uniform2(var, n, val);
                    break;

                case 3:
                    GL.Uniform3(var, n, val);
                    break;

                case 4:
                    GL.Uniform4(var, n, val);
                    break;

                default:
                    GL.UniformMatrix4(var, n, false, val);
                    break;
            }
        }

        public void setValue(String Name, float val)
        {
            int var = getUniform(Name);

            GL.Uniform1(var, val);
        }

        public void setValue(String Name, int val)
        {
            int var = getUniform(Name);

            GL.Uniform1(var, val);
        }

        private static int LoadShader(string ShaderFile)
        {
            System.IO.StreamReader streamReader = new System.IO.StreamReader(ShaderFile);
            string shaderstring = streamReader.ReadToEnd();
            streamReader.Close();

            int variables = shaderstring.IndexOf("[Variables]");
            int vshaderi = shaderstring.IndexOf("[Vertex]");
            int fshaderi = shaderstring.IndexOf("[Fragment]");

            string variablesstring = shaderstring.Substring(variables + "[Variables]".Length, vshaderi - variables - "[Variables]".Length);
            string vertexshaderstring = shaderstring.Substring(vshaderi + "[Vertex]".Length, fshaderi - vshaderi - "[Vertex]".Length);
            string fragmentshaderstring = shaderstring.Substring(fshaderi + "[Fragment]".Length, shaderstring.Length - fshaderi - "[Fragment]".Length);

            vertexshaderstring = variablesstring + "\n" + vertexshaderstring;
            fragmentshaderstring = variablesstring + "\n" + fragmentshaderstring;

            return LoadResourceShader(vertexshaderstring, fragmentshaderstring);
        }

        private static void ShowShaderLog(int shader)
        {
            int tamanho = 0;	// tamanho do infolog
            int total = 0;		// total de caracteres escritos na string
            StringBuilder infolog;		// infolog

            // Obtém tamanho do infolog
            GL.GetShader(shader, ShaderParameter.InfoLogLength, out tamanho);

            // Se há algo no infolog...
            if (tamanho > 1)
            {
                // Aloca memória e obtém o infolog
                infolog = new StringBuilder(tamanho);

                GL.GetShaderInfoLog(shader, tamanho, out total, infolog);

                MessageBox.Show(infolog.ToString());
            }
        }

        private static void ShowProgramLog(int obj)
        {
            int infologLength = 0;
            int charsWritten = 0;
            StringBuilder infoLog;

            GL.GetProgram(obj, ProgramParameter.InfoLogLength, out infologLength);

            if (infologLength > 1)
            {
                infoLog = new StringBuilder(infologLength);
                GL.GetProgramInfoLog(obj, infologLength, out charsWritten, infoLog);

                MessageBox.Show(infoLog.ToString());
            }
        }

        private static int LoadResourceShader(string vertexShaderString, string fragmentShaderString)
        {
            int vertexLength = 0;
            int fragmentLength = 0;
            string[] vertexShader = new string[1];
            string[] fragmentShader = new string[1];

            vertexShader[0] = vertexShaderString;
            fragmentShader[0] = fragmentShaderString;

            vertexLength = vertexShaderString.Length;
            fragmentLength = fragmentShaderString.Length;

            int shaderProgram = GL.CreateProgram();
            int vertexShaderObject = GL.CreateShader(ShaderType.VertexShader);
            int fragmentShaderObject = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vertexShaderObject, 1, vertexShader, ref vertexLength);
            GL.ShaderSource(fragmentShaderObject, 1, fragmentShader, ref fragmentLength);

            int status = 0;

            GL.CompileShader(vertexShaderObject);
            GL.GetShader(vertexShaderObject, ShaderParameter.CompileStatus, out status);
            if (status != 0)
            {
                ShowShaderLog(vertexShaderObject);
            }

            GL.CompileShader(fragmentShaderObject);
            GL.GetShader(fragmentShaderObject, ShaderParameter.CompileStatus, out status);
            if (status != 0)
            {
                ShowShaderLog(fragmentShaderObject);
            }

            GL.AttachShader(shaderProgram, vertexShaderObject);
            GL.AttachShader(shaderProgram, fragmentShaderObject);

            GL.LinkProgram(shaderProgram);

            GL.GetProgram(shaderProgram, ProgramParameter.LinkStatus, out status);
            if (status != 0)
            {
                ShowProgramLog(shaderProgram);
            }

            return shaderProgram;
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;
                if (iProgram != 0)
                    GL.DeleteProgram(iProgram);
            }
        }
    }
}
