using Enjune.Attribute;
using Enjune.Graphic.Asset;
using Enjune.Misc;
using JetBrains.Annotations;

namespace Enjune.Graphic.Modeling;

/// <summary>
/// Multiple meshes with same topology
/// </summary>
[LogParams(method: LogParamsAttribute.Method.ToString)]
public sealed class MeshInstance
{
    /// <summary>
    /// Common topology across all entries
    /// </summary>
    public readonly PrimitiveTopology CommonTopology;
    public ReadOnlySpan<Entry> Entries => _entries.AsSpan();
    private List<Entry> _entries;

    /// <summary>
    /// Should be used only internally cause doesn't check for same topology across all meshes
    /// </summary>
    /// <param name="commonTopology"></param>
    /// <param name="entries"></param>
    private MeshInstance(PrimitiveTopology commonTopology, List<Entry> entries)
    {
        CommonTopology = commonTopology;
        _entries = entries;
    }

    public static MeshInstance Of(Entry entry) => new(entry.Geometry.Topology, [entry]);

    public static MeshInstance Of(PrimitiveTopology commonTopology, List<Entry> entries)
    {
        entries.RemoveAll(e =>
        {
            if (e.Geometry.Topology == commonTopology)
                return false;
            Logger.Warn(typeof(MeshInstance),
                $"Removing {e} cause it have inappropriate topology: {e.Geometry.Topology}");
            return true;
        });
        return new MeshInstance(commonTopology, entries);
    }
    
    public static MeshInstance CreateEmpty(PrimitiveTopology commonTopology) 
        => new(commonTopology, new List<Entry>());

    public void Add(Entry entry)
    {
        if (entry.Geometry.Topology != CommonTopology)
        {
            Logger.Warn(this, $"Can not add entry {entry} cause it have inappropriate topology: {entry.Geometry.Topology}");
            return;
        }
        _entries.Add(entry);
    }

    public override string ToString() => $"{nameof(MeshInstance)}[{CommonTopology}]";

    public struct Entry()
    {
        public required Mesh Geometry { get; init; }
        public CompiledMaterial? Material;
        public Color Color = Color.One;

        public (CompiledMaterial?, Color) Properties => (Material, Color);
    }

    [LogParams(method: LogParamsAttribute.Method.ToString)]
    public class Builder(PrimitiveTopology commonTopology, int capacity = 4)
    {
        private readonly List<Entry> _entries = new(capacity);
        private readonly PrimitiveTopology _commonTopology = commonTopology;

        public Builder Add(Entry entry)
        {
            if (entry.Geometry.Topology != _commonTopology)
                Logger.Warn(this,
                    $"Can not add entry {entry} cause it have inappropriate topology: {entry.Geometry.Topology}");
            else
                _entries.Add(entry);

            return this;
        }
        
        public Builder Clear()
        {
            _entries.Clear();
            return this;
        }

        public override string ToString() => $"{Logger.GetTypeName<Builder>()}[{_commonTopology}]";

        /// <summary>
        /// if meshes with same color and texture should be merged into one to safe space.
        /// It is pretty heavy operation to merge, should be used for static objects
        /// </summary>
        /// <param name="mergeSimilar"></param>
        /// <returns></returns>
        public MeshInstance Build(bool mergeSimilar)
        {
            if (!mergeSimilar) 
                return new MeshInstance(_commonTopology, _entries);
            
            List<Entry> newEntries = new List<Entry>();
            foreach (var group in _entries.GroupBy(e => e.Properties))
            {
                var merged = Mesh.Merge(group.Select(e => e.Geometry).ToArray());
                newEntries.Add(new Entry
                {
                    Geometry = merged,
                    Material = group.Key.Item1,
                    Color = group.Key.Item2
                });
            }

            return new MeshInstance(_commonTopology, newEntries);
        }
    }
}