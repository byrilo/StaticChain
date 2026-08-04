using Unity.Netcode;
using Unity.Collections;

public struct PhraseEntry : INetworkSerializable, System.IEquatable<PhraseEntry>
{
    public ulong ClientId;
    public FixedString128Bytes Phrase;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Phrase);
    }

    public bool Equals(PhraseEntry other) => ClientId == other.ClientId;
}