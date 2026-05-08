using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.Graphic.Input.UI;

public sealed class Ui
{
    private readonly List<UiElement> _roots;
    private readonly List<Model.Entry> _meshes = [];
    
    public readonly Matrix4 ModelTransform = Matrix4.Identity;
    public readonly Matrix4 ViewTransform = Matrix4.Identity;
    public Matrix4 ProjectionTransform 
        => Matrix4.CreateOrthographicOffCenter(0, Size.X, 0, Size.Y, -10, 10);

    public Vector2 Size;
    public float PixelsPerUnit = 1;

    public Ui(Vector2 initialSize, params UiElement[] roots)
    {
        _roots = roots.ToList();
        Size = initialSize;
        UpdateEntire();
    }

    public Model CreateModel()
    {
        _meshes.Clear();
        _roots.ForEach(GetMeshes);
        return new Model(_meshes.ToArray());

        void GetMeshes(UiElement element)
        {
            if (element.LocalHidden) return;
            _meshes.AddRange(element.Meshes);
            element.Children?.ForEach(GetMeshes);
        }
    }
    
    public void UpdateEntire()
    {
        var rect = new Rect((0, 0), Size);
        _roots.ForEach(child => UpdateAndGenerateMeshes(rect, child));
        return;
        
        void UpdateAndGenerateMeshes(Rect parent, UiElement element)
        {
            if (element.LocalHidden) return;
            element.UpdateAndRegenerateMeshes(parent, PixelsPerUnit);
            element.Children?.ForEach(ch => UpdateAndGenerateMeshes(element.GlobalRect, ch));
        }
    }
    

    public void LogHierarchy()
    {
        LogWithDepth(0, $"{nameof(Ui)}: size={Size}; pixelsPerUnit={PixelsPerUnit};");
        _roots.ForEach(ch => ch.LogHierarchyRecursively(1));
    }
    
    public static void LogWithDepth(int depth, object? message) 
        => Logger.Log(typeof(Ui), new string(' ', depth*2) + message);
}