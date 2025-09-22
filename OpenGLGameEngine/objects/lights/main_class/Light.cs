using OpenGLGameEngine.rendering.lowLevelClasses.lighting;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLGameEngine.objects.lights.main_class
{
    public abstract class Light
    {
        public static float calculateLightRadius(float intensity, Vector3 attenuation)
        {
            const float darkestvalue = 0.015625f;

            float constant = attenuation.X;
            float linear = attenuation.Y;
            float quadratic = attenuation.Z;

            // take the quadratic formula of the equation 0.015625 = Imax/Attenutation, where Attenutation is some equation of the form a*d^2 + b*d + c

            return (-linear + (float)Math.Sqrt((linear * linear) - 4f * quadratic * (constant - intensity * darkestvalue))) / (2f * quadratic);
        }
        public string name;
        public LIGHT_TYPE type;

        public string getName()
        {
            return name;
        }

        public int getLightType()
        {
            return (int)type;
        }

        public abstract void addToUBO(UniformBufferObject ubo, int index);
    }
}
