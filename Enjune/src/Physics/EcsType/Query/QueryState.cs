namespace Enjune.Physics.EcsType.Query;

public struct QueryState
{
    public Signature RequiredComponents;
    public Signature ExcludedComponents;
    //public Signature OptionalComponents;

    public int ArchetypeGeneration;
    
}