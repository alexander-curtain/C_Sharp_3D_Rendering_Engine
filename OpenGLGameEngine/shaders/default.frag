#version 420 core 
layout (location = 0) out vec4 outputColor;  

in vec2 texCoord;
//in vec3 fragNormal;
in vec3 fragPos;
in mat3 TBN;
// shadow data in
in ShadowData{
    vec4 lightSpaceFrag;
} shadowData_in;



const int NUMBER_OF_DIRECTIONAL_LIGHTS = 1;
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
    sampler2D albedo;
    sampler2D roughMap;
    sampler2D normalMap;
    sampler2D displacementMap;

    vec3 ambient;
    
    float shininess;
    float reflectivity;
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
uniform sampler2D shadowMap;




// function declaration
vec3 CalcDirectionalLight(DirectionalLight light, vec3 normal, vec3 viewDir, float roughness);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir, float roughness);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir, float roughness);

vec3 computeReflectionColor(samplerCube reflectionMap, vec3 viewDir, vec3 norm, float reflectionStrength);
float rand(vec2 co);

float shadowCalc(vec4 fragPosLightSpace, vec3 norm, vec3 lightDir);
vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir);

void main()
{
        // Aux Variables
    //vec3 norm = normalize(fragNormal);                  // normal
    vec3 viewDir = normalize(viewPos - fragPos);        // the direction of the camera relative to the current position
    vec3 tangentViewDir = normalize(TBN * viewDir); // tangent space

    // calculates the displacementMap
    vec2 paraTexCoord = ParallaxMapping(texCoord, tangentViewDir);
    
    
    vec4 texColor = texture(material.albedo, paraTexCoord); // texture colour at this point
    if(texColor.a < 0.125) discard;
    float roughness = texture(material.roughMap, paraTexCoord).r;

    // normal map normal calc
    vec3 tangentNormal = texture(material.normalMap, paraTexCoord).rgb;
    tangentNormal = tangentNormal * 2.0 - 1.0;  // converts to [-1, 1]

    //update normal
    vec3 norm = normalize(TBN * tangentNormal);

    vec3 colour = vec3(0.0);
    for(int i = 0; i < directionalLightsNum; i++) { colour += CalcDirectionalLight(dirLight[i], norm, viewDir, roughness);}
    for(int i = 0; i < pointLightsNum; i++) { colour += CalcPointLight(pointlight[i], norm, viewDir, roughness); }
    for(int i = 0; i < spotLightsNum; i++) { colour += CalcSpotLight(spotLight[i], norm, viewDir, roughness); }

    vec3 finalColour = colour * vec3(texColor);
    finalColour += computeReflectionColor(cubeMap, -viewDir, norm, material.reflectivity);

    outputColor = vec4(finalColour, texColor.w);

    //outputColor = vec4(normalize(TBN[0]) * 0.5 + 0.5, 1.0);
 }










 const float height_scale = 0.02;

 const float minLayers = 8.0;
const float maxLayers = 24.0;
float numLayers = mix(maxLayers, minLayers, max(dot(vec3(0.0, 0.0, 1.0), viewDir), 0.0));  

 // additional Techniques
vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir)
{ 
    // calculate the size of each layer
    float layerDepth = 1.0 / numLayers;
    // depth of current layer
    float currentLayerDepth = 0.0;
    // the amount to shift the texture coordinates per layer (from vector P)
    vec2 P = viewDir.xy * height_scale; 
    vec2 deltaTexCoords = P / numLayers;

    vec2  currentTexCoords     = texCoords;
    float currentDepthMapValue = 1 - texture(material.displacementMap, currentTexCoords).r;
  
    while(currentLayerDepth < currentDepthMapValue)
    {
        // shift texture coordinates along direction of P
        currentTexCoords -= deltaTexCoords;
        // get depthmap value at current texture coordinates
        currentDepthMapValue = 1 - texture(material.displacementMap, currentTexCoords).r;  
        // get depth of next layer
        currentLayerDepth += layerDepth;  
    }

    vec2 prevTexCoords = currentTexCoords + deltaTexCoords;

    // get depth after and before collision for linear interpolation
    float afterDepth  = currentDepthMapValue - currentLayerDepth;
    float beforeDepth =  (1 - texture(material.displacementMap, currentTexCoords).r) - currentLayerDepth + layerDepth;
 
    // interpolation of texture coordinates
    float weight = afterDepth / (afterDepth - beforeDepth);
    vec2 finalTexCoords = prevTexCoords * weight + currentTexCoords * (1.0 - weight);

    return finalTexCoords;  

    return currentTexCoords;
} 







// light definitions
     // directional light
