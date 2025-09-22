using OpenGLGameEngine.objects.lights.main_class;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;


namespace OpenGLGameEngine.rendering.lowLevelClasses.lighting
{
    public enum LIGHT_TYPE
    {
        dirLight = 0,
        pointLight = 1,
        spotLight = 2
    }
    enum LIGHT_COUNT
    {
        dirLight = 1,
        pointLight = 16,
        spotLight = 16
    }
    enum BYTESREQUIRED
    {
        dirLight = 64,
        pointLight = 80,
        spotLight = 96
    }

    
    public class LightingHandler
    {
        List<Light> lights = new List<Light>();                                     // big dumb of the lights                        
        private Dictionary<string, (int index, int indexInUBO)> lightsDict = new Dictionary<string, (int index, int indexInUBO)>(); // stores lights by name for getters/setters
        int[] lightIndexes = new int[Enum.GetValues(typeof(LIGHT_TYPE)).Length]; // stores where we put the next light in the array
        UniformBufferObject lightUBO;                                           // use to add the lights into the UBO as an interface

        public LightingHandler() {
            List<(std140DT, string)> lightDataStructure = new List<(std140DT, string)>();

            (std140DT, string)[] directionalLightStructure = {
                (std140DT.Vec3, "direction"),

                (std140DT.Vec3, "ambient"),
                (std140DT.Vec3, "diffuse"),
                (std140DT.Vec3, "specular")
            };

            (std140DT, string)[] pointLightStructure = {
                (std140DT.Vec3, "position"),

                (std140DT.Vec3, "ambient"),
                (std140DT.Vec3, "diffuse"),
                (std140DT.Vec3, "specular"),

                (std140DT.Single, "constant"),
                (std140DT.Single, "linear"),
                (std140DT.Single, "quadratic"),
                (std140DT.Single, "padding0")   // padding
            };

            (std140DT, string)[] spotLightStructure = {
                (std140DT.Vec3, "position"),
                (std140DT.Vec3, "direction"),

                (std140DT.Single, "innerAngle"),
                (std140DT.Single, "outerAngle"),
                (std140DT.Single, "padding0"),  // padding
                (std140DT.Single, "padding1"),  // padding

                (std140DT.Vec3, "ambient"),
                (std140DT.Vec3, "diffuse"),
                (std140DT.Vec3, "specular"),

            };


            // adds the constants
            lightDataStructure.Add((std140DT.Single, "directionalLightsNum"));
            lightDataStructure.Add((std140DT.Single, "pointLightsNum"));
            lightDataStructure.Add((std140DT.Single, "spotLightsNum"));
            lightDataStructure.Add((std140DT.Single, "padding0"));               // padding

            // add directional lights, point lights, & spot lights to the directories
            for (int i = 0; i < (int)LIGHT_COUNT.dirLight; i++)
            {
                for (int j = 0; j < directionalLightStructure.Length; j++)
                {
                    (std140DT, string) currentDT = directionalLightStructure[j];
                    lightDataStructure.Add((currentDT.Item1, $"dirLight[{i}]" + "." + currentDT.Item2));
                }
            }
            // add point lights, point lights, & spot lights to the directories
            for (int i = 0; i < (int)LIGHT_COUNT.pointLight; i++)
            {
                for (int j = 0; j < pointLightStructure.Length; j++)
                {
                    lightDataStructure.Add((pointLightStructure[j].Item1, $"pointlight[{i}]" + "." + pointLightStructure[j].Item2));
                }
            }
            // add spot lights, point lights, & spot lights to the directories
            for (int i = 0; i < (int)LIGHT_COUNT.spotLight; i++)
            {
                for (int j = 0; j < spotLightStructure.Length; j++)
                {
                    lightDataStructure.Add((spotLightStructure[j].Item1, $"spotLight[{i}]" + "." + spotLightStructure[j].Item2));
                }
            }

            // creates the ubo to handle this
            lightUBO = new UniformBufferObject(BINDING_UBO.Lights, lightDataStructure.ToArray(), BufferUsageHint.DynamicDraw);
            
        }






