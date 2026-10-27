using Enjune.File;
using Enjune.KitStart.TAAL;
using Enjune.Registering;

namespace Minecraft2.Block;

public class BlockTypes
{
    public static readonly WritableRegistry<BlockType> 
        Registry = WritableRegistry<BlockType>.CreateAndRegister(Identifier.Of(Program.Assembly, "block_type"));
    
    public readonly BlockType Air;
    public readonly BlockType Dirt;
    public readonly BlockType Grass;
    public readonly BlockType Cobblestone;
    public readonly BlockType Snow;
    
    private readonly TextureArrayAssetLoader _assetLoader;

    public BlockTypes(TextureArrayAssetLoader assetLoader)
    {
        _assetLoader = assetLoader;
        Air = Register("air", new BlockType { Render = null, Collision = null });
        Dirt = Register("dirt", "Dirt.png");
        Grass = Register("grass", "Grass.png");
        Grass.Render!.BiomeColor = true;
        Cobblestone = Register("cobblestone", "Cobblestone.png");
        Snow = Register("snow", "Snow.png");
    }

    private static BlockType Register(string name, BlockType blockType)
    {
        Registry.Register(Identifier.Of(Program.Assembly, name), blockType);
        return blockType;
    }
    
    private BlockType Register(string name, string textureName)
    {
        var texIdx = _assetLoader.AddMaterialAndGetIndex(RawTAALMaterial.FromTexture(
            AssemblyPath.Of(Program.Assembly, textureName)));
        return Register(
            name,
            new BlockType
            {
                Render = new BlockRender { TextureIndex = texIdx },
                Collision = BlockCollision.Full
            });
    }
}