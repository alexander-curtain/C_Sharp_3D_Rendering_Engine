using Assimp;
using OpenGLGameEngine.rendering;
using OpenGLGameEngine.rendering.lowLevelClasses.shader;
using OpenGLGameEngine.textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLGameEngine.objects.Functional
{
    public struct Material // NOTE it is up to you to ensure that your shader is correct.
    {
        public Texture? Albedo;                // texture unit 0
        public Texture? Roughness;              // texture unit 1
        public Texture? NormalMap;              // texture unit 2
        public Texture? DisplacementMap;        // texture unit 3


        public Vector3? ambientLight;
        public Vector3? diffuse;
        public Vector3? specularLight;
        public float? shininess;
        public float? reflectivity;


        public Material(Texture texture, Texture normalMap, float shine, float reflectivity)
        {
            // default constructor
            this.Albedo = texture;
            this.NormalMap = normalMap;

            this.shininess = shine;
            this.reflectivity = reflectivity;
            this.ambientLight = new Vector3(1.0f);
            this.specularLight = new Vector3(1.0f);
        }


        // http://devernay.free.fr/cours/opengl/materials.html, more info on materials.
        public Material(bool usingCustum, Texture? texture = null, Vector3? diffuse = null, Vector3? ambientLight = null, Vector3? specularLight = null, float? shininess = null, float? reflectiveness = null, Texture? specularMap = null, Texture? normalMap = null, Texture? displacementMap = null)
        {
            if (texture is Texture albedo) { this.Albedo = albedo; } else { this.diffuse = diffuse ?? new Vector3(1.0f); }

            if (Roughness is Texture specMap) { this.Roughness = specMap; }
            if (NormalMap is Texture normMap) { this.NormalMap = normalMap; }
            if (DisplacementMap is Texture dispMap) { this.DisplacementMap = dispMap; }


            if (ambientLight is Vector3 _) { this.ambientLight = ambientLight ?? new Vector3(1.0f); }
            if (specularLight is Vector3 _) { this.specularLight = specularLight ?? new Vector3(1.0f); }
            if (shininess is float _) { this.shininess = shininess ?? 1f; }
            if (reflectiveness is float _) { this.reflectivity = reflectiveness ?? 0f; }
        }

        public Material(FULLTEXTURE textures)
        {
            this.Albedo = textures.albedo;
            this.Roughness = textures.roughness;
            this.NormalMap = textures.normal;
            this.DisplacementMap = textures.displacement;
        }



        public void bindUniforms(Shader shader)
        {
            // try and reduce calls here, as the number of conditionals will kill any compiler optimisations.

            // texture is required
            if (Albedo is Texture albedo) { albedo.Use(TextureUnit.Texture0); shader.SetInt("material.albedo", 0);} 
            // sub-textures
            if (Roughness is Texture specMap) { specMap.Use(TextureUnit.Texture1); shader.SetInt("material.roughMap", 1); }
            if (NormalMap is Texture normMap) { normMap.Use(TextureUnit.Texture2); shader.SetInt("material.normalMap", 2); }
            if (DisplacementMap is Texture dispMap) { dispMap.Use(TextureUnit.Texture3); shader.SetInt("material.displacementMap", 3); }

            // vectors
            if (diffuse is Vector3 diffLight)       {  shader.SetVector3("material.diffuse", diffLight);  }
            if (specularLight is Vector3 specLight) { shader.SetVector3("material.specular", specLight); }
            if (ambientLight is Vector3 ambLight)   { shader.SetVector3("material.ambient", ambLight);  }
            if (shininess is float shine)           { shader.SetFloat("material.shininess", shine); }
            if (reflectivity is float reflective) { shader.SetFloat("material.reflectivity", reflective); }
        }



    }


    public struct FULLTEXTURE
    {
        public Texture albedo;
        public Texture roughness;
        public Texture normal;
        public Texture displacement;

        
        
        public FULLTEXTURE(string albedo, string roughness, string normal, string displacement)
        {
            this.albedo = Texture.LoadFromFile(albedo);
            this.roughness = Texture.LoadFromFileNonImage(roughness);
            this.normal = Texture.LoadFromFileNonImage(normal);
            this.displacement = Texture.LoadFromFileNonImage(displacement);
        }


        public static FULLTEXTURE loadFromFolder(string folder)
        {
            string[] textureTypes = { "diffuse", "rough", "normal", "displace" };
            string[] extensions = { ".jpg", ".png" };

            string FindTexture(string type)
            {
                foreach (var ext in extensions)
                {
                    string path = Path.Combine(folder, type + ext);
                    if (File.Exists(path))
                        return path;
                }

                throw new FileNotFoundException($"Could not find texture for '{type}' in folder '{folder}'");
            }

            string albedoPath = FindTexture("diffuse");
            string roughnessPath = FindTexture("rough");
            string normalPath = FindTexture("normal");
            string displacementPath = FindTexture("displace");

            return new FULLTEXTURE(albedoPath, roughnessPath, normalPath, displacementPath);
        }

    }
}
