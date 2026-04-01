#version 330 core

in vec4 vertexColor;
in vec2 textureCoord;
in int textureLayer;

out vec4 fragColor;

uniform bool colorProvided;
uniform sampler2DArray textureArray;

void main() {
    vec4 textureColor = texture(textureArray, vec3(textureCoord, textureLayer));
    if (colorProvided){
        fragColor = vertexColor * textureColor;
    } else {
        fragColor = textureColor;
    }
}