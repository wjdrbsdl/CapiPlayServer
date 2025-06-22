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

public class GameMarkerData
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


}