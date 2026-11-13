using System.Diagnostics;
using System.Runtime.CompilerServices;
using Enjune.Graphic;
using Enjune.Graphic.Modeling;
using Enjune.Logger;
using Enjune.Misc;
using Enjune.Parallelism;
using Minecraft2.Block;
using Minecraft2.Graphic;
using Minecraft2.Misc;
using static Minecraft2.Misc.Utils;

namespace Minecraft2.Terrain;

public class ChunkMeshGenerator(App app) : ConcurrentWorker<Guid, ChunkMeshGenerator.Job, ChunkMeshGenerator.Job>
{
    public readonly record struct Job(
        IReadonlyChunk TargetChunk,
        IReadonlyChunk? DownChunk,
        IReadonlyChunk? UpChunk,
        IReadonlyChunk? NorthChunk,
        IReadonlyChunk? SouthChunk,
        IReadonlyChunk? EastChunk,
        IReadonlyChunk? WestChunk,
        ChunkModel Model,
        byte BlockSize // basically LoD: bigger size -> lower quality
    );
    
    
    private readonly App _app = app;
    private readonly BlockType _outOfBounds = app.Blocks.Cobblestone;
    
    private static readonly Vector4 SideColor = System.Drawing.Color.FromArgb(255, 150, 150, 150).ToTk();
    
