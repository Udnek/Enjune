using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Minecraft2.Ecs.Component;
using Minecraft2.Misc;
using OpenTK.Mathematics;

namespace Minecraft2.Ecs.System;

public class ChunkLoadSystem : ISystem
{
    public void OnInit(World world)
    {
        var noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        for (int chunkX = 0; chunkX < 5; chunkX++)
        {
            for (int chunkZ = 0; chunkZ < 5; chunkZ++)
            {
                var chunk = new Chunk((chunkX, 0, chunkZ));
                for (int blockX = 0; blockX < Chunk.Size.X; blockX++)
                {
                    for (int blockZ = 0; blockZ < Chunk.Size.Z; blockZ++)
                    {
                        var y = noise.GetNoise(chunkX*Chunk.Size.X +blockX, chunkZ*Chunk.Size.Z +blockZ);
                        var height = (int) Math.Clamp(y , 0, Chunk.Size.Y);
                        for (int i = 0; i < height; i++)
                        {
                            chunk[new(blockX, i, blockZ)] = true;
                        }
                    }
                }

                //chunk.IsDirty = true;
                world.AddEntity(new Entity.Assembly()
                    .AddComponent(new ChunkComponent{Chunk = chunk}));
            }
        }
    }

    public void OnUpdate()
    {
        
    }
}