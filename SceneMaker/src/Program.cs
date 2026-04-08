using Enjune.File;
using Enjune.Graphic.Font;

namespace SceneMaker;

class Program
{
    static void Main(string[] args)
    {
        new FontLoader().Load(AssemblyPath.Of(Enjune.Enjune.Assembly, "Fonts", "papyrus.ttf"));
        // Enjune.Enjune.Run(new App(), args);
    }
}