using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;

[Serializable]
class SampleFile
{
//    public MCvBox2D[,] Tiles;
    public myBox[,] boxs;
    public TilesType TilesType;
    
    public SampleFile(MCvBox2D[,] theTiles,TilesType theTilesType)
    {
        TilesType = theTilesType;
        int Length0 = theTiles.GetLength(0);
        int Length1 = theTiles.GetLength(1);
        boxs = new myBox[Length0, Length1];
            
        for (int i = 0; i < Length0; i++)
        {
            for (int j = 0; j < Length1; j++)
            {
                boxs[i, j] = new myBox(theTiles[i, j]);
            }
        }
    }
}