    protected override Job Work(Job job)
    {
        job.Model.Clear();
        
        var renderTimer = Stopwatch.StartNew();
        if (job.BlockSize == 1)
            GenerateDefaultSized(job);
        else
            GenerateLoD(job);

        // job.Model.GetMesh(PrimitiveTopology.Line)
        //     .AddCuboid(new (), new (){ TextureIndex = 4 }, 
        //         Position.Zero, Chunk.Size, TextureQuad.Full);
        
        renderTimer.Stop();
        //Logger.Debug(this, $"Generated ({job.BlockSize}) {job.TargetChunk.Position} mesh in {renderTimer.ElapsedMilliseconds} ms");
        return job;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldRenderFace(BlockState? adjacentBlock)
    {
        if (adjacentBlock is null) return false; // facing unloaded chunk
        return adjacentBlock.Value.Type.Render is null; // facing air-like
    }

    private static BlockState? Sample(Job job, BlockPos locPos)
    {
        if (job.TargetChunk.BlockLayer.ContainsLocalBlockPos(locPos))
            return job.TargetChunk.BlockLayer.GetByLocal(locPos);
        
        IReadonlyChunkLayer<BlockState>? chunk;
        if (locPos.X >= Chunk.Size.X)
            chunk = job.EastChunk?.BlockLayer;
        else if (locPos.X < 0)
            chunk = job.WestChunk?.BlockLayer;
        
        else if (locPos.Y >= Chunk.Size.Y)
            chunk = job.UpChunk?.BlockLayer;
        else if (locPos.Y < 0)
            chunk = job.DownChunk?.BlockLayer;

        else if (locPos.Z >= Chunk.Size.Z)
            chunk = job.NorthChunk?.BlockLayer;
        else if (locPos.Z < 0)
            chunk = job.SouthChunk?.BlockLayer;
        else
            throw new Exception($"Can not sample {locPos}");
        
        var globalPos = job.TargetChunk.BlockLayer.ToGlobalPos(locPos);
        
        return chunk?.GetByGlobal(globalPos);
    }
    
    private static void GenerateDefaultSized(Job job)
    {
        var mesh = job.Model.GetMesh(PrimitiveTopology.Triangle);
        const byte blockSize = 1;
        for (int x = 0; x < Chunk.Size.X; x+=blockSize)
        {
            for (int y = 0; y < Chunk.Size.Y; y+=blockSize)
            {
                for (int z = 0; z < Chunk.Size.Z; z+=blockSize)
                {
                    var blockRender = job.TargetChunk.BlockLayer.GetByLocal((x, y, z)).Type.Render;
                    if (blockRender is null) continue;
                    
                    var mainColor = blockRender.BiomeColor ? (0.3f, 0.6f, 0.2f, 1) : Vector4.One;
                    var sideColor = mainColor * SideColor;

                    var mainPrim = new GraphicChunkApi.Primitive
                    {
                        TextureIndex = blockRender.TextureIndex,
                        Color = PackColor(mainColor)
                    };
                    var sidePrim = mainPrim with { Color = PackColor(sideColor) };

                    // bottom
                    if (ShouldRenderFace(Sample(job, (x, y-1, z))))
                    {
                        mesh.AddQuad(
                            new (), mainPrim,
                            PackBytes(x, y, z), PackBytes(x+blockSize, y, z), 
                            PackBytes(x+blockSize, y, z+blockSize), PackBytes(x, y, z+blockSize), 
                            TextureQuad.Full
                        );
                    }
                    // top
                    if (ShouldRenderFace(Sample(job, (x, y+1, z))))
                    {
                        mesh.AddQuad(
                            new(), mainPrim,
                            PackBytes(x, y+blockSize, z), PackBytes(x, y+blockSize, z+blockSize), 
                            PackBytes(x+blockSize, y+blockSize, z+blockSize), PackBytes(x+blockSize, y+blockSize, z), 
                            TextureQuad.Full
                        );
                    }
                    // xy -z
                    if (ShouldRenderFace(Sample(job, (x, y, z-1))))
                    {
                        mesh.AddQuad(
                            new(), sidePrim,
                            PackBytes(x, y, z), PackBytes(x, y+blockSize, z), 
                            PackBytes(x+blockSize, y+blockSize, z), PackBytes(x+blockSize, y, z), 
                            TextureQuad.Full
                        );
                    }
                    // xy +z
                    if (ShouldRenderFace(Sample(job, (x, y, z+1))))
                    {
                        mesh.AddQuad(
                            new(), sidePrim,
                            PackBytes(x, y, z+blockSize), PackBytes(x+blockSize, y, z+blockSize), 
                            PackBytes(x+blockSize, y+blockSize, z+blockSize), PackBytes(x, y+blockSize, z+blockSize), 
                            TextureQuad.Full
                        );
                    }
                    // yz -x
                    if (ShouldRenderFace(Sample(job, (x-1, y, z))))
                    {
                        mesh.AddQuad(
                            new(), sidePrim,
                            PackBytes(x, y, z), PackBytes(x, y, z+blockSize), 
                            PackBytes(x, y+blockSize, z+blockSize), PackBytes(x, y+blockSize, z), 
                            TextureQuad.Full
                        );
                    }
                    // yz +x
                    if (ShouldRenderFace(Sample(job, (x+1, y, z))))
                    {
                        mesh.AddQuad(
                            new(), sidePrim,
                            PackBytes(x+blockSize, y, z), PackBytes(x+blockSize, y+blockSize, z), 
                            PackBytes(x+blockSize, y+blockSize, z+blockSize), PackBytes(x+blockSize, y, z+blockSize), 
                            TextureQuad.Full
                        );
                    }
                }
            }
        }
    }
    
    private BlockState? DownsampleNoCache(Job job, BlockPos pos, int blockSize)
    {
        for (int y = pos.Y+blockSize-1; y >= pos.Y; y--)
        {
            var blockState = Sample(job, pos with {Y = y});
            if (blockState is null) return null;
            if (blockState.Value.Type.Render is null) continue;
            return blockState;
        }

        return _app.Blocks.Air.DefaultState;
        
        //return Sample(job, pos + BlockPos.One*(blockSize-1));
        // int dirts = 0;
        // for (int x = from.X; x < from.X+blockSize; x += 1)
        // {
        //     for (int y = from.Y; y < from.Y+blockSize; y += 1)
        //     {
        //         for (int z = from.Z; z < from.Z+blockSize; z += 1)
        //         {
        //             var blockType = Sample(job, (x, y, z));
        //             if (blockType == _app.Blocks.Grass)
        //                 return blockType;
        //             if (blockType == _app.Blocks.Dirt)
        //                 dirts += 1;
        //         }
        //     }
        // }
        //
        // if (dirts > 0)
        //     return _app.Blocks.Dirt;
        // return _app.Blocks.Air;
    }
    
    private BlockState? Downsample(Dictionary<BlockPos, BlockState?> cache, Job job, BlockPos from, int blockSize)
    {
        // if (cache.TryGetValue(from, out var sample))
        //     return sample;

        var sample = DownsampleNoCache(job, from, blockSize);
        //cache[from] = sample;
        return sample;
    }
    
    private void GenerateLoD(Job job)
    {
        Dictionary<BlockPos, BlockState?> cache = [];
        var blockSize = job.BlockSize;
        var mesh = job.Model.GetMesh(PrimitiveTopology.Triangle);
        var texture = TextureQuad.Full; // var texture = new TextureQuad
        // {
        //     TopLeft = TextureQuad.Full.TopLeft * blockSize,
        //     TopRight = TextureQuad.Full.TopRight * blockSize,
        //     BotLeft = TextureQuad.Full.BotLeft * blockSize,
        //     BotRight = TextureQuad.Full.BotRight * blockSize
        // };
        for (int x = 0; x < Chunk.Size.X; x+=blockSize)
        {
            for (int y = 0; y < Chunk.Size.Y; y+=blockSize)
            {
                for (int z = 0; z < Chunk.Size.Z; z+=blockSize)
                {
                    var blockType = Downsample(cache, job, (x, y, z), blockSize);
                    var blockRender = blockType?.Type.Render;
                    if (blockRender is null) continue;

                    var mainColor = blockRender.BiomeColor ? (0.3f, 0.6f, 0.2f, 1) : Vector4.One;
                    var sideColor = mainColor * SideColor;

                    var mainPrim = new GraphicChunkApi.Primitive
                    {
                        TextureIndex = blockRender.TextureIndex,
                        Color = PackColor(mainColor)
                    };
                    var sidePrim = mainPrim with { Color = PackColor(sideColor) };
                    
                    // bottom
                    if (ShouldRenderFace(Downsample(cache, job, (x, y-blockSize, z), blockSize)))
                    {
                        mesh.AddQuad(
                            new(), mainPrim,
                            PackBytes(x, y, z), PackBytes(x+blockSize, y, z), 
                            PackBytes(x+blockSize, y, z+blockSize), PackBytes(x, y, z+blockSize), 
                            texture
                        );
                    }
                    // top
                    if (ShouldRenderFace(Downsample(cache, job, (x, y+blockSize, z), blockSize)))
                    {
                        mesh.AddQuad(
                            new(), mainPrim,
                            PackBytes(x, y+blockSize, z), PackBytes(x, y+blockSize, z+blockSize), 
                            PackBytes(x+blockSize, y+blockSize, z+blockSize), PackBytes(x+blockSize, y+blockSize, z), 
                            texture
                        );
                    }
                    // xy -z
                    if (ShouldRenderFace(Downsample(cache, job, (x, y, z-blockSize), blockSize)))
                    {
                        mesh.AddQuad(
                            new(), sidePrim,
                            PackBytes(x, y, z), PackBytes(x, y+blockSize, z), 
                            PackBytes(x+blockSize, y+blockSize, z), PackBytes(x+blockSize, y, z), 
                            texture
                        );
                    }
                    // xy +z
                    if (ShouldRenderFace(Downsample(cache, job, (x, y, z+blockSize), blockSize)))
                    {
                        mesh.AddQuad(
                            new(), sidePrim,
                            PackBytes(x, y, z+blockSize), PackBytes(x+blockSize, y, z+blockSize), 
                            PackBytes(x+blockSize, y+blockSize, z+blockSize), PackBytes(x, y+blockSize, z+blockSize), 
                            texture
                        );
                    }
                    // yz -x
                    if (ShouldRenderFace(Downsample(cache, job, (x-blockSize, y, z), blockSize)))
                    {
                        mesh.AddQuad(
                            new(), sidePrim,
                            PackBytes(x, y, z), PackBytes(x, y, z+blockSize), 
                            PackBytes(x, y+blockSize, z+blockSize), PackBytes(x, y+blockSize, z), 
                            texture
                        );
                    }
                    // yz +x
                    if (ShouldRenderFace(Downsample(cache, job, (x+blockSize, y, z), blockSize)))
                    {
                        mesh.AddQuad(
                            new(), sidePrim,
                            PackBytes(x+blockSize, y, z), PackBytes(x+blockSize, y+blockSize, z), 
                            PackBytes(x+blockSize, y+blockSize, z+blockSize), PackBytes(x+blockSize, y, z+blockSize), 
                            texture
                        );
                    }
                }
            }
        }
    }
}