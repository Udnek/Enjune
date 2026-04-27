#version 430 core

struct Material {
    vec4 color;
    int textureId;
};
layout ( std430, binding = 0) readonly buffer MaterialBuffer {
    Material materials[];
};
layout ( std430, binding = 1) readonly buffer MatIdBuffer {
    int matIds[];
};

in vec2 texPos;
in vec3 nonNormNormal;
in vec3 fragPos;

uniform sampler2DArray uTextures;
uniform vec4 uGlobalColor;
uniform vec3 uViewPos;

out vec4 fragColor;

void main() {
    Material mat = materials[matIds[gl_PrimitiveID]];
    vec4 textureColor = texture(uTextures, vec3(texPos, mat.textureId));
    
    vec4 baseColor = mat.color * textureColor * uGlobalColor;
    if (baseColor.a < 0.1) discard;
    
    vec3 norm = normalize(nonNormNormal);
    vec3 lightPos = vec3(0, 0, 0);
    vec3 lightDir = normalize(fragPos-lightPos);
    
    // ambient
    vec3 ambient = vec3(0.1);
    
    // diffuse
    vec3 diffuse = vec3(max(0, dot(norm, -lightDir)));
    
    // specular
    vec3 viewDir = normalize(fragPos- uViewPos);
    vec3 reflectDir = reflect(lightDir, norm);
    vec3 specular = vec3(pow(max(0, dot(viewDir, -reflectDir)), 32) * 0.7);

    baseColor.xyz *= (ambient + diffuse + specular);
    fragColor = baseColor;
}