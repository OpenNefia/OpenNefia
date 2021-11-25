namespace Why.Core.Serialization
{
    public interface ISelfSerialize
    {
        void Deserialize(string value);

        string Serialize();
    }
}
