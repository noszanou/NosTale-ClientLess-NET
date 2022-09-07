namespace NosTale_ClientLess_NET.Packet.Bullshit
{
    public static class Lmao
    {
        public static string GenerateCClose(byte type)
        {
            return new CClosePacket { Type = type }.PacketToString();
        }

        public static string GenerateSelect(byte slot)
        {
            return new SelectPacket { Slot = slot }.PacketToString();
        }

        public static string GenerateLbs(byte type)
        {
            return new LbsPacket { Type = type }.PacketToString();
        }

        public static string GenerateNpInfo(byte page)
        {
            return new NpInfoPacket { Page = page }.PacketToString();
        }

        public static string GenerateBpClose()
        {
            return "bp_close";
        }

        public static string GenerateGameStart()
        {
            return "game_start";
        }

        public static string GenerateFStashEnd()
        {
            return "f_stash_end";
        }
    }
}