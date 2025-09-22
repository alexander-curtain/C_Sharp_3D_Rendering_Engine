#version 430 core
out float FragColor;
  
in vec2 TexCoords;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D texNoise;

uniform vec3 samples[64];

layout(std140, binding = 0) uniform ViewProjection {
    mat4 view;
    mat4 projection;
};
layout(std140, binding = 1) uniform cameraPositionAndView {
    vec3 viewPos; float _padA;
    vec3 viewDir; float _padB;
};


const int kernelSize = 64;
uniform float radius = 1.0;
uniform float bias = 0.0;
uniform float power = 0.5;
uniform vec2 framebufferSize;

void main()
{
    vec2 screenSize = framebufferSize;

    vec2 noiseScale = vec2(screenSize.x / 4.0, screenSize.y / 4.0);

    vec3 fragPos   = texture(gPosition, TexCoords).xyz;
    vec3 normal    = texture(gNormal, TexCoords).rgb;
    vec3 randomVec = texture(texNoise, TexCoords * noiseScale).xyz;  

    vec3 tangent   = normalize(randomVec - normal * dot(randomVec, normal));
    vec3 bitangent = cross(normal, tangent);
    mat3 TBN       = mat3(tangent, bitangent, normal);  

    float occlusion = 0.0;
    for (int i = 0; i < kernelSize; ++i) {
        // sample position in world-space
        vec3 samplePos = fragPos + TBN * samples[i] * radius;

        // project sample position into screen-space to sample from g-buffer
        vec4 offset = projection * view * vec4(samplePos, 1.0); 
        offset.xyz /= offset.w;
        offset.xyz = offset.xyz * 0.5 + 0.5;

        // world-space position of geometry at that texel
        vec3 sampleWorldPos = texture(gPosition, offset.xy).xyz;

        // check depth difference along view ray
        float sampleDepth = (view * vec4(sampleWorldPos, 1.0)).z; // view-space depth
        float testDepth   = (view * vec4(samplePos, 1.0)).z;

        float rangeCheck  = smoothstep(0.0, 1.0, radius / abs(fragPos.z - sampleWorldPos.z));

        occlusion += (sampleDepth >= testDepth + bias ? 1.0 : 0.0) * rangeCheck;
    }


    occlusion = 1.0 - (occlusion / kernelSize);     
    FragColor = pow(occlusion, power);
}