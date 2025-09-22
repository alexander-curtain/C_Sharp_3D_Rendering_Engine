using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLGameEngine.rendering.lowLevelClasses.FBO
{
    public enum FBOTEXTURETYPE
    {
        Colour = 0,
        SingleColourChannel = 2,
        Depth = 1,
    }
    /// <summary>
    /// 
    /// Only one important part of the FBO Texture Attchment must be meantioned, that is that the first of the list 
    /// will always take prioty as the 'main' texture of the FBO when in a shader.
    /// 
    /// </summary>
    public struct FBOTextureAttachment
    {
        public FBOTEXTURETYPE attachment;
        public string name;

        public FBOTextureAttachment(FBOTEXTURETYPE textureType, string name) {
            attachment = textureType;
            this.name = name;
        }
    }
    /// <summary>
    /// 
    /// FrameBufferObject
    /// 
    /// The Frame Buffer Object itself doesn't have much power, it acts primarily as a constructor of the framebuffer
    /// and as a way to easily bind the framebuffer as the render target.
    /// 
    /// the colour attachments are handled with the Frame Buffer Handler which let's you access the textureID's via the name allocated to them.
    /// 
    /// Primary Methods:
    ///     1. FrameBufferObject(int width, int height, FrameBufferAttachment[] attachments)
    ///     2. Bind()
    /// 
    /// </summary>
    public class FrameBufferObject
    {
        int Handle;
        public int Width;
        public int Height;

        public string mainTexture { get; }
        List<string> attchmentNames = new List<string>();

        public void Bind() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
        public static void Unbind() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);


        public FrameBufferObject(int width, int height, FBOTextureAttachment[] attachments)
        {
            mainTexture = attachments[0].name;
            Width = width;
            Height = height;

            Handle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

            int numberOfColourAttachments = 0;
            FrameBufferHandler frameBufferHandler = DataHandler.Get.getFrameBufferHandler();

            // run through all the attchments given
            foreach (FBOTextureAttachment attachment in attachments)
            {
                attchmentNames.Add(attachment.name);
                // Remember when adding a new element type to include "frameBufferHandler.addFrameBufferTexture(attachment.name, colourID);"
                
                // each attchment requires a seperate method for binding it to the FBO, this acts to iterate through all the expected attchments and complete them.
                switch (attachment.attachment)
                {
                    case FBOTEXTURETYPE.Colour:
                        int colourID = GL.GenTexture();

                        frameBufferHandler.addFrameBufferTexture(attachment.name, colourID);

                        GL.BindTexture(TextureTarget.Texture2D, colourID);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, nint.Zero);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + numberOfColourAttachments,
                            TextureTarget.Texture2D, colourID, 0);

                        numberOfColourAttachments++;
                        break;

                    case FBOTEXTURETYPE.Depth:
                        int depthID = GL.GenTexture();
                        frameBufferHandler.addFrameBufferTexture(attachment.name, depthID);

                        GL.BindTexture(TextureTarget.Texture2D, depthID);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, width, height,
                            0, PixelFormat.DepthComponent, PixelType.UnsignedInt, nint.Zero);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthID, 0);

                        break;

                    case FBOTEXTURETYPE.SingleColourChannel:
                        int singleColourID = GL.GenTexture();
                        frameBufferHandler.addFrameBufferTexture(attachment.name, singleColourID);

                        GL.BindTexture(TextureTarget.Texture2D, singleColourID);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R16, width, height, 0, PixelFormat.Red, PixelType.Float, nint.Zero);

                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + numberOfColourAttachments,
                            TextureTarget.Texture2D, singleColourID, 0);

                        numberOfColourAttachments++;
                        break;

                    default:
                        Console.WriteLine("invalid attchment number, check that this attachment type is valid");
                        break;
                }
            }

            // if number of colour attachments is >0 then we set that number of colour buffers to draw (eg. mrt with 4 targets)
            // else we disable the buffers rendering pipeline to save time rendering just a depth buffer
            if (numberOfColourAttachments > 0)
            {
                var colourAttachments = Enumerable.Range(0, numberOfColourAttachments).Select(i => DrawBuffersEnum.ColorAttachment0 + i).ToArray();

                GL.DrawBuffers(numberOfColourAttachments, colourAttachments);
            }
            else
            {
                GL.DrawBuffer(DrawBufferMode.None);
                GL.ReadBuffer(ReadBufferMode.None);
            }

            //Debugging
            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new Exception($"Framebuffer incomplete: {status}");

            // unbinds
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        }
    }
}
