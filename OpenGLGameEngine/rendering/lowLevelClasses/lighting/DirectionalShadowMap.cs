using OpenGLGameEngine.aux_functions;
using OpenGLGameEngine.rendering.lowLevelClasses.FBO;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLGameEngine.rendering.lowLevelClasses.lighting
{
    public class DirectionalShadowMap
    {

        Vector3 renderPosition;

        string lightReferenceName;
        int resolution; // eg. 1024x1024
        float renderDistance; // far plane distance
        float renderWindowWidth; // how far in the relative xy directions the map spreads over.

        LightingHandler lightingHandler; // to get current light positions/directions from
        Camera camera;

        Matrix4 lightSpaceMatrix;

        FrameBufferObject fbo;
        public DirectionalShadowMap(string lightName, int resolution, float renderWindowWidth, float renderDistance)
        {
            this.renderWindowWidth = renderWindowWidth;
            lightReferenceName = lightName;
            this.resolution = resolution;
            this.renderDistance = renderDistance;
            lightingHandler = DataHandler.Get.getLightingHandler();
            renderPosition = new Vector3(0.0f);
            camera = DataHandler.Get.getCamera();

            fbo = new FrameBufferObject(resolution, resolution, [new FBOTextureAttachment(FBOTEXTURETYPE.Depth, "DirectionalShadowMap")]);
        }


        public Matrix4 getLightSpaceMatrix()
        {
            const float cameraOffsetBlend = 0.3f;
            Vector3 cameraDirection = camera.getViewDirection();
            cameraDirection.Y = 0f;
            cameraDirection.Normalize();
            cameraDirection = cameraDirection * (0.5f * cameraOffsetBlend) * renderWindowWidth;


            Vector3 lightPosition = camera.getViewPos() + cameraDirection + lightingHandler.getDirection(lightReferenceName).Value * renderDistance / 1.414f;

            Vector3 lightDirection = lightPosition - new Vector3(lightingHandler.getDirection(lightReferenceName).Value); // TODO HANDLE CUBE MAP'S WHICH DON'T HAVE A DIRECTION UNLIKE DIRECTIONAL AND SPOTLIGHT
            float nearPlane = 0.25f;
            float farPlane = renderDistance;

            Matrix4 lightProjection = Matrix4.CreateOrthographic(renderWindowWidth, renderWindowWidth, nearPlane, farPlane); // TODO add a way to adjust the width/height for larger volumes of renedring
            // TODO add branch for rendering if the matrix is a spot light and has projection rather than orthographic.
            
            Matrix4 lightView = Matrix4.LookAt(lightPosition, lightDirection, new Vector3(0.0f, 1.0f, 0.0f));

            Matrix4 lightSpaceMatrix = lightView * lightProjection; // note that opentk swaps matrix multiplication orders

            return lightSpaceMatrix;
        }

        public int getTextureID()
        {
            return DataHandler.Get.getFrameBufferHandler().getFrameBufferTexture("DirectionalShadowMap");
        }
        public void setRenderPosition(Vector3 newPosition)
        {
            renderPosition = newPosition;
        }

        public void bindShadowMap()
        {
            GL.Enable(EnableCap.DepthTest); GL.DepthFunc(DepthFunction.Less); GL.DepthMask(true);
            fbo.Bind();
            GL.Viewport(0, 0, resolution, resolution);

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
        }

        public void updateShaderUniformDirectional(Shader shader)
        {
            GL.ActiveTexture(TextureUnit.Texture15);
            GL.BindTexture(TextureTarget.Texture2D, getTextureID());
            shader.SetInt("directionalShadowMap", 15);
        }
    }
}
