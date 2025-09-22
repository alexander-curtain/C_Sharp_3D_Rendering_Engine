#version 420 core

in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D billboardTexture;

void main()
{
    vec4 colour = texture(billboardTexture, TexCoord);
    if (colour.w < 0.125) discard;
    FragColor = colour;
}
