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
uniform vec4 globalColor;

out vec4 fragColor;

void main() {
    Material mat = materials[matIds[gl_PrimitiveID]];
    vec4 textureColor = texture(textureArray, vec3(textureCoord, mat.textureId));

    vec4 col = mat.color * textureColor * globalColor;
    fragColor = col;
}