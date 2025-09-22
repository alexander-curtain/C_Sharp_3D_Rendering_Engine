using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenGLGameEngine.textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLGameEngine.rendering.lowLevelClasses.texture
{
    public class TextureHandler
    {
        private Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
        public TextureHandler() {
            string directory = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;

            // required textures
            Texture flare1 = Texture.LoadFromFile(directory + "\\textures\\flare\\flare.png", OpenTK.Graphics.OpenGL4.TextureWrapMode.ClampToBorder);
            Texture flare2 = Texture.LoadFromFile(directory + "\\textures\\flare\\flare2.png", OpenTK.Graphics.OpenGL4.TextureWrapMode.ClampToBorder);
            Texture halo = Texture.LoadFromFile(directory + "\\textures\\flare\\lenticularHalo.png", OpenTK.Graphics.OpenGL4.TextureWrapMode.ClampToBorder);

            addTexture("flare1", flare1);
            addTexture("flare2", flare2);
            addTexture("lenticularHalo", halo);
        }
        public void addTexture(string name, Texture texture)
        {
            textures.Add(name, texture);
        }

        public Texture getTexture(string name)
        {
            return textures[name];
        }


    }
}
