namespace NosTale_ClientLess_NET.Packet;

public class SelectPacket : IPacket
{
    public byte Slot { get; set; }

    public string PacketToString()
    {
        return $"select {Slot}";
    }
}