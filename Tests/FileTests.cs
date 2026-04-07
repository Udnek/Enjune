using System.Diagnostics;
using System.Reflection;
using Enjune.File;
using Path = System.IO.Path;

namespace Tests;

public class FileTests
{
    
    private Assembly _assembly = typeof(FileTests).Assembly;
    
    [Fact]
    public void AssemblyPathTest()
    {
        Assert.Equal("Tests.Resources.Aboba", new AssemblyPath(_assembly, "Aboba").ToString());
        Assert.Equal("Tests.Resources", new AssemblyPath(_assembly, "Aboba").Parent().ToString());
        Assert.Equal("Tests.Resources", new AssemblyPath(_assembly, "Aboba.exe").ThisDirectory().ToString());
        Assert.Equal("Tests.Resources.Kek", new AssemblyPath(_assembly, "Kek","Aboba.exe").Parent().ToString());
        
        Assert.Equal("Tests.Resources.Kek.Aboba.exe", new AssemblyPath(_assembly).ResolveRaw("./Kek/Aboba.exe").ToString());
        Assert.Equal("Tests.Resources.Lol.exe", new AssemblyPath(_assembly, "Kek").ResolveRaw("../Lol.exe").ToString());
        Assert.Equal("Tests.Resources.Kek.Aboba.exe", new AssemblyPath(_assembly).ResolveRaw(@".\Kek\Aboba.exe").ToString());
        Assert.Equal("Tests.Resources.Lol.exe", new AssemblyPath(_assembly, "Kek").ResolveRaw(@"..\Lol.exe").ToString());
    }

    [Fact]
    public void ExternalPathTest()
    {
        Assert.Equal(Path.GetFullPath("."), ExternalPath.Of(".").ToString());
        Assert.Equal(Directory.GetParent(Path.GetFullPath("."))!.FullName, ExternalPath.Of(".").Parent().ToString());
    }
    
}