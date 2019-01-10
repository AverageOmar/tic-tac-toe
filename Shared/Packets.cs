using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public enum PacketType
    {
        EMPTY,
        NICKNAME,
        CHATMESSAGE,
        TURN,
        PLAYER,
        RESET,
    }

    [Serializable]
    public class Packet
    {
        public PacketType type = PacketType.EMPTY;
    }

    [Serializable]
    public class ResetPacket : Packet
    {
        public ResetPacket()
        {
            this.type = PacketType.RESET;
        }
    }

    [Serializable]
    public class PlayerPacket : Packet
    {
        public int player = 0;

        public PlayerPacket(int p)
        {
            this.type = PacketType.PLAYER;
            this.player = p;
        }
    }

    [Serializable]
    public class TurnPacket : Packet
    {
        public int turn = 0;
        public string button = "";
        public string letter = "";

        public TurnPacket(int t, string b, string l)
        {
            this.type = PacketType.TURN;
            this.turn = t;
            this.button = b;
            this.letter = l;
        }
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
