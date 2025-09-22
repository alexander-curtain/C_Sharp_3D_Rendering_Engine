using ImGuiNET;
using OpenGLGameEngine.aux_functions;
using OpenGLGameEngine.objects;
using OpenGLGameEngine.rendering.lowLevelClasses.FBO;
using OpenGLGameEngine.rendering.lowLevelClasses.imgui;
using OpenGLGameEngine.rendering.lowLevelClasses.lighting;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenGLGameEngine.textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;


namespace OpenGLGameEngine.rendering.lowLevelClasses.rendering_pipelining
{
    /// <summary>
    /// 
    /// 
    /// 
    /// </summary>
    public class PostProcessingHandler
    {

        // post processing stack
        private List<PostProcessingEffect> postprocessingEffects = new List<PostProcessingEffect>();

        int sizeX;
        int sizeY;

        int indexOfLastSequentialPostProcessingEffect = 0;
        PostProcessingEffect guassianBlur;

        FrameBufferObject gBuffer;

        public void initPostProcessingEffects()
        {
            // lighting
            Shader lighting = DataHandler.Get.getShaderHandler().getShader("lighting");
            FBOTextureAttachment[] highlightsExtractor = [new FBOTextureAttachment(FBOTEXTURETYPE.Colour, "Lighting"), new FBOTextureAttachment(FBOTEXTURETYPE.Colour, "Highlights")];
            FrameBufferObject lightingExtractor = new FrameBufferObject(sizeX, sizeY, highlightsExtractor);

            // tone mapping
            Shader tonemappingPP = DataHandler.Get.getShaderHandler().getShader("toneMapper");
            PostProcessingEffect tonemap = new PostProcessingEffect("tonemapping", DataHandler.Get.sizeX, DataHandler.Get.sizeY, tonemappingPP, PPRenderingMethod.renderWithBloomBlur);
            tonemappingPP.SetFloat("exposure", 1.0f);


            // Gaussian Blur Post Processing effect
            const float scalingFactorGuassian = 0.5f;
            guassianBlur = new PostProcessingEffect("guassianBlur", (int)((float)sizeX * scalingFactorGuassian), (int)((float)sizeY * scalingFactorGuassian), DataHandler.Get.getShaderHandler().getShader("guassianBlur"), PPRenderingMethod.PingPongGuassianBlur);

            // SSAO

            float screenSizeSSAO = 0.4f;
            int sizeXSSAO = (int)((float)sizeX * screenSizeSSAO);
            int sizeYSSAO = (int)((float)sizeY * screenSizeSSAO);

            List<float> kernalDistrubution = new List<float>(); // note these are 3d vectors
            int samplesCount = 64;
            Random rand = new Random();
            for (int i = 0; i < samplesCount; ++i)
            {
                Vector3 sample = new Vector3(0.0f);
                do {
                    sample = new Vector3(
                        (float)(rand.NextDouble() * 2.0 - 1.0),
                        (float)(rand.NextDouble() * 2.0 - 1.0),
                        (float)(rand.NextDouble())
                       );
                    sample.Normalize();
                } while (sample.Z > 0.25f);


                // bias away from edge towards centre
                float scale = (float)i / 64.0f;
                scale =  AuxMath.lerp(0.1f, 1.0f, scale * scale);
                sample *= scale;

                kernalDistrubution.Add(sample.X); kernalDistrubution.Add(sample.Y); kernalDistrubution.Add(sample.Z);
            }

            float[] ssaoNoise = new float[48];
            for (int i = 0; i < 16; i++)
            {
                ssaoNoise[i * 3] = (float)(rand.NextDouble() * 2.0 - 1.0);
                ssaoNoise[i * 3 + 1] = (float)(rand.NextDouble() * 2.0 - 1.0);
                ssaoNoise[i * 3 + 2] = 0.0f;
            }
            Texture ssaoNoiseTexture = Texture.GenerateTextureFromArray(ssaoNoise, 4, PixelFormat.Rgb);
            Shader ssaoShader = DataHandler.Get.getShaderHandler().getShader("ssao");
            ssaoShader.SetVector2("framebufferSize", new Vector2(sizeXSSAO, sizeYSSAO));
            FrameBufferObject ssaoFbo = new FrameBufferObject(sizeXSSAO, sizeYSSAO, [new FBOTextureAttachment(FBOTEXTURETYPE.SingleColourChannel, "ssao")]);
            PostProcessingEffect ssao = new PostProcessingEffect("ssao", sizeXSSAO, sizeYSSAO, ssaoShader, PPRenderingMethod.ssao, ssaoFbo);

            ssao.addAuxData("kernal", kernalDistrubution.ToArray());
            ssao.addAuxData("ssaoNoise", [(float)(ssaoNoiseTexture.Handle)]);

            Shader ssaoBlurShader = DataHandler.Get.getShaderHandler().getShader("ssaoBlur");
            PostProcessingEffect ssaoBlur = new PostProcessingEffect("ssaoBlur", sizeXSSAO, sizeYSSAO, ssaoBlurShader, PPRenderingMethod.defaultRenderAdjustScreenSize, ssaoFbo);
            ssaoBlur.setInputTexture(ssao.getTextureID());


            Shader visualiseLight = DataHandler.Get.getShaderHandler().getShader("lightVisualisation");


            addPostProcessingEffect(ssao, false);
            addPostProcessingEffect(ssaoBlur, false);
            addPostProcessingEffect(new PostProcessingEffect("Lighting", sizeXSSAO, sizeYSSAO, lighting, PPRenderingMethod.lightingPass, lightingExtractor));
     
            addPostProcessingEffect(new PostProcessingEffect("visualiseLight", sizeX / 4, sizeY / 4, visualiseLight, PPRenderingMethod.lightVisualisation, lightingExtractor));
            addPostProcessingEffect(guassianBlur, false);
            addPostProcessingEffect(tonemap);
        }


