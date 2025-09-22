#version 420 core 

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoord;
layout(location = 3) in mat4 model;

out vec2 texCoord;
out vec3 fragNormal;
out vec3 fragPos;

layout(std140, binding = 0) uniform ViewProjection {
    mat4 view;
    mat4 projection;
};


void main(void)
{   
    mat3 normalMatrix = mat3(transpose(inverse(model))); // TODO this is inefficent and should be replaced soon 

    gl_Position = projection * view * model * vec4(aPosition, 1.0);
    fragPos = vec3(model * vec4(aPosition, 1.0)); // fixed order
    fragNormal = normalMatrix * aNormal; // use inverse transpose of model 3x3
    texCoord = aTexCoord;
}