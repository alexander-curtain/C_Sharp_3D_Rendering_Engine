#version 330 core
layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 BrightColor;

in vec2 TexCoords;

uniform sampler2D screenTexture;

const float saturation = 1.0; // 0.0 = grayscale, 1.0 = full color

void main()
{
    vec4 color = texture(screenTexture, TexCoords);
    
    float gray = dot(color.rgb, vec3(0.2126, 0.7152, 0.0722));
    vec3 grayscale = vec3(gray);

    vec3 result = mix(grayscale, color.rgb, saturation);

    FragColor = vec4(result, color.a);
    BrightColor = dot(result, vec3(0.2126, 0.7152, 0.0722)) < 1 ? vec4(0.0, 0.0, 0.0, 1.0) : vec4(result, 1.0);
}
