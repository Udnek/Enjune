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

in vec2 textureCoord;
in vec3 normal;
in vec3 inWorldPos;

uniform sampler2DArray textureArray;
uniform vec4 globalColor;

out vec4 fragColor;

void main() {
    Material mat = materials[matIds[gl_PrimitiveID]];
    vec4 textureColor = texture(textureArray, vec3(textureCoord, mat.textureId));
    
    vec4 col = mat.color * textureColor * globalColor;
    if (col.a < 0.1) discard;
    
    // diffuse
    vec3 lightPos = vec3(0, 0, 0);
    vec3 n = normalize(normal);
    vec3 lightDir = normalize(lightPos - inWorldPos);
    col.xyz *= max(0, dot(n, lightDir));
    
    
    fragColor = col; //vec4(normal, 1);
}