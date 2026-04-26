using System.Runtime.InteropServices;
using Enjune.File;
using Enjune.Graphic.Asset;
using Enjune.Graphic.GraphicApi.OpenGL.Component;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Array;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Texture;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;
using Enjune.Graphic.GraphicApi.Vertex.Colored;
using Enjune.Graphic.GraphicApi.Vertex.Material;
using Enjune.Misc;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using BeginMode = OpenTK.Graphics.OpenGL.BeginMode;

namespace Enjune.Graphic.GraphicApi.OpenGL;

public sealed partial class OpenGlApi : GLDisposable, IGraphicApi
{
    private unsafe Window* _window;
    
    private CompiledAssets _assets = null!;
    
    // buffers
    private Vao _mainVao = null!;
    private Vbo<MaterialVertexData> _matVertexVbo = null!;
    private Ssbo<MatId> _matIdSsbo = null!;
    private Ssbo<MaterialData> _materialSsbo = null!;
    private Ebo _ebo = null!;
    
    private Vao _colorVao = null!;
    private Vbo<ColoredVertexData> _colorVertexVbo = null!;

    private TextureArray _textureArray = null!;
    
    private Vbo<Vector2> pixelVbo; // todo remove?
    private Vao pixelVao; // todo remove?
    
    // shaders
    private ShaderProgram _mainShader = null!;
    private ShaderProgram _textShader = null!;
    private ShaderProgram _pixelShader = null!;
    private ShaderProgram _currentShader = null!;
    private ShaderProgram _colorShader = null!;
    
    // storing it here fucking gc won't erase it
    private GLFWCallbacks.KeyCallback _keyCallback = null!;
    private GLFWCallbacks.CursorPosCallback _cursorCallback = null!;
    private GLFWCallbacks.MouseButtonCallback _mouseButtonCallback = null!;
    private GLFWCallbacks.FramebufferSizeCallback _sizeChangeCallback = null!;
    private DebugProc _debugProc = null!;
    
    // uniforms
    private Matrix4Uniform _model = null!;
    private Matrix4Uniform _view = null!;
    private Matrix4Uniform _projection = null!;
    private Vector4Uniform _globalColor = null!;
    private Vector3Uniform _viewPos = null!;
    private TextureUniform _textureUniform = null!;
    
