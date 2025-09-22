using OpenGLGameEngine.objects.lights.main_class;
using OpenGLGameEngine.rendering;
using OpenGLGameEngine.rendering.lowLevelClasses.lighting;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLGameEngine.objects.lights
{
    public class DirectionalLight : Light, IHasDirectionUBO, IHasColour
    {
        private Vector3 _direction;

        private Vector3 _ambient;
        private Vector3 _diffuse;
        private Vector3 _specular;

        int index;
        public DirectionalLight(string name, Vector3 direction, Vector3 ambient, Vector3 diffuse, Vector3 specular)
        {
            this.name = name; // inherited from Light
            this.type = LIGHT_TYPE.dirLight; // inherited from Light

            _direction = Vector3.Normalize(direction);
            _ambient = ambient;
            _diffuse = diffuse;
            _specular = specular;
        }

        public override void addToUBO(UniformBufferObject ubo, int index)
        {
            this.index = index;

            ubo.setVec3($"dirLight[{index}].direction", _direction);

            // Colour Setting 
            ubo.setVec3($"dirLight[{index}].ambient", _ambient);
            ubo.setVec3($"dirLight[{index}].diffuse", _diffuse);
            ubo.setVec3($"dirLight[{index}].specular", _specular);
        }

        public Vector3 getDirection()
        {
            return this._direction;
        }

        public void setDirection(Vector3 newDirection, UniformBufferObject ubo)
        {
            this._direction = newDirection;
            ubo.setVec3($"dirLight[{index}].direction", newDirection);
        }

        public Vector3 getAmbient()
        {
            return this._ambient;
        }

        public Vector3 getDiffuse()
        {
            return this._diffuse;
        }

        public Vector3 getSpecular()
        {
            return this._specular;
        }

        public void setAmbient(Vector3 colour)
        {
            this._ambient = colour;
            UniformBufferObject ubo = DataHandler.Get.getLightingHandler().getLightUBO();
            ubo.setVec3($"dirLight[{index}].ambient", _ambient);
        }

        public void setDiffuse(Vector3 colour)
        {
            this._diffuse = colour;
            UniformBufferObject ubo = DataHandler.Get.getLightingHandler().getLightUBO();
            ubo.setVec3($"dirLight[{index}].diffuse", _diffuse);
        }

        public void setSpecular(Vector3 colour)
        {
            this._specular = colour;
            UniformBufferObject ubo = DataHandler.Get.getLightingHandler().getLightUBO();
            ubo.setVec3($"dirLight[{index}].specular", _specular);
        }


    }
}
