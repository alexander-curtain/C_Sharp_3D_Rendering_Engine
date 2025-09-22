#version 420 core 


layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoord;
layout(location = 3) in vec3 aTangent;

layout(std140, binding = 0) uniform ViewProjection {
    mat4 view;
    mat4 projection;
};

layout(std140, binding = 4) uniform LightSpaceMatrices {
    mat4 lightSpaceMatrices;
};


out vec2 texCoord;
//out vec3 fragNormal;
out vec3 fragPos;
out mat3 TBN;
out ShadowData {
    vec4 lightSpaceFrag;
} shadowData_out;



uniform mat4 model;
uniform mat3 normalMatrix;





void main(void)
{
    vec4 worldPosition = model * vec4(aPosition, 1.0);
    gl_Position = projection * view * worldPosition;

    fragPos = vec3(worldPosition);
    //fragNormal = normalMatrix * aNormal;
    texCoord = aTexCoord;

    // Normal map TBN matrix construction
    vec3 T = normalize(normalMatrix * aTangent.xyz);
    vec3 N = normalize(normalMatrix * aNormal);
    T = normalize(T - dot(T, N) * N);
    vec3 B = cross(N, T) ; 
    TBN = mat3(T, B, N); 

   shadowData_out.lightSpaceFrag = lightSpaceMatrices * vec4(fragPos, 1.0);
    

}
