#version 430 core

struct Material {
    vec4 color;
    int textureId;
};
struct PointLight {
    vec3 pos;
    vec4 color;
};

layout ( std430, binding = 0) readonly buffer MaterialBuffer {
    Material materials[];
};
layout ( std430, binding = 1) readonly buffer MatIdBuffer {
    int matIds[];
};
layout ( std430, binding = 2) readonly buffer PointLightBuffer {
    PointLight pointLights[];
};

in vec2 texPos;
in vec3 nonNormNormal;
in vec3 fragPos;

uniform sampler2DArray uTextures;
uniform vec4 uGlobalColor;
uniform vec3 uViewPos;
uniform int uLightsLength;

out vec4 fragColor;

void main() {
    Material mat = materials[matIds[gl_PrimitiveID]];
    vec4 textureColor = texture(uTextures, vec3(texPos, mat.textureId));
    
    vec4 baseColor = mat.color * textureColor * uGlobalColor;
    if (baseColor.a < 0.1) discard;

    vec3 norm = normalize(nonNormNormal);
    vec3 viewDir = normalize(fragPos- uViewPos);

    vec3 additionalColor = vec3(0);
    
    // ambient
    vec3 ambient = vec3(0.1);
    additionalColor += ambient; 
    for (int i = 0; i < uLightsLength; i++)
    {
        PointLight light = pointLights[i];
        vec3 lightDir = normalize(fragPos-light.pos);
        
        // diffuse
        vec3 diffuse = light.color.xyz * max(0, dot(norm, -lightDir));
        
        // specular
        vec3 reflectDir = reflect(lightDir, norm);
        vec3 specular = vec3(pow(max(0, dot(viewDir, -reflectDir)), 32) * 0.7);
        
        additionalColor += (diffuse + specular);
    }
    
    baseColor.xyz *= additionalColor;
    fragColor = baseColor;
}