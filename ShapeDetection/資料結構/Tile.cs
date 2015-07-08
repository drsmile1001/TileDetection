using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV.Structure;

/// <summary>用四個角定義的磁磚</summary>
class Tile
{
    /// <summary>左上點</summary>
    public PointF conerLT;
    /// <summary>左下點</summary>
    public PointF conerLD;
    /// <summary>右上點</summary>
    public PointF conerRT;
    /// <summary>右下點</summary>
    public PointF conerRD;

    public PointF center()
    {
        double[] Xs = new double[4] { conerLT.X, conerRT.X, conerLD.X, conerRD.X };
        double[] Ys = new double[4] { conerLT.Y, conerRT.Y, conerLD.Y, conerRD.Y };
        float x = (float)Xs.Average();
        float y = (float)Ys.Average();
        return new PointF(x, y);
    }

    private PointF[] topEdge;
    private PointF[] leftEdge;
    private PointF[] rightEdge;
    private PointF[] downEdge;

    public PointF[] TopEdge { get { return topEdge; } }
    public PointF[] LeftEdge { get { return leftEdge; } }
    public PointF[] RightEdge { get { return rightEdge; } }
    public PointF[] DownEdge { get { return downEdge; } }

    /// <summary>建構式，透過MCvBox2D。</summary>
    public Tile(MCvBox2D box)
    {
        float angle = box.angle;
        float Width = box.size.Width;
        float Height = box.size.Height;


        if (angle > 45.0f || angle < -45.0f)
        {
            if (angle > 45.0f)
            {
                angle -= 90.0f;
            }
            else
            {
                angle += 90.0f;
            }
            myTool.Swap<float>(ref Width, ref Height);
        }

        angle = (float)((double)angle / 180.0 * Math.PI);

        conerLT.X = (float)(
            (double)box.center.X
            - (double)Width * Math.Cos(angle) * 0.5
            + (double)Height * Math.Sin(angle) * 0.5);
        conerLT.Y = (float)(
            (double)box.center.Y
            - (double)Width * Math.Sin(angle) * 0.5
            - (double)Height * Math.Cos(angle) * 0.5);

        conerRT.X = (float)(
            (double)box.center.X
            + (double)Width * Math.Cos(angle) * 0.5
            + (double)Height * Math.Sin(angle) * 0.5);
        conerRT.Y = (float)(
            (double)box.center.Y
            + (double)Width * Math.Sin(angle) * 0.5
            - (double)Height * Math.Cos(angle) * 0.5);

        conerLD.X = (float)(
            (double)box.center.X
            - (double)Width * Math.Cos(angle) * 0.5
            - (double)Height * Math.Sin(angle) * 0.5);
        conerLD.Y = (float)(
            (double)box.center.Y
            - (double)Width * Math.Sin(angle) * 0.5
            + (double)Height * Math.Cos(angle) * 0.5);

        conerRD.X = (float)(
            (double)box.center.X
            + (double)Width * Math.Cos(angle) * 0.5
            - (double)Height * Math.Sin(angle) * 0.5);
        conerRD.Y = (float)(
            (double)box.center.Y
            + (double)Width * Math.Sin(angle) * 0.5
            + (double)Height * Math.Cos(angle) * 0.5);


        #region 建立邊
        setEdge(ref topEdge, conerLT, conerRT);
        setEdge(ref rightEdge, conerRT, conerRD);
        setEdge(ref downEdge, conerRD, conerLD);
        setEdge(ref leftEdge, conerLD, conerLT);

        #endregion
    }

    private void setEdge(ref PointF[] edge, PointF a, PointF b)
    {
        edge = new PointF[20];
        PointF A = a;
        PointF B = b;
        PointF deff = new PointF(B.X - A.X, B.Y - A.Y);
        for (int i = 0; i < 20; i++)
        {
            edge[i] = new PointF(A.X + ((float)i / 20) * deff.X, A.Y + ((float)i / 20) * deff.Y);
        }
    }

    public Tile()
    {

    }




}
