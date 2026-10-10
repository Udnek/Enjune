using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Misc;
using Minecraft2.Ecs.Component;
using OpenTK.Mathematics;

namespace Minecraft2.Misc;

public class ChunkWorld
{
    private readonly Dictionary<ChunkPos, Entity> _loadedChunks = [];
    private readonly HashSet<ChunkPos> _queuedForGeneration = [];
    private readonly App _app;

    public ChunkWorld(App app) => _app = app;

    /// <summary>
    /// Loads new chunk and adds to ecs world or just returns already loaded
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public void Load(ChunkPos pos)
    {
        // already loaded
        if (_loadedChunks.TryGetValue(pos, out var alreadyLoaded))
        {
            _app.World.ModifyEntityComponent<ChunkComponent>(alreadyLoaded, c => c with { ToBeUnloaded = false });
            return;
        }
        
        // already queued
        if (_queuedForGeneration.Contains(pos))
        {
            Logger.Highlight(this, $"{pos} already queued for generation");
            return;
        }

        _queuedForGeneration.Add(pos);
        _app.TerrainGenerator.Enqueue(pos);
    }

    public void Unload(ChunkPos pos)
    {
        if (_loadedChunks.Remove(pos, out var chunk))
        {
            _app.World.RemoveEntityComponent<ChunkComponent>(chunk);
            Logger.Highlight(this, $"Unloading {pos} {chunk}");
        }
        else if (_queuedForGeneration.Remove(pos))
        {
            _app.TerrainGenerator.CancelJob(pos);
            Logger.Highlight(this, $"Dequeuing generation of {pos}");
        }
        else
            Logger.Highlight(this, $"Trying to unloaded {pos}, but already unloaded");
    }

    /// <summary>
    /// Tries to pull all generated chunks from TerrainGenerator
    /// </summary>
    public void PullLoading()
    {
        while (_app.TerrainGenerator.TryDequeue(out var res))
        {
            var (pos, chunk) = res;
            if (!_queuedForGeneration.Remove(pos))
            {
                Logger.Warn(this, $"Got chunk that was not queued: {pos}");
                return;
            }
            
            var entity = _app.World.AddEntity(new Entity.Assembly()
                .AddComponent(new ChunkComponent
                {
                    Chunk = chunk,
                    Pos = pos
                }));
            _loadedChunks[pos] = entity;
            Logger.Highlight(this, $"Successfully loaded {pos}");
        }
    }
}