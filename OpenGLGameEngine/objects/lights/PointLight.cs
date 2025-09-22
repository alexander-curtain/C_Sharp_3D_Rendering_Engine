using OpenGLGameEngine.objects.lights.main_class;
using OpenGLGameEngine.rendering;
using OpenGLGameEngine.rendering.lowLevelClasses.lighting;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenTK.Mathematics;


namespace OpenGLGameEngine.objects.lights 
{
    public class PointLight : Light, IHasPositionUBO, IHasColour 
    {
        Vector3 position;
        int index;

        // colour
        Vector3 ambient;
        Vector3 diffuse;
        Vector3 specular;
        
        // attenuation http://www.ogre3d.org/tikiwiki/tiki-index.php?page=-Point+Light+Attenuation
        float constant;
        float linear;
        float quadratic;

        public Vector3 Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public PointLight(string name, Vector3 position, Vector3 ambient, Vector3 diffuse, Vector3 specular, Vector3 attenutation) {
            this.position = position;
            this.name = name;
            this.type = LIGHT_TYPE.pointLight;

            this.ambient = ambient;
            this.diffuse = diffuse;
            this.specular = specular;

            constant = attenutation.X;
            linear = attenutation.Y;
            quadratic = attenutation.Z;
        }


        public override void addToUBO(UniformBufferObject ubo, int index)
        {
            ubo.setVec3($"pointlight[{index}].position", position);

            // Colour Setting 
            ubo.setVec3($"pointlight[{index}].ambient", ambient);
            ubo.setVec3($"pointlight[{index}].diffuse", diffuse);
            ubo.setVec3($"pointlight[{index}].specular", specular);

            // Attenutation Factor
            ubo.setFloat($"pointlight[{index}].constant", constant);
            ubo.setFloat($"pointlight[{index}].linear", linear);
            ubo.setFloat($"pointlight[{index}].quadratic", quadratic);
        }

        public Vector3 getPosition() { return position; }

        public void setPosition(Vector3 newPosition, UniformBufferObject ubo)
        {
            position = newPosition;
            ubo.setVec3($"pointlight[{index}].position", position);
        }

        public Vector3 getAmbient()
        {
            return this.ambient;
        }

        public Vector3 getDiffuse()
        {
            return this.diffuse;
        }

        public Vector3 getSpecular()
        {
            return this.specular;
        }

        public void setAmbient(Vector3 colour)
        {
            this.ambient = colour;
            UniformBufferObject ubo = DataHandler.Get.getLightingHandler().getLightUBO();
            ubo.setVec3($"pointlight[{index}].ambient", ambient);
        }

        public void setDiffuse(Vector3 colour)
        {
            this.diffuse = colour;
            UniformBufferObject ubo = DataHandler.Get.getLightingHandler().getLightUBO();
            ubo.setVec3($"pointlight[{index}].diffuse", diffuse);
        }

        public void setSpecular(Vector3 colour)
        {
            this.specular = colour;
            UniformBufferObject ubo = DataHandler.Get.getLightingHandler().getLightUBO();
            ubo.setVec3($"pointlight[{index}].specular", specular);
        }
    }
}
