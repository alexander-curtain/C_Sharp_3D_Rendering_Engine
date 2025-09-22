using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLGameEngine.aux_functions
{
    public static class AuxMath
    {
        public static float lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }
    }
}
