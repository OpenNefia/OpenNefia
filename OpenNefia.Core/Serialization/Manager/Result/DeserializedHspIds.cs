using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Serialization.Manager.Result
{
    public class DeserializedHspIds<TValue> :
        DeserializedDictionary<HspIds<TValue>, string, TValue>
        where TValue : struct
    {
        public DeserializedHspIds(
            HspIds<TValue> value,
            IReadOnlyDictionary<DeserializationResult, DeserializationResult> mappings)
            : base(value, mappings, dict => DoCreate(dict, value.HspOrigin))
        {
        }

        private static HspIds<TValue> DoCreate(Dictionary<string, TValue> elements, string origin)
        {
            var hspIds = new HspIds<TValue>(origin);

            foreach (var (key, val) in elements)
            {
                hspIds.Add(key, val);
            }

            return hspIds;
        }

        public override object RawValue => Value;

        public override DeserializationResult PushInheritanceFrom(DeserializationResult source)
        {
            var newRes = base.PushInheritanceFrom(source).Cast<DeserializedDictionary<HspIds<TValue>, string, TValue>>();

            var sourceRes = source.Cast<DeserializedHspIds<TValue>>();
            var hspIds = new HspIds<TValue>(Value.HspOrigin);

            foreach (var (keyRes, valRes) in newRes.Value)
            {
                hspIds.Add(keyRes, valRes);
            }

            return new DeserializedHspIds<TValue>(hspIds, newRes.Mappings);
        }

        public override DeserializationResult Copy()
        {
            var newRes = base.Copy().Cast<DeserializedDictionary<HspIds<TValue>, string, TValue>>();

            var hspIds = new HspIds<TValue>(Value.HspOrigin);

            foreach (var (keyRes, valRes) in newRes.Value)
            {
                hspIds.Add(keyRes, valRes);
            }

            return new DeserializedHspIds<TValue>(hspIds, newRes.Mappings);
        }
    }
}
