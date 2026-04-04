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

uniform sampler2DArray textureArray;

out vec4 fragColor;

void main() {
    Material mat = materials[matIds[gl_PrimitiveID]];
    vec4 textureColor = texture(textureArray, vec3(textureCoord, mat.textureId));
    vec4 vertexColor = mat.color;
    
    vec4 col = vertexColor * textureColor;
    fragColor = col;
}