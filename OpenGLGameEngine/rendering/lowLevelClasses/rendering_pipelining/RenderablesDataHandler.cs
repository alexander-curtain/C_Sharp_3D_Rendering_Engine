using OpenGLGameEngine.objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLGameEngine.rendering.lowLevelClasses.rendering_pipelining
{


    /// <summary>
    /// 
    /// Below Represent the Basic building blocks of IRenderable types and sorting of their types for complexity in rendering
    /// 
    /// we will commonly reuse certain groups of objects in rendering, this leads to have 'flags' on them
    /// so we can specify which groups to sort them into, eg. rendering physical objects  in both a shader pass and a screen space pass.
    /// 
    /// One IRenderable can go into Multiple Rendering Groups.
    /// 
    /// in adding a field it is important that this is just a flag methods still need to be created to evoke them
    /// This includes two primary functions which need updating 
    ///     -GetRenderingGroup()
    ///     -filterIntoRenderingGroup()
    /// 
    /// in addition they will likely need to be added into a rendering stage, and that stage added to the stack of the rendering handler.
    /// 
    /// </summary>
    public readonly record struct RenderFlags(
     bool Physical = false,
     bool Instanced = false,
     bool CastShadows = false,
     bool Transparent = false,
     bool SkyBox = false,
     bool CustomShader = false,
     bool Billboard = false);

    public enum RENDERING_GROUP
    {
        physicalModels = 0,
        instancedModels = 1,
        shadowCast = 2,
        shadowCastInstance = 3,
        transparent = 4,
        skybox = 5,
        customShader = 6,
        billboards = 7
    }

    /// <summary>
    /// 
    /// In Trying to Render many objects we must confront that culling and handling of these objects typically requires CPU 
    /// usage such that we get the desired results.
    /// 
    /// Abstractions Made:
    ///     1. Renderable objects have 'Flags', some objects are instanced, some cast shadow, some are transparent.
    ///         such attrubutes require processing, For example, Transparency must be rendered last, and must be sorted into order of depth,
    ///         whilst objects which cast shadows need to be independently rendered in a 'shadow pass' such it brings us to...
    ///     2.  Rendering Groups, since some objects need to be grouped together to render commonly, we just store pointers to these objects in 
    ///         big lists, and run through them when called to render. 
    ///             this does come with a downside, the programmer needs to handle insertion conditions if they wish to use this feature.
    ///             see function "filterIntoRenderingGroup()" and "getRenderingGroup()"
    /// 
    /// Main Ideas in this class
    ///     1. Rendering Groups
    ///     2. Scenes
    /// 
    /// 
    /// </summary>
    public class RenderablesDataHandler
    {

        // Access Renderables by Name
        private Dictionary<string, IRenderable> _RenderableDict = new Dictionary<string, IRenderable>();


        private IRenderable skybox;
        private List<IRenderable> physicalModels = new List<IRenderable>();
        private List<IRenderable> instancedModels = new List<IRenderable>();
        private List<IRenderable> shadowCastingModels = new List<IRenderable>();
        private List<IRenderable> shadowCastingModelsInstanced = new List<IRenderable>();
        private List<IRenderable> transparent = new List<IRenderable>();
        private List<IRenderable> CustomShader = new List<IRenderable>();
        private List<IRenderable> Billboards = new List<IRenderable>();

        // object getter, takes in a name and returns the object it corosponds to.
        public IRenderable getObj(string name)
        {
            _RenderableDict.TryGetValue(name, out var obj);
            return obj;
        }

        //pushes the renderables to the scene, this is a required function called by the dataHandler.
        public void pushRenderables(IRenderable[] renderables)
        {
            foreach (IRenderable obj in renderables)
            {
                _RenderableDict.Add(obj.getName(), obj);
                filterIntoRenderingGroup(obj);
            }
        }


        // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
        //
        //                  [Functions which need to be updated when Rendering groups update!]
        //
        // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

        // Allocates how rendering flags imply that an object is pushed into rendering group lists!
        private void filterIntoRenderingGroup(IRenderable obj)
        {
            RenderFlags r = obj.getFlags();
            // default models
            if (r.Physical) { physicalModels.Add(obj); }
            if (r.Instanced) { instancedModels.Add(obj); }

            // shadow casting
            if (r.CastShadows && !r.Instanced) { shadowCastingModels.Add(obj); }
            if (r.CastShadows && r.Instanced) { shadowCastingModelsInstanced.Add(obj); }

            // complex
            if (r.Transparent) { transparent.Add(obj); }
            if (r.CustomShader) { CustomShader.Add(obj); }
            if (r.SkyBox) { skybox = obj; }
            if (r.Billboard) { Billboards.Add(obj); }
        }

        // let's us grab rendering groups via their enum rather than through a messy set of getters and setters.
        public List<IRenderable> getRenderingGroup(RENDERING_GROUP type)
        {
            switch (type)
            {
                case RENDERING_GROUP.physicalModels:
                    return physicalModels;

                case RENDERING_GROUP.instancedModels:
                    return instancedModels;

                case RENDERING_GROUP.shadowCast:
                    return shadowCastingModels;

                case RENDERING_GROUP.shadowCastInstance:
                    return shadowCastingModelsInstanced;

                case RENDERING_GROUP.transparent:
                    return transparent;

                case RENDERING_GROUP.customShader:
                    return CustomShader;

                case RENDERING_GROUP.billboards:
                    return Billboards;

                case RENDERING_GROUP.skybox:
                    List<IRenderable> skybox = new List<IRenderable>();
                    skybox.Add(this.skybox);
                    return skybox;



                default:
                    Console.WriteLine("Not valid Rendering Group");
                    return null;
            }
        }
    }
}
