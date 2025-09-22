using OpenGLGameEngine.rendering.lowLevelClasses.FBO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLGameEngine.rendering.lowLevelClasses.shader
{
    public class ShaderHandler
    {
        /// <summary>
        /// Handling and preventing unneeded Shader rewrites is an important aspect of performance
        /// 
        /// the goal of this class is to have all the required shaders attched and easy to access for many classes to reuse.
        /// 
        /// we define these shaders in the constructor (to keep them all together)
        /// 
        /// Primary method[s]
        ///     1. getShader(string name){}
        /// 
        /// 
        /// 
        /// </summary>
        /// 

        // Shaders Data
        private Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
        readonly string directory = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
        public ShaderHandler()
        {

            // Physical Shaders
            Shader defaultShader = new Shader(directory + "\\shaders\\default.vert", directory + "\\shaders\\default.frag");
            Shader texturedGBuffer = new Shader(directory + "\\shaders\\gbuffer\\gBuffer.vert", directory + "\\shaders\\gbuffer\\gBuffer.frag");

            Shader defaultInstanced = new Shader(directory + "\\shaders\\instanced\\instanced_model\\instanced_model.vert", directory + "\\shaders\\instanced\\instanced_model\\instanced_model.frag");
            Shader depthOnlyShader = new Shader(directory + "\\shaders\\shadow\\depthPass.vert", directory + "\\shaders\\shadow\\depthPass.frag");
            Shader instancedDepthOnlyShader = new Shader(directory + "\\shaders\\shadow\\depthPassInstanced.vert", directory + "\\shaders\\shadow\\depthPassInstanced.frag");
            Shader defaultBillboard = new Shader(directory + "\\shaders\\instanced\\billboard.vert", directory + "\\shaders\\instanced\\billboard.frag");

            shaders.Add("default", defaultShader);
            shaders.Add("deferredRender", texturedGBuffer);
            shaders.Add("defaultInstanced", defaultInstanced);
            shaders.Add("depthPass", depthOnlyShader);
            shaders.Add("depthPassInstanced", instancedDepthOnlyShader);
            shaders.Add("billboard", defaultBillboard);

            // Post Processing Shaders
            Shader defaultPostProcessing = new Shader(directory + "\\shaders\\postprocessing\\screenQuad.vert", directory + "\\shaders\\postprocessing\\default.frag");
            Shader lighting = new Shader("C:\\Users\\alexa\\source\\repos\\Callus\\OpenGLGameEngine\\shaders\\postprocessing\\lighting\\lighting.vert", "C:\\Users\\alexa\\source\\repos\\Callus\\OpenGLGameEngine\\shaders\\postprocessing\\lighting\\lighting.frag");
            Shader guassianBlur = new Shader(directory + "\\shaders\\postprocessing\\screenQuad.vert", directory + "\\shaders\\postprocessing\\guassianBlur.frag");
            Shader toneMapper = new Shader(directory + "\\shaders\\postprocessing\\screenQuad.vert", directory + "\\shaders\\postprocessing\\tonemapping\\ToneMapping.frag");
            Shader ssao = new Shader(directory + "\\shaders\\postprocessing\\screenQuad.vert", directory + "\\shaders\\postprocessing\\ssao\\ssao.frag");
            Shader ssaoBlur = new Shader(directory + "\\shaders\\postprocessing\\screenQuad.vert", directory + "\\shaders\\postprocessing\\ssao\\ssaoBlur.frag");
            Shader lightVisualisation = new Shader(directory + "\\shaders\\postprocessing\\screenQuad.vert", directory + "\\shaders\\postprocessing\\lighting\\lightSourceOverlay.frag");


            shaders.Add("lighting", lighting);
            shaders.Add("guassianBlur", guassianBlur);
            shaders.Add("toneMapper", toneMapper);
            shaders.Add("ssao", ssao);
            shaders.Add("ssaoBlur", ssaoBlur);
            shaders.Add("lightVisualisation", lightVisualisation);
        }

        public Shader getShader(string shaderName)
        {
            shaders.TryGetValue(shaderName, out var shader);
            if (shader == null)
            {
                throw new Exception("Not Shader Named \"" + shaderName + "\"");
            }
            return shader;
        }
    }
}
