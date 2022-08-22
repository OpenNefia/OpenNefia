using OpenNefia.Core.IoC;
using OpenNefia.Core.Random;
namespace OpenNefia.Content.Resists
{
    public sealed partial class ResistsSystem
    {
        [Dependency] private readonly IRandom _rand = default!;

        /// <inheritdoc/>
        public IEnumerable<ElementPrototype> EnumerateResistableElements()
        {
            return _protos.EnumeratePrototypes<ElementPrototype>()
                .Where(elemProto => elemProto.CanResist);
        }

        public ElementPrototype PickRandomElement()
        {
            return _rand.Pick(EnumerateResistableElements().ToList());
        }

        public ElementPrototype PickRandomElementByRarity()
        {
            var element = PickRandomElement();
            for (var i = 0; i < element.Rarity; i++)
            {
                var otherElement = PickRandomElement();
                if (otherElement.Rarity < element.Rarity && _rand.OneIn(2))
                    element = otherElement;
            }
            return element;
        }
    }
}