﻿using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Game;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.UI
{
    public static class UiUtils
    {
        public static string GetKeyName(Core.Input.Keyboard.Key key)
        {
            return Enum.GetName(typeof(Core.Input.Keyboard.Key), key)!.ToLowerInvariant();
        }

        public static bool IsPointInVisibleScreen(this IGraphics gr, Vector2i screenPos)
        {
            return screenPos.X >= 0 && screenPos.Y >= 0 && screenPos.X < gr.WindowSize.X && screenPos.Y < gr.WindowSize.Y - Constants.INF_MSGH;
        }

        public static void GetCenteredParams(Vector2i size, out UIBox2i bounds, int yOffset = 0)
            => GetCenteredParams(size.X, size.Y, out bounds, yOffset);

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

        public static void GetCenteredParams(int width, int height, out UIBox2i bounds, int yOffset = 0)
        {
            var graphics = IoCManager.Resolve<IGraphics>();
            var coords = IoCManager.Resolve<ICoords>();
            var field = IoCManager.Resolve<IFieldLayer>();
            var hud = IoCManager.Resolve<IHudLayer>();

            var elementSize = new Vector2i(width, height);
            var elementBounds = UIBox2i.FromDimensions(new Vector2i(0, yOffset), elementSize);

            var gameBounds = new UIBox2i(Vector2i.Zero, graphics.WindowSize);

            var fitted = ApplyBoxFit(UiBoxFit.None, elementSize, gameBounds.Size);
            bounds = UiAlignment.Center.Inscribe(fitted.DestinationSize, gameBounds);
            bounds.Top += yOffset;
            bounds.Bottom += yOffset;
        }

        public static void DebugDraw(IDrawable elem)
        {
            Love.Graphics.SetColor(Love.Color.Red);
            Love.Graphics.Rectangle(Love.DrawMode.Line, elem.X, elem.Y, elem.Width, elem.Height);
            Love.Graphics.SetColor(Love.Color.Blue);
            Love.Graphics.Line(elem.X, elem.Y, elem.X + elem.Width, elem.Y + elem.Height);
        }

        public static string DisplayWeight(int weight)
        {
            var integer = Math.Abs(weight / 1000);
            var fractional = Math.Abs(weight % 1000 / 100);
            return $"{integer}.{fractional}s";
        }

        public static Vector2i NotePosition(UIBox2i bounds, IDrawable text, int xOffset = 0)
        {
            return new(bounds.Right - text.Width - 140 - xOffset,
                       bounds.Bottom - 65 - bounds.Height % 8);
        }
    }
}
