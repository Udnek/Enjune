global using Vector2 = OpenTK.Mathematics.Vector2;
global using Vector3 = OpenTK.Mathematics.Vector3;
global using Vector4 = OpenTK.Mathematics.Vector4;
global using Quaternion = OpenTK.Mathematics.Quaternion;
global using Matrix4 = OpenTK.Mathematics.Matrix4;
global using OpenTK.Graphics.OpenGL4;
global using Error = Enjune.Misc.Error;

// aliases
global using Position = OpenTK.Mathematics.Vector3;
global using Color = OpenTK.Mathematics.Vector4;
global using TextureCoord = OpenTK.Mathematics.Vector2;

global using GlfwKey = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
global using Nanoseconds = long;
global using Fps = float;


global using EntityId = ushort; // (n=16) n-bits Entity ID -> 2^n entities are allowed to exist
global using ComponentTypeId = uint; // (n=8) n-bits type ID -> 2^n components are allowed to exist
global using SignatureInteger = uint; // (n=32) n-bits mask -> n components per entity allowed

global using TexId = int;
global using MatId = int;