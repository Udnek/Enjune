using System.Collections.Concurrent;
using System.Threading.Channels;
using Enjune.Graphic.Api;
using Enjune.Graphic.Key;
using Enjune.KitStart.Modeling;

namespace Minecraft2.Bridge;

public class GraphicEngine
{
    private Matrix4 _projection;
    private Matrix4 _view;

    private readonly App _app;
    private readonly IGraphicApi _grxApi;
    
    private readonly ConcurrentDictionary<Guid, ChunkModel> _chunks = [];
    private readonly ConcurrentQueue<Command> _commands = [];

    private IUniform<Uniforms.Camera> CameraUniform = null!;
    private IUniform<Uniforms.Model> ModelUniform = null!;
    
    public GraphicEngine(App app, IGraphicApi grxApi)
    {
        _app = app;
        _grxApi = grxApi;
    }

    public Error? Init()
    {
        if (_grxApi.CreateUniform<Uniforms.Camera>("camera").GetIsFailed(out CameraUniform!, out var error))
            return "can not create camera uniform: " + error;
        
        if (_grxApi.CreateUniform<Uniforms.Model>("model").GetIsFailed(out ModelUniform!, out error))
            return "can not create model uniform: " + error;
        
        if (_grxApi.CreateShader<GraphicChunkData.Vertex, GraphicChunkData.Primitive, >())

        _projection = Matrix4.CreatePerspectiveFieldOfView(
            MathF.PI / 2, (float) _app.InputHandler.WindowSize.X / _app.InputHandler.WindowSize.Y, 0.1f, 1000f);

        return null;
    }
    
    public void Update()
    {
        var inputHandler = _app.InputHandler;
        _view = _app.FlyingController.View;
        
        // command
        while (_commands.TryDequeue(out var command))
        {
            switch (command.Kind)
            {
                case CommandKind.AddChunk:
                    _chunks[command.Id] = command.AddModel;
                    break;
                case CommandKind.RefitChunk:
                    _chunks[command.Id].Refit();
                    break;
                case CommandKind.RemoveChunk:
                    _chunks.Remove(command.Id, out var model);
                    command.TakeModel!.TryWrite(model!);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        // window size change
        if (inputHandler.WindowSizeChanged)
        {
            _projection = Matrix4.CreatePerspectiveFieldOfView(
                MathF.PI / 2, (float) inputHandler.WindowSize.X / inputHandler.WindowSize.Y, 0.1f, 1000f);
            
            CameraUniform.Set(new Uniforms.Camera(){Projection = _projection, View = _view});
        }
        
        // render
        foreach (var (_, model) in _chunks)
        {
            model.Render();
        }
    }
    
    #region Commands

    public void EnqueueChunkAddition(Guid id, ChunkModel chunkModel)
    {
        _commands.Enqueue(new Command
        {
            Kind = CommandKind.AddChunk,
            Id = id,
            AddModel = chunkModel
        });   
    }
    
    public void EnqueueChunkRefit(Guid id)
    {
        _commands.Enqueue(new Command
        {
            Kind = CommandKind.RefitChunk,
            Id = id
        });
    }

    public void EnqueueChunkRemoval(Guid id, ChannelWriter<ChunkModel> takeChannel)
    {
        _commands.Enqueue(new Command
        {
            Kind = CommandKind.RemoveChunk,
            Id = id,
            TakeModel = takeChannel
        });
    }

    #endregion
    
    private struct Command
    {
        public required CommandKind Kind;
        public required Guid Id;
        public ChannelWriter<ChunkModel>? TakeModel;
        public ChunkModel AddModel;
    }
    
    private enum CommandKind
    {
        AddChunk,
        RefitChunk,
        RemoveChunk
    }
}