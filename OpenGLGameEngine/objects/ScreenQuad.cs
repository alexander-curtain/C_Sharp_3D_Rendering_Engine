using OpenGLGameEngine.rendering;
using OpenGLGameEngine.rendering.lowLevelClasses.rendering_pipelining;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLGameEngine.objects
{
    public class ScreenQuad : IRenderable
    {

        static float[] _data = {
            // positions   // texCoords
            -1.0f,  1.0f,  0.0f, 1.0f, // top-left
            -1.0f, -1.0f,  0.0f, 0.0f, // bottom-left
             1.0f, -1.0f,  1.0f, 0.0f, // bottom-right

            -1.0f,  1.0f,  0.0f, 1.0f, // top-left
             1.0f, -1.0f,  1.0f, 0.0f, // bottom-right
             1.0f,  1.0f,  1.0f, 1.0f  // top-right
        };

        int vao;
        int vbo;
        public int textureHandle;
        public Shader shader;


        public ScreenQuad(Shader shader, int textureHandle)
        {
            this.shader = shader;
            this.textureHandle = textureHandle;
            InitializeBuffers([VBO_OBJECT.Vertices2d, VBO_OBJECT.UV]);
        }




        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                  Constructor Aux
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */
        private void InitializeBuffers(VBO_OBJECT[] vbos)
        {
            //generate + bind VAO
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            // generate + bind VBO
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            // update GPU data
            GL.BufferData(BufferTarget.ArrayBuffer, _data.Length * sizeof(float), _data, BufferUsageHint.StaticDraw);

            VBOAux.generateVBOS(vbos);

            shader.Use();
        }


        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                      Render Methods
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */
        public void Render()
        {
            shader.Use();

            // bind quad to render
            GL.BindVertexArray(vao);
            GL.Disable(EnableCap.DepthTest);

            // set texture to FBO texture
            bindUniforms();

            // draw quad
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        public void SimpleRender()
        {
            GL.Disable(EnableCap.DepthTest);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        public void bindUniforms()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureHandle);
            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "screenTexture"), 0);
        }

        public RenderFlags getFlags()
        {
            return new RenderFlags(); // has no flags
        }


        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                  [Getters / Setters]
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */
        public void updateTextureHandle(int textureHandle)
        {
            this.textureHandle = textureHandle;
        }

        public string getName()
        {
            throw new NotImplementedException();
        }

        public void setName(string name)
        {
            throw new NotImplementedException();
        }
    }
}
