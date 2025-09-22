#version 430 core

layout (location = 0) out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D screenTexture;
uniform sampler2D bloomBlur;
uniform float exposure;



layout(std140, binding = 3) uniform timeUBO {
    float time;
};


void main()
{

    vec3 hdr = texture(screenTexture, TexCoords).rgb;
    vec3 bloomBlur = texture(bloomBlur, TexCoords).rgb;
    hdr += bloomBlur;


    // inverse gamma correction
    hdr = pow(hdr, vec3(2.2));
    // exposure tone mapping
    vec3 mapped = vec3(1.0) - exp(-hdr * exposure);

    // gamma correction applied back
    vec3 final = pow(mapped, vec3(1.0 / 2.2));
    


    FragColor = vec4(final, 1.0);
}
