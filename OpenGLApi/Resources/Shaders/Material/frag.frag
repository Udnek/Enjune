
in vec2 texPos;
in vec3 nonNormNormal;
in vec3 fragPos;
in vec4 fragPosInLightSpace[5];

uniform sampler2DArray uTextures;
uniform sampler2DArray uShadowMaps;
uniform vec4 uGlobalColor;
uniform vec3 uViewPos;

out vec4 fragColor;

bool isLightedBy(int lightId, vec3 lightDir, vec3 fragNorm);
vec3 calcLightColor();
void main() {
    PerPrimitive perPrim = perPrimitives[gl_PrimitiveID];
    Material mat = materials[perPrim.matId];
    vec4 textureColor = texture(uTextures, vec3(texPos, mat.textureId));
    
    vec4 baseColor = perPrim.color * mat.color * textureColor * uGlobalColor;
    if (baseColor.a < 0.1) discard;
    
    baseColor.xyz *= calcLightColor();
    fragColor = baseColor;
}

vec3 calcLightColor(){
    vec3 color = vec3(0.3); // ambient minimum
    
    vec3 norm = normalize(nonNormNormal);
    vec3 viewDir = normalize(fragPos- uViewPos);

    for (int i = 0; i < lightsLength; i++)
    {
        SpotLight light = spotLights[i];
        vec3 lightDir = normalize(fragPos-light.pos);
        
        if (!isLightedBy(i, lightDir, norm)) continue;

        // diffuse
        vec3 diffuse = light.color.xyz * max(0, dot(norm, -lightDir));

        // specular
        vec3 reflectDir = reflect(lightDir, norm);
        vec3 specular = vec3(pow(max(0, dot(viewDir, -reflectDir)), 32) * 0.7);

        color += (diffuse + specular);
    }
    return color;
}

bool isLightedBy(int lightId, vec3 lightDir, vec3 fragNorm){
    vec3 lightNdc = fragPosInLightSpace[lightId].xyz / fragPosInLightSpace[lightId].w; // [-1; 1]
    lightNdc = (lightNdc + 1) / 2; // [-1; 1] -> [0; 1]
    bool outOfBounds = any(lessThan(lightNdc, vec3(0))) || any(greaterThan(lightNdc, vec3(1)));
    if (outOfBounds) return false;

    float closestDepth = texture(uShadowMaps, vec3(lightNdc.xy, lightId)).r;
    float currentDepth = lightNdc.z;
    float bias = max(0.0001, 0.001*(1-dot(fragNorm, lightDir)));
    
    return (currentDepth - bias) < closestDepth;
}