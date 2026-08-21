using Enjune.Graphic.Modeling;
using Enjune.Misc;
using OpenTK.Mathematics;
using SceneMaker.Ecs.Component;

namespace SceneMaker.Misc;

public static class EditorMisc
{
    
    public static Mesh? TraceLineObject(ModelComponent modelComp, Transform transform, Vector3 camPos, Vector3 camDir, float minimumAngleDegrees)
    {
        var model = modelComp.Model?.Get(out _);
        if (model is null) return null;
        
        // applying 
        var modelMatInv = transform.Matrix.Inverted();
        camDir = modelMatInv.TransformDirection(camDir).Normalized();
        camPos = modelMatInv.TransformPosition(camPos);

        Mesh? nearestMesh = null;
        var minDist = float.MaxValue;
        foreach (var pair in model.Meshes)
        {
            var mesh = pair.Mesh;
            for (var indexIndex = 0; indexIndex < mesh.Indexes.Length; indexIndex+=2)
            {
                var verIndex0 = mesh.Indexes[indexIndex];
                var verIndex1 = mesh.Indexes[indexIndex+1];
                if (!MathUtils.RayIntersectsLine(camPos, camDir, mesh.Vertices[verIndex0], mesh.Vertices[verIndex1], out var cosAngle))
                    continue;
                
                var degAngle = MathHelper.RadiansToDegrees(MathF.Acos(cosAngle));
                if (degAngle < minimumAngleDegrees && degAngle < minDist)
                {
                    nearestMesh = mesh;
                    minDist = degAngle;
                }
            }
        }
        return nearestMesh;
    }
    
    public static bool TraceObject(Vector3 cameraPos, Vector3 direction, ModelComponent modelComp, Transform modelTransform, out float distance)
    {
        var model = modelComp.Model?.Get(out _);
        if (model is null) // TODO || obj.RenderableModel?.CurrentPrimitive != IGraphicApi.Primitive.Triangle)
        {
            distance = 0;
            return false;
        }
        var modelMatInv = modelTransform.Matrix.Inverted();
        
        var localDirection = modelMatInv.TransformDirection(direction).Normalized();
        var localCameraPos = modelMatInv.TransformPosition(cameraPos);
        
        foreach (var (mesh, _) in model.Meshes)
        {
            for (var indexIndex = 0; indexIndex < mesh.Indexes.Length; indexIndex+=3)
            {
                var verIndex0 = mesh.Indexes[indexIndex];
                var verIndex1 = mesh.Indexes[indexIndex+1];
                var verIndex2 = mesh.Indexes[indexIndex+2];
                if (MathUtils.RayIntersectsTriangle(localCameraPos, localDirection,
                        mesh.Vertices[verIndex0], mesh.Vertices[verIndex1], mesh.Vertices[verIndex2], out distance))
                {
                    return true;
                }
            }
        }

        distance = 0;
        return false;
    }
}