using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public struct ServerToClientEntityInfoMessage : NetworkMessage
{
    public string uid;
    public byte[] infoBytes;
}

public static class EntitySync
{
    public static readonly Dictionary<string, Info> InfoMap = new Dictionary<string, Info>();

    public static void RegisterHandlers()
    {
        NetworkClient.ReplaceHandler<ServerToClientEntityInfoMessage>(OnServerToClientEntityInfoMessage, false);
    }

    public static void SendInfoToAll(Info info)
    {
        if (!NetworkServer.active || info == null) return;
        byte[] bytes = Helper.SerializeObject(info);
        NetworkServer.SendToAll(new ServerToClientEntityInfoMessage { uid = info.uid, infoBytes = bytes });
    }

    public static void SendInfo(NetworkConnectionToClient conn, Info info)
    {
        if (!NetworkServer.active || conn == null || info == null) return;
        byte[] bytes = Helper.SerializeObject(info);
        conn.Send(new ServerToClientEntityInfoMessage { uid = info.uid, infoBytes = bytes });
    }

    private static void OnServerToClientEntityInfoMessage(ServerToClientEntityInfoMessage message)
    {
        if (message.infoBytes == null || string.IsNullOrEmpty(message.uid)) return;
        Info info = Helper.DeserializeObject<Info>(message.infoBytes);
        if (info == null) return;
        InfoMap[message.uid] = info;
    }
}
