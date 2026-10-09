using System.Diagnostics;
using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Graphic;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Minecraft2.Ecs.Component;
using OpenTK.Mathematics;

namespace Minecraft2.Ecs.System.Chunk;

public class ChunkRenderSystem : AppSystem
{
    private Query<ChunkComponent, GraphicLinkComponent> _query = null!;

    public override void OnInit(World world)
    {
        _query = new QueryBuilder(world)
            .Retrieve<ChunkComponent, GraphicLinkComponent>();
    }

    public override void OnUpdate()
    {
        int c = 0;
        _query.ForEach((_, ref _, ref _) => c += 1);
        Logger.Highlight(this, $"Renderable chunks: {c}");
        // _query.ForEach((_, ref graphicLink, ref chunkComp) =>
        // {
        //     var chunk = chunkComp.Chunk;
        //     Logger.Highlight(this, $"Rendering {chunk}");
        //     if (!chunk.IsDirty) return;
        //     
        //     
        //     // mesh generation
        //     var model = App.GraphicEngine.Objects[graphicLink.GraphicId].Model;
        //     if (model is DynamicRenderableModel dynamicRenderable)
        //         RegenerateModel(chunk, dynamicRenderable);
        //     else
        //         Logger.Warn(this, $"Can not update model: model for {chunk} is not dynamic");
        //
        //     chunk.IsDirty = false;
        // });
    }

    private void RegenerateModel(Misc.Chunk chunk, DynamicRenderableModel model)
    {
        var material = App.DirtMaterial;
        var renderBegin = Stopwatch.StartNew();
        for (int x = 0; x < Misc.Chunk.Size.X; x++)
        {
            for (int y = 0; y < Misc.Chunk.Size.Y; y++)
            {
                for (int z = 0; z < Misc.Chunk.Size.Z; z++)
                {
                    const bool air = false;
                    const bool outOfBounds = air;
                    
                    if (chunk[new Vector3i(x, y, z)] == air) 
                        continue;

                    // bottom
                    if (chunk.SafeGet(new Vector3i(x, y-1, z), outOfBounds) == air)
                        model.Add(new MeshInstance.Entry
                        {
                            Material = material,
                            Geometry = Mesh.Quad(
                                (x, y, z), (x+1, y, z), 
                                (x+1, y, z+1), (x, y, z+1), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    // top
                    if (chunk.SafeGet(new Vector3i(x, y+1, z), outOfBounds) == air)
                        model.Add(new MeshInstance.Entry
                        {
                            Material = material,
                            Geometry = Mesh.Quad(
                                (x, y+1, z), (x, y+1, z+1), 
                                (x+1, y+1, z+1), (x+1, y+1, z), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    // xy -z
                    if (chunk.SafeGet(new Vector3i(x, y, z-1), outOfBounds) == air)
                        model.Add(new MeshInstance.Entry
                        {
                            Material = material,
                            Geometry = Mesh.Quad(
                                (x, y, z), (x, y+1, z), 
                                (x+1, y+1, z), (x+1, y, z), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    // xy +z
                    if (chunk.SafeGet(new Vector3i(x, y, z+1), outOfBounds) == air)
                        model.Add(new MeshInstance.Entry
                        {
                            Material = material,
                            Geometry = Mesh.Quad(
                                (x, y, z+1), (x+1, y, z+1), 
                                (x+1, y+1, z+1), (x, y+1, z+1), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    // yz -x
                    if (chunk.SafeGet(new Vector3i(x-1, y, z), outOfBounds) == air)
                        model.Add(new MeshInstance.Entry
                        {
                            Material = material,
                            Geometry = Mesh.Quad(
                                (x, y, z), (x, y, z+1), 
                                (x, y+1, z+1), (x, y+1, z), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    
                    // yz +x
                    if (chunk.SafeGet(new Vector3i(x+1, y, z), outOfBounds) == air)
                        model.Add(new MeshInstance.Entry
                        {
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
            Geometry = Mesh.Cuboid(Position.Zero, Misc.Chunk.Size, TextureQuad.Full, PrimitiveTopology.LineStrip),
            Color = new Color(0, 1, 0, 0.5f)
        });
        
        model.Refit();
        
        renderBegin.Stop();
        Logger.Highlight(this, $"Generated mesh for {chunk} in {renderBegin.ElapsedMilliseconds} ms");
    }
}