namespace Enjune.File.ModelReader;

public class DotObjMaterial(string name)
{
    public readonly string Name = name;
    public ResourcePath? TexturePath { get; set; }
}