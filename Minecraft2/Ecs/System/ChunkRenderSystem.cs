using System.Diagnostics;
using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Graphic;
using Enjune.Graphic.Api;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Minecraft2.Bridge;
using Minecraft2.Ecs.Component;
using Minecraft2.Misc;
using OpenTK.Mathematics;

namespace Minecraft2.Ecs.System;

public class ChunkRenderSyncSystem : ISystem
{
    private Query<GraphicLinkComponent, ChunkComponent> _query = null!;
    private readonly GraphicEngine _graphicEngine;

    public ChunkRenderSyncSystem(App app)
    {
        _graphicEngine = app.GraphicEngine;
    }

    public void OnInit(World world)
    {
        _query = new QueryBuilder(world)
            .Retrieve<GraphicLinkComponent, ChunkComponent>();
    }

    public void OnUpdate()
    {
        _query.ForEach((_, ref graphicLink, ref chunkComp) =>
        {
            var chunk = chunkComp.Chunk;
            if (!chunk.IsDirty) return;
            // mesh generation

            var model = _graphicEngine.Objects[graphicLink.GraphicId].Model;
            if (model is DynamicRenderableModel dynamicRenderable)
                RegenerateModel(chunk, dynamicRenderable);
            else
                Logger.Warn(this, $"Can not update model: model for {chunk} is not dynamic");

            chunk.IsDirty = false;
        });
    }

    private void RegenerateModel(Chunk chunk, DynamicRenderableModel model)
    {
        var renderBegin = Stopwatch.StartNew();
        for (int x = 0; x < Chunk.Size.X; x++)
        {
            for (int y = 0; y < Chunk.Size.Y; y++)
            {
                for (int z = 0; z < Chunk.Size.Z; z++)
                {
                    if (chunk[new Vector3i(x, y, z)] == false) 
                        continue;
                    // bottom
                    if (chunk.SafeGet(new Vector3i(x, y-1, z), true) == false)
                        model.Add(new MeshInstance.Entry
                        {
                            Geometry = Mesh.Quad(
                                (x, y, z), (x+1, y, z), 
                                (x+1, y, z+1), (x, y, z+1), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    // top
                    if (chunk.SafeGet(new Vector3i(x, y+1, z), true) == false)
                        model.Add(new MeshInstance.Entry
                        {
                            Geometry = Mesh.Quad(
                                (x, y+1, z), (x, y+1, z+1), 
                                (x+1, y+1, z+1), (x+1, y+1, z), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    // xy -z
                    if (chunk.SafeGet(new Vector3i(x, y, z-1), true) == false)
                        model.Add(new MeshInstance.Entry
                        {
                            Geometry = Mesh.Quad(
                                (x, y, z), (x, y+1, z), 
                                (x+1, y+1, z), (x+1, y, z), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    // xy +z
                    if (chunk.SafeGet(new Vector3i(x, y, z+1), true) == false)
                        model.Add(new MeshInstance.Entry
                        {
                            Geometry = Mesh.Quad(
                                (x, y, z+1), (x, y+1, z+1), 
                                (x+1, y+1, z+1), (x+1, y, z+1), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    // yz -x
                    if (chunk.SafeGet(new Vector3i(x-1, y, z), true) == false)
                        model.Add(new MeshInstance.Entry
                        {
                            Geometry = Mesh.Quad(
                                (x, y, z), (x, y+1, z), 
                                (x, y+1, z+1), (x, y, z+1), 
                                TextureQuad.Full, calculateNormals: false)
                        });
                    
                    // yz +x
                    if (chunk.SafeGet(new Vector3i(x-1, y, z), true) == false)
                        model.Add(new MeshInstance.Entry
                        {
                            Geometry = Mesh.Quad(
                                (x+1, y, z), (x+1, y, z+1), 
                                (x+1, y+1, z+1), (x+1, y+1, z), 
                                TextureQuad.Full, calculateNormals: false)
                        });

                    // model.Add(new MeshInstance.Entry
                    // {
                    //     Geometry = Mesh.Cube(new Position(x, y, z), 1, TextureQuad.Full)
                    // });
                }
            }
        }
        
        model.Add(new MeshInstance.Entry
        {
            Geometry = Mesh.Cuboid(Position.Zero, Chunk.Size, TextureQuad.Full, PrimitiveTopology.LineStrip),
            Color = new Color(0, 1, 0, 0.5f)
        });
        
        model.Refit();
        
        renderBegin.Stop();
        Logger.Highlight(this, $"Generated mesh for {chunk} in {renderBegin.ElapsedMilliseconds} ms");
    }
}