global using Vector3 = OpenTK.Mathematics.Vector3;
global using Vector4 = OpenTK.Mathematics.Vector4;

global using Position = OpenTK.Mathematics.Vector3;
global using Color = OpenTK.Mathematics.Vector4;
global using GlfwKey = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
global using Nanoseconds = long;
global using Fps = float;


global using EntityId = ushort; // (n=16) n-bits Entity ID -> 2^n entities are allowed to exist
global using ComponentTypeId = byte; // (n=8) n-bits type ID -> 2^n components are allowed to exist
//global using Signature = uint; // (n=32) n-bits mask -> n components per entity allowed

using Enjune;

new App().Run();