#version 420 core

layout (location = 0) in vec2 aPos;       // local quad vertex position (-0.5 to 0.5)
layout (location = 1) in vec2 UV;         // texture coordinate
layout (location = 2) in vec3 offset;     // instance world position

layout(std140, binding = 0) uniform ViewProjection {
    mat4 view;
    mat4 projection;
};
layout(std140, binding = 1) uniform cameraPositionAndView {
    vec3 viewPos;
    vec3 viewDir;
};

layout(std140, binding = 3) uniform timeAux {
    float time;
};

out vec2 TexCoord;

void main()
{
    // modify for motion
    vec3 trueOffset = offset;
    trueOffset.x -= (time * 0.001 + gl_InstanceID) * 3;
    trueOffset.z -= (time * 0.001 + gl_InstanceID) * 3;

    trueOffset.x = mod(trueOffset.x, 100) - 50;
    trueOffset.z = mod(trueOffset.z, 100) - 50;

    // Direction from billboard to camera, projected to XZ plane (ignore Y axis)
    vec3 toCamera = viewPos - trueOffset;
    toCamera.y = 0.0;
    toCamera = normalize(toCamera);

    
    // Right vector (perpendicular to toCamera and world up)
    vec3 right = normalize(vec3(-toCamera.z, 0.0, toCamera.x));
    vec3 up = vec3(0.0, 1.0, 0.0); // Y-axis fixed upright

    // Expand quad in right/up direction
    vec3 worldPos = trueOffset + (right * aPos.x) + (up * aPos.y);

    gl_Position = projection * view * vec4(worldPos, 1.0);
    TexCoord = UV;
}
