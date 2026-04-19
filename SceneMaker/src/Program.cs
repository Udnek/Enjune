using Enjune.File;
using Enjune.File.ModelReader;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Font;
using Enjune.Misc;

namespace SceneMaker;

class Program
{
    private static void Main(string[] args)
    {
        Enjune.Enjune.Run(new App(), args);
    }
}