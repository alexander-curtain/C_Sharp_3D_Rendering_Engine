using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;

namespace OpenGLGameEngine.objects.Functional
{
    public static class ObjectLoader
    {

        public static float[] loadWavefront(string fileAddress)
        {
            var context = new AssimpContext();
            Scene scene = context.ImportFile(fileAddress, PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals); // note if normals are already present generate normals is overwritten

            Mesh mesh = scene.Meshes[0];

            List<float> vertexData = new();
            foreach (var face in mesh.Faces)
            {
                foreach (int index in face.Indices)
                {
                    // Position
                    var pos = mesh.Vertices[index];
                    vertexData.Add(pos.X);
                    vertexData.Add(pos.Y);
                    vertexData.Add(pos.Z);

                    // Normal
                    var norm = mesh.HasNormals ? mesh.Normals[index] : new Vector3D(0, 0, 0);
                    vertexData.Add(norm.X);
                    vertexData.Add(norm.Y);
                    vertexData.Add(norm.Z);

                    // Texture Coord
                    var tex = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0][index] : new Vector3D(0, 0, 0);
                    vertexData.Add(tex.X);
                    vertexData.Add(tex.Y);
                }
            }

            return vertexData.ToArray();
        }

        public static float[] LoadWavefrontWithTangents(string fileAddress)
        {
            var context = new AssimpContext();
            Scene scene = context.ImportFile(fileAddress,
                PostProcessSteps.Triangulate |
                PostProcessSteps.CalculateTangentSpace);

            Mesh mesh = scene.Meshes[0];

            List<float> vertexData = new();
            foreach (var face in mesh.Faces)
            {
                foreach (int index in face.Indices)
                {
                    // Position
                    var pos = mesh.Vertices[index];
                    vertexData.Add(pos.X);
                    vertexData.Add(pos.Y);
                    vertexData.Add(pos.Z);

                    // Normal
                    var norm = mesh.HasNormals ? mesh.Normals[index] : new Vector3D(0, 0, 0);
                    vertexData.Add(norm.X);
                    vertexData.Add(norm.Y);
                    vertexData.Add(norm.Z);

                    // Texture Coord
                    var tex = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0][index] : new Vector3D(0, 0, 0);
                    vertexData.Add(tex.X);
                    vertexData.Add(tex.Y);

                    var tangent = mesh.Tangents[index];
                    // Tangent
                    vertexData.Add(tangent.X);
                    vertexData.Add(tangent.Y);
                    vertexData.Add(tangent.Z);
                }
            }

            return vertexData.ToArray();
        }
    }
}
