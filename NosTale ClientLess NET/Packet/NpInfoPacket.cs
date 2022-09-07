namespace NosTale_ClientLess_NET.Packet
{
    public class NpInfoPacket : IPacket
    {
        public byte Page { get; set; }

        public string PacketToString()
        {
            return $"npinfo {Page}";
        }
    }
}