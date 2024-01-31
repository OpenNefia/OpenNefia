using OpenNefia.Core.Maths;

namespace OpenNefia.VisualAI.Engine
{
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