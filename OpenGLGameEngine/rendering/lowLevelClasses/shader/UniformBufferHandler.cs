using OpenGLGameEngine.aux_functions;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace OpenGLGameEngine.rendering.lowLevelClasses.shader
{
    public enum BINDING_UBO
    {
        ViewProjection = 0,
        CameraData = 1,
        Lights = 2,
        TimeAux = 3,
        shadowData = 4

    }

    public class UniformBufferHandler
    {
        const int numberOfUniformBuffers = 15; // YOU MUST UPDATE THIS TO ADD UBOs

        // says which binding index the requested variable name belongs to <VarName, binding>
        public Dictionary<string, int> address = new Dictionary<string, int>(); 
        UniformBufferObject[] uniformBufferObjects = new UniformBufferObject[numberOfUniformBuffers];
        Stopwatch stopwatch = new Stopwatch();
        public UniformBufferHandler()
        {
            stopwatch.Start();
            /* 
             * =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
             *         [START]       Variable Declaration For UBOs
             * =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
             */


            //ViewProjection = 0
            (std140DT, string)[] vars0 = [(std140DT.Matrix4, "view"), (std140DT.Matrix4, "projection")];
            initializeUniformBufferObject(BINDING_UBO.ViewProjection, vars0);


            //CameraPosView = 1
            (std140DT, string)[] vars1 = [(std140DT.Vec3, "cameraPosition"), (std140DT.Vec3, "cameraDirection")];
            initializeUniformBufferObject(BINDING_UBO.CameraData, vars1);


            //Lights = 2 Handled by the Lighting Handler

            //Time = 3
            (std140DT, string)[] vars3 = [(std140DT.Single, "time")];
            initializeUniformBufferObject(BINDING_UBO.TimeAux, vars3, BufferUsageHint.StreamDraw);

            //Shadows = 4 Handled by the Lighting Handler, TODO, automate this so we can set an arbitrary number of cascading shadowMap
            (std140DT, string)[] vars4 = [(std140DT.Matrix4, "directional[0]")];
            initializeUniformBufferObject(BINDING_UBO.shadowData, vars4, BufferUsageHint.DynamicDraw);

            /* 
             * =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
             *         [END]           Variable Declaration For UBOs
             * =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
             */



            
        }

        public void onRender(Camera camera)
        {
            //update Binding 0, matrices
            getUBO(BINDING_UBO.ViewProjection).setMat4("view", camera.getView());
            getUBO(BINDING_UBO.ViewProjection).setMat4("projection", camera.getProjection());

            //update Binding 1, camera data
            getUBO(BINDING_UBO.CameraData).setVec3("cameraPosition", camera.getViewPos());
            getUBO(BINDING_UBO.CameraData).setVec3("cameraDirection", camera.getViewDirection());

            //update Binding 3, time
            getUBO(BINDING_UBO.TimeAux).setFloat("time", stopwatch.ElapsedMilliseconds);
        }
        public void updateLightSpaceMatrices(Matrix4[] lightSpaceMatrices)
        {
            for (int i = 0; i < lightSpaceMatrices.Length; i++)
            {
                getUBO(BINDING_UBO.shadowData).setMat4($"directional[{i}]", lightSpaceMatrices[i]);
            }
        }

        public UniformBufferObject getUBO(BINDING_UBO pos)
        {
            return uniformBufferObjects[(int)pos];
        }



        private void initializeUniformBufferObject(BINDING_UBO binding, (std140DT, string)[] variables, BufferUsageHint usage = BufferUsageHint.StreamDraw)
        {
            uniformBufferObjects[(int)binding] = new UniformBufferObject(binding, variables, usage);
        }

    }


    public enum std140DT // records the bytes required for certain DataTypes
    {
        Single = 4, // Ints, Bools, Floats, etc.
        Int = 4,
        Bool = 4,
        Float = 4,
        Vec2 = 8,
        Vec3 = 16,
        Vec4 = 16,
        Matrix4 = 64,
        ArrayElement = 16,

        // structs
        dirLight = 64,
        pointLight = 80,
        spotLight = 96
    }



    // handles all the writing operations.
    public struct UniformBufferObject
    {
        // <Variable Name, (StartByte, Bytes To Write)>
        public Dictionary<string, (int, int)> address;
        public int Handle;
        public int binding;

        int Align(int offset, int alignment) => offset + alignment - 1 & ~(alignment - 1);
        public UniformBufferObject(BINDING_UBO bindingNumber, (std140DT, string)[] dataInput, BufferUsageHint bufferWriteFrequency)
        {
            address = new Dictionary<string, (int, int)>();
            binding = (int)bindingNumber;

            // loop through to set the size and the memory addresses start/end point to write to
            int size = 0;
            for (int i = 0; i < dataInput.Length; i++)
            {
                int alignment = (int)dataInput[i].Item1;
                size = Align(size, alignment);
                address.Add(dataInput[i].Item2, (size, (int)dataInput[i].Item1));
                size += (int)dataInput[i].Item1;
            }

            // Bind the Buffer
            Handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, Handle);
            GL.BufferData(BufferTarget.UniformBuffer, size, nint.Zero, bufferWriteFrequency);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, binding, Handle);
        }


        // setters
        public void setMat4(string variableName, Matrix4 matrix)
        {
            (int start, int size) = address[variableName];
            GL.BindBuffer(BufferTarget.UniformBuffer, Handle);
            GL.BufferSubData(BufferTarget.UniformBuffer, start, size, ref matrix.Row0.X);
        }
        public void setVec3(string variableName, Vector3 vec3)
        {
            (int start, int size) = address[variableName];
            GL.BindBuffer(BufferTarget.UniformBuffer, Handle);
            GL.BufferSubData(BufferTarget.UniformBuffer, start, size - 4, ref vec3.X);
        }
        public void setFloat(string variableName, float flt)
        {
            (int start, int size) = address[variableName];
            GL.BindBuffer(BufferTarget.UniformBuffer, Handle);
            GL.BufferSubData(BufferTarget.UniformBuffer, start, size, ref flt);
        }
        public void setInt(string variableName, int integer)
        {
            (int start, int size) = address[variableName];
            GL.BindBuffer(BufferTarget.UniformBuffer, Handle);
            GL.BufferSubData(BufferTarget.UniformBuffer, start, size, ref integer);
        }
    }
}
