#version 420 core 
layout(location = 0) in vec3 aPos;
out vec3 TexCoords;

layout(std140, binding = 0) uniform ViewProjection {
    mat4 view;
    mat4 projection;
};

void main()
{

    mat4 viewNoTranslation = view;
    viewNoTranslation[3].xyz = vec3(0.0);

    vec4 pos = projection * viewNoTranslation * vec4(aPos, 1.0);
    gl_Position = pos.xyww; // force depth = 1.0
    TexCoords = aPos;
}
