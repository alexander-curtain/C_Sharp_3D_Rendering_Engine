using OpenGLGameEngine.aux_functions;
using OpenGLGameEngine.objects;
using OpenGLGameEngine.rendering.lowLevelClasses.FBO;
using OpenGLGameEngine.rendering.lowLevelClasses.imgui;
using OpenGLGameEngine.rendering.lowLevelClasses.lighting;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenGLGameEngine.rendering.lowLevelClasses.rendering_pipelining
{


    public class RenderingHandler
    {
        private List<RenderStage> renderStageStack = new List<RenderStage>();

        public RenderingHandler()
        {

            // define the render stage stack
            renderStageStack.Add(RenderStages.stageDefaultShadow);

            renderStageStack.Add(RenderStages.SetupScreenSpaceRendering);
            renderStageStack.Add(RenderStages.deferred);
            renderStageStack.Add(RenderStages.skybox);
            renderStageStack.Add(RenderStages.postProcessing);

            //renderStageStack.Add(RenderStages.billboards);
            //renderStageStack.Add(RenderStages.otherModels);



            //renderStageStack.Add(RenderStages.transparentObjects);


        }


        public void Render()
        {
            // -=-=-=-=-=-=-=-=-=- [RENDERING MODELS BEGINS] -=-=-=-=-=-=-=-=-=-
            // loop through render stages
            foreach (RenderStage renderStage in renderStageStack)
            {
                renderStage();
            }

            // -=-=-=-=-=-=-=-=-=- [RENDERING MODELS ENDS] -=-=-=-=-=-=-=-=-=-
        }
    }
}
