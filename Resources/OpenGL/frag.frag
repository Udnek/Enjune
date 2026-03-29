#version 330 core

in vec4 vertexColor;
in vec2 textureCoord;

out vec4 fragColor;

uniform bool colorProvided;
uniform sampler2D texture0;

void main() {
    vec4 textureColor = texture(texture0, textureCoord);
    if (colorProvided){
        fragColor = vertexColor * textureColor;
    } else {
        fragColor = textureColor;
    }
}