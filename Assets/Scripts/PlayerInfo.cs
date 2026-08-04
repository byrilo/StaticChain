using Unity.Netcode;

public struct PlayerInfo : INetworkSerializable, System.IEquatable<PlayerInfo>
{
    public ulong ClientId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
    }

    public bool Equals(PlayerInfo other) => ClientId == other.ClientId;
}