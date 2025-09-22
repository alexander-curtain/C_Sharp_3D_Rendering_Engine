using OpenTK.Compute.OpenCL;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using System.Drawing;

using StbImageWriteSharp;
using OpenTK.Graphics.OpenGL4;
using System;


using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using ColorComponents = StbImageSharp.ColorComponents;


/*
 * Taken from https://github.com/opentk/LearnOpenTK/blob/master/Common/Texture.cs
 * 
 */

namespace OpenGLGameEngine.textures
{

    // A helper class, much like Shader, meant to simplify loading textures.
    public class Texture
    {
        public readonly int Handle;

        public static Texture LoadFromFile(string path, TextureWrapMode mode = TextureWrapMode.Repeat)
        {
            // Generate handle
            int handle = GL.GenTexture();

            // Bind the handle
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            StbImage.stbi_set_flip_vertically_on_load(1);

            // Here we open a stream to the file and pass it to StbImageSharp to load.
            using (Stream stream = File.OpenRead(path))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)mode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)mode);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return new Texture(handle);
        }

        public static Texture LoadFromFileNonImage(string path)
        {
            // Generate handle
            int handle = GL.GenTexture();

            // Bind the handle
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            StbImage.stbi_set_flip_vertically_on_load(1);

            // Here we open a stream to the file and pass it to StbImageSharp to load.
            using (Stream stream = File.OpenRead(path))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, image.Width, image.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, image.Data);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return new Texture(handle);
        }

        public static Texture GenerateTextureFromArray(float[] pixels, int rowLength, PixelFormat pixelFormat = PixelFormat.Rgba)
        {
            int handle = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            int floatsPerPixel = pixelFormat == PixelFormat.Red ? 1 : pixelFormat == PixelFormat.Rgb ? 3 : 4; // if you have a bug it's probably this line.

            int totalPixels = pixels.Length / floatsPerPixel;
            int height = (int)Math.Ceiling((float)totalPixels / (float)rowLength);


            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, rowLength, height, 0, pixelFormat, PixelType.Float, pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);


            return new Texture(handle);
        }

        public Texture(int glHandle)
        {
            Handle = glHandle;
        }

        // Activate texture
        // Multiple textures can be bound, if your shader needs more than just one.
        // If you want to do that, use GL.ActiveTexture to set which slot GL.BindTexture binds to.
        // The OpenGL standard requires that there be at least 16, but there can be more depending on your graphics card.
        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    public static void SaveTexture(int textureId, int width, int height, string filename)
        {
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            // Read pixels from GPU
            byte[] pixels = new byte[width * height * 4];
            GL.GetTexImage(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            // Flip vertically because OpenGL textures are bottom-left
            byte[] flipped = new byte[pixels.Length];
            int rowBytes = width * 4;
            for (int y = 0; y < height; y++)
            {
                Array.Copy(pixels, (height - 1 - y) * rowBytes, flipped, y * rowBytes, rowBytes);
            }

            // Write PNG safely
            using FileStream fs = File.OpenWrite(filename);
            var writer = new ImageWriter();
            writer.WritePng(flipped, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, fs);
        }



            public static void SaveFBOTexture(int fboId, int width, int height, string filename)
        { 
            // Bind the framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboId);

            // Make sure the read buffer is correct (for color attachments)
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);

            // Read pixels from the framebuffer
            byte[] pixels = new byte[width * height * 4];
            GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            // Flip vertically
            byte[] flipped = new byte[pixels.Length];
            int rowBytes = width * 4;
            for (int y = 0; y < height; y++)
                Array.Copy(pixels, (height - 1 - y) * rowBytes, flipped, y * rowBytes, rowBytes);


            // Write PNG using StbImageWriteSharp
            using FileStream fs = File.OpenWrite(filename);
            var writer = new ImageWriter();
            writer.WritePng(flipped, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, fs);

            // Unbind framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }


}

}