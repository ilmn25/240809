using System.Collections.Generic;
using Mirror;
using UnityEngine;

public struct HostToClientSnapshotChunkMessage : NetworkMessage
{
    public int index;
    public int totalChunks;
    public byte[] chunk;
    public bool isSave;
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
            conn.Send(new HostToClientSnapshotChunkMessage { index = i, totalChunks = totalChunks, chunk = chunks[i], isSave = true });
        }
    }

    public static void SendChunkBatchToAll(List<Vector3Int> chunkCoords)
    {
        if (!NetworkServer.active || chunkCoords == null || chunkCoords.Count == 0) return;
        var map = new Dictionary<Vector3Int, Chunk>();
        foreach (var coord in chunkCoords)
        {
            Chunk chunk = World.Inst[coord];
            if (chunk != null && chunk != Chunk.Zero)
                map[coord] = chunk;
        }
        if (map.Count == 0) return;
        byte[] bytes = Helper.SerializeObject(map);
        byte[][] split = Helper.SplitBytes(bytes, Server.MaxSnapshotChunkSize);
        int total = split.Length;
        foreach (var kv in NetworkServer.connections)
        {
            if (!kv.Value.isReady) continue;
            for (int i = 0; i < total; i++)
                kv.Value.Send(new HostToClientSnapshotChunkMessage { index = i, totalChunks = total, chunk = split[i], isSave = false });
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

        if (message.isSave)
        {
            Save save = Helper.DeserializeObject<Save>(allBytes);
            if (save == null) return;
            save.id = null;
            Save.Inst = save;
            Scene.SwitchSave(save);
        }
        else
        {
            // Save must be loaded before we can write chunks into the world
            if (Save.Inst == null) return;
            var map = Helper.DeserializeObject<Dictionary<Vector3Int, Chunk>>(allBytes);
            if (map == null) return;
            foreach (var kv in map)
            {
                World.Inst[kv.Key] = kv.Value;
                NavMap.SetChunk(kv.Key);
            }
        }
    }

    public static void Clear()
    {
        receivedSnapshotChunks.Clear();
        expectedSnapshotChunks = 0;
    }
}
