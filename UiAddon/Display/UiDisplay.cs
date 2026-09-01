using System.Collections.Specialized;
using Enjune.Attribute;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using UiAddon.Element;

namespace UiAddon.Display;

[LogParams(logCallingMethod: true)]
public abstract class UiDisplay
{
    #region Public

    public IList<Model.Entry> Meshes => _meshes;
    public UiElement? Parent = null;

    #endregion

    private readonly ObservableList<Model.Entry> _meshes = [];
    private readonly float _zOffset;
    protected float Z => Parent?.GlobalZ ?? 0 + _zOffset;

    protected UiDisplay(float zOffset)
    {
        _zOffset = zOffset;
        _meshes.AfterElementAdded += _ => NotifyParentAboutMeshChanges();
        _meshes.AfterElementRemoved += _ => NotifyParentAboutMeshChanges();
    }
    
    public abstract void UpdateMeshes(Rect oldRect, Rect newRect);
    
    private void NotifyParentAboutMeshChanges()
    {
        if (Parent is null)
        {
            Logger.Warn(this, $"can not notify cause {nameof(Parent)} is null");
            return;
        }
        Parent.OnMeshesChanged();
    }
}