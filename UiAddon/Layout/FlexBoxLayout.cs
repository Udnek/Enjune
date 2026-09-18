using Enjune.Misc;
using JetBrains.Annotations;
using UiAddon.Element;

namespace UiAddon.Layout;

/// <summary>
/// https://css-tricks.com/snippets/css/a-guide-to-flexbox/
/// </summary>
public readonly record struct FlexBoxLayout() : ILayout
{
    #region Properties as container

    public Margin Padding { get; init; } = Margin.No;
    public float ChildGap { get; init; } = 0; // space between children
    public Direction ContentDirection { get; init; } = Direction.LeftToRight; // content alignment

    #endregion

    #region Properties as item in container
    
    public Alignment SelfAlign { get; init; } = Alignment.InOrder; // where element want to be aligned in parent
    public DimensionBehaviour MainMode { get; init; } = DimensionBehaviour.Grow;
    public DimensionBehaviour CrossMode { get; init; } = DimensionBehaviour.Grow;

    public float MainMax { get; init; } = float.PositiveInfinity;
    public float MainMin { get; init; } = 50;
    public float CrossMax { get; init; } = float.PositiveInfinity;

    public float CrossMin { get; init; } = 50;

    // size when it don't want to grow;
    public Vector2 DesiredSizeXy { get; init; } = new();
    
    #endregion

    [System.Diagnostics.Contracts.Pure]
    private Vector2 DesiredSize(Axis main) => (main == Axis.X) ? DesiredSizeXy : DesiredSizeXy.Yx;

    [System.Diagnostics.Contracts.Pure]
    private FlexBoxLayout WithDesiredSize(Axis main, Vector2 value) => this with {DesiredSizeXy = (main == Axis.X) ? value : value.Yx};

    public ILayout UpdateSelfLayoutAndChildrenRects(Rect selfRect, IList<IUiElement> allChildren)
    {
        var min = new Vector2(float.MaxValue);
        var max = new Vector2(float.MinValue);
        var newLayout = UpdateChildren(selfRect, allChildren);
        bool atLeastOneChild = false;
        foreach (var (child, _) in ValidChildren(allChildren))
        {
            if (!selfRect.FullyContains(child.Rect.Val)) 
                Logger.Warn(this, $"Child {child.Rect.Val} out of bounds of parent {selfRect}");
            min = Vector2.ComponentMin(min, child.Rect.Val.Min);
            max = Vector2.ComponentMax(max, child.Rect.Val.Max);
            atLeastOneChild = true;
        }

        if (atLeastOneChild)
        {
            min -= (Padding.Left, Padding.Bottom);
            max += (Padding.Right, Padding.Top);
        }

        var desiredSize = newLayout.DesiredSizeXy;
        var (xMode, yMode) = ToXy(MainMode, CrossMode);
        if (xMode == DimensionBehaviour.Fit) 
            desiredSize.X = max.X - min.X;
        else if (xMode == DimensionBehaviour.Grow)
            desiredSize.X = selfRect.Width;
        
        if (yMode == DimensionBehaviour.Fit) 
            desiredSize.Y = max.Y - min.Y;
        else if (xMode == DimensionBehaviour.Grow)
            desiredSize.Y = selfRect.Height;
        
        
        newLayout = newLayout with { DesiredSizeXy = desiredSize};
        
        return newLayout;
    }

    private FlexBoxLayout UpdateChildren(Rect selfRect, IList<IUiElement> allChildren)
    {
        var main = DirToAx(ContentDirection);
        Vector2 spaceTaken = new Vector2();

        // getting main/cross margin
        ((float Start, float End) mainPadding, (float Start, float End) crossPadding) = ToMainCross((Padding.Left, Padding.Right), (Padding.Bottom, Padding.Top));
        
        spaceTaken += (
            mainPadding.Start + mainPadding.End,
            crossPadding.Start + crossPadding.End);

        Dictionary<IUiElement, Vector2> childToFinalSize = [];
        foreach (var (child, childLayout) in ValidChildren(allChildren))
        {
            var size = childLayout.DesiredSize(main);
            // clamping size
            size = Vector2.Clamp(size, 
                (childLayout.MainMin, childLayout.CrossMin), 
                (childLayout.MainMax, childLayout.CrossMax));

            spaceTaken.X += size.X; // adding space when main
            spaceTaken.Y = Math.Max(size.Y, spaceTaken.Y); // maxing when cross
            
            childToFinalSize[child] = size; // setting clamped value
        }
        
        if (childToFinalSize.Count == 0)
            return this;
        
        // we are adding gaps between children along main axis
        spaceTaken += ((childToFinalSize.Count - 1) * ChildGap, 0);

        var crossPossibleSize = ToMainCross(selfRect.Size).Y - crossPadding.Start - crossPadding.End;
        
        
        #region Resizing

        Vector2 spaceAvailable = ToMainCross(selfRect.Size.X, selfRect.Size.Y) - spaceTaken;
        
        // main

        #region Growing
        
        while (spaceAvailable.X > 0)
        {
            int growers = 0;
            foreach (var (child, childLayout) in ValidChildren(allChildren))
            {
                if (childLayout.MainMode != DimensionBehaviour.Grow) continue;
                var canGrow = childLayout.MainMax - childToFinalSize[child].X;
                if (canGrow > 0)
                    growers += 1;
            }
            if (growers == 0) // we have no one to grow
                break;
            
            var growPerChild = spaceAvailable.X / growers;
            foreach (var (child, childLayout) in ValidChildren(allChildren))
            {
                if (childLayout.MainMode != DimensionBehaviour.Grow) continue;
                var size = childToFinalSize[child];
                var canGrow = childLayout.MainMax - size.X;
                if (canGrow <= 0) continue;
                var willGrow = Math.Min(canGrow, growPerChild);
                spaceAvailable.X -= willGrow;
                size.X += willGrow;
                childToFinalSize[child] = size;
            }
        }
        
        #endregion

        #region Shrinking

        while (spaceAvailable.X < 0)
        {
            int shrinkers = 0;
            foreach (var (child, childLayout) in ValidChildren(allChildren))
            {
                var canShrink = childToFinalSize[child].X - childLayout.MainMin;
                if (canShrink > 0)
                    shrinkers += 1;
            }
            if (shrinkers == 0) // we have no one to shrink
                break;
            
            var shrinkPerChild = -spaceAvailable.X / shrinkers;
            foreach (var (child, childLayout) in ValidChildren(allChildren))
            {
                var size = childToFinalSize[child];
                var canShrink = size.X - childLayout.MainMin;
                if (canShrink <= 0) continue;
                var willShrink = Math.Min(canShrink, shrinkPerChild);
                spaceAvailable.X += willShrink;
                size.X -= willShrink;
                childToFinalSize[child] = size;
            }
        }
        
        #endregion

        // cross
        
        foreach (var (child, childLayout) in ValidChildren(allChildren))
        {
            var size = childToFinalSize[child];
            if (size.Y < crossPossibleSize && childLayout.CrossMode == DimensionBehaviour.Grow)
            {
                size.Y = Math.Min(childLayout.CrossMax, crossPossibleSize);
            }
            else if (size.Y > crossPossibleSize)
            {
                size.Y = Math.Max(childLayout.CrossMin, crossPossibleSize);
            }
            childToFinalSize[child] = size;
        }
        
        #endregion

        #region Applying Rect
        
        var @this = this;
        [MustUseReturnValue]
        Vector2 ProceedRectChange(IUiElement child, Vector2 offset, float mainMul)
        {
            var size = childToFinalSize[child];
            size.X *= mainMul;
            var newRect = new Rect(@this.ToXy(offset), @this.ToXy(offset + size));
            child.SetRectAsParent(newRect);
            offset.X += size.X + @this.ChildGap * mainMul;
            return offset;
        }
        
        Vector2 startOffset = ToMainCross(selfRect.Min) + (mainPadding.Start, crossPadding.Start);
        Vector2 endOffset = (ToMainCross(selfRect.Max).X, ToMainCross(selfRect.Min).Y);
        endOffset += (-mainPadding.End, crossPadding.End);
        float mainMul = 1;
        if (ContentDirection is (Direction.RightToLeft or Direction.TopToDown)) // reverse
        {
            (startOffset, endOffset) = (endOffset, startOffset);
            mainMul *= -1;
        }


        //fistCorner = currentOffset;
        List<(IUiElement Child, FlexBoxLayout ChildLayout)> centering = []; // TODO
        List<(IUiElement Child, FlexBoxLayout ChildLayout)> ending = [];
        
        foreach (var (child, childLayout) in ValidChildren(allChildren))
        {
            if (childLayout.SelfAlign == Alignment.Center)
            {
                centering.Add((child, childLayout));
                continue;
            }
            if (childLayout.SelfAlign == Alignment.End)
            {
                ending.Add((child, childLayout));
                continue;
            }

            // auto 
            startOffset = ProceedRectChange(child, startOffset, mainMul);
        }
        
        // end
        foreach (var (child, _) in ending)
        {
            endOffset = ProceedRectChange(child, endOffset, -mainMul);
        }
        
        #endregion

        return this;
    }

    #region Utility

    [System.Diagnostics.Contracts.Pure]
    private Vector2 ToMainCross(Vector2 v) => DirToAx(ContentDirection) == Axis.X ? v : v.Yx;
    
    [System.Diagnostics.Contracts.Pure]
    private Vector2 ToXy(Vector2 v) => DirToAx(ContentDirection) == Axis.X ? v : v.Yx;
    
    [System.Diagnostics.Contracts.Pure]
    private (T Main, T Cross) ToMainCross<T>(T x, T y) => DirToAx(ContentDirection) == Axis.X ? (x, y) : (y, x);
    
    [System.Diagnostics.Contracts.Pure]
    private (T Main, T Cross) ToXy<T>(T x, T y) => DirToAx(ContentDirection) == Axis.X ? (x, y) : (y, x);

    private IEnumerable<(IUiElement Child, FlexBoxLayout ChildLayout)> ValidChildren(IList<IUiElement> raw)
    {
        foreach (var child in raw)
        {
            if (child.Layout.Val is FlexBoxLayout childLayout)
                yield return (child, childLayout);
            else
                Logger.Warn(this,
                    $"Child {child} has incompatible layout {child.Layout}, expected {typeof(FlexBoxLayout)}");
        }
    }

    private float NonNegative(string propertyName, float value)
    {
        if (value >= 0) return value;
        Logger.Warn(this, $"{propertyName} must be non-negative, got {value}");
        return 0;
    }

    private static Axis DirToAx(Direction dir)
    {
        return dir switch
        {
            Direction.LeftToRight => Axis.X,
            Direction.RightToLeft => Axis.X,
            _ => Axis.Y
        };
    }
    
    #endregion
    
    private enum Axis
    {
        X, Y
    }
    
    public enum Direction
    {
        LeftToRight, RightToLeft,
        TopToDown, DownToTop
    }
    
    public enum Alignment
    {
        InOrder, Center, End 
    }
    
    public enum DimensionBehaviour
    {
        Fixed, Grow, Fit
    }
}
