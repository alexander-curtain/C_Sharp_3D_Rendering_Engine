using OpenGLGameEngine.aux_functions;
using OpenGLGameEngine.objects;
using OpenGLGameEngine.objects.lights.main_class;
using OpenGLGameEngine.rendering.lowLevelClasses.FBO;
using OpenGLGameEngine.rendering.lowLevelClasses.lighting;
using OpenGLGameEngine.rendering.lowLevelClasses.rendering_pipelining;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenGLGameEngine.rendering.lowLevelClasses.texture;
using OpenTK.Mathematics;

namespace OpenGLGameEngine.rendering
{


    // Scenes are the basic building block of how we insert objects in 'chunks' together. with names, render objects and lights for the current scene.
    public struct Scene
    {
        public string name;
        public IRenderable[] renderables;
        public Light[] lights;
        public Scene(string name, IRenderable[] renderables, Light[] lights)
        {
            this.name = name;

            this.renderables = renderables;
            this.lights = lights;
        }
    }

    /// <summary>
    /// 
    /// The Purpose of the datahandler is to be a static class which allows us to gain access to other Handlers 
    /// and rendering objects (global variables)
    /// 
    /// The Data it contains generally relates to the state of the machine, this is state variables which will
    /// likely need to be accessed by many parts of the program, and as such it makes sense to include them here
    /// instead of passing them through or storing them in strange functions.
    /// 
    /// Aim primarily to use this as read only as multi threading may ruin code later if abused for write operations.
    /// 
    /// A basic summary of it's storage is as follows:
    ///     1. LightingHandler
    ///     2. UniformBufferHandler
    ///     3. FrameBufferHandler
    ///     4. PostProcessingHandler
    ///     5. Scenes/IRenderables Groups
    ///     -=-=-= complex -=-=-=-=-
    ///     6. Camera
    ///     7. Shaders
    ///     8. ShadowMap
    ///     
    /// Notable Exceptions:
    ///     1. it does not contain the rendering Handler (since ".render()" must be called in the window class)
    /// 
    /// The DataHandler is primarily getters and setters, although it does also handle the insertion and deletion of scenes
    /// this is okay for now, but once quadtrees or the so are implemented this may be passed onto a new handler.
    /// </summary>
    internal class DataHandler
    {
        private static DataHandler _instance;

        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *                      Data Stores
         * 
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */

        // Handlers
        private LightingHandler LightingHandler = new LightingHandler();
        private UniformBufferHandler uniformBufferHandler = new UniformBufferHandler();
        private FrameBufferHandler frameBufferHandler = new FrameBufferHandler();
        private PostProcessingHandler postProcessingHander;
        private ShaderHandler shaderHandler = new ShaderHandler();
        private RenderablesDataHandler RenderingGroupsHandler = new RenderablesDataHandler();
        private TextureHandler textureHandler = new TextureHandler();
        private List<Scene> _scenes = new List<Scene>();



        // complex datatypes
        DirectionalShadowMap dirShadowMap;
        Camera camera;

        // Aux Data
        public string directory = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;

        public int sizeX;
        public int sizeY;

        //
        //                          initialized data (eg. shaders)
        //
        private DataHandler()
        {
            // forces a camera
            this.camera = new Camera(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 5f, 0.03f, 1f, zFar: 70f);
        }


        // how to get reference to the dataHandler
        public static DataHandler Get
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataHandler();
                }
                return _instance;

            }
        }


        /*
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         *            Function Calls (getters/setters)
         *  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
         */

        //
        //          [Getters]
        //
        public ShaderHandler getShaderHandler()
        {
            return this.shaderHandler;
        }
        public UniformBufferHandler getUniformBufferHandler()
        {
            return this.uniformBufferHandler;
        }
        public LightingHandler getLightingHandler()
        {
            return this.LightingHandler;
        }
        public DirectionalShadowMap getDirectionalShadowMap()
        {
            return this.dirShadowMap;
        }
        public PostProcessingHandler getPostProcessingHandler()
        {
            return this.postProcessingHander;
        }
        public FrameBufferHandler getFrameBufferHandler()
        {
            return this.frameBufferHandler;
        }
        public TextureHandler getTextureHandler()
        {
            return this.textureHandler;
        }
        public Camera getCamera()
        {
            return this.camera;
        }
        public List<IRenderable> getRenderingGroup(RENDERING_GROUP type)
        {
            return this.RenderingGroupsHandler.getRenderingGroup(type);
        }








        //
        //          [Setters]
        //

        public void setDirectionalShadowMap(DirectionalShadowMap newShadowMap)
        {
            this.dirShadowMap = newShadowMap;
        }

        public void setWindowSize(int x, int y)
        {
            this.sizeX = x; this.sizeY = y;
            if (this.postProcessingHander == null)
            {
                postProcessingHander = new PostProcessingHandler(x, y);
            }

            this.camera = new Camera(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), 5f, 0.03f, sizeX / (float)sizeY, zFar: 70f);
        }
        public void pushScene(Scene scene)
        {
            this.LightingHandler.addLights(scene.lights);
            this._scenes.Add(scene);
            this.RenderingGroupsHandler.pushRenderables(scene.renderables);
            
        }








    }
}
