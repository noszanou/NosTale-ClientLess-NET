namespace NosTale_ClientLess_NET.Packet;

public class CClosePacket : IPacket
{
    public byte Type { get; set; }

    public string PacketToString()
    {
        return $"c_close {Type}";
    }
}