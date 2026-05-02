using System.Diagnostics;
using System.Reflection;
using Enjune.File;
using Path = System.IO.Path;

namespace Tests;

/// <summary>
/// human slop
/// </summary>
public class FileTests
{
    
    private Assembly _assembly = typeof(FileTests).Assembly;
    
    [Fact]
    public void AssemblyPathTest()
    {
        Assert.Equal("Tests.Resources.Aboba", AssemblyPath.Of(_assembly, "Aboba").ToString());
        Assert.Equal("Tests.Resources", AssemblyPath.Of(_assembly, "Aboba").Parent().ToString());
        Assert.Equal("Tests.Resources", AssemblyPath.Of(_assembly, "Aboba.exe").ThisDirectory().ToString());
        Assert.Equal("Tests.Resources.Kek", AssemblyPath.Of(_assembly, "Kek","Aboba.exe").Parent().ToString());
        
        Assert.Equal("Tests.Resources.Kek.Aboba.exe", AssemblyPath.Of(_assembly).ResolveRaw("./Kek/Aboba.exe").ToString());
        Assert.Equal("Tests.Resources.Lol.exe", AssemblyPath.Of(_assembly, "Kek").ResolveRaw("../Lol.exe").ToString());
        Assert.Equal("Tests.Resources.Kek.Aboba.exe", AssemblyPath.Of(_assembly).ResolveRaw(@".\Kek\Aboba.exe").ToString());
        Assert.Equal("Tests.Resources.Lol.exe", AssemblyPath.Of(_assembly, "Kek").ResolveRaw(@"..\Lol.exe").ToString());
    }

    [Fact]
    public void ExternalPathTest()
    {
        Assert.Equal(Path.GetFullPath("."), ExternalPath.Of(".").ToString());
        Assert.Equal(Directory.GetParent(Path.GetFullPath("."))!.FullName, ExternalPath.Of(".").Parent().ToString());
    }
    
}