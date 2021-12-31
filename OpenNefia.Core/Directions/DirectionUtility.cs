using System.Collections.Generic;
using OpenNefia.Core.Directions;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Random;

namespace OpenNefia.Core.Directions
{
    public static class DirectionUtility
    {
        /// <summary>
        ///     Gets random directions until none are left
        /// </summary>
        /// <returns>An enumerable of the directions.</returns>
        public static IEnumerable<Direction> RandomDirections()
        {
            var directions = new[]
            {
                Direction.East,
                Direction.SouthEast,
                Direction.South,
                Direction.SouthWest,
                Direction.West,
                Direction.NorthWest,
                Direction.North,
                Direction.NorthEast,
            };

            var robustRandom = IoCManager.Resolve<IRandom>();
            var n = directions.Length;

            while (n > 1)
            {
                n--;
                var k = robustRandom.Next(n + 1);
                var value = directions[k];
                directions[k] = directions[n];
                directions[n] = value;
            }

            foreach (var direction in directions)
            {
                yield return direction;
            }
        }

        public static EntityCoordinates Offset(this EntityCoordinates coordinates, Direction direction)
        {
            return coordinates.Offset(direction.ToIntVec());
        }

        public static MapCoordinates Offset(this MapCoordinates coordinates, Direction direction)
        {
            return coordinates.Offset(direction.ToIntVec());
        }
    }
}
