using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;

[Serializable]
class IdentifyTileFileV2 : IdentifyTileFile
{
    public Point WorkAreaLeftTop;
    public Point WorkAreaRightDown;
    public Grid WorkAreaType;
}