        public PostProcessingHandler(int windowWidth, int windowHeight)
        { 
            sizeX = windowWidth;
            sizeY = windowHeight;

            // G Buffer definition
            FBOTextureAttachment[] firstPassRenderAttachments =
                [new FBOTextureAttachment(FBOTEXTURETYPE.Colour, "gColour"),
                new FBOTextureAttachment(FBOTEXTURETYPE.Colour, "gNormal"),
                new FBOTextureAttachment(FBOTEXTURETYPE.Colour, "gPosition"),
                new FBOTextureAttachment(FBOTEXTURETYPE.Depth, "gDepth")];
            gBuffer = new FrameBufferObject(sizeX, sizeY, firstPassRenderAttachments);

            initPostProcessingEffects();

        }
        

        // the main render function serves to iterate through the stack of Post processing effects and handle the final render pass.
        // it's design is general enough that it does not need change.
        public void render()
        {
            GL.Disable(EnableCap.DepthTest);
            postprocessingEffects[0].bind();
            postprocessingEffects[0].render(); // applies saturation effects and gives the output texture for all others to use.

            guassianBlur.setInputTexture(getTextureFrom("Highlights")); // resets the guassian blur to the first pass's mrt

            if (postprocessingEffects.Count > 1) postprocessingEffects[1].bind();

            // iterate through post processing effects
            for (int i = 1; i < postprocessingEffects.Count - 1; i++)
            {
                postprocessingEffects[i].render();
                postprocessingEffects[i + 1].bind();
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); 
            GL.Viewport(0, 0, sizeX, sizeY); 
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            postprocessingEffects[postprocessingEffects.Count - 1].render();
            

            // renable depth testing
            GL.Enable(EnableCap.DepthTest);
        }

        // this is used in main rendering function so all the render 3d models end up on a framebuffer object
        public void bindFirstPass()
        {
            gBuffer.Bind();
        }

        // used to safely add new post processing effects
        // note the use of 'sequential'. this is used so that some offshoots can render without removing the abstract of the screen texture being a batton of a textures.
        public void addPostProcessingEffect(PostProcessingEffect effect, bool sequential = true)
        {
            postprocessingEffects.Add(effect);

            // we want some effects to take an auxiliary role in the chain, eg. bluring an MRT for bloom
            // because of this only some elements will pass their textures to the next one
            // eg. depth of field -> saturation -> sharpness kernal, vs. branching, firstPass -> (highlights->blur) & ([blur]->HDR) -> output
            // note that if a texture is non-sequential it must manually set it's input texture elsewhere.
            if (sequential)
            {
                PostProcessingEffect currentEffect = postprocessingEffects[postprocessingEffects.Count - 1];
                PostProcessingEffect previousEffect = postprocessingEffects[indexOfLastSequentialPostProcessingEffect];

                indexOfLastSequentialPostProcessingEffect = postprocessingEffects.Count - 1;

                // makes the postprocessing effect take the input texture of the last sequential texture.
                currentEffect.setInputTexture(previousEffect.getTextureID());
            }
        }



        // this is a quality of life function so that it's easy for any function to just taken in the original data.
        public int getFirstColourPassTexture()
        {
            return DataHandler.Get.getFrameBufferHandler().getFrameBufferTexture("gColour");
        }
        // this is a quality of life function so it's easy to reference the depth from the camera.
        public int getFirstDepthPassTexture()
        {
            return DataHandler.Get.getFrameBufferHandler().getFrameBufferTexture("gDepth");
        }

