namespace NosTale_ClientLess_NET.Packet.Encrypt_Decrypt;

public class PacketEncryptor : IPacketEncryptor
{
    public int EncryptionKey { get; set; }

    public byte[] EncryptWorldPacket(string value, bool session = false)
    {
        var output = new List<byte>();

        var mask = new string(value.Select(c =>
        {
            var b = (sbyte)c;
            if (c == '#' || c == '/' || c == '%')
                return '0';
            if ((b -= 0x20) == 0 || (b += unchecked((sbyte)0xF1)) < 0 || (b -= 0xB) < 0 ||
                b - unchecked((sbyte)0xC5) == 0)
                return '1';
            return '0';
        }).ToArray());

        int packetLength = value.Length;

        var sequenceCounter = 0;
        var currentPosition = 0;

        while (currentPosition <= packetLength)
        {
            int lastPosition = currentPosition;
            while (currentPosition < packetLength && mask[currentPosition] == '0')
                currentPosition++;

            int sequences;
            int length;

            if (currentPosition != 0)
            {
                length = currentPosition - lastPosition;
                sequences = length / 0x7E;
                for (var i = 0; i < length; i++, lastPosition++)
                {
                    if (i == sequenceCounter * 0x7E)
                    {
                        if (sequences == 0)
                        {
                            output.Add((byte)(length - i));
                        }
                        else
                        {
                            output.Add(0x7E);
                            sequences--;
                            sequenceCounter++;
                        }
                    }

                    output.Add((byte)((byte)value[lastPosition] ^ 0xFF));
                }
            }

            if (currentPosition >= packetLength)
                break;

            lastPosition = currentPosition;
            while (currentPosition < packetLength && mask[currentPosition] == '1')
                currentPosition++;

            if (currentPosition == 0) continue;

            length = currentPosition - lastPosition;
            sequences = length / 0x7E;
            for (var i = 0; i < length; i++, lastPosition++)
            {
                if (i == sequenceCounter * 0x7E)
                {
                    if (sequences == 0)
                    {
                        output.Add((byte)((length - i) | 0x80));
                    }
                    else
                    {
                        output.Add(0x7E | 0x80);
                        sequences--;
                        sequenceCounter++;
                    }
                }

                var currentByte = (byte)value[lastPosition];
                switch (currentByte)
                {
                    case 0x20:
                        currentByte = 1;
                        break;

                    case 0x2D:
                        currentByte = 2;
                        break;

                    case 0xFF:
                        currentByte = 0xE;
                        break;

                    default:
                        currentByte -= 0x2C;
                        break;
                }

                if (currentByte == 0x00) continue;

                if (i % 2 == 0)
                    output.Add((byte)(currentByte << 4));
                else
                    output[output.Count - 1] = (byte)(output.Last() | currentByte);
            }
        }

        output.Add(0xFF);

        var sessionNumber = (sbyte)((EncryptionKey >> 6) & 0xFF & 0x80000003);

        if (sessionNumber < 0)
            sessionNumber = (sbyte)(((sessionNumber - 1) | 0xFFFFFFFC) + 1);

        var sessionKey = (byte)(EncryptionKey & 0xFF);

        if (session)
            sessionNumber = -1;

        switch (sessionNumber)
        {
            case 0:
                for (var i = 0; i < output.Count; i++)
                    output[i] = (byte)(output[i] + sessionKey + 0x40);
                break;

            case 1:
                for (var i = 0; i < output.Count; i++)
                    output[i] = (byte)(output[i] - (sessionKey + 0x40));
                break;

            case 2:
                for (var i = 0; i < output.Count; i++)
                    output[i] = (byte)((output[i] ^ 0xC3) + sessionKey + 0x40);
                break;

            case 3:
                for (var i = 0; i < output.Count; i++)
                    output[i] = (byte)((output[i] ^ 0xC3) - (sessionKey + 0x40));
                break;

            default:
                for (var i = 0; i < output.Count; i++)
                    output[i] = (byte)(output[i] + 0x0F);
                break;
        }

        return output.ToArray();
    }

    public byte[] EncryptLoginPacket(string value)
    {
        var output = new byte[value.Length + 1];
        for (var i = 0; i < value.Length; i++)
        {
            output[i] = (byte)((value[i] ^ 0xC3) + 0xF);
        }
        output[^1] = 0xD8;
        return output;
    }
}