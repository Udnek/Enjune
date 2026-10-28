using System.Collections.Concurrent;
using Enjune.Graphic.Api;
using Enjune.Graphic.Modeling;
using Enjune.KitStart.TAAL;
using Enjune.Logger;
using Enjune.Misc;
using Minecraft2.Misc;

namespace Minecraft2.Graphic;

public class GraphicEngine
{
    private readonly App _app;
    private readonly IGraphicApi _grxApi;

    // chunks
    private readonly RecyclingPool<RenderableChunkModel> _chunkModelPool;
    private readonly Dictionary<Guid, (RenderableChunkModel Model, Matrix4 Transform)> _chunks = [];
    private readonly ConcurrentQueue<ChunkCommand> _chunkCommands = [];
    
    // uniforms
    private IUniform<Uniforms.Camera> _cameraUniform = null!;
    private IUniform<Uniforms.Model> _modelUniform = null!;
    // shaders
    private IShader<GraphicChunkApi.Vertex, GraphicChunkApi.Primitive> _chunkShader = null!;
    private IShader<GraphicScreenApi.Vertex, GraphicScreenApi.Primitive> _screenShader = null!;
    // textures
    private ITextureArray _blockTexture = null!;
    private IDynamicTexture _screenTexture = null!;
    // texture slots
    private ITextureSlot<ITextureArray> _blockTextureSlot = null!;
    private ITextureSlot<IDynamicTexture> _screenTextureSlot = null!;
    
    // misc
    private Matrix4 _projection;
    private IRenderableMesh<GraphicScreenApi.Vertex, GraphicScreenApi.Primitive> _screenMesh;
    
    public GraphicEngine(App app, IGraphicApi grxApi)
    {
        _app = app;
        _grxApi = grxApi;
        _chunkModelPool = new(() =>
        {
            if (RenderableChunkModel.Create(_grxApi)
                .GetIsFailed(out var mesh, out var error))
            {
                Logger.Error(this, "can not create mesh: " + error);
            }
            return mesh!;
        });
    }

    public Error? Init(CompiledTAAL blockAssets)
    {
        // textures
        if (_grxApi.CreateTextureArray(blockAssets.Textures).GetIsFailed(out _blockTexture!, out var error))
            return "can not create block texture: " + error;
        
        if (_grxApi.CreateDynamicTexture(_app.InputHandler.WindowSize).GetIsFailed(out _screenTexture!, out error))
            return "can not create screen texture: " + error;
        
        // texture slots
        if (_grxApi.CreateTextureSlot<ITextureArray>("block_tex").GetIsFailed(out _blockTextureSlot!, out error))
            return "can not create block texture slot: " + error;
        
        if (_grxApi.CreateTextureSlot<IDynamicTexture>("screen_tex").GetIsFailed(out _screenTextureSlot!, out error))
            return "can not create screen texture slot: " + error;
        
        // uniforms
        if (_grxApi.CreateUniform<Uniforms.Camera>("camera").GetIsFailed(out _cameraUniform!, out error))
            return "can not create camera uniform: " + error;
        
        if (_grxApi.CreateUniform<Uniforms.Model>("model").GetIsFailed(out _modelUniform!, out error))
            return "can not create model uniform: " + error;

        // shaders
        if (_grxApi.CreateShader<GraphicChunkApi.Vertex, GraphicChunkApi.Primitive, GraphicChunkApi.Varyings>(
                GraphicChunkApi.VertexShader, GraphicChunkApi.FragmentShader)
            .GetIsFailed(out _chunkShader!, out error))
        {
            return "can not create chunk shader: " + error;
        }
        
        if (_grxApi.CreateShader<GraphicScreenApi.Vertex, GraphicScreenApi.Primitive, GraphicScreenApi.Varyings>(
                GraphicScreenApi.VertexShader, GraphicScreenApi.FragmentShader)
            .GetIsFailed(out _screenShader!, out error))
        {
            return "can not create screen shader: " + error;
        }

        var screenMesh = new Mesh<GraphicScreenApi.Vertex, GraphicScreenApi.Primitive>(PrimitiveTopology.Triangle);
        screenMesh.Add([
            new() { Position = (-1f, -1f), Uv = (0, 0) },
            new() { Position = (1f, -1f), Uv = (1, 0) },
            new() { Position = (1f, 1f), Uv = (1, 1) },
            new() { Position = (-1f, 1f), Uv = (0, 1) }
        ], [0, 1, 2, 0, 2, 3], new GraphicScreenApi.Primitive());
        if (_grxApi.CreateRenderableMesh(screenMesh).GetIsFailed(out _screenMesh!, out error))
            return "can not create screen mesh: " + error;
        
        _projection = Matrix4.CreatePerspectiveFieldOfView(
            MathF.PI / 2, (float) _app.InputHandler.WindowSize.X / _app.InputHandler.WindowSize.Y, 0.1f, 1000f);
        
        return null;
    }