        public void addLights(Light[] lightsInScene)
        {
            //update lights
            foreach (var light in lightsInScene) {
                lights.Add(light);                                                  // add lights to the big dumb list
                light.addToUBO(lightUBO, lightIndexes[light.getLightType()]);  // updates the UBO to include the light data

                lightsDict.Add(light.getName(), (lights.Count - 1, lightIndexes[light.getLightType()]));

                lightIndexes[light.getLightType()]++;
            }

            lightUBO.setInt("directionalLightsNum", lightIndexes[(int)LIGHT_TYPE.dirLight]);
            lightUBO.setInt("pointLightsNum", lightIndexes[(int)LIGHT_TYPE.pointLight]);
            lightUBO.setInt("spotLightsNum", lightIndexes[(int)LIGHT_TYPE.spotLight]);


        }












        /*
         *                          -=-=- [GETTERS & SETTERS] -=-=-
         */

        // update position of light
        public void setPosition(string name, Vector3 newPos)
        {
            if (!lightsDict.TryGetValue(name, out var index)) { Console.WriteLine("No Light Found Called: " + name); return; }
            if (lights[index.index] is IHasPositionUBO light)
            {
                light.setPosition(newPos, lightUBO);
            }
        }

        public Vector3? getPosition(string name)
        {
            // try and find the index in the dict
            if (!lightsDict.TryGetValue(name, out var index))  { return null; }

            // if present check if it has position, if so return it
            if (lights[index.index] is IHasPositionUBO light)
            {
                return light.getPosition();
            }
            return null;
        }

        //update direction of light
        public void setDirection(string name, Vector3 newDir)
        {
            if (!lightsDict.TryGetValue(name, out var index)) { Console.WriteLine("No Light Found Called: " + name); return; }
            if (lights[index.index] is IHasDirectionUBO light)
            {
                light.setDirection(newDir, lightUBO);
            }

        }

        public Vector3? getDirection(string name)
        {
            // try and find the index in the dict
            if (!lightsDict.TryGetValue(name, out var index)) { return null; }

            // if present check if it has position, if so return it
            if (lights[index.index] is IHasDirectionUBO light)
            {
                return light.getDirection();
            }
            return null;
        }

        //light colour updaters
        public void setAmbient(string name, Vector3 ambient)
        {
            if (!lightsDict.TryGetValue(name, out var index)) { Console.WriteLine("No Light Found Called: " + name); return; }
            if (lights[index.index] is IHasColour light)
            {
                light.setAmbient(ambient);
            }

        }
        public Vector3? getAmbient(string name)
        {
            // try and find the index in the dict
            if (!lightsDict.TryGetValue(name, out var index)) { return null; }

            // if present check if it has position, if so return it
            if (lights[index.index] is IHasColour light)
            {
                return light.getAmbient();
            }
            return null;
        }
        public void setDiffuse(string name, Vector3 diffuse)
        {
            if (!lightsDict.TryGetValue(name, out var index)) { Console.WriteLine("No Light Found Called: " + name); return; }
            if (lights[index.index] is IHasColour light)
            {
                light.setDiffuse(diffuse);
            }

        }
        public Vector3? getDiffuse(string name)
        {
            // try and find the index in the dict
            if (!lightsDict.TryGetValue(name, out var index)) { return null; }

            // if present check if it has position, if so return it
            if (lights[index.index] is IHasColour light)
            {
                return light.getDiffuse();
            }
            return null;
        }
        public void setSpecular(string name, Vector3 specular)
        {
            if (!lightsDict.TryGetValue(name, out var index)) { Console.WriteLine("No Light Found Called: " + name); return; }
            if (lights[index.index] is IHasColour light)
            {
                light.setSpecular(specular);
            }

        }
        public Vector3? getSpecular(string name)
        {
            // try and find the index in the dict
            if (!lightsDict.TryGetValue(name, out var index)) { return null; }

            // if present check if it has position, if so return it
            if (lights[index.index] is IHasColour light)
            {
                return light.getSpecular();
            }
            return null;
        }

        public UniformBufferObject getLightUBO()
        {
            return lightUBO;
        }
    }
}
