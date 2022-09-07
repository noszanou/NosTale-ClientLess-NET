namespace NosTale_ClientLess_NET.Packet.Encrypt_Decrypt;

public class PacketDecryptor : IPacketDecryptor
{
    private static readonly char[] Keys = { ' ', '-', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'n' };

    public string DecryptLoginPacket(byte[] bytes, int size)
    {
        var output = "";
        for (var i = 0; i < size; i++)
        {
            output += Convert.ToChar(bytes[i] - 0xF);
        }
        return output;
    }

    public List<string> DecryptWorldPacket(byte[] bytes, int size)
    {
        var output = new List<string>();

        var currentPacket = "";
        var index = 0;

        while (index < size)
        {
            byte currentByte = bytes[index];
            index++;

            if (currentByte == 0xFF)
            {
                output.Add(currentPacket);
                currentPacket = "";
                continue;
            }

            var length = (byte)(currentByte & 0x7F);

            if ((currentByte & 0x80) != 0)
            {
                while (length != 0)
                {
                    if (index <= size)
                    {
                        currentByte = bytes[index];
                        index++;

                        var firstIndex = (byte)(((currentByte & 0xF0u) >> 4) - 1);
                        var first = (byte)(firstIndex != 255 ? firstIndex != 14 ? Keys[firstIndex] : '\u0000' : '?');
                        if (first != 0x6E)
                            currentPacket += Convert.ToChar(first);

                        if (length <= 1)
                            break;

                        var secondIndex = (byte)((currentByte & 0xF) - 1);
                        var second = (byte)(secondIndex != 255 ? secondIndex != 14 ? Keys[secondIndex] : '\u0000' : '?');
                        if (second != 0x6E)
                            currentPacket += Convert.ToChar(second);

                        length -= 2;
                    }
                    else
                    {
                        length--;
                    }
                }
            }
            else
            {
                while (length != 0)
                {
                    if (index < size)
                    {
                        currentPacket += Convert.ToChar(bytes[index] ^ 0xFF);
                        index++;
                    }
                    else if (index == size)
                    {
                        currentPacket += Convert.ToChar(0xFF);
                        index++;
                    }

                    length--;
                }
            }
        }
        return output;
    }
}