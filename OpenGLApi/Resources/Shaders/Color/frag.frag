
in vec4 color;
in vec2 texPos;

uniform vec4 uGlobalColor;
uniform sampler2DArray uTextures;

out vec4 fragColor;

void main() {
    PerPrimitive perPrim = perPrimitives[gl_PrimitiveID];
    Material mat = materials[perPrim.matId];
    vec4 textureColor = texture(uTextures, vec3(texPos, mat.textureId));

    vec4 baseColor = perPrim.color * mat.color * textureColor * uGlobalColor;
    if (baseColor.a < 0.1) discard;
    
    fragColor = baseColor;
}