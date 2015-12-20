using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

/// <summary>用四個角定義的磁磚</summary>
public class Tile
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

    private Dictionary<EdgeDirection, PointF[]> _Edge = new Dictionary<EdgeDirection, PointF[]>();

    public IReadOnlyDictionary<EdgeDirection, PointF[]> Edge { get { return _Edge; } }

#if false
    public PointF[] TopEdge { get; private set; }

    public PointF[] LeftEdge { get; private set; }

    public PointF[] RightEdge { get; private set; }

    public PointF[] DownEdge { get; private set; }

    public PointF[] AllEdge { get { return TopEdge.Union(LeftEdge).Union(RightEdge).Union(DownEdge).ToArray(); } } 
#endif

    public PointF[] AllEdge
    {
        get
        {
            return
                Edge[EdgeDirection.Top]
                .Union(Edge[EdgeDirection.Right])
                .Union(Edge[EdgeDirection.Down])
                .Union(Edge[EdgeDirection.Left]).ToArray();
        }
    }

    private Dictionary<EdgeDirection, PointF[]> _HalfEdge = new Dictionary<EdgeDirection, PointF[]>();

    public IReadOnlyDictionary<EdgeDirection, PointF[]> HalfEdge { get { return _HalfEdge; } }

#if false
    public PointF[] TopHalfEdge { get; private set; }

    public PointF[] LeftHalfEdge { get; private set; }

    public PointF[] RightHalfEdge { get; private set; }

    public PointF[] DownHalfEdge { get; private set; } 
#endif

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
        _Edge.Add(EdgeDirection.Top, GenLinePointSet(conerLT, conerRT));
        _Edge.Add(EdgeDirection.Right, GenLinePointSet(conerRT, conerRD));
        _Edge.Add(EdgeDirection.Down, GenLinePointSet(conerRD, conerLD));
        _Edge.Add(EdgeDirection.Left, GenLinePointSet(conerLD, conerLT));
#if false
        TopEdge = GenLinePointSet(conerLT, conerRT);
        RightEdge = GenLinePointSet(conerRT, conerRD);
        DownEdge = GenLinePointSet(conerRD, conerLD);
        LeftEdge = GenLinePointSet(conerLD, conerLT); 
#endif
        _HalfEdge.Add(EdgeDirection.Top, GenLinePointSet(conerLT, conerRT, 40, 10));
        _HalfEdge.Add(EdgeDirection.Right, GenLinePointSet(conerLT, conerRT, 40, 10));
        _HalfEdge.Add(EdgeDirection.Down, GenLinePointSet(conerRD, conerLD, 40, 10));
        _HalfEdge.Add(EdgeDirection.Left, GenLinePointSet(conerLD, conerLT, 40, 10));
#if false
        TopHalfEdge = GenLinePointSet(conerLT, conerRT, 40, 10);
        RightHalfEdge = GenLinePointSet(conerRT, conerRD, 40, 10);
        DownHalfEdge = GenLinePointSet(conerRD, conerLD, 40, 10);
        LeftHalfEdge = GenLinePointSet(conerLD, conerLT, 40, 10); 
#endif
        #endregion 建立邊
    }

    private PointF[] GenLinePointSet(PointF a, PointF b,int pointCount = 20,int deadPointCount = 0)
    {
        if (2*deadPointCount >= pointCount)
        {
            throw new System.ArgumentException("死區點數量過多");
        }
        if (pointCount <= 0)
        {
            throw new System.ArgumentException("取樣點不足");
        }
        PointF[] edge = new PointF[pointCount - 2*deadPointCount];
        PointF diff = new PointF(b.X - a.X, b.Y - a.Y);
        for (int i = deadPointCount; i < pointCount - deadPointCount; i++)
        {
            edge[i-deadPointCount] = new PointF(a.X + ((float)i / pointCount) * diff.X, a.Y + ((float)i / pointCount) * diff.Y);
        }
        return edge;
    }

    public Tile()
    {
    }
}


public enum EdgeDirection
{
    Top, Down, Right, Left
}