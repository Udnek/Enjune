using System.Reflection;
using Enjune.Logger;
using Enjune.Misc;
using Minecraft2.Misc;
using Minecraft2.Terrain;

namespace Minecraft2;

public class Program
{
    internal static readonly Assembly Assembly = typeof(Program).Assembly;
    private static readonly Logger.Domain Mc2Domain = new("MC2", ConsoleColor.Yellow);
    private static readonly Logger.Domain MeshDomain = new("Mesh", ConsoleColor.Magenta);
    private static readonly Logger.Domain TerrainDomain = new("Terrain", ConsoleColor.DarkGreen);
    private static readonly Logger.Domain PoolDomain = new("Pool", ConsoleColor.Cyan);
    
    internal static void Main(string[] args)
    {
        Logger.RegisterNamespaceToDomain(Assembly, "", Mc2Domain);
        Logger.RegisterTypeToDomain(typeof(ChunkMeshGenerator), MeshDomain);
        Logger.RegisterTypeToDomain(typeof(BaseTerrainGenerator), TerrainDomain);
        Logger.RegisterTypeToDomain(typeof(RecyclingPool<>), PoolDomain);
        Logger.IgnoreDebugLogs = false;
        //Logger.EnableDebugFor(PoolDomain);
        Enjune.Enjune.Run(new App(), args);
    }
}