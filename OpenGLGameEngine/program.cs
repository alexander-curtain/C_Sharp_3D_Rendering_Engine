using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;



namespace OpenGLGameEngine
{
    public static class Program
    {
        private static void Main()
        {
            // edit window configuration
            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(1080, 800),
                Title = "Alex's cool engine",
                // This is needed to run on macos
                Flags = ContextFlags.ForwardCompatible,

            };

            var gameWindowSettings = new GameWindowSettings
            {
                UpdateFrequency = 60.0
            };


            using (Window window = new Window(gameWindowSettings, nativeWindowSettings)) {window.Run();}
            
        }
    }
}