using ImGuiNET;
using OpenGLGameEngine.aux_functions;
using OpenGLGameEngine.objects;
using OpenGLGameEngine.objects.Functional;
using OpenGLGameEngine.objects.lights;
using OpenGLGameEngine.objects.lights.main_class;
using OpenGLGameEngine.rendering;
using OpenGLGameEngine.rendering.lowLevelClasses.imgui;
using OpenGLGameEngine.rendering.lowLevelClasses.lighting;
using OpenGLGameEngine.rendering.lowLevelClasses.rendering_pipelining;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenGLGameEngine.textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.IO;
using System.Runtime.Intrinsics.X86;

namespace OpenGLGameEngine
{
    public class Window : GameWindow
    {
        private float time = 0;
        private bool mouseGrabbed = true;
        private float lightscalar = 3.5f;
        private float radiusAO = 0.667f;
        private float biasAO = 0.03125f;
        private float powerAO = 4.0f;

        System.Numerics.Vector3 lightcolour = new System.Numerics.Vector3(0.0f);
        System.Numerics.Vector3 lightDir = new System.Numerics.Vector3(1.0f, 0.4f, 1.0f);


        private RenderingHandler renderingHandler;
        private ImGuiController _imguiController;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {

        }

        protected override void OnLoad()
        {
            base.OnLoad();

            string exeDirectory = AppContext.BaseDirectory;
            string directory = Directory.GetParent(exeDirectory).Parent.Parent.Parent.FullName;

            _imguiController = new ImGuiController(ClientSize.X, ClientSize.Y);

            // binds the cursor to be locked in the middle of the screen
            CursorState = CursorState.Grabbed;



            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            // transparency
            GL.Enable(EnableCap.Blend); // enable transparency
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); // blending mode for transparent materials
            // depth rendering
            GL.Enable(EnableCap.DepthTest);           // enables drawing furtherest then nearest fragments
            GL.DepthFunc(DepthFunction.Less);
            // backfrace culling
            GL.Enable(EnableCap.CullFace);            // Enable face culling
            GL.FrontFace(FrontFaceDirection.Ccw);     // Define front face (counter-clockwise by default)

            const float sunLightMult = 1.0f;
            const float carryovertoambient = 1.0f;

            DataHandler.Get.setWindowSize(Size.X, Size.Y);

            // generates lights in the scene
            PointLight light = new PointLight("orbit", new Vector3(0.0f, 5.0f, 0.0f), new Vector3(0.1f), new Vector3(0.7f), new Vector3(1.0f), new Vector3(0.50f, 0.05f, 0.0032f));
            DirectionalLight directionalLight = new DirectionalLight("sun", new Vector3(0.414f, 0.15f, 0.414f), new Vector3(0.0f * sunLightMult * carryovertoambient, 0.1f * sunLightMult * carryovertoambient, 0.23f * sunLightMult * carryovertoambient), new Vector3(1.0f * sunLightMult, 0.91f * sunLightMult, 0.68f * sunLightMult), new Vector3(1.0f * sunLightMult));
            SpotLight spotLight1 = new SpotLight("toplight", new Vector3(0.0f, -2.0f, 1.0f), new Vector3(0.0f, 1.0f, 0.0f), float.Pi / 12, float.Pi / 12 + 0.1f, new Vector3(0.5f*0.25f, 0.41f*0.25f, 0.3529f*0.25f), new Vector3(0.5f * 5f, 0.41f * 5f, 0.3529f * 5f), new Vector3(0.1f));
            
            
            Material mat = new Material(FULLTEXTURE.loadFromFolder("C:\\Users\\alexa\\source\\repos\\Callus\\OpenGLGameEngine\\textures\\stonebrick\\"));
            Material mat2 = new Material(FULLTEXTURE.loadFromFolder("C:\\Users\\alexa\\source\\repos\\Callus\\OpenGLGameEngine\\textures\\parallaxmaptut\\"));
            
            Model cube = new Model("cube", "C:\\Users\\alexa\\source\\repos\\Callus\\OpenGLGameEngine\\testModels\\cube.obj", mat);

            Model cube1 = new Model("cube1", "C:\\Users\\alexa\\source\\repos\\Callus\\OpenGLGameEngine\\testModels\\cubeRounded.obj", mat);

            Model cube2 = new Model("cube2", "C:\\Users\\alexa\\source\\repos\\Callus\\OpenGLGameEngine\\testModels\\AOCube.obj", mat);

            cube2.setModelMatrix(Matrix4.CreateTranslation(-3.0f, 0.0f, 0.0f));

            cube.setModelMatrix(Matrix4.CreateScale(20.0f, 0.1f, 20.0f) * Matrix4.CreateTranslation(0f, -5f, 0f));

            //Model ball = new Model("ball", "C:\\Users\\alexa\\source\\repos\\Callus\\OpenGLGameEngine\\testModels\\8ball.obj", mat);
            
            SkyBox skybox = new SkyBox("C:\\Users\\alexa\\source\\repos\\Callus\\OpenGLGameEngine\\textures\\skybox2\\", ".png");
            
            List<Light> lights = new List<Light>();
            Random rand = new Random();
            float displacement = 10.0f;

            for (int i = 0; i < 0; i++)
            {
                lights.Add(new PointLight($"{i} light", new Vector3((2.0f*(float)rand.NextDouble() - 1.0f)* displacement, (2.0f * (float)rand.NextDouble() - 1.0f) * displacement, (2.0f * (float)rand.NextDouble() - 1.0f) * displacement), new Vector3(0.3f), new Vector3(0.4f), new Vector3(1.0f), new Vector3(0.50f, 0.05f, 0.0032f)));
            }


