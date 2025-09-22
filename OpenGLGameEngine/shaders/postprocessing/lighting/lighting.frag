#version 430 core
layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 BrightColor;

in vec2 TexCoords;

uniform sampler2D gColour;
uniform sampler2D gNormal;
uniform sampler2D gPosition;
uniform sampler2D AO;
uniform sampler2D shadowMap;

const int NUMBER_OF_DIRECTIONAL_LIGHTS = 1;
const int NUMBER_OF_POINT_LIGHTS = 16;
const int NUMBER_OF_SPOT_LIGHTS = 16;

const float shine = 1.0;

struct DirectionalLight {
    vec3 direction;                             float _pad0;
    vec3 ambient;                               float _pad1;
    vec3 diffuse;                               float _pad2;
    vec3 specular;                              float _pad3;
};

struct PointLight {
    vec3 position;                              float _pad0;
    vec3 ambient;                               float _pad1;
    vec3 diffuse;                               float _pad2;
    vec3 specular;                              float _pad3;
    float constant;
    float linear;
    float quadratic;
    float padding;
};

struct SpotLight {
    vec3 position;                              float _pad0;
    vec3 direction;                             float _pad1;
    float innerAngle;
    float outerAngle;
    float pad1;
    float pad2;
    vec3 ambient;                               float _pad2;
    vec3 diffuse;                               float _pad3;
    vec3 specular;                              float _pad4;
};

layout(std140, binding = 1) uniform cameraPositionAndView {
    vec3 viewPos; float _padA;
    vec3 viewDir; float _padB;
};

layout(std140, binding = 2) uniform LightingData {
    int directionalLightsNum;
    int pointLightsNum;
    int spotLightsNum;                           float _padC; // keep vec4 alignment
    DirectionalLight dirLight[NUMBER_OF_DIRECTIONAL_LIGHTS];
    PointLight      pointLights[NUMBER_OF_POINT_LIGHTS];
    SpotLight       spotLights[NUMBER_OF_SPOT_LIGHTS];
};

layout(std140, binding = 4) uniform LightSpaceMatrices {
    mat4 lightSpaceMatrices;
};

vec3 CalcDirectionalLight(DirectionalLight light, vec3 normal, vec3 viewDir, float roughness, vec3 position, float AO);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir, float roughness, vec3 position, float AO);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir, float roughness, vec3 position, float AO);

float shadowCalc(vec4 fragPosLightSpace, vec3 norm, vec3 lightDir);
float rand(vec2 co);

void main()
{
    vec3 normal = texture(gNormal, TexCoords).rgb;

    vec3 position = texture(gPosition, TexCoords).rgb;
    vec3 viewDirection = normalize(viewPos - position);

    vec4 albedo = texture(gColour, TexCoords);
    float roughness = albedo.a;

    
    if (abs(normal.r) < 0.02 && abs(normal.g) < 0.02 && abs(normal.b) < 0.02) {
        FragColor = vec4(albedo.rgb, 1.0);
        BrightColor = (dot(albedo.rgb, vec3(0.2126, 0.7152, 0.0722)) < 1.0) ? vec4(0.0, 0.0, 0.0, 1.0) : vec4(albedo.rgb, 1.0);
        return;
    }

    vec4 colour = vec4(0.0);
    float AO = texture(AO, TexCoords).r;

    for (int i = 0; i < directionalLightsNum; i++) { colour += vec4(CalcDirectionalLight(dirLight[i], normal, viewDirection, roughness, position, AO), 0.0); }
    for (int i = 0; i < pointLightsNum;       i++) { colour += vec4(CalcPointLight(pointLights[i], normal, viewDirection, roughness, position, AO), 0.0); }
    for (int i = 0; i < spotLightsNum;        i++) { colour += vec4(CalcSpotLight(spotLights[i], normal, viewDirection, roughness, position, AO), 0.0); }

    vec3 finalColour = colour.rgb * albedo.rgb;
        


    FragColor = vec4(finalColour, 1.0);



    float luma = dot(finalColour, vec3(0.2126, 0.7152, 0.0722));
    BrightColor = (luma < 1.0) ? vec4(0.0, 0.0, 0.0, 1.0) : vec4(finalColour, 1.0);
}





























