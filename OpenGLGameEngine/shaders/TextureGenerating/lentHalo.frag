#version 420 core 
out vec4 FragColor;
  
in vec2 TexCoords;

uniform sampler2D screenTexture;

uniform float radius = 400;
uniform float startPoint = 1.0;
uniform float displaceTime = 1.0;



float f3(float wavelength, float theta){
    return 436.9 * (568/wavelength) * exp(-(theta - 3*(wavelength/568)));
}
vec3 wavelengthToRGB(float len);

void main() {
	vec2 normalisedCoord = (2.0*TexCoords) -  1.0;
	float distanceToCentre = dot(normalisedCoord, normalisedCoord);
    vec3 colour = vec3(0.0);


	// from domain [0-1] -> [0.0 - 2pi] to normalise into radians
	distanceToCentre = distanceToCentre * startPoint + displaceTime;


    colour = wavelengthToRGB(distanceToCentre * radius) * 0.0016 * f3(distanceToCentre * 300.0 + 450.0, distanceToCentre * 6.28);



	FragColor = vec4(colour, 1.0);
}





const int LEN_MIN = 380;
const int LEN_MAX = 780;
const int LEN_STEP = 5;

const int NUM_SAMPLES = 81;

const float[] X = {
    0.000160, 0.000662, 0.002362, 0.007242, 0.019110, 0.043400, 0.084736, 0.140638, 0.204492, 0.264737,
    0.314679, 0.357719, 0.383734, 0.386726, 0.370702, 0.342957, 0.302273, 0.254085, 0.195618, 0.132349,
    0.080507, 0.041072, 0.016172, 0.005132, 0.003816, 0.015444, 0.037465, 0.071358, 0.117749, 0.172953,
    0.236491, 0.304213, 0.376772, 0.451584, 0.529826, 0.616053, 0.705224, 0.793832, 0.878655, 0.951162,
    1.014160, 1.074300, 1.118520, 1.134300, 1.123990, 1.089100, 1.030480, 0.950740, 0.856297, 0.754930,
    0.647467, 0.535110, 0.431567, 0.343690, 0.268329, 0.204300, 0.152568, 0.112210, 0.081261, 0.057930,
    0.040851, 0.028623, 0.019941, 0.013842, 0.009577, 0.006605, 0.004553, 0.003145, 0.002175, 0.001506,
    0.001045, 0.000727, 0.000508, 0.000356, 0.000251, 0.000178, 0.000126, 0.000090, 0.000065
};

const float[] Y = {
    0.000017, 0.000072, 0.000253, 0.000769, 0.002004, 0.004509, 0.008756, 0.014456, 0.021391, 0.029497,
    0.038676, 0.049602, 0.062077, 0.074704, 0.089456, 0.106256, 0.128201, 0.152761, 0.185190, 0.219940,
    0.253589, 0.297665, 0.339133, 0.395379, 0.460777, 0.531360, 0.606741, 0.685660, 0.761757, 0.823330,
    0.875211, 0.923810, 0.961988, 0.982200, 0.991761, 0.999110, 0.997340, 0.982380, 0.955552, 0.915175,
    0.868934, 0.825623, 0.777405, 0.720353, 0.658341, 0.593878, 0.527963, 0.461834, 0.398057, 0.339554,
    0.283493, 0.228254, 0.179828, 0.140211, 0.107633, 0.081187, 0.060281, 0.044096, 0.031800, 0.022602,
    0.015905, 0.011130, 0.007749, 0.005375, 0.003718, 0.002565, 0.001768, 0.001222, 0.000846, 0.000586,
    0.000407, 0.000284, 0.000199, 0.000140, 0.000098, 0.000070, 0.000050, 0.000036, 0.000025
};

const float[] Z = {
    0.000705, 0.002928, 0.010482, 0.032344, 0.086011, 0.197120, 0.389366, 0.656760, 0.972542, 1.282500,
    1.553480, 1.798500, 1.967280, 2.027300, 1.994800, 1.900700, 1.745370, 1.554900, 1.317560, 1.030200,
    0.772125, 0.570060, 0.415254, 0.302356, 0.218502, 0.159249, 0.112044, 0.082248, 0.060709, 0.043050,
    0.030451, 0.020584, 0.013676, 0.007918, 0.003988, 0.001091, 0.0, 0.0, 0.0, 0.0,
    0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
    0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
    0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
    0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
};

const mat3 MATRIX_SRGB_D65 = mat3(
    3.2404542, -1.5371385, -0.4985314,
   -0.9692660,  1.8760108,  0.0415560,
    0.0556434, -0.2040259,  1.0572252
);

float gamma_srgb(float c) {
    if(c <= 0.0031308)
        return 12.92 * c;
    return pow(c, 1.0 / 2.4) * 1.055 - 0.055;
}

float clip(float c) {
    return clamp(c, 0.0, 1.0);
}

float interpolateX(int index, float offset) {
    if(offset == 0.0)
        return X[index];
    float x0 = float(index * LEN_STEP);
    float x1 = x0 + float(LEN_STEP);
    float y0 = X[index];
    float y1 = X[index + 1];
    return y0 + offset * (y1 - y0) / (x1 - x0);
}

float interpolateY(int index, float offset) {
    if(offset == 0.0)
        return Y[index];
    float x0 = float(index * LEN_STEP);
    float x1 = x0 + float(LEN_STEP);
    float y0 = Y[index];
    float y1 = Y[index + 1];
    return y0 + offset * (y1 - y0) / (x1 - x0);
}

float interpolateZ(int index, float offset) {
    if(offset == 0.0)
        return Z[index];
    float x0 = float(index * LEN_STEP);
    float x1 = x0 + float(LEN_STEP);
    float y0 = Z[index];
    float y1 = Z[index + 1];
    return y0 + offset * (y1 - y0) / (x1 - x0);
}


// Returns sRGB color for a given wavelength (in nm) as vec3(0..1)
vec3 wavelengthToRGB(float len) {
    if(len < float(LEN_MIN) || len > float(LEN_MAX))
        return vec3(0.0);

    len -= float(LEN_MIN);
    int index = int(floor(len / float(LEN_STEP)));
    float offset = len - float(LEN_STEP * index);

    float x = interpolateX(index, offset);
    float y = interpolateY(index, offset);
    float z = interpolateZ(index, offset);

    vec3 xyz = vec3(x, y, z);
    vec3 rgb = MATRIX_SRGB_D65 * xyz;
    rgb *= pow(y, 2.3); // scale by luminance to reduce violet oversampling

    rgb = vec3(gamma_srgb(rgb.r), gamma_srgb(rgb.g), gamma_srgb(rgb.b));
    return vec3(clip(rgb.r), clip(rgb.g), clip(rgb.b));
}
