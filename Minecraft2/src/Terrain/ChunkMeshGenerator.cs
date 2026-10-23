using System.Diagnostics;
using Enjune.Graphic;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Minecraft2.Bridge;

namespace Minecraft2.Misc;

public class ChunkMeshGenerator(App app) : ConcurrentWorker<Guid, (Chunk Chunk, ChunkModel Model), (Chunk Chunk, ChunkModel Model)>
{
    protected override (Chunk Chunk, ChunkModel Model) Work((Chunk Chunk, ChunkModel Model) job)
    {
        var (chunk, model) = job;
        model.Clear();
        Vector3 sideColor = System.Drawing.Color.FromArgb(255, 150, 150, 150).ToTk().Xyz;
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
                            new(), new(){Color = sideColor},
                            (x, y, z), (x, y+1, z), 
                            (x+1, y+1, z), (x+1, y, z), 
                            TextureQuad.Full
                        );
                    }
                    // xy +z
                    if (chunk.SafeGet(new ChunkPos(x, y, z + 1), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), new(){Color = sideColor},
                            (x, y, z+1), (x+1, y, z+1), 
                            (x+1, y+1, z+1), (x, y+1, z+1), 
                            TextureQuad.Full
                        );
                    }
                    // yz -x
                    if (chunk.SafeGet(new ChunkPos(x - 1, y, z), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), new(){Color = sideColor},
                            (x, y, z), (x, y, z+1), 
                            (x, y+1, z+1), (x, y+1, z), 
                            TextureQuad.Full
                        );
                    }
                    // yz +x
                    if (chunk.SafeGet(new ChunkPos(x + 1, y, z), outOfBounds) == air)
                    {
                        model.GetMesh(PrimitiveTopology.Triangle).AddQuad(
                            new(), new(){Color = sideColor},
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
        //Thread.Sleep(TimeSpan.FromSeconds(0.5));
        Logger.Highlight(this, $"Generated mesh in {renderTimer.ElapsedMilliseconds} ms");
        return job;
    }
}