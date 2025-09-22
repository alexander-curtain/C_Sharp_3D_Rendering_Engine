using OpenGLGameEngine.objects.lights.main_class;
using OpenGLGameEngine.rendering;
using OpenGLGameEngine.rendering.lowLevelClasses.lighting;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenGLGameEngine.objects.lights
{
    public class SpotLight : Light, IHasDirectionUBO, IHasPositionUBO, IHasColour
    {
        Vector3 _position;
        Vector3 _direction;
        float _innerAngle;
        float _outerAngle;

        int index;

        Vector3 _ambient;
        Vector3 _diffuse;
        Vector3 _specular;

        public SpotLight(string name, Vector3 position, Vector3 direction, float innerAngle, float outerAngle, Vector3 ambient, Vector3 diffuse, Vector3 specular)
        {
            this.name = name;
            this.type = LIGHT_TYPE.spotLight;

            this._position = position;
            this._direction = direction;
            this._innerAngle = (float)Math.Cos(innerAngle);
            this._outerAngle = (float)Math.Cos(outerAngle);
            this._ambient = ambient;
            this._diffuse = diffuse;
            this._specular = specular;
        }
        public override void addToUBO(UniformBufferObject ubo, int index)
        {
            // Spotlight Values
            ubo.setVec3($"spotLight[{index}].position", _position);
            ubo.setVec3($"spotLight[{index}].direction", _direction);
            ubo.setFloat($"spotLight[{index}].innerAngle", _innerAngle);
            ubo.setFloat($"spotLight[{index}].outerAngle", _outerAngle);

            // Colour Setting 
            ubo.setVec3($"spotLight[{index}].ambient", _ambient);
            ubo.setVec3($"spotLight[{index}].diffuse", _diffuse);
            ubo.setVec3($"spotLight[{index}].specular", _specular);
        }

        public Vector3 getPosition()
        {
            return this._position;
        }

        public void setPosition(Vector3 newPosition, UniformBufferObject ubo)
        {
            this._position = newPosition;
            ubo.setVec3($"spotLight[{index}].position", newPosition);
        }
        public Vector3 getDirection()
        {
            return this._direction;
        }

        public void setDirection(Vector3 newDirection, UniformBufferObject ubo)
        {
            this._direction = newDirection;
            ubo.setVec3($"spotLight[{index}].direction", newDirection);
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
            ubo.setVec3($"spotLight[{index}].ambient", _ambient);
        }

        public void setDiffuse(Vector3 colour)
        {
            this._diffuse = colour;
            UniformBufferObject ubo = DataHandler.Get.getLightingHandler().getLightUBO();
            ubo.setVec3($"spotLight[{index}].diffuse", _diffuse);
        }

        public void setSpecular(Vector3 colour)
        {
            this._specular = colour;
            UniformBufferObject ubo = DataHandler.Get.getLightingHandler().getLightUBO();
            ubo.setVec3($"spotLight[{index}].specular", _specular);
        }


    }
}
