using Enjune.File;
using StbImageSharp;

namespace Enjune.Graphic;

public class TextureManager
{
    private TexId _newId = 0;
    private Dictionary<ResourcePath, TexId> _textureToId = new();

    public readonly TexId ErrorTexture;

    public TextureManager()
    {
        ErrorTexture = AddTextureAndGetId(new ResourcePath("atlas.png"));
    }
    
    public TexId AddTextureAndGetId(ResourcePath texturePath)
    {
        if (_textureToId.TryGetValue(texturePath, out var id)) 
            return id;
        
        // or else add new
        _textureToId.Add(texturePath, _newId);
        Logger.Log(this, $"added texture {texturePath} with id={_newId}");
        return _newId++;
    }

    public void Compile()
    {
        foreach (var (path, id) in _textureToId)
        {
            var image = FileManager.LoadImage(path, out var error);
            if (image == null)
            {
                Logger.Warn(this, "TextureManager: can not ");
            }
        }
    }
}