using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum MarkerType
{
    DropItem, Clue, SelfClue, Decoration, Trap
}

public enum MarkerSpawnType
{
    Base, OnClose
}

public class ServerMarkerData
{
    public int markId;
    public int dropItemId;
    public MarkerSpawnType markerSpawnType = MarkerSpawnType.Base;
    public MarkerType markerType = MarkerType.DropItem;
    public string name;
    public int spawnStep;
    public int deleteStep;
    public float positionX;
    public float positionY;
    public float positionZ;
    public float rotationX;
    public float rotationY;
    public float rotationZ;

    public ServerMarkerData()
    {

    }

    public ServerMarkerData(int markId, float posX, float posY, float posZ)
    {
        this.markId = markId;
        this.positionX = posX;
        this.positionY = posY;
        this.positionZ = posZ;

        // 나머지는 기본값으로 설정
        this.dropItemId = 0;
        this.markerSpawnType = MarkerSpawnType.Base;
        this.markerType = MarkerType.DropItem;
        this.name = string.Empty;
        this.spawnStep = 0;
        this.deleteStep = 0;
        this.rotationX = 0f;
        this.rotationY = 0f;
        this.rotationZ = 0f;
    }

    public ServerMarkerData(ServerMarkerData origin)
    {
        this.markId = origin.markId;
        this.dropItemId = origin.dropItemId;
        this.markerSpawnType = origin.markerSpawnType;
        this.markerType = origin.markerType;
        this.name = origin.name;
        this.spawnStep = origin.spawnStep;
        this.deleteStep = origin.deleteStep;
        this.positionX = origin.positionX;
        this.positionY = origin.positionY;
        this.positionZ = origin.positionZ;
        this.rotationX = origin.rotationX;
        this.rotationY = origin.rotationY;
        this.rotationZ = origin.rotationZ;
    }
}