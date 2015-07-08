using System;
using System.Drawing;

[Serializable]
internal class IdentifyTileFileV2 : IdentifyTileFile
{
    public Point WorkAreaLeftTop;
    public Point WorkAreaRightDown;
    public Grid WorkAreaType;
}