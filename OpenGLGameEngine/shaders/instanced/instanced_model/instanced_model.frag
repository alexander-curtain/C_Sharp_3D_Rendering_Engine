#version 420 core 
layout (location = 0) out vec4 outputColor;  

in vec2 texCoord;
in vec3 fragNormal;
in vec3 fragPos;

const int NUMBER_OF_DIRECTIONAL_LIGHTS = 2;
const int NUMBER_OF_POINT_LIGHTS = 16;
const int NUMBER_OF_SPOT_LIGHTS = 16;

struct DirectionalLight {
    vec3 direction;                             float padding0;

    vec3 ambient;                               float padding1;
    vec3 diffuse;                               float padding2;
    vec3 specular;                              float padding3;

};
struct PointLight {
    vec3 position;                              float padding0;

    vec3 ambient;                               float padding1;
    vec3 diffuse;                               float padding2;
    vec3 specular;                              float padding3;

    float constant;
    float linear;
    float quadratic;
    float padding; // 16
};
struct SpotLight {
    vec3 position;                              float padding0;
    vec3 direction;                             float padding1;

    float innerAngle;
    float outerAngle;
    float pad1;         
    float pad2;     // 16

    vec3 ambient;                               float padding2;
    vec3 diffuse;                               float padding3;
    vec3 specular;                              float padding4;

};

struct Material {
    sampler2D diffuse;

    vec3 ambient;
    vec3 specular;
    
    float shininess;
};

// UBO' Declarations
layout(std140, binding = 1) uniform cameraPositionAndView {
    vec3 viewPos;
    vec3 viewDir;
};
layout(std140, binding = 2) uniform lights {
    int directionalLightsNum;
    int pointLightsNum;
    int spotLightsNum;                                  float padding;
    
    DirectionalLight dirLight[NUMBER_OF_DIRECTIONAL_LIGHTS];
    PointLight pointlight[NUMBER_OF_POINT_LIGHTS];
    SpotLight spotLight[NUMBER_OF_SPOT_LIGHTS];
};

// variable declaration
uniform Material material;
uniform samplerCube cubeMap;


// function declaration
vec3 CalcDirectionalLight(DirectionalLight light, vec3 normal, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir);
vec3 getReflectionRefractionColor(samplerCube cubeMap, vec3 viewDir, vec3 normal, float ior);
vec3 computeReflectionColor(samplerCube reflectionMap, vec3 viewDir, vec3 norm, vec3 baseColor, float reflectionStrength);

void main()
{
        // Aux Variables
    vec3 norm = normalize(fragNormal);                  // normal
    vec3 viewDir = normalize(viewPos - fragPos);        // the direction of the camera relative to the current position
    vec4 texColor = texture(material.diffuse, texCoord); // texture colour at this point
    if(texColor.a < 0.125) discard;

    vec3 colour = vec3(0.0);
    for(int i = 0; i < directionalLightsNum; i++) { colour += CalcDirectionalLight(dirLight[i], norm, viewDir);}
    for(int i = 0; i < pointLightsNum; i++) { colour += CalcPointLight(pointlight[i], norm, viewDir); }
    for(int i = 0; i < spotLightsNum; i++) { colour += CalcSpotLight(spotLight[i], norm, viewDir); }
    

    vec3 finalColour = clamp(colour * vec3(texColor), 0.0, 1.0);
    vec3 finalreflection = computeReflectionColor(cubeMap, -viewDir, norm, finalColour,  0.1);  //update strength

    
    outputColor = vec4(finalreflection, texColor.w);

 }




















// light definitions
     // directional light
vec3 CalcDirectionalLight(DirectionalLight light, vec3 normal, vec3 viewDir) {
    vec3 lightDir = normalize(light.direction);            // light direction onto frag
    vec3 lightReflected = reflect(-lightDir, normal);       // reflected light direction off current frag

    vec3 ambient = light.ambient * material.ambient;
    vec3 diffuse = max(dot(normal, lightDir), 0.0) * light.diffuse;
    vec3 specular = pow(max(dot(lightReflected, viewDir), 0.0), material.shininess) * light.specular * material.specular;

    return ambient + diffuse + specular;
}

    // point light
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir){
    vec3 lightdir = normalize(light.position - fragPos);            // direction of light
    vec3 reflectedLightDir = normalize(reflect(-lightdir, normal)); // light direction reflected about the normal
    float lambda = length(light.position - fragPos);                // distance to the light

    float attenuation = 1.0 / (light.constant + light.linear * lambda + light.quadratic * (lambda * lambda)); // inverse square law
    vec3 ambient = light.ambient * material.ambient * attenuation;
    vec3 diffuse = max(dot(normal, lightdir), 0.0f) * light.diffuse * attenuation;
    vec3 specular = pow(max(dot(reflectedLightDir,  viewDir), 0), material.shininess) * light.specular * material.specular * attenuation;

    return ambient + diffuse + specular;
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir) {
    vec3 spotlightDir = normalize(light.position - fragPos);                                    // Angle from current fragPos to the spotlight
    float theta = dot(normalize(fragPos - light.position), normalize(-light.direction));        // Angle from direction of the light to current fragPos
    float epsilon   = light.innerAngle - light.outerAngle;                                      // Difference between the inner and out cones of light
    float intensity  = clamp((theta - light.outerAngle) / epsilon, 0.0, 1.0);                   // Fade Value inbetween inner and outer cones (between 0 and 1)


    vec3 ambient = light.ambient * material.ambient;
    vec3 diffuse = (max(dot(normal, normalize(light.direction)), 0.0f) * light.diffuse) * intensity;
    vec3 specular = (pow(max(dot(normalize(reflect(normalize(-light.direction), normal)),  viewDir), 0), material.shininess) * light.specular * material.specular) * intensity;

    return ambient + diffuse + specular;
}





// reflection and refraction
vec3 getReflectionRefractionColor(
    samplerCube cubeMap,
    vec3 viewDir,
    vec3 normal,
    float ior         // index of refraction (n1 / n2)
) {
    vec3 I = normalize(viewDir);
    vec3 N = normalize(normal);
    float eta = 1.0 / ior;

    vec3 R = reflect(I, N);
    vec3 refractDir = refract(I, N, eta);

    vec3 reflectedColor = texture(cubeMap, R).rgb;
    vec3 refractedColor = texture(cubeMap, refractDir).rgb;

    // Fresnel term (Schlick approximation)
    float cosTheta = clamp(dot(-I, N), 0.0, 1.0);
    float fresnel = mix(0.1, 1.0, pow(1.0 - cosTheta, 5.0));

    // Blend reflection and refraction
    return mix(refractedColor, reflectedColor, fresnel);
}



vec3 computeReflectionColor(
    samplerCube reflectionMap,
    vec3 I, // view direction
    vec3 N, // normal
    vec3 baseColor,
    float reflectionStrength
) {

    vec3 R = reflect(I, N);        // Reflected direction
    vec3 reflectedColor = texture(reflectionMap, R).rgb;

    // Blend base color with reflection
    return mix(baseColor, reflectedColor, reflectionStrength);
}