        public PostProcessingEffect GetPostProcessingEffect(string name)
        {
            foreach (PostProcessingEffect effect in postprocessingEffects)
            {
                if (effect.name == name) return effect;
            }
            Console.WriteLine("No Effect Found called " + name);
            return postprocessingEffects[0];
        }

        // this is an aux quality of life command for getting FBO texture IDs from the dataHandler
        public static int getTextureFrom(string name)
        {
            return DataHandler.Get.getFrameBufferHandler().getFrameBufferTexture(name);
        }

    }




    /// <summary>
    /// 
    /// This is the additional Rendering methods for when a Post Processing Effect is called.
    /// 
    /// each rendering requires different amount of work from the CPU, this delegate let's us define these dynamically
    /// 
    /// to understand why, let's discuss blur on an MRT.
    /// To make blur effecient we need pingpong buffers or to modify state variables in the shader and change render targets
    /// whilst many others just require that we run the render on a screen quad and it's applied.
    /// 
    /// to make this abstraction we can dynamically insert a "PostProcessingRenderMethod[s]" 
    /// as the render() method when called.
    /// 
    /// </summary>
    /// <param name="state"> this is just a way to pass the internal variables of the postprocessing effect into the delegate</param>
    
    // this let's us dynammically change how the post processing effect renders (eg. like how the blur post processing effect takes in 
    public delegate void PostProcessingRenderMethod(PostProcessingEffect state);

    // gives us our rendering methods
    public static class PPRenderingMethod
    {
        // this is default, just runs the screenQuad to render.
        public static void defaultRender(PostProcessingEffect state)
        {
            state.screenQuad.Render();
        }

        public static void lightVisualisation(PostProcessingEffect state)
        {
            Shader shader = state.getShader();
            int flare1 = DataHandler.Get.getTextureHandler().getTexture("flare1").Handle;
            int flare2 = DataHandler.Get.getTextureHandler().getTexture("flare2").Handle;
            int lenticularHalo = DataHandler.Get.getTextureHandler().getTexture("lenticularHalo").Handle;

            state.setAuxTexture("gPosition", "gPosition", 1);
            
            state.setAuxTexture("flare1", flare1, 2);
            state.setAuxTexture("flare2", flare2, 3);
            state.setAuxTexture("lenticular", lenticularHalo, 4);

            float sizeX = state.frameBufferObject.Width;
            float sizeY = state.frameBufferObject.Height;

            GL.Viewport(0, 0, (int)(sizeX), (int)(sizeY));
            state.screenQuad.Render();

            GL.Viewport(0, 0, (int)(sizeX), (int)(sizeY));

        }
        public static void defaultRenderAdjustScreenSize(PostProcessingEffect state)
        {
            float sizeX = state.frameBufferObject.Width;
            float sizeY = state.frameBufferObject.Height;
            GL.Viewport(0, 0, (int)(sizeX), (int)(sizeY));
            state.screenQuad.Render();
            GL.GenerateTextureMipmap(state.getTextureID());
            GL.Viewport(0, 0, (int)(DataHandler.Get.sizeX), (int)(DataHandler.Get.sizeY));
            

        }
        public static void ssao(PostProcessingEffect state)
        {
            float sizeX = state.frameBufferObject.Width;
            float sizeY = state.frameBufferObject.Height;
            GL.Viewport(0, 0, (int)(sizeX), (int)(sizeY));

            state.getShader().Use();
            int ssaoNoiseTextureID = (int)(state.getAuxData("ssaoNoise")[0]);
            state.getShader().SetVec3Array("samples", state.getAuxData("kernal")); // I am overbinding this, can fix later, for now I just want to justify the existance of "getAuxData" variable

            state.setAuxTexture("gNormal", "gNormal", 0);
            state.setAuxTexture("gPosition", "gPosition", 1);
            state.setAuxTexture("texNoise", ssaoNoiseTextureID, 2);

            state.screenQuad.SimpleRender();
        }
        public static void lightingPass(PostProcessingEffect state)
        {
            state.setAuxTexture("gColour", "gColour", 0);
            state.setAuxTexture("gNormal", "gNormal", 1);
            state.setAuxTexture("gPosition", "gPosition", 2);
            state.setAuxTexture("AO", "ssao", 3);

            state.setAuxTexture("shadowMap", "DirectionalShadowMap", 15);

            LightingHandler lightingHandler = DataHandler.Get.getLightingHandler();

            

            state.screenQuad.SimpleRender();
            GL.GenerateTextureMipmap(state.getTextureID());
        }

        public static void renderWithBloomBlur(PostProcessingEffect state)
        {
            state.setAuxTexture("bloomBlur", "guassianBlur", 1); // updates the texture to be the bloom blur texture
            state.screenQuad.Render();
        }

