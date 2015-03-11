using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV.Structure;

#region 暫時用來取代MCvBox2D的class
[Serializable]
public class myBox
{
    PointF center;
    SizeF size;
    float angle;
    public myBox(MCvBox2D inputMcvBox2D)
    {
        center = inputMcvBox2D.center;
        size = inputMcvBox2D.size;
        angle = inputMcvBox2D.angle;
    }
    public MCvBox2D getMcvBox2D()
    {
        return new MCvBox2D(center, size, angle);
    }
}
#endregion
