using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Game;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using System.Diagnostics.CodeAnalysis;

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
            return screenPos.X >= 0 && screenPos.Y >= 0 && screenPos.X < gr.WindowPixelSize.X && screenPos.Y < gr.WindowPixelSize.Y - Constants.INF_MSGH;
        }

        public static void GetCenteredParams(Vector2 size, out UIBox2 bounds, float yOffset = 0f)
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

        public static void GetCenteredParams(float width, float height, out UIBox2 bounds, float yOffset = 0f)
        {
            var graphics = IoCManager.Resolve<IGraphics>();

            var elementSize = new Vector2(width, height);
            var gameBounds = new UIBox2(Vector2.Zero, graphics.WindowSize);

            var fitted = ApplyBoxFit(UiBoxFit.None, elementSize, gameBounds.Size);
            bounds = UiAlignment.Center.Inscribe(fitted.DestinationSize, gameBounds);

            bounds.Top += yOffset;
            bounds.Bottom += yOffset;
        }

        public static void DebugDraw(IDrawable elem)
        {
            Love.Graphics.SetColor(Love.Color.Red);
            GraphicsS.RectangleS(elem.UIScale, Love.DrawMode.Line, elem.X, elem.Y, elem.Width, elem.Height);
            Love.Graphics.SetColor(Love.Color.Blue);
            GraphicsS.LineS(elem.UIScale, elem.X, elem.Y, elem.X + elem.Width, elem.Y + elem.Height);
        }

        public static string DisplayWeight(int weight)
        {
            var integer = Math.Abs(weight / 1000);
            var fractional = Math.Abs(weight % 1000 / 100);
            return $"{integer}.{fractional}s";
        }

        public static Vector2 NotePosition(UIBox2 bounds, IDrawable text, float xOffset = 0f)
        {
            return new(bounds.Right - text.Width - 140 - xOffset,
                       bounds.Bottom - 65 - bounds.Height % 8);
        }

        public static bool TryGetDirectionFromKeyFunction(BoundKeyFunction func, [NotNullWhen(true)] out Direction? dir)
        {
            dir = null;

            if (func == EngineKeyFunctions.North)
            {
                dir = Direction.North;
            }
            else if (func == EngineKeyFunctions.South)
            {
                dir = Direction.South;
            }
            else if (func == EngineKeyFunctions.West)
            {
                dir = Direction.West;
            }
            else if (func == EngineKeyFunctions.East)
            {
                dir = Direction.East;
            }
            else if (func == EngineKeyFunctions.Northeast)
            {
                dir = Direction.NorthEast;
            }
            else if (func == EngineKeyFunctions.Northwest)
            {
                dir = Direction.NorthWest;
            }
            else if (func == EngineKeyFunctions.Southeast)
            {
                dir = Direction.SouthEast;
            }
            else if (func == EngineKeyFunctions.Southwest)
            {
                dir = Direction.SouthWest;
            }

            return dir != null;
        }
    }
}
