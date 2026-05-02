#version 430 core

in vec2 texPos;

uniform sampler2D uScreenTexture;

out vec4 fragColor;

void main() {
    vec4 col = texture(uScreenTexture, texPos);
    //fragColor = vec4(vec3(col.x+col.y+col.z)/3, 1);
    fragColor = col;
}