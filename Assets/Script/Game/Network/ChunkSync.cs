using System.Collections.Generic;
using Mirror;
using UnityEngine;

public struct HostToClientSnapshotChunkMessage : NetworkMessage
{
    public int index;
    public int totalChunks;
    public byte[] chunk;
}

public static class ChunkSync
{
    private static readonly Dictionary<int, byte[]> receivedSnapshotChunks = new();
    private static int expectedSnapshotChunks;

    public static void RegisterHandlers()
    {
        NetworkClient.ReplaceHandler<HostToClientSnapshotChunkMessage>(OnHostToClientSnapshotChunkMessage, false);
    }

    public static void SendSaveChunks(NetworkConnectionToClient conn, Save save)
    {
        if (!NetworkServer.active || conn == null || save == null) return;

        byte[] bytes = Helper.SerializeObject(save);
        byte[][] chunks = Helper.SplitBytes(bytes, Server.MaxSnapshotChunkSize);
        int totalChunks = chunks.Length;

        for (int i = 0; i < totalChunks; i++)
        {
            conn.Send(new HostToClientSnapshotChunkMessage { index = i, totalChunks = totalChunks, chunk = chunks[i] });
        }
    }

    private static void OnHostToClientSnapshotChunkMessage(HostToClientSnapshotChunkMessage message)
    {
        if (expectedSnapshotChunks == 0) expectedSnapshotChunks = message.totalChunks;
        if (message.totalChunks != expectedSnapshotChunks)
        {
            expectedSnapshotChunks = message.totalChunks;
            receivedSnapshotChunks.Clear();
        }

        receivedSnapshotChunks[message.index] = message.chunk;
        if (receivedSnapshotChunks.Count < expectedSnapshotChunks) return;

        byte[] allBytes = Helper.CombineChunks(receivedSnapshotChunks, expectedSnapshotChunks);
        receivedSnapshotChunks.Clear();
        expectedSnapshotChunks = 0;
        Scene.SwitchSave(Helper.DeserializeObject<Save>(allBytes));
        Console.Print("Received data from host, loading in...");
    }
}