    private void ProceedCommands()
    {
        while (_chunkCommands.TryDequeue(out var command))
        {
            switch (command.Kind)
            {
                case ChunkCommandKind.AddChunk:
                    if (_chunks.ContainsKey(command.Id))
                        Logger.Error(this, $"Trying to replace chunk that already exists: {command.Id}");
                    else
                        _chunks[command.Id] = (_chunkModelPool.Take(), command.NewChunkTransform);
                    break;
                case ChunkCommandKind.RefitChunk:
                    if (_chunks.TryGetValue(command.Id, out var chunk)) 
                        chunk.Model.Refit(command.RefitModel);
                    else
                        Logger.Error(this, 
                            $"Can not refit with " + 
                            $"{(command.RefitModel is null ? "null" : "non-null")} model chunk that doesn't exist: {command.Id}");
                    break;
                case ChunkCommandKind.RemoveChunk:
                    if (_chunks.Remove(command.Id, out chunk)) 
                        _chunkModelPool.Recycle(chunk.Model);
                    else 
                        Logger.Error(this, $"Trying to remove chunk that doesn't exist: {command.Id}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    public void Update()
    {
        var inputHandler = _app.InputHandler;
        var view = _app.FlyingController.View;
        
        // command
        ProceedCommands();
        
        // window size change
        if (inputHandler.WindowSizeChanged)
        {
            _screenTexture.Size = inputHandler.WindowSize;
            _projection = Matrix4.CreatePerspectiveFieldOfView(
                MathF.PI / 2, (float) inputHandler.WindowSize.X / inputHandler.WindowSize.Y, 0.1f, 1000f);
        }
        
        // setting uniforms
        _cameraUniform.Set(new Uniforms.Camera{Projection = _projection, View = view});
        
        // render chunks
        _blockTextureSlot.BindToSample(_blockTexture);
        _screenTexture.BindAsRenderTarget();
        _grxApi.ClearCurrentRenderTarget();
        foreach (var (_, chunk) in _chunks)
        {
            _modelUniform.Set(new Uniforms.Model {Matrix = chunk.Transform});
            chunk.Model.Render(_chunkShader);
        }
        
        // render to screen
        _screenTextureSlot.BindToSample(_screenTexture);
        _grxApi.BindScreenRenderTarget();
        _grxApi.ClearCurrentRenderTarget();
        _screenShader.Render(_screenMesh);
        
        _grxApi.UpdateScreen();
    }
    
    #region Commands

    public void EnqueueChunkAddition(Guid id, Matrix4 transform)
    {
        _chunkCommands.Enqueue(new ChunkCommand
        {
            Kind = ChunkCommandKind.AddChunk,
            Id = id,
            NewChunkTransform = transform
        });   
    }
    
    public void EnqueueChunkRefit(Guid id, ChunkModel? model)
    {
        _chunkCommands.Enqueue(new ChunkCommand
        {
            Kind = ChunkCommandKind.RefitChunk,
            RefitModel = model,
            Id = id
        });
    }

    public void EnqueueChunkRemoval(Guid id)
    {
        _chunkCommands.Enqueue(new ChunkCommand
        {
            Kind = ChunkCommandKind.RemoveChunk,
            Id = id
        });
    }
    
    #endregion
    
    private struct ChunkCommand
    {
        public required ChunkCommandKind Kind;
        public required Guid Id;
        public ChunkModel? RefitModel;
        public Matrix4 NewChunkTransform;
    }
    
    private enum ChunkCommandKind
    {
        AddChunk,
        RefitChunk,
        RemoveChunk
    }
}