            for (int i = 0; i < 0; i++)
            {
                lights.Add(new SpotLight($"{i} slight", new Vector3((2.0f * (float)rand.NextDouble() - 1.0f) * displacement, (2.0f * (float)rand.NextDouble() - 1.0f) * displacement, (2.0f * (float)rand.NextDouble() - 1.0f) * displacement), new Vector3(0.0f, 1.0f, 0.0f), float.Pi / 12, float.Pi / 12 + 0.1f, new Vector3(0.1f), new Vector3(0.9f, 0.81f, 0.2f), new Vector3(1.0f)));
            }

            lights.Add(directionalLight);

            Scene scene = new Scene("main", [cube, cube1, cube2, skybox], lights.ToArray());

            DataHandler.Get.pushScene(scene);
            DataHandler.Get.setDirectionalShadowMap(new DirectionalShadowMap("sun", 4096, 40.0f, 50f));

            lightcolour = (System.Numerics.Vector3)DataHandler.Get.getLightingHandler().getDiffuse("sun").Value;

            renderingHandler = new RenderingHandler();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);


            // We add the time elapsed since last frame, times 4.0 to speed up animation, to the total amount of time passed.
            time += 1.0f * (float)e.Time;

            float xFloat = (float)Math.Sin(time);
            float zFloat = (float)Math.Cos(time);

            // renders
            renderingHandler.Render();

            ImGui.Begin("DebugBar");
            ImGui.Text("Your debug info here...");
            ImGui.SliderFloat("light multiply", ref lightscalar, 0f, 10f);
            ImGui.SliderFloat("light multiply", ref lightscalar, 0f, 10f);
            ImGui.SliderFloat3("lightDir", ref lightDir, -1f, 1f);
            ImGui.End();

            ImGui.Begin("debug");
            ImGui.Image(DataHandler.Get.getFrameBufferHandler().getFrameBufferTexture("ssao"), new System.Numerics.Vector2((float)Size.X / 5.0f, (float)Size.Y / 5.0f));
            ImGui.End();

            ImGui.Begin("shadowmap");
            ImGui.Image(DataHandler.Get.getFrameBufferHandler().getFrameBufferTexture("DirectionalShadowMap"), new System.Numerics.Vector2((float)Size.X / 5.0f, (float)Size.Y / 5.0f));
            ImGui.End();

            ImGui.Begin("debugSlider");
            ImGui.SliderFloat("Radius of SSAO", ref radiusAO, 0.0f, 5.0f);
            ImGui.SliderFloat("bias of SSAO", ref biasAO, -2.0f, 2.0f);
            ImGui.SliderFloat("Power of SSAO", ref powerAO, 0.1f, 5.0f);
            ImGui.End();

            Shader ssao = DataHandler.Get.getShaderHandler().getShader("ssao");

            ssao.SetFloat("radius", radiusAO);
            ssao.SetFloat("bias", biasAO);
            ssao.SetFloat("power", powerAO);


            Vector3 sunColourOpentk = (Vector3)lightcolour;

            DataHandler.Get.getLightingHandler().setDiffuse("sun", sunColourOpentk * new Vector3(lightscalar));
            DataHandler.Get.getLightingHandler().setSpecular("sun", new Vector3(1.0f) * new Vector3(lightscalar));
            DataHandler.Get.getLightingHandler().setDirection("sun", Vector3.Normalize((Vector3)lightDir));

            _imguiController.Render();

            
            SwapBuffers(); 

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            _imguiController.Update(this, (float)e.Time);

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            if (input.IsKeyReleased(Keys.G))
            {
                mouseGrabbed = !mouseGrabbed;
                DataHandler.Get.getCamera().updateDeltaXYMouse(MouseState);
            }

            if (mouseGrabbed)
            {
                CursorState = CursorState.Grabbed;
            } else
            {
                CursorState = CursorState.Normal;
            }



            if (input.IsKeyDown(Keys.Minus))
            {
                modifyExposure(0.0f);
            }
            if (input.IsKeyDown(Keys.Equal))
            {
                addExposure(1.0f);
            }


            if (mouseGrabbed)
            {
                DataHandler.Get.getCamera().useCamera(KeyboardState, MouseState, (float)e.Time);
            }
             
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            DataHandler.Get.sizeX = Size.X;
            DataHandler.Get.sizeY = Size.Y;

            _imguiController.WindowResized(ClientSize.X, ClientSize.Y);

            GL.Viewport(0, 0, Size.X, Size.Y);
            // TODO add method that updates aspect ratio of camera on rescale.
        }





        // remove later

        float exposure = 1.0f;
        public void addExposure(float newV)
        {
            Shader toneMappingPP = DataHandler.Get.getShaderHandler().getShader("toneMapper");
            exposure += 0.0001f;
            exposure *= 1.04f;
            toneMappingPP.Use();
            toneMappingPP.SetFloat("exposure", exposure);

        }
        public void modifyExposure(float newV)
        {
            Shader toneMappingPP = DataHandler.Get.getShaderHandler().getShader("toneMapper");
            exposure *= 0.96f;
            toneMappingPP.Use();
            toneMappingPP.SetFloat("exposure", exposure);

        }
    }

}