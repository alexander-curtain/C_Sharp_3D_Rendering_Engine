using OpenGLGameEngine.objects.Functional;
using OpenGLGameEngine.rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using OpenGLGameEngine.rendering.lowLevelClasses.rendering_pipelining;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;

namespace OpenGLGameEngine.objects
{
    public class InstancedModel : IHasName, IHasShader, IRenderable
    {
        private float[] _modelData;
        private Matrix4[] _modelTransforms;
        // TODO add inverse matrices to the VBO to reduce normalising matrices performance cost.

        private Material material;

        private int vao;
        private int vbo;
        private int instanceVBO;

        private Shader shader;
        private RenderFlags flags;

        private string name;


        public InstancedModel(string name, string filePath, Matrix4[] instancedTransformations, Material material, Shader? shader = null)
        {
            this.flags = new RenderFlags(Instanced: true, CastShadows: true); // forces the instanced render flag

            _modelData = ObjectLoader.loadWavefront(filePath);
            this.name = name;
            this._modelTransforms = instancedTransformations;
            this.shader = shader == null ? DataHandler.Get.getShaderHandler().getShader("defaultInstanced") : shader;
            this.material = material;

            InitializeBuffersInstanced([VBO_OBJECT.Vertices, VBO_OBJECT.Normal, VBO_OBJECT.UV], [VBO_OBJECT.Mat4Column, VBO_OBJECT.Mat4Column, VBO_OBJECT.Mat4Column, VBO_OBJECT.Mat4Column]);
        }
        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                  Rendering Methods
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */
        public void Render()
        {
            shader.Use();

            GL.BindVertexArray(vao);

            this.bindUniforms();

            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, _modelData.Length / 6, _modelTransforms.Length);
        }
        public void SimpleRender()
        {
            GL.BindVertexArray(vao);

            GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, _modelData.Length / 6, _modelTransforms.Length);
        }

        public void bindUniforms()
        {
            material.bindUniforms(shader);
        }

        private void InitializeBuffersInstanced(VBO_OBJECT[] vbos, VBO_OBJECT[] instancedVBO)
        {
            //generate + bind VAO
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            // generate + bind VBO
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            // update GPU data
            GL.BufferData(BufferTarget.ArrayBuffer, _modelData.Length * sizeof(float), _modelData, BufferUsageHint.StaticDraw);

            VBOAux.generateVBOS(vbos);


            // [Instancing]
            instanceVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, instanceVBO);

            // allocate GPU space
            GL.BufferData(BufferTarget.ArrayBuffer, _modelTransforms.Length * sizeof(float) * 16, _modelTransforms, BufferUsageHint.StaticDraw);

            VBOAux.generateVBOS(instancedVBO, vbos.Length);

            // sets all the instance VBO values to be set per instance rather than per vertex.
            for (int i = 0; i < instancedVBO.Length; i++) { GL.VertexAttribDivisor(i + vbos.Length, 1); }
        }




        public string getName()
        {
            return this.name;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public Shader getShader()
        {
            return this.shader;
        }

        public void setShader(Shader shader)
        {
            throw new NotImplementedException();
        }


        public RenderFlags getFlags()
        {
            return flags;
        }


    }
}
