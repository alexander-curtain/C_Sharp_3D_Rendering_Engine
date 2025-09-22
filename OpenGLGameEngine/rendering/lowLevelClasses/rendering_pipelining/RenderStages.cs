using OpenGLGameEngine.aux_functions;
using OpenGLGameEngine.objects;
using OpenGLGameEngine.rendering.lowLevelClasses.lighting;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;


namespace OpenGLGameEngine.rendering.lowLevelClasses.rendering_pipelining
{
    public delegate void RenderStage();
    public static class RenderStages
    {

        public static RenderStage SetupScreenSpaceRendering = () =>
        {
            // global data collection
            PostProcessingHandler postProcessingHandler = DataHandler.Get.getPostProcessingHandler();
            UniformBufferHandler uniformBufferHandler = DataHandler.Get.getUniformBufferHandler();
            Camera camera = DataHandler.Get.getCamera();

            // Binds the FBO
            postProcessingHandler.bindFirstPass();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            uniformBufferHandler.onRender(camera);
        };

        public static RenderStage postProcessing = () =>
        {
            DataHandler.Get.getPostProcessingHandler().render();
        };

        public static RenderStage deferred = () =>
        {
            Shader shader = DataHandler.Get.getShaderHandler().getShader("deferredRender");
            List<IRenderable> models = DataHandler.Get.getRenderingGroup(RENDERING_GROUP.physicalModels);

            shader.Use();
            GL.Disable(EnableCap.Blend);

            foreach (IRenderable model in models)
            {
                // this pipelines lets us use the model whilsts only casting the shader once
                model.bindUniforms();
                model.SimpleRender();
            }

        };






        public static RenderStage forwardRender = () =>
        {
            Shader shader = DataHandler.Get.getShaderHandler().getShader("default");
            int ShadowMapTextureID = DataHandler.Get.getDirectionalShadowMap().getTextureID();
            List<IRenderable> models = DataHandler.Get.getRenderingGroup(RENDERING_GROUP.physicalModels);

            shader.Use();



            GL.ActiveTexture(TextureUnit.Texture15);
            GL.BindTexture(TextureTarget.Texture2D, ShadowMapTextureID);
            shader.SetInt("shadowMap", 15);


            foreach (IRenderable model in models)
            {
                // this pipelines lets us use the model whilsts only casting the shader once
                model.bindUniforms();
                model.SimpleRender();
            }
        };


        // this is temp but will be changed once instance models are more fleshed out
        public static RenderStage otherModels = () =>
        {
            List<IRenderable> instancedModels = DataHandler.Get.getRenderingGroup(RENDERING_GROUP.instancedModels);
            List<IRenderable> customShaderModels = DataHandler.Get.getRenderingGroup(RENDERING_GROUP.customShader);

            foreach (IRenderable model in instancedModels)
            {
                model.Render();
            }

            foreach (IRenderable model in customShaderModels)
            {
                model.Render();
            }
        };

        public static RenderStage billboards = () =>
        {
            List<IRenderable> billboards = DataHandler.Get.getRenderingGroup(RENDERING_GROUP.billboards);
            
            GL.Enable(EnableCap.Blend); GL.Disable(EnableCap.CullFace);
            foreach (IRenderable model in billboards)
            {
                model.Render();
            }
            GL.Enable(EnableCap.CullFace); GL.Disable(EnableCap.Blend);
        };

        public static RenderStage skybox = () =>
        {
            SkyBox skybox = DataHandler.Get.getRenderingGroup(RENDERING_GROUP.skybox)[0] as SkyBox;
            Camera camera = DataHandler.Get.getCamera();

            skybox.updateCamera(camera.getView(), camera.getProjection());

            skybox.Render();
        };

        public static RenderStage transparentObjects = () =>
        { 
            List<IRenderable> transparentModels = DataHandler.Get.getRenderingGroup(RENDERING_GROUP.billboards);

            GL.Enable(EnableCap.Blend); GL.DepthMask(false);
            foreach (IRenderable model in transparentModels)
            {
                model.Render();
            }
            GL.DepthMask(true); GL.Disable(EnableCap.Blend);
        };


        public static RenderStage stageDefaultShadow = () =>
        {
            // collect global data required.
            DirectionalShadowMap shadowMap = DataHandler.Get.getDirectionalShadowMap();
            (int, int) windowSize = new(DataHandler.Get.sizeX, DataHandler.Get.sizeY);
            Shader shader = DataHandler.Get.getShaderHandler().getShader("depthPass");
            Shader instancedShader = DataHandler.Get.getShaderHandler().getShader("depthPassInstanced");

            List<IRenderable> shadowCasters = DataHandler.Get.getRenderingGroup(RENDERING_GROUP.shadowCast);
            List<IRenderable> shadowCastersInstanced = DataHandler.Get.getRenderingGroup(RENDERING_GROUP.shadowCastInstance);

            UniformBufferHandler uniformBufferHandler = DataHandler.Get.getUniformBufferHandler();


            // gets the light space matrix
            Matrix4 lightSpaceMatrix = shadowMap.getLightSpaceMatrix();

            // binds the fbo to be the render target
            shadowMap.bindShadowMap();

            // BEGIN RENDERING
            GL.Enable(EnableCap.CullFace);            // Enable face culling
            GL.FrontFace(FrontFaceDirection.Ccw);

            // run default shader method
            shader.Use();
            shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            foreach (IRenderable obj in shadowCasters)
            {
                shader.SetMatrix4("model", ((IHasModelMatrix)obj).getModelMatrix()); // if it cast shadows, it should have a model matrix, this may cause errors.
                obj.SimpleRender();
            }

            // run code for instanced models
            instancedShader.Use();
            instancedShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            foreach (IRenderable obj in shadowCastersInstanced)
            {
                obj.SimpleRender();
            }

            // END RENDERING
            GL.FrontFace(FrontFaceDirection.Ccw);


            // update the lightSpaceMatrix in the UBO
            uniformBufferHandler.getUBO(BINDING_UBO.shadowData).setMat4("directional[0]", lightSpaceMatrix);

            // reset the size of the viewport from the resolution of the shadow map.
            GL.Viewport(0, 0, windowSize.Item1, windowSize.Item2);
        };
    }
}
