namespace NosTale_ClientLess_NET.Packet
{
    public class LbsPacket : IPacket
    {
        public byte Type { get; set; }

        public string PacketToString()
        {
            return $"lbs {Type}";
        }
    }
}