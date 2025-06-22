using System;
using System.Collections.Generic;



public class ServerMapData
{
    public List<ServerMarkerData> markerList;


    public ServerMapData()
    {
        markerList = new();
        Random ran = new Random();

        for (int i = 0; i < 7; i++)
        {
            float x = ran.Next(-3, 3);
            float y = ran.Next(0, 2);
            float z = ran.Next(-3, 3);
            ServerMarkerData testData = new ServerMarkerData(10101, x,y,z);
            markerList.Add(testData);
        }

    }


    public ServerMapData(ServerMapData origin)
    {
        markerList = new List<ServerMarkerData>();
        for (int i = 0; i < origin.markerList.Count; i++)
        {
            ServerMarkerData copyMarkerData = new ServerMarkerData(origin.markerList[i]);
            markerList.Add(copyMarkerData);
        }
    }

    public override string ToString()
    {
        if (markerList == null || markerList.Count == 0)
            return "No markers.";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var marker in markerList)
        {
            sb.AppendLine($"[Name: {marker.name}, SpawnType: {marker.markerSpawnType}, DropItemId: {marker.dropItemId}]");
        }

        return sb.ToString();
    }
}

