
global using OpenTK.Graphics.OpenGL4;

global using Vector2 = OpenTK.Mathematics.Vector2;
global using Vector3 = OpenTK.Mathematics.Vector3;
global using Vector4 = OpenTK.Mathematics.Vector4;

global using Position = OpenTK.Mathematics.Vector3;
global using Color = OpenTK.Mathematics.Vector4;
global using TextureCoord = OpenTK.Mathematics.Vector2;

global using GlfwKey = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
global using Nanoseconds = long;
global using Fps = float;
global using TexId = int;
using Enjune;
using Enjune.Graphic.GraphicApi;
using Enjune.Graphic.OpenGL;

var app = new App();
app.Init();
app.Run();
