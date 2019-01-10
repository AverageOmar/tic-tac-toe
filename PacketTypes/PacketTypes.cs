using System;
using System.Collections.Generic;
using System.Text;

namespace PacketTypes
{
    public enum PacketType
    {
        EMPTY,
        NICKNAME,
        CHATMESSAGE,
    }

    [Serializable]
    public class Packet
    {
        public PacketType type = PacketType.EMPTY;
    }

    [Serializable]
    public class NickNamePacket : Packet
    {
        public string nickName = String.Empty;

        public NickNamePacket(string nickName)
        {
            this.type = PacketType.NICKNAME;
            this.nickName = nickName;
        }
    }

    [Serializable]
    public class ChatMessagePacket : Packet
    {
        public string message = String.Empty;

        public ChatMessagePacket(string message)
        {
            this.type = PacketType.CHATMESSAGE;
            this.message = message;
        }
    }
}
