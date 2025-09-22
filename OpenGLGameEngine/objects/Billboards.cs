using OpenGLGameEngine.objects.Functional;
using OpenGLGameEngine.rendering;
using OpenGLGameEngine.textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenGLGameEngine.rendering.lowLevelClasses.rendering_pipelining;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;

namespace OpenGLGameEngine.objects
{
    public class Billboards : IRenderable
    {
        string name;
        int vao;
        int vbo;
        int instanceVBO;
        float[] instancePositions;
        public RenderFlags flags;


        int instances;
        Shader shader;
        Texture texture;

        static float[] _data =  {
                // x, y,    u, v
                // Triangle 1
                -0.5f, -0.5f,  0.0f, 0.0f, // bottom-left
                 0.5f, -0.5f,  1.0f, 0.0f, // bottom-right
                 0.5f,  0.5f,  1.0f, 1.0f, // top-right

                // Triangle 2
                -0.5f, -0.5f,  0.0f, 0.0f, // bottom-left
                 0.5f,  0.5f,  1.0f, 1.0f, // top-right
                -0.5f,  0.5f,  0.0f, 1.0f  // top-left
            };


        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                    Constructors
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */
        public Billboards(string name, float[] positionsVec3, Texture texture) {

            this.name = name;
            shader = DataHandler.Get.getShaderHandler().getShader("billboard");
            this.texture = texture;
            flags = new RenderFlags(Billboard: true); 

            

            instancePositions = positionsVec3;
            instances = positionsVec3.Length / 3; // since the instance positions are an array of floats, dividing by 3 is the count of the instances

            InitializeBuffersInstanced([VBO_OBJECT.Vertices2d, VBO_OBJECT.UV], [VBO_OBJECT.Vertices]);
            
        }
        // override the shader if you want particle systems or something complicated than y fixed axis billboard
        public Billboards(string name, float[] positionsVec3, Texture texture, Shader shader)
        {
            this.name = name;
            this.shader = shader;
            this.texture = texture;
            flags = new RenderFlags(Billboard: true);

            instancePositions = positionsVec3;
            instances = positionsVec3.Length / 3;

            //initalises the perVertex vbo
            InitializeBuffersInstanced([VBO_OBJECT.Vertices2d, VBO_OBJECT.UV], [VBO_OBJECT.Vertices]);

        }


        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                  Buffer data
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */
        private void InitializeBuffersInstanced(VBO_OBJECT[] vbos, VBO_OBJECT[] instancedVBO)
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


            // [Instancing]
            instanceVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);

            // allocate GPU space
            GL.BufferData(BufferTarget.ArrayBuffer, instancePositions.Length * sizeof(float), instancePositions, BufferUsageHint.StaticDraw);

            VBOAux.generateVBOS(instancedVBO, vbos.Length);

            // sets instance VBO values to update per instance rather than per vertex
            for (int i = 0; i < instancedVBO.Length; i++) { GL.VertexAttribDivisor(i + vbos.Length, 1); } 

            shader.Use();
        }
        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                  IRendering Methods
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */
        public void Render()
        {
            shader.Use();

            this.bindUniforms();

            GL.BindVertexArray(vao);

            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, instances);
        }
        public void SimpleRender()
        {
            GL.BindVertexArray(vao);

            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, instances);
        }

        public void bindUniforms()
        {
            texture.Use(TextureUnit.Texture0);
            shader.SetInt("billboardTexture", 0);
        }


        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                  [Getters / Setters]
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */

        public string getName()
        {
            return name;
        }

        public void setName(string name)
        {
            this.name = name;
        }
        public RenderFlags getFlags()
        {
            return this.flags;
        }


    }
}
