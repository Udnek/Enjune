using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Logger;
using Enjune.Misc;
using Minecraft2.Ecs.Component;
using Minecraft2.Ecs.System.Misc;
using Minecraft2.Terrain;

namespace Minecraft2.Ecs.System.Chunks;

public class ChunkChooseLoDSystem : AppSystem
{
    private static readonly int MaxLoDPowOf2 = (int) Math.Log2(Math.Min(Chunk.Size.X, Math.Min(Chunk.Size.Y, Chunk.Size.Z)));

    static ChunkChooseLoDSystem()
    {
        Logger.Info(typeof(ChunkChooseLoDSystem), $"{nameof(MaxLoDPowOf2)}: {MaxLoDPowOf2}");
    }
    
    private World _world = null!;
    private Query<ChunkComponent, ChunkGraphicComponent> _chunkQuery = null!;
    
    public override void OnInit(World world)
    {
        _world = world;
        _chunkQuery = new QueryBuilder(world)
            .Retrieve<ChunkComponent, ChunkGraphicComponent>();
    }

    public override void OnUpdate()
    {
        var playerTransform = _world.GetEntityComponent<Transform>(App.PlayerEntity);
        if (playerTransform is null)
        {
            Logger.Warn(this, $"Player doesn't have {Logger.GetTypeName<Transform>()} component");
            return;
        }
        
        _chunkQuery.ForEach((_, ref chunkComp, ref chunkGrx) =>
        {
            if (chunkComp.ToBeUnloaded) return;

            var old = chunkGrx.BlockSize;
            chunkGrx.BlockSize = 1 << Math.Min(
                MaxLoDPowOf2, 
                (Chunk.ToChunkPos(playerTransform.Value.Position) - chunkComp.Chunk.Position).ManhattanLength/5); // powers of 2
            
            if (old != chunkGrx.BlockSize)
                chunkGrx.BlockSizeChanged = true;
        });
    }
    
}