    public Error? Init(CompiledAssets assets, int width, int height, string title, int verticesCapacity,
        IUserInputHandler keyHandler,
        IGraphicApi.WindowSizeChangeHandler windowSizeHandler)
    {
        _assets = assets;
        
        // error callback
        GLFW.SetErrorCallback((error, description) =>
        {
            Logger.Error(this, $"{error}: {description}");
        });

        if (!GLFW.Init())
            return "unable to initialize GLFW";

        // GLFW configuration
        GLFW.DefaultWindowHints();
        GLFW.WindowHint(WindowHintBool.Visible, true);
        GLFW.WindowHint(WindowHintBool.Resizable, true);
        GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 4);
        GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 6);
        GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Compat);
        GLFW.WindowHint(WindowHintBool.OpenGLForwardCompat, true);

        // window creation
        unsafe
        {
            _window = GLFW.CreateWindow(width, height, title, null, null);
            if (_window == null)
                return "failed to create GLFW window";
            
            // callbacks
            
            IGraphicApi.KeyAction FromGlwf(InputAction glfw)
            {
                return glfw switch
                {
                    InputAction.Release => IGraphicApi.KeyAction.Release,
                    InputAction.Press => IGraphicApi.KeyAction.Press,
                    InputAction.Repeat => IGraphicApi.KeyAction.Repeat,
                    _ => throw new Exception($"unknown key action: {glfw}")
                };
            }
            
            _keyCallback = (window, key, scancode, glAction, mods) => keyHandler.HandleKey(key, FromGlwf(glAction));
            GLFW.SetKeyCallback(_window, _keyCallback);
            
            _mouseButtonCallback = (window, button, action, mods) => keyHandler.HandleMouseKey(button, FromGlwf(action));
            GLFW.SetMouseButtonCallback(_window, _mouseButtonCallback);
            
            _sizeChangeCallback = (_, newWidth, newHeight) => windowSizeHandler(newWidth, newHeight);
            GLFW.SetFramebufferSizeCallback(_window, _sizeChangeCallback);

            _cursorCallback = (window, x, y) => keyHandler.HandleCursor((int)x, (int)y);
            GLFW.SetCursorPosCallback(_window, _cursorCallback);
            // end callbacks

            if (GLFW.RawMouseMotionSupported())
            {
                GLFW.SetInputMode(_window, RawMouseMotionAttribute.RawMouseMotion, true);
                Logger.Log(this,"raw mouse input enabled");
            }
            else
                Logger.Warn(this,"raw mouse input isn't supported; mouse motion might be janky");
                
            
            GLFW.MakeContextCurrent(_window);
            GLFW.ShowWindow(_window);
        }
        
        // without that shit it won't work
        GL.LoadBindings(new GLFWBindingsContext());
        
        Logger.Log(this, $"OpenGL version: {GL.GetString(StringName.Version)}");
        
        // enable features
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.CullFace);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        // debug
        GL.Enable(EnableCap.DebugOutput);
        _debugProc = (source, type, id, severity, length, messagePointer, param) =>
        {
            string message = Marshal.PtrToStringUTF8(messagePointer, length);
            if (type == DebugType.DebugTypeError)
            {
                Logger.Error(this, message);
                throw new Exception(message);  
            }
            else
                Logger.Log(this, message);

        };
        GL.DebugMessageCallback(_debugProc, IntPtr.Zero);
        
        // just in case
        GL.GetInteger(GetPName.MaxTextureSize, out var maxTextureSize);
        GL.GetInteger(GetPName.MaxArrayTextureLayers, out var maxArrayLayers);
        Logger.Log(this, $"max possible texture size: {maxTextureSize}");
        Logger.Log(this, $"max possible texture array layers: {maxArrayLayers}");
        
        // other components init
        return InitComponents(verticesCapacity);
    }
    
    private const string AttributePosition = "position";
    private const string AttributeTexture = "texcoord";
    
    private const string UniformTexture = "textureArray";
    private const string UniformModel = "model";
    private const string UniformView = "view";
    private const string UniformProjection = "projection";
    private const string UniformGlobalColor = "globalColor";
    
    private Error? InitComponents(int verticesCapacity)
    {
        _pixelShader = new ShaderProgram();
        var error = _pixelShader.Init(
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "Shaders", "Point", "frag.frag"),
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "Shaders", "Point", "vert.vert"));
        if (error != null) return error;
        
        _mainShader = new ShaderProgram();
        error = _mainShader.Init(
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "Shaders", "Main", "frag.frag"),
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "Shaders", "Main", "vert.vert"));
        if (error != null) return error;
        
        _colorShader = new ShaderProgram();
        error = _colorShader.Init(
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "Shaders", "Color", "frag.frag"),
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "Shaders", "Color", "vert.vert"));
        if (error != null) return error;

        _textShader = new ShaderProgram();
        error = _textShader.Init(
            AssemblyPath.Of(Enjune.Assembly,"OpenGL", "Shaders", "Text", "frag.frag"),
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "Shaders", "Text", "vert.vert"));
        if (error != null) return error;
        
        // uniforms
        _model = new Matrix4Uniform(UniformModel, Matrix4.Identity, _mainShader, _textShader, _colorShader);
        _view = new Matrix4Uniform(UniformView, Matrix4.Identity, _mainShader, _textShader, _colorShader);
        _viewPos = new Vector3Uniform("viewPos", Vector3.Zero, _mainShader);
        _projection = new Matrix4Uniform(UniformProjection, 
            Matrix4.CreatePerspectiveFieldOfView(MathF.PI / 2, 1.0f, 0.1f, 1000.0f), 
            _mainShader, _textShader, _colorShader);
        _textureUniform = new TextureUniform(UniformTexture, 0, _mainShader, _textShader);
        _globalColor = new Vector4Uniform(UniformGlobalColor, new Color(1f), _mainShader, _textShader, _pixelShader, _colorShader);
        

        // textures
        _textureArray = new TextureArray(_assets, TextureUnit.Texture0);
        
        // buffers
        _mainVao = new Vao();
        // todo better calc capacities
        _materialSsbo = new Ssbo<MaterialData>(0, 20);
        _matIdSsbo = new Ssbo<MatId>(1, verticesCapacity);
        _matVertexVbo = new Vbo<MaterialVertexData>(verticesCapacity);
        _ebo = new Ebo(verticesCapacity);
        _colorVao = new Vao();
        _colorVertexVbo = new Vbo<ColoredVertexData>(verticesCapacity);
        
        // mat attributes
        {
            var attributes = new VaoAttributes<MaterialVertexData>(_mainVao, _matVertexVbo, _mainShader);
            attributes.Add<float>(VertexAttribPointerType.Float, AttributePosition, 3);
            attributes.Add<float>(VertexAttribPointerType.Float, AttributeTexture, 2);
            attributes.Add<float>(VertexAttribPointerType.Float, "inNorm", 3);
            attributes.Compile();
        }
        
        // color attributes
        {
            var attributes = new VaoAttributes<ColoredVertexData>(_colorVao, _colorVertexVbo, _colorShader);
            attributes.Add<float>(VertexAttribPointerType.Float, "inPosition", 3);
            attributes.Add<float>(VertexAttribPointerType.Float,"inColor", 4);
            attributes.Compile();
        }

        // loading materials
        {
            MaterialData ToData(CompiledMaterial mat) => new(mat.Raw.Color, mat.TextureId);
            var matBuffer = new FixedBuffer<MaterialData>(_assets.Materials.Length);
            matBuffer.Put(_assets.Materials.Select(ToData).ToArray());
            _materialSsbo.BindAndPush(matBuffer);
        }
        
        SelectShader(_mainShader);

        pixelVao = new Vao();
        pixelVao.Bind();
        pixelVbo = new Vbo<Vector2>(99999);
        {
            var attributes = new VaoAttributes<Vector2>(pixelVao, pixelVbo, _pixelShader);
            attributes.Add<float>(VertexAttribPointerType.Float, "pixelPosition", 2);
            attributes.Compile();
        }
        
        return null;
    }

    private void SelectShader(ShaderProgram shader)
    {
        if (_currentShader == shader) return;
        _currentShader = shader;
        shader.Bind();
    }
    
    public void SwitchShader(IGraphicApi.ShaderType type)
    {
        switch (type)
        {
            case IGraphicApi.ShaderType.Main:
                SelectShader(_mainShader);
                break;
            case IGraphicApi.ShaderType.Text:
                SelectShader(_textShader);
                break;
            case IGraphicApi.ShaderType.Color:
                SelectShader(_colorShader);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    // todo add officialy or remove
    public void RenderPixelsToScreenBuffer(FixedBuffer<Vector2> pixelPosition)
    {
        GL.Disable(EnableCap.DepthTest);
        _pixelShader.Bind();

        pixelVao.Bind();
        pixelVbo.BindAndPush(pixelPosition);
        GL.DrawArrays(PrimitiveType.Points, 0, pixelPosition.Count*2);
        
        // bind back
        _currentShader.Bind();

        GL.Enable(EnableCap.DepthTest); 
    }

    public void RenderToScreenBuffer(MaterialVertexBuffer buffer, IGraphicApi.Primitive primitive = IGraphicApi.Primitive.Triangle)
    {
        _mainVao.Bind();
        _matVertexVbo.BindAndPush(buffer.VertexVbo);
        _matIdSsbo.BindAndPush(buffer.MatIdSsbo);
        _ebo.BindAndPush(buffer.Ebo);
        
        SelectShader(_mainShader);

        GL.DrawElements(fromApi(primitive), buffer.Ebo.Count, DrawElementsType.UnsignedInt, 0);
    }

    public void RenderToScreenBuffer(ColoredVertexBuffer buffer, IGraphicApi.Primitive primitive = IGraphicApi.Primitive.Triangle)
    {
        _colorVao.Bind();
        _colorVertexVbo.BindAndPush(buffer.Vbo);
        _ebo.BindAndPush(buffer.Ebo);
        
        SelectShader(_colorShader);

        GL.DrawElements(fromApi(primitive), buffer.Ebo.Count, DrawElementsType.UnsignedInt, 0);
    }
    
    protected override void DisposeGLData()
    {
        _mainVao.Dispose();
        _colorVao.Dispose();
        
        _materialSsbo.Dispose();
        _matIdSsbo.Dispose();
        _matVertexVbo.Dispose();
        _colorVertexVbo.Dispose();
        _ebo.Dispose();
        _mainShader.Dispose();
        _textShader.Dispose();
        _textureArray.Dispose();
        _colorShader.Dispose();
        
        unsafe
        {
            GLFW.SetKeyCallback(_window, null);
            GLFW.SetFramebufferSizeCallback(_window, null);
            GLFW.DestroyWindow(_window);
        }

        GLFW.Terminate();
        GLFW.SetErrorCallback(null);
    }
}