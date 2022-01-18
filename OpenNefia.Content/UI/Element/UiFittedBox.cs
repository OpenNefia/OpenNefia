using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Element
{
    public enum UiBoxFit
    {
        /// <summary>
        /// Center the element, but perform no scaling.
        /// </summary>
        None,

        /// <summary>
        /// Scale the element to be contained within the viewport.
        /// </summary>
        Contain
    }

    /// <summary>
    /// <para>
    /// A point within a rectangle.
    /// </para>
    /// <para>
    /// (0.0, 0.0) is center of the rectangle. (-1.0, -1.0) is the top left.
    /// (1.0, 1.0) is the bottom right.
    /// </para>
    /// </summary>
    public struct UiAlignment
    {
        /// <summary>
        /// Distance fraction in the vertical direction.
        /// </summary>
        public float X;

        /// <summary>
        /// Distance fraction in the horizontal direction.
        /// </summary>
        public float Y;

        /// <summary>
        /// The center point, both horizontally and vertically.
        /// </summary>
        public static readonly UiAlignment Center = new(0f, 0f);

        public UiAlignment(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Returns a box of the given size aligned within the given box with this
        /// alignment.
        /// </summary>
        public UIBox2 Inscribe(Vector2 size, UIBox2 box)
        {
            float halfWidth = (box.Width - size.X) / 2.0F;
            float halfHeight = (box.Height - size.Y) / 2.0F;
            return UIBox2.FromDimensions(new Vector2(box.Left + halfWidth + X * halfWidth,
                                                     box.Top + halfHeight + Y * halfHeight),
                                         size);
        }
    }

    /// <param name="SourceSize">Subpart of the child element to show.</param>
    /// <param name="DestinationSize">Subpart of the parent element to show the child element in.</param>
    public record struct FittedSizes(Vector2 SourceSize, Vector2 DestinationSize);

    /// <summary>
    /// Container element for fitting a child UI element using size constraints.
    /// </summary>
    /// <remarks>
    /// Inspired by Flutter's BoxFit/FittedBox.
    /// </remarks>
    public class UiFittedBox : UiElement
    {
        private IUiElement? _child;
        public IUiElement? Child 
        {
            get => _child; 
            set
            {
                _child = value;
                RelayoutChild();
            }
        }

        public UiBoxFit BoxFit { get; set; }

        public UiAlignment Alignment { get; set; }

        public UiFittedBox(IUiElement? child = null)
        {
            Child = child;
        }

        /// <param name="inputSize">Preferred size of the child element to be fitted.</param>
        /// <param name="outputSize">Size of the parent box to fit the child in.</param>
        /// <returns>The portion of the child element to display, and the portion in the
        /// parent to display the child in.</returns>
        public static FittedSizes ApplyBoxFit(UiBoxFit fit, Vector2 inputSize, Vector2 outputSize)
        {
            if (inputSize.X <= 0 || inputSize.Y <= 0 || outputSize.X <= 0 || outputSize.Y <= 0)
                return new FittedSizes(Vector2.Zero, Vector2.Zero);

            Vector2 sourceSize;
            Vector2 destinationSize;

            switch (fit)
            {
                case UiBoxFit.Contain:
                    sourceSize = inputSize;
                    if (outputSize.X / outputSize.Y > sourceSize.X / sourceSize.Y)
                    {
                        destinationSize = new(sourceSize.X * outputSize.Y / sourceSize.Y, outputSize.Y);
                    }
                    else
                    {
                        destinationSize = new(outputSize.X, sourceSize.Y * outputSize.X / sourceSize.X);
                    }
                    break;
                case UiBoxFit.None:
                default:
                    sourceSize = new(MathF.Min(inputSize.X, outputSize.X), MathF.Min(inputSize.Y, outputSize.Y));
                    destinationSize = sourceSize;
                    break;
            }

            return new FittedSizes(sourceSize, destinationSize);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);

            RelayoutChild();
        }

        private void RelayoutChild()
        {
            if (Child == null)
                return;

            Child.GetPreferredSize(out var preferredChildSize);
            var fitted = ApplyBoxFit(BoxFit, preferredChildSize, Size);
            var aligned = Alignment.Inscribe(fitted.DestinationSize, Rect);

            Child.SetPosition(aligned.Left, aligned.Top);
            Child.SetSize(aligned.Width, aligned.Height);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Child?.Update(dt);
        }

        public override void Draw()
        {
            base.Draw();
            Child?.Draw();
        }

        public override void Dispose()
        {
            Child?.Dispose();
        }
    }
}