vec3 CalcDirectionalLight(DirectionalLight light, vec3 normal, vec3 viewDir, float roughness) {
    vec3 lightDir = normalize(light.direction);            // light direction onto frag
    vec3 lightReflected = reflect(-lightDir, normal);       // reflected light direction off current frag
    vec3 halfway = normalize(lightDir + viewDir);       //blinn-phong term

    vec3 ambient = light.ambient * material.ambient;
    vec3 diffuse = max(dot(normal, lightDir), 0.0) * light.diffuse;
    vec3 specular = pow(max(dot(lightReflected, halfway), 0.0), material.shininess) * light.specular * roughness;


    // calculate how in shadow the object is (value between 0 and 1)
    float shadow = shadowCalc(shadowData_in.lightSpaceFrag, normal, lightDir);

    return ambient + (diffuse * shadow) + (specular * shadow);
}

    // point light
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir, float roughness){
    vec3 lightDir = normalize(light.position - fragPos);            // direction of light
    vec3 reflectedLightDir = normalize(reflect(-lightDir, normal)); // light direction reflected about the normal
    vec3 halfway = normalize(lightDir + viewDir);                   //blinn-phong term

    float lambda = length(light.position - fragPos);                // distance to the light

    float attenuation = 1.0 / (light.constant + light.linear * lambda + light.quadratic * (lambda * lambda)); // inverse square law
    vec3 ambient = light.ambient * material.ambient * attenuation;
    vec3 diffuse = max(dot(normal, lightDir), 0.0f) * light.diffuse * attenuation;
    vec3 specular = pow(max(dot(normal,  halfway), 0), material.shininess) * light.specular * roughness * attenuation;

    return ambient + diffuse + specular;
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir, float roughness) {
    vec3 spotlightDir = normalize(light.position - fragPos);                                    // Angle from current fragPos to the spotlight
    float theta = dot(normalize(fragPos - light.position), normalize(-light.direction));        // Angle from direction of the light to current fragPos
    float epsilon   = light.innerAngle - light.outerAngle;                                      // Difference between the inner and out cones of light
    float intensity  = clamp((theta - light.outerAngle) / epsilon, 0.0, 1.0);                   // Fade Value inbetween inner and outer cones (between 0 and 1)

    vec3 halfway = normalize(spotlightDir + viewDir);                                           //blinn-phong term

    vec3 ambient = light.ambient * material.ambient;
    vec3 diffuse = (max(dot(normal, normalize(light.direction)), 0.0f) * light.diffuse) * intensity;
    vec3 specular = (pow(max(dot(halfway, normal), 0), material.shininess) * light.specular * roughness) * intensity;

    return ambient + diffuse + specular;
}



// Shadow Calculations directional
float shadowCalc(vec4 fragPosLightSpace, vec3 norm, vec3 lightDir) {
    float bias = max(0.03 * (1.0 - dot(norm, lightDir)), 0.005);  

    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w; // Converts to ranges [-1, 1]
    projCoords = projCoords * 0.5 + 0.5;    // converts depth values to [0, 1]

    float closestDepth = texture(shadowMap, projCoords.xy).r;  // samples the depth at where this fragment should be in the shadowMap
    float currentDepth = projCoords.z;  

    // if the depth value is less than the sampled depth, then this fragment must be behind something, thus in shadow.
    //float shadow = currentDepth - bias > closestDepth  ? 0.0 : 1.0;   
    
    vec2 texelSize = 1.0 / vec2(textureSize(shadowMap, 0).xy);
    float shadow = 0.0;
    int samples = 24; // more samples, smoother shadow
    float radius = 16.0;

    for (int i = 0; i < samples; ++i) {
        // Pseudo-random angle and distance
        float angle = rand(projCoords.xy + float(i)) * 6.2831; // [0, 2PI]
        float dist = rand(projCoords.xy + float(i) * 2.0) * radius;

        vec2 offset = vec2(cos(angle), sin(angle)) * dist * texelSize;
        float pcfDepth = texture(shadowMap, projCoords.xy + offset).r;
        shadow += currentDepth - bias >= pcfDepth ? 0.0 : 1.0;
    }

    shadow /= float(samples);


    

    if(projCoords.z > 1.0) // prevent shadow always succeding when outside the shadow map
        shadow = 1.0;

    return shadow;
}



// pseduo random number generator
float rand(vec2 co) {
    return fract(sin(dot(co.xy, vec2(12.9898,78.233))) * 43758.5453);
}




vec3 computeReflectionColor(
    samplerCube reflectionMap,
    vec3 I, // view direction
    vec3 N, // normal
    float reflectionStrength
) {

    vec3 R = reflect(I, N);        // Reflected direction
    vec3 reflectedColor = texture(reflectionMap, R).rgb;

    return reflectedColor * reflectionStrength;
}