// lights
vec3 CalcDirectionalLight(DirectionalLight light, vec3 normal, vec3 viewDir, float roughness, vec3 position, float AO) {
    vec3 lightDir = normalize(light.direction);
    vec3 lightReflected = reflect(-lightDir, normal);
    vec3 halfway = normalize(lightDir + viewDir);

    vec3 ambient  = light.ambient * AO;
    vec3 diffuse  = max(dot(normal, lightDir), 0.0) * light.diffuse;
    vec3 specular = pow(max(dot(lightReflected, halfway), 0.0), shine) * light.specular * roughness;

    vec4 lightSpacePosition = lightSpaceMatrices * vec4(position, 1.0);
    float shadow = shadowCalc(lightSpacePosition, normal, lightDir);

    return ambient + (diffuse + specular) * shadow;
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir, float roughness, vec3 position, float AO) {
    vec3 lightDir = normalize(light.position - position);
    vec3 halfway = normalize(lightDir + viewDir);

    float lambda = length(light.position - position);
    float attenuation = 1.0 / (light.constant + light.linear * lambda + light.quadratic * (lambda * lambda));

    vec3 ambient  = light.ambient * attenuation * AO;
    vec3 diffuse  = max(dot(normal, lightDir), 0.0) * light.diffuse * attenuation;
    vec3 specular = pow(max(dot(normal, halfway), 0.0), shine) * light.specular * roughness * attenuation;

    return ambient + diffuse + specular;
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir, float roughness, vec3 position, float AO) {
    vec3 spotlightDir = normalize(light.position - position);
    float theta   = dot(normalize(position - light.position), normalize(-light.direction));
    float epsilon = light.innerAngle - light.outerAngle;
    float intensity = clamp((theta - light.outerAngle) / max(epsilon, 1e-6), 0.0, 1.0);

    vec3 halfway = normalize(spotlightDir + viewDir);

    vec3 ambient  = light.ambient * AO;
    vec3 diffuse  = max(dot(normal, normalize(light.direction)), 0.0) * light.diffuse * intensity;
    vec3 specular = pow(max(dot(halfway, normal), 0.0), shine) * light.specular * roughness * intensity;

    return ambient + diffuse + specular;
}

vec2 vogelDisk(int i, int N, float radius, float rotate, float focus)
{
    // Golden angle in radians
    const float GOLDEN_ANGLE = 2.4;

    // Jitter each ring a tiny bit by adding rotate to the index; helps de-band
    float fi = (float(i) + rotate);

    // Angle: spiral with golden angle; add rotation to decorrelate per-pixel
    float theta = fi * GOLDEN_ANGLE + rotate * 6.283185307179586; //

    // Radial distribution:
    //   base r  sqrt((i+0.5)/N) gives roughly uniform disk coverage.
    //   'focus' lets you bias density (like a power curve).
    float t  = (fi + 0.5) / float(N);
    float r  = pow(sqrt(t), max(0.0, 1.0 + focus));

    // Scale by desired radius
    r *= radius;

    return r * vec2(cos(theta), sin(theta));
}



// shadow
float shadowCalc(vec4 fragPosLightSpace, vec3 norm, vec3 lightDir) {
    float bias = max(0.002 * (1.0 - dot(norm, lightDir)), 0.0001);
    const float lightSize = 40;
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;

    vec2 texelSize = 1.0 / vec2(textureSize(shadowMap, 0));
    float currentDepth = projCoords.z;

    // blocker search space
    int blockerSamples = 8;
    float blockerRadius = 10;
    float avgBlockerDepth = 0.0;
    int blockerCount = 0;

    for (int i = 0; i < blockerSamples; ++i) {
        vec2 offset = vogelDisk(i, blockerSamples, blockerRadius, 1.0, 1.0) * texelSize;
        float sampleDepth = texture(shadowMap, projCoords.xy + offset).r;
        if (sampleDepth < currentDepth - bias * 4) {
            avgBlockerDepth += sampleDepth;
            blockerCount++;
        }
    }

    if (blockerCount > 0) {
        avgBlockerDepth /= float(blockerCount);
    } else {
        return 1.0; // no blocker, fully lit
    }

    // step 2.0, 
    // if blockers occur we PCF over the radius

    float shadow = 1.0;
    int samples = 24;
    float radius = ((currentDepth - avgBlockerDepth) * lightSize)/avgBlockerDepth;

    for (int i = 0; i < samples; ++i) {
        vec2 offset = vogelDisk(i, samples, radius, 1.0, 0.0) * texelSize;
        float pcfDepth = texture(shadowMap, projCoords.xy + offset).r;
        shadow += float(currentDepth - bias <= pcfDepth);
    }

    shadow /= samples;
    


    if (projCoords.z > 1.0) shadow = 1.0;
    float shadowGradient = (rand(projCoords.xy)*(1-shadow)*(shadow))*(1 - shadow); // if 1 or 0 will be normal, adds noise to prevent banding , additional shadow - 1 term bias it propintal to darker regions which suits the eye better
    return shadow + shadowGradient;
}



float rand(vec2 co) {
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}
