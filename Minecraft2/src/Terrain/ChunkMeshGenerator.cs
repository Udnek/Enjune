using System.Diagnostics;
using Enjune.Graphic;
using Enjune.Graphic.Modeling;
using Enjune.Misc;

namespace Minecraft2.Misc;

public class ChunkMeshGenerator(App app) : ConcurrentWorker<(Chunk Chunk, DynamicRenderableModel Model), Nothing>
{
    protected override Nothing Work((Chunk Chunk, DynamicRenderableModel Model) job)
    {
        var (chunk, model) = job;
        model.Clear();
        var material = app.DirtMaterial;
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
                        model.Add(new MeshInstance.Entry
                        {
                            Material = material,
                            Geometry = Mesh.Quad(
                                (x, y, z), (x+1, y, z), 
                                (x+1, y, z+1), (x, y, z+1), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    // top
                    if (chunk.SafeGet(new ChunkPos(x, y+1, z), outOfBounds) == air)
                        model.Add(new MeshInstance.Entry
                        {
                            Material = material,
                            Geometry = Mesh.Quad(
                                (x, y+1, z), (x, y+1, z+1), 
                                (x+1, y+1, z+1), (x+1, y+1, z), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    // xy -z
                    if (chunk.SafeGet(new ChunkPos(x, y, z-1), outOfBounds) == air)
                        model.Add(new MeshInstance.Entry
                        {
                            Color = sideColor,
                            Material = material,
                            Geometry = Mesh.Quad(
                                (x, y, z), (x, y+1, z), 
                                (x+1, y+1, z), (x+1, y, z), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    // xy +z
                    if (chunk.SafeGet(new ChunkPos(x, y, z+1), outOfBounds) == air)
                        model.Add(new MeshInstance.Entry
                        {
                            Color = sideColor,
                            Material = material,
                            Geometry = Mesh.Quad(
                                (x, y, z+1), (x+1, y, z+1), 
                                (x+1, y+1, z+1), (x, y+1, z+1), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    // yz -x
                    if (chunk.SafeGet(new ChunkPos(x-1, y, z), outOfBounds) == air)
                        model.Add(new MeshInstance.Entry
                        {
                            Color = sideColor,
                            Material = material,
                            Geometry = Mesh.Quad(
                                (x, y, z), (x, y, z+1), 
                                (x, y+1, z+1), (x, y+1, z), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    
                    // yz +x
                    if (chunk.SafeGet(new ChunkPos(x+1, y, z), outOfBounds) == air)
                        model.Add(new MeshInstance.Entry
                        {
                            Color = sideColor,
                            Material = material,
                            Geometry = Mesh.Quad(
                                (x+1, y, z), (x+1, y+1, z), 
                                (x+1, y+1, z+1), (x+1, y, z+1), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                }
            }
        }
        
        model.Add(new MeshInstance.Entry
        {
            Geometry = Mesh.Cuboid(Position.Zero, Chunk.Size, TextureQuad.Full, PrimitiveTopology.Line),
            Color = new Color(0, 1, 0, 0.5f)
        });
        
        renderTimer.Stop();
        Logger.Highlight(this, $"Generated mesh in {renderTimer.ElapsedMilliseconds} ms");
        return Nothing.Instance;
    }
}