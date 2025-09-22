#version 420 core 
layout (location = 0) out vec4 gColour;  
layout (location = 1) out vec4 gNorm;  
layout (location = 2) out vec4 gPos;  

const float height_scale = 0.02;


in vec2 texCoord;
in vec3 fragPos;
in vec3 normal;
in mat3 TBN;


struct Material {
    sampler2D albedo;
    sampler2D roughMap;
    sampler2D normalMap;
    sampler2D displacementMap; 
};

// UBO' Declarations
layout(std140, binding = 1) uniform cameraPositionAndView {
    vec3 viewPos;
    vec3 viewDir;
};

// variable declaration
uniform Material material;


vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir);

void main()
{
        // Aux Variables
    vec3 viewDir = normalize(viewPos - fragPos);        // the direction of the camera relative to the current position
    

    // displacementMapping
    vec3 tangentViewDir = normalize(TBN * viewDir); // tangent space
    vec2 paraTexCoord = ParallaxMapping(texCoord, tangentViewDir);

    // normal map normal calc
    vec3 tangentNormal = texture(material.normalMap, paraTexCoord).rgb;
    tangentNormal = tangentNormal * 2.0 - 1.0; 
    vec3 norm = normalize(TBN * tangentNormal); // update the normal based on the normal map

    // albedo texture
    vec4 texColor = texture(material.albedo, paraTexCoord); // texture colour at this point
    float roughness = texture(material.roughMap, paraTexCoord).r; // get the roughness of the micro surface

    


    gPos = vec4(fragPos, 1.0);
    gNorm = vec4(norm, 1.0);
    gColour = vec4(texColor.rgb, roughness);
 }











 // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
 //
 //                       [Displacement Map Aux Function]
 //
 //  -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-




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
} 

