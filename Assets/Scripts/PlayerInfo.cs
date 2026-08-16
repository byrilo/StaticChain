using Unity.Netcode;
using Unity.Collections;

public struct PlayerInfo : INetworkSerializable, System.IEquatable<PlayerInfo>
{
    public ulong ClientId;
    public FixedString32Bytes Nickname;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Nickname);
    }

    public bool Equals(PlayerInfo other) => ClientId == other.ClientId;
}
