using MinesServer.Networking.Server.Packets;
using MinesServer.Networking.Server.Packets.World;
using System.Collections.Generic;

namespace Fodinae.Assets.Scripts.Networking
{
    public class RobotPositionPacket : IHBPacket
    {
        public ushort BotId { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public byte Rotation { get; set; } // 0: Right, 1: Up, 2: Left, 3: Down

        public RobotPositionPacket(ushort botId, ushort x, ushort y, byte rotation)
        {
            BotId = botId;
            X = x;
            Y = y;
            Rotation = rotation;
        }
    }

    public class RobotMetadataPacket : IServerPacketPayload
    {
        public ushort BotId { get; set; }
        public string Nickname { get; set; }
        public string SkinPath { get; set; }

        public RobotMetadataPacket(ushort botId, string nickname, string skinPath)
        {
            BotId = botId;
            Nickname = nickname;
            SkinPath = skinPath;
        }
    }
}
