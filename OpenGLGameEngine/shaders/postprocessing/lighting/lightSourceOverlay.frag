#version 430 core
layout (location = 0) out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D screenTexture;
uniform sampler2D gPosition;

uniform sampler2D flare1;
uniform sampler2D flare2;

uniform sampler2D lenticular;

// paper implemented from:
//https://www.researchgate.net/publication/2593999_Physically-Based_Glare_Effects_for_Digital_Images

const int NUMBER_OF_DIRECTIONAL_LIGHTS = 1;
const int NUMBER_OF_POINT_LIGHTS = 16;
const int NUMBER_OF_SPOT_LIGHTS = 16;

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


layout(std140, binding = 2) uniform LightingData {
    int directionalLightsNum;
    int pointLightsNum;
    int spotLightsNum;                           float _padC; // keep vec4 alignment
    DirectionalLight dirLight[NUMBER_OF_DIRECTIONAL_LIGHTS];
    PointLight      pointLights[NUMBER_OF_POINT_LIGHTS];
    SpotLight       spotLights[NUMBER_OF_SPOT_LIGHTS];
};

layout(std140, binding = 0) uniform ViewProjection {
    mat4 view;
    mat4 projection;
};

layout(std140, binding = 1) uniform cameraPositionAndView {
    vec3 viewPos; float _padA;
    vec3 viewDir; float _padB;
};



vec2 rotate(vec2 v, float t);
float continuousAngleFunction(vec3 point, vec3 center, float sigma);
vec3 calcFlare(vec2 normalisedCoord, vec3 lightPosition, vec3 lightColour);

void main()
{
    vec2 normalisedCoord = (2.0*TexCoords) -  1.0;

    vec3 colour = texture(screenTexture, TexCoords).rgb;


    // add flare texturing to point lights and spotlights in scene
    for (int i = 0; i < pointLightsNum; i++) {  colour += calcFlare(normalisedCoord, pointLights[i].position, pointLights[i].diffuse); }
    for (int i = 0; i < spotLightsNum; i++) { colour += calcFlare(normalisedCoord, spotLights[i].position, spotLights[i].diffuse);}
    
    FragColor = vec4(colour, 1.0);
}

// main function
vec3 calcFlare(vec2 normalisedCoord, vec3 lightPosition, vec3 lightColour) {
        // gets apsect ratio
       ivec2 texSize = textureSize(gPosition, 0); 
       float aspect = float(texSize.x) / float(texSize.y);



       float lightSpaceDistance = length(lightPosition - viewPos);

       vec3 colour = vec3(0.0);
       float lumins = dot(lightColour, vec3(0.2126, 0.7152, 0.0722));


        // Light in view space
        vec4 lightView = view * vec4(lightPosition, 1.0);
        // Cull if behind camera
        if (lightView.z > 0.0) { return vec3(0.0);}

        // Project to clip-space
        vec4 clipPos = projection * lightView;
        vec3 ndcPos = clipPos.xyz / clipPos.w;
        vec2 screenPos = ndcPos.xy * 0.5 + 0.5;

        // position of the light
       vec3 worldPos = texture(gPosition, screenPos).xyz;          // geometry position
    


        // Screen-space glow around light
        float screenspacedistance = distance(screenPos, TexCoords);
        float intensity = 1.0 - smoothstep(0.0, lumins/4, screenspacedistance);

        // generates the halo effect 
        float haloRadius = 0.1; // halo radius in percentage of screen (assuming 1:1)

        
        vec2 haloSamplePosition = (((TexCoords - screenPos) + haloRadius) * (1.0 / (2.0 * haloRadius)));
        vec3 haloSample = texture(lenticular, haloSamplePosition).rgb;
        haloSample = mix(haloSample * texture(flare1, haloSamplePosition).rgb, haloSample, 1 / lightSpaceDistance);
        haloSample *= lightColour;


        // generates the lines effect
        haloRadius = 0.5;
        float rotationSeed = continuousAngleFunction(viewPos, lightPosition, 0.5);

        // note we sample two points in the glare due to having two eyes, these rotate in opposite directions
        haloSamplePosition = ((TexCoords - screenPos) + haloRadius) * (1.0 / (2.0 * haloRadius));
        
        vec2 glareSamplePoint = rotate(haloSamplePosition, rotationSeed + dot(viewDir, vec3(1.0)));
        vec2 glareSamplePoint2 = rotate(haloSamplePosition, -(rotationSeed + dot(viewDir, vec3(1.0))));

        float glarefalloff = 0.0001 * (10.91 / pow((screenspacedistance+0.02), 3) + (10.0 / pow((screenspacedistance+0.02), 2)));

        vec3 glareSample = texture(flare2, glareSamplePoint).rgb * glarefalloff;
        glareSample += texture(flare1, glareSamplePoint2).rgb * glarefalloff ;

        const float reduceBias = 0.05;


        // this section denotes how we prevent light writing over objects, and rendering despite behind the camera.
        if (length(worldPos) < 1e-3) {
            // due to non-rendered pixels defaulting to 0.0, 0.0, 0.0, we need this special case.
                colour += min(lightColour * intensity, vec3(1.0));
                
                colour += glareSample * lightColour * reduceBias;
            colour += haloSample * exp(-pow(lightSpaceDistance * 1e-4, 2)) * reduceBias * 3;
        } else {
            // evaluates whether the light is hiden behind geomerty using the GBuffer
            vec4 fragView = view * vec4(worldPos, 1.0);
            float fragViewZ = -fragView.z;
            float lightViewZ = -lightView.z;

            if (lightViewZ < fragViewZ) {
                colour += min(lightColour * intensity, vec3(1.0));
                
                colour += glareSample * lightColour * reduceBias;
                colour += haloSample * exp(-pow(lightSpaceDistance * 1e-4, 2)) * reduceBias * 3;
            }
        }

        return colour;
}


// rotates around a 0.5 centre
vec2 rotate(vec2 v, float t) {
    // shift so center is at origin
    vec2 shifted = v - vec2(0.5);

    // rotate
    float s = sin(t);
    float c = cos(t);
    shifted = vec2(
        c * shifted.x - s * shifted.y,
        s * shifted.x + c * shifted.y
    );

    // shift back
    return shifted + vec2(0.5);
}

// for slight rotation causing changes in the density of the eye (as represented by rotation)
float continuousAngleFunction(vec3 point, vec3 center, float sigma)
{
    vec3 v = normalize(point - center);

    // Use components to create a smooth variation
    // This changes continuously as the direction changes
    float angleFactor = v.x*v.y + v.y*v.z + v.z*v.x;

    // Optional: Gaussian-like smoothing
    return exp(-(angleFactor*angleFactor)/(sigma*sigma));
}