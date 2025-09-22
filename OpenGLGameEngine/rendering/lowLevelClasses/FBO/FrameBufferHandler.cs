using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenGLGameEngine.rendering.lowLevelClasses.FBO
{
    /// <summary>
    /// Accessing FBO's textures is fairly daunting,
    /// this class primarily serves storing them and accessing them
    /// 
    /// the high level idea is that all FBO textures have "names" (strings) which let you access the ID of their texture
    /// this means that in effects to get access to an FBO's texture you merely need to refer the name you need
    /// 
    /// main call within the class is ("getFrameBufferTexture"), but "addFrameBufferTexture" is also an important method.
    /// 
    /// 
    /// </summary>
    public class FrameBufferHandler
    {
        private Dictionary<string, int> texturesDict = new Dictionary<string, int>();
        public FrameBufferHandler() { 
        
        }

        public void addFrameBufferTexture(string name, int textureID)
        {
            texturesDict.Add(name, textureID);
        }

        public int getFrameBufferTexture(string textureName)
        {
            if (texturesDict.TryGetValue(textureName, out var index))
            { return index; }
            else 
            { Console.WriteLine("No FBO texture attachment called: " + textureName); throw new Exception("no FBO found with name " + textureName); }
        }
    }
}
