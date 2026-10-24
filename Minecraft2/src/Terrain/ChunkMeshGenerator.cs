using System.Diagnostics;
using Enjune.Graphic;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Minecraft2.Block;
using Minecraft2.Graphic;
using Minecraft2.Misc;

namespace Minecraft2.Terrain;

public class ChunkMeshGenerator(App app) : ConcurrentWorker<Guid, (Chunk Chunk, ChunkModel Model), (Chunk Chunk, ChunkModel Model)>
{
    private readonly App _app = app;
    
    protected override (Chunk Chunk, ChunkModel Model) Work((Chunk Chunk, ChunkModel Model) job)
    {
        var (chunk, model) = job;
        model.Clear();
        Vector3 sideColor = System.Drawing.Color.FromArgb(255, 150, 150, 150).ToTk().Xyz;
        var air = _app.Blocks.Air;
        var outOfBounds = air;
        var renderTimer = Stopwatch.StartNew();
        for (int x = 0; x < Chunk.Size.X; x++)
        {
            for (int y = 0; y < Chunk.Size.Y; y++)
            {
                for (int z = 0; z < Chunk.Size.Z; z++)
                {
                    var blockRender = chunk[new ChunkPos(x, y, z)]?.Render;
                    if (blockRender is null) continue;
                    
                    var primitive = new GraphicChunkData.Primitive
                    {
                        TextureIndex = blockRender.TextureIndex
                    };

                    if (blockRender.BiomeColor)
                    {
                        primitive.Color = (0.3f, 0.6f, 0.2f);
                    }
                    
                    // ambient occlusion

                    // bottom
                    if (chunk.SafeGet(new ChunkPos(x, y-1, z), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), primitive,
                            (x, y, z), (x+1, y, z), 
                            (x+1, y, z+1), (x, y, z+1), 
                            TextureQuad.Full
                        );
                    }
                    // top
                    if (chunk.SafeGet(new ChunkPos(x, y + 1, z), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), primitive,
                            (x, y+1, z), (x, y+1, z+1), 
                            (x+1, y+1, z+1), (x+1, y+1, z), 
                            TextureQuad.Full
                        );
                    }
                    // xy -z
                    if (chunk.SafeGet(new ChunkPos(x, y, z - 1), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), primitive * sideColor,
                            (x, y, z), (x, y+1, z), 
                            (x+1, y+1, z), (x+1, y, z), 
                            TextureQuad.Full
                        );
                    }
                    // xy +z
                    if (chunk.SafeGet(new ChunkPos(x, y, z + 1), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), primitive * sideColor,
                            (x, y, z+1), (x+1, y, z+1), 
                            (x+1, y+1, z+1), (x, y+1, z+1), 
                            TextureQuad.Full
                        );
                    }
                    // yz -x
                    if (chunk.SafeGet(new ChunkPos(x - 1, y, z), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), primitive * sideColor,
                            (x, y, z), (x, y, z+1), 
                            (x, y+1, z+1), (x, y+1, z), 
                            TextureQuad.Full
                        );
                    }
                    // yz +x
                    if (chunk.SafeGet(new ChunkPos(x + 1, y, z), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), primitive * sideColor,
                            (x+1, y, z), (x+1, y+1, z), 
                            (x+1, y+1, z+1), (x+1, y, z+1), 
                            TextureQuad.Full
                        );
                    }
                }
            }
        }
        
        model.GetMesh(PrimitiveTopology.Line)
            .AddCuboid(new (), new (){ TextureIndex = 0 }, 
                Position.Zero, Chunk.Size, TextureQuad.Full);
        
        renderTimer.Stop();
        Logger.Highlight(this, $"Generated mesh in {renderTimer.ElapsedMilliseconds} ms");
        return job;
    }
}