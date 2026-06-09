using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Enjune;
using Enjune.Data;
using Enjune.Data.Json;
using Enjune.File;
using Enjune.File.ModelReader;
using Enjune.Misc;
using Enjune.World;

namespace SceneMaker;

class Program
{
    private static void Main(string[] args)
    {
        Enjune.Enjune.Run(new EcsApp(), args);
    }
}