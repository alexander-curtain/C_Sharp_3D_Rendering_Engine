using OpenGLGameEngine.objects.Functional;
using OpenGLGameEngine.textures;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenGLGameEngine.rendering.lowLevelClasses.rendering_pipelining;

namespace OpenGLGameEngine.rendering
{
    public enum RENDERINGFLAG
    {
        default_shader = 1,
        has_transparency= 2,
        billboard = 3,
        skybox = 4,
        nondefault_shader = 5
    }
    public interface IRenderable : IHasName
    {
        void Render();
        void SimpleRender();
        void bindUniforms();

        RenderFlags getFlags();
    }
    public interface IHasMaterial
    {
        public Material getMaterial();
        public void setMaterial(Material material);
    }

    public interface IHasPosition
    {
        public Vector3 getPosition();
        public void setPosition(Vector3 newPosition, Shader shader);
    }
    public interface IHasDirection
    {
        public Vector3 getDirection();
        public void setDirection(Vector3 newDirection, Shader shader);
    }

    public interface IHasPositionUBO
    {
        public Vector3 getPosition();
        public void setPosition(Vector3 newPosition, UniformBufferObject ubo);
    }

    public interface IHasDirectionUBO
    {
        public Vector3 getDirection();
        public void setDirection(Vector3 newPosition, UniformBufferObject ubo);
    }

    public interface IHasModelMatrix
    {
        public Matrix4 getModelMatrix();
        public void setModelMatrix(Matrix4 modelMatrix);
    }
    public interface IHasNormalMatrix : IHasModelMatrix 
    {
        public Matrix3 getNormalMatrix();
    }

    public interface IHasColour
    {

        public Vector3 getAmbient();
        public Vector3 getDiffuse();
        public Vector3 getSpecular();
        public void setAmbient(Vector3 colour);
        public void setDiffuse(Vector3 colour);
        public void setSpecular(Vector3 colour);
    }

    public interface IHasShader
    {
        public Shader getShader();
        public void setShader(Shader shader);
    }

    public interface IHasName
    {
        public string getName();
        public void setName(string name);
    }

    public enum VBO_OBJECT
    {
        Vertices = 3,
        UV = 2,
        Normal = 3,
        Vertices2d = 2,
        Tangents = 3,
        // VBO can only accept 4 floats per chunk, so matrices must be seperated into their columns eg. Mat2Column Mat2Column = mat2
        Mat4Column = 4,
        Mat3Column = 3
    }

    public static class VBOAux
    {
        public static int bytesRequired(VBO_OBJECT obj)
        {
            return elements(obj) * sizeof(float);
        }

        // returns the size of the vector. eg. each normal is a vec3, uvs are vec2
        private static int elements(VBO_OBJECT vbo)
        {
            return (int)vbo;
        }

        // returns the total number of bytes per unit
        public static int getStride(VBO_OBJECT[] obj)
        {
            int bytes = 0;

            for (int i = 0; i < obj.Length; i++)
            {
                bytes += bytesRequired(obj[i]);
            }

            return bytes;
        }
        // runs through the list of VBOs given to activate them and allocate their correct memory.
        public static void generateVBOS(VBO_OBJECT[] vbos, int layoutOffset = 0) {

            int stride = getStride(vbos);
            int offset = 0;

            for (int i = 0; i < vbos.Length; i++)
            {
                GL.EnableVertexAttribArray(i + layoutOffset);
                GL.VertexAttribPointer(
                    i + layoutOffset,
                    elements(vbos[i]),                            // size
                    VertexAttribPointerType.Float,                // type
                    false,                                        // normalized
                    stride,                                       // stride in bytes
                    offset                                        // offset in bytes
                );

                offset += elements(vbos[i]) * sizeof(float); // advance offset correctly
            }
        }
    }
}
