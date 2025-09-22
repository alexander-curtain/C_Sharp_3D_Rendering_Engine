using OpenGLGameEngine.objects.Functional;
using OpenGLGameEngine.rendering;
using OpenGLGameEngine.textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using OpenGLGameEngine.rendering.lowLevelClasses.rendering_pipelining;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;

namespace OpenGLGameEngine.objects
{
    public class Model : IRenderable, IHasName, IHasModelMatrix, IHasNormalMatrix, IHasShader, IHasMaterial
    {
        private RenderFlags _flags;
        private float[] _data;
        private Material material;
        private Matrix4 modelMatrix = Matrix4.Identity;
        Matrix3 normalMatrix;

        private int vao;
        private int vbo;
        private Shader shader;

        private string name;

        public Model(string name, string filePath, Material material, string shader = "deferredRender", RenderFlags? flags = null)
        {
            this._flags = flags == null ? new RenderFlags(CastShadows: true, Physical: true) : flags.Value;
            this.material = material;
            this.name = name;
            VBO_OBJECT[] bufferformat = [];

            bufferformat = getVBOformat();
            _data = loadWaveFront(filePath);

            this.shader = DataHandler.Get.getShaderHandler().getShader(shader);

            // saves data by simply
            normalMatrix = Matrix3.Transpose(new Matrix3(modelMatrix).Inverted());

            InitializeBuffers(bufferformat);
        }




        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                  Loading Aux Methods
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

        public VBO_OBJECT[] getVBOformat()
        {
            switch (material.NormalMap != null || material.DisplacementMap != null)
            {
                case true: return [VBO_OBJECT.Vertices, VBO_OBJECT.Normal, VBO_OBJECT.UV, VBO_OBJECT.Tangents];
                case false: return [VBO_OBJECT.Vertices, VBO_OBJECT.Normal, VBO_OBJECT.UV];
            }
        }
        public float[] loadWaveFront(string filePath)
        {
            switch (material.NormalMap != null || material.DisplacementMap != null)
            {
                case true: return ObjectLoader.LoadWavefrontWithTangents(filePath);
                case false: return ObjectLoader.loadWavefront(filePath);
            }
        }

        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                  IRendering Methods
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */

        public void bindUniforms()
        {
            material.bindUniforms(this.shader);

            // model space
            shader.SetMatrix4("model", modelMatrix);
            shader.SetMatrix3("normalMatrix", normalMatrix);
        }
        public void Render()
        {
            this.shader.Use();

            GL.BindVertexArray(vao);

            bindUniforms();

            GL.DrawArrays(PrimitiveType.Triangles, 0, _data.Length / 6);
        }
        public void SimpleRender()
        {
            GL.BindVertexArray(vao);

            GL.DrawArrays(PrimitiveType.Triangles, 0, _data.Length / 6);
        }





        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                  [Getters / Setters]
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */

        public Matrix4 getModelMatrix()
        {
            return modelMatrix;
        }

        public void setModelMatrix(Matrix4 modelMatrix)
        {
            this.modelMatrix = modelMatrix;
        }

        public string getName()
        {
            return name;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public Shader getShader()
        {
            return shader;
        }

        public void setShader(Shader shader)
        {
            this.shader = shader;
        }

        public RenderFlags getFlags()
        {
            return this._flags;
        }

        public Material getMaterial()
        {
            return this.material;
        }

        public void setMaterial(Material material)
        {
            //TODO, should be updating the buffers in the shader, but this is too complex for now.
            this.material = material;
        }

        public Matrix3 getNormalMatrix()
        {
            return this.normalMatrix;
        }

    }
}
