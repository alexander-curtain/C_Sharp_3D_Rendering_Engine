using Assimp;
using OpenGLGameEngine.aux_functions;
using OpenGLGameEngine.rendering;
using OpenGLGameEngine.textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenGLGameEngine.rendering.lowLevelClasses.rendering_pipelining;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;


namespace OpenGLGameEngine.objects
{
    public class SkyBox : IRenderable
    {
        float[] _data = {
                -1.0f,  1.0f, -1.0f,
                -1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                -1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f
            };

        CubeMap cubeMap;

        private int vao;
        private int vbo;
        public Shader shader { get; private set; }

        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                  [Constructor]
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */
        public SkyBox(string filePath, string fileExtension = ".png")
        {
            cubeMap = new CubeMap(filePath, fileExtension);
            shader = new Shader("C:\\Users\\alexa\\source\\repos\\Callus\\OpenGLGameEngine\\shaders\\skybox\\skybox.vert", "C:\\Users\\alexa\\source\\repos\\Callus\\OpenGLGameEngine\\shaders\\skybox\\skybox.frag");

            InitializeBuffers([VBO_OBJECT.Vertices]);

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
        }

        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                  Render Methods
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */
        public void Render()
        {
            GL.DepthFunc(DepthFunction.Lequal);
            shader.Use();


            GL.BindVertexArray(vao);

            this.bindUniforms();

            GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 36);
            GL.DepthFunc(DepthFunction.Less);
        }
        public void SimpleRender()
        {
            GL.BindVertexArray(vao);
            GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 36);
        }

        public void bindUniforms()
        {
            cubeMap.Bind(TextureUnit.Texture0);
            cubeMap.updateShader(shader, 0, "skybox");
            GL.BindVertexArray(vao);
        }

        // since the skybox doesn't follow the same rules as other viewproject matrix (since it doesn't have displacement) we need to update the viewProject Manually
        public void updateCamera(Matrix4 view, Matrix4 projection)
        {
            shader.Use();
            view.Row3.Xyz = Vector3.Zero; // reset the translation

            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);
        }

        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                  [Getters / Setters]
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */


        public string getName()
        {
            return "skybox";
        }

        public void setName(string name)
        {
            Console.WriteLine("attempted to change skybox name, invalid operation");
        }

        public RenderFlags getFlags()
        {
            return new RenderFlags(SkyBox: true);
        }
    }
}
