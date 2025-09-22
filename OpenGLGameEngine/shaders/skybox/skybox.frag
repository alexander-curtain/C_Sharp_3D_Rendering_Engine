#version 330 core
in vec3 TexCoords;
layout (location = 0) out vec4 gColour;  
layout (location = 1) out vec4 gNorm;  
layout (location = 2) out vec4 gPos;  

uniform samplerCube skybox;

const float hdrScalingFactor = 0.5;

void main()
{
    vec3 colour = texture(skybox, TexCoords).rgb * hdrScalingFactor; 

    gColour = vec4(colour / (1 - colour), 1.0);
    gNorm = vec4(0.0);
    gPos = vec4(0.0);
}
