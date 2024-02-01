using OpenNefia.Core.Maths;
using OpenNefia.VisualAI.Engine;

namespace OpenNefia.VisualAI.UserInterface
{
    /// <summary>
    /// UI representation of a tile in the Visual AI grid.
    /// </summary>
    public abstract record VisualAITile(VisualAIPlan Plan, Vector2i Position)
    {
        public sealed record Empty(VisualAIPlan Plan, Vector2i Position) : VisualAITile(Plan, Position);
        public sealed record Line(VisualAIPlan Plan, Vector2i Position, LineType Type, Vector2i EndPosition) : VisualAITile(Plan, Position);
        public sealed record Block(VisualAIPlan Plan, Vector2i Position, VisualAIBlock BlockValue) : VisualAITile(Plan, Position);

        public enum LineType
        {
            Right,
            Down
        }
    }
}