        // guassian blur uses pingpong buffer iteration to achieve more blur, as such it has a loop required and multiple render passes to save computation time.
        // note you need to set the input texture outside this method.
        public static void PingPongGuassianBlur(PostProcessingEffect state)
        {
            const int iterations = 2;

            float sizeX = state.frameBufferObject.Width;
            float sizeY = state.frameBufferObject.Height;

            state.bind();
            Shader shader = state.screenQuad.shader;
            shader.Use();

            GL.Viewport(0, 0, (int)(sizeX), (int)(sizeY));
            // first render is applied to the input texture.
            shader.SetInt("horizontal", 0);  state.screenQuad.Render();
            GL.GenerateTextureMipmap(state.getTextureID());

            // second render pass is applied onto it's self
            state.setInputTexture(state.getTextureID());
            shader.SetInt("horizontal", 1); state.screenQuad.Render();

            for (int i = 1; i < iterations; i++)
            {
                shader.SetInt("horizontal", 0);
                state.screenQuad.Render();
                shader.SetInt("horizontal", 1);
                state.screenQuad.Render();
            }
            GL.Viewport(0, 0, (int)(DataHandler.Get.sizeX), (int)(DataHandler.Get.sizeY));
        }
    }

    /// <summary>
    /// 
    /// 
    /// The Post Processing Struct is just an abstraction of dealing with FBO's directly onto Screen Quads
    /// it let's us just define the inputs and outputs and move on without manually defining them.
    /// 
    /// Although it should be noted still that with increased complexity it's important to view this as just an 
    /// fbo+screenQuad which is abstracted.
    /// 
    /// Primary Methods
    ///     - Bind() 
    ///     - Render()
    ///     - setAuxTexture()
    /// </summary>
    public struct PostProcessingEffect {
        public FrameBufferObject frameBufferObject;
        public ScreenQuad screenQuad;
        public PostProcessingRenderMethod renderMethod;
        public Dictionary<string, float[]> auxData = new Dictionary<string, float[]>();

        public string name;
        public PostProcessingEffect(string name, int width, int height, Shader shader, PostProcessingRenderMethod? renderMethodPostProcessing = null, FrameBufferObject? fbo = null) {
            this.name = name;
            frameBufferObject = fbo ?? new FrameBufferObject(width, height, [new FBOTextureAttachment(FBOTEXTURETYPE.Colour, name)]);
            screenQuad = new ScreenQuad(shader, getTextureFrom(frameBufferObject.mainTexture));
            renderMethod = renderMethodPostProcessing ?? PPRenderingMethod.defaultRender;
        }

        // this method is for adding additional textures to an post processing effect (eg. if it needs a depth & a velocity buffer, then set depth primary and 'add' velocity buffer)
        // additional this function can be usurped by object.shader.setint(..., ...)
        public void setAuxTexture(string nameOfParameterInShader, string TextureName, int textureUnit)
        {
            int textureID = DataHandler.Get.getFrameBufferHandler().getFrameBufferTexture(TextureName);
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            screenQuad.shader.SetInt(nameOfParameterInShader, textureUnit); ;
        }
        // override which uses textureNumber instead of referencing it with the FBO handlers dict
        public void setAuxTexture(string nameOfParameterInShader, int textureID, int textureUnit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            screenQuad.shader.SetInt(nameOfParameterInShader, textureUnit); ;
        }
        // every screen quad has a main texture it renders from, this method just acts to set that to some textureID
        public void setInputTexture(int TextureID)
        {
            screenQuad.textureHandle = TextureID;
        }

        // when active all draw calls are rendering onto this screen quad
        public void bind()
        {
            frameBufferObject.Bind();
        }
        //to store uniforms such as SSAO kernals etc. for uniform variables
        public void addAuxData(string name, float[] data)
        {
            auxData.Add(name, data);
        }


        // calls the prefered delegate method inputed (typically just 'screenQuad.render()')
        public void render()
        {
            renderMethod(this);
        }

        // gets the primary output texture
        public int getTextureID()
        {
            return getTextureFrom(frameBufferObject.mainTexture);
        }
        //to store uniforms such as SSAO kernals etc.
        public float[] getAuxData(string name)
        {
            return auxData[name];
        }
        // quality of life so you can directly access the shader from the state.
        public Shader getShader()
        {
            return this.screenQuad.shader;
        }

        // this is an aux quality of life command for getting FBO texture IDs from the dataHandler
        public static int getTextureFrom(string name)
        {
            return DataHandler.Get.getFrameBufferHandler().getFrameBufferTexture(name);
        }
    }

}
