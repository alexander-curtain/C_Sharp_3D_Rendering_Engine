using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLGameEngine.textures
{
    public class CubeMap
    {
        public int Handle { get; private set; }
        public int Size { get; private set; }
        public SizedInternalFormat Format { get; private set; }

        private static readonly string[] DefaultOrder = {
            "right", "left", "top", "bottom", "front", "back"
        };


        // default constructor, lets you load cubemap from folder
        public CubeMap(string folderPath, string extension = ".png", string[] faceOrder = null)
        {
            faceOrder ??= DefaultOrder;
            if (faceOrder.Length != 6)
                throw new ArgumentException("Cubemap must have 6 face names.");

            Handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, Handle);

            StbImage.stbi_set_flip_vertically_on_load(0); // No flip for cubemaps

            for (int i = 0; i < 6; i++)
            {
                string path = Path.Combine(folderPath, faceOrder[i] + extension);
                if (!File.Exists(path))
                    throw new FileNotFoundException($"Cubemap face not found: {path}");

                using var stream = File.OpenRead(path);
                var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                if (i == 0)
                    Size = image.Width;

                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i,
                              0,
                              PixelInternalFormat.Rgba,
                              image.Width,
                              image.Height,
                              0,
                              PixelFormat.Rgba,
                              PixelType.UnsignedByte,
                              image.Data);
            }

            SetTextureParameters();
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }

        private void SetTextureParameters()
        {
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        }

        public void Bind(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.TextureCubeMap, Handle);
        }

        public void updateShader(Shader shader, int unit, string attributeName)
        {
            shader.SetInt(attributeName, unit);
        }
    }
}
