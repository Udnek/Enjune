using System.Diagnostics;
using Enjune.Graphic;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Minecraft2.Bridge;

namespace Minecraft2.Misc;

public class ChunkMeshGenerator(App app) : ConcurrentWorker<(Guid GrxId, Chunk Chunk, ChunkModel Model), Nothing>
{
    protected override Nothing Work((Guid GrxId, Chunk Chunk, ChunkModel Model) job)
    {
        var (_, chunk, model) = job;
        model.Clear();
        Color sideColor = System.Drawing.Color.FromArgb(255, 150, 150, 150).ToTk();
        var renderTimer = Stopwatch.StartNew();
        for (int x = 0; x < Chunk.Size.X; x++)
        {
            for (int y = 0; y < Chunk.Size.Y; y++)
            {
                for (int z = 0; z < Chunk.Size.Z; z++)
                {
                    const bool air = false;
                    const bool outOfBounds = air;
                    
                    if (chunk[new ChunkPos(x, y, z)] == air) 
                        continue;

                    // bottom
                    if (chunk.SafeGet(new ChunkPos(x, y-1, z), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), new(),
                            (x, y, z), (x+1, y, z), 
                            (x+1, y, z+1), (x, y, z+1), 
                            TextureQuad.Full
                        );
                    }
                    // top
                    if (chunk.SafeGet(new ChunkPos(x, y + 1, z), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), new(),
                            (x, y+1, z), (x, y+1, z+1), 
                            (x+1, y+1, z+1), (x+1, y+1, z), 
                            TextureQuad.Full
                        );
                    }
                    // xy -z
                    if (chunk.SafeGet(new ChunkPos(x, y, z - 1), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), new(),
                            (x, y, z), (x, y+1, z), 
                            (x+1, y+1, z), (x+1, y, z), 
                            TextureQuad.Full
                        );
                    }
                    // xy +z
                    if (chunk.SafeGet(new ChunkPos(x, y, z + 1), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), new(),
                            (x, y, z+1), (x+1, y, z+1), 
                            (x+1, y+1, z+1), (x, y+1, z+1), 
                            TextureQuad.Full
                        );
                    }
                    // yz -x
                    if (chunk.SafeGet(new ChunkPos(x - 1, y, z), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), new(),
                            (x, y, z), (x, y, z+1), 
                            (x, y+1, z+1), (x, y+1, z), 
                            TextureQuad.Full
                        );
                    }
                    // yz +x
                    if (chunk.SafeGet(new ChunkPos(x + 1, y, z), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), new(),
                            (x+1, y, z), (x+1, y+1, z), 
                            (x+1, y+1, z+1), (x+1, y, z+1), 
                            TextureQuad.Full
                        );
                    }
                }
            }
        }
        
        model.GetMesh(PrimitiveTopology.Line)
            .AddCuboid(new (), new (), Position.Zero, Chunk.Size, TextureQuad.Full);
        
        renderTimer.Stop();
        Logger.Highlight(this, $"Generated mesh in {renderTimer.ElapsedMilliseconds} ms");
        return Nothing.Instance;
    }
}