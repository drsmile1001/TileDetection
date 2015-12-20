using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;
using System.Drawing;

class myTool
{
    /// <summary>交換數值</summary>
    /// <typeparam name="T">數值類型</typeparam>
    public static void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp;
        temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

    /// <summary>交換McvBox2D的寬高</summary>
    public static void SwapMcvBox2DHeightWidth(ref MCvBox2D box)
    {
        float temp = box.size.Height;
        box.size.Height = box.size.Width;
        box.size.Width = temp;
    }


    /// <summary>將Y-X平面的角度轉為X-Y平面，其0度指向X軸，逆時針為正</summary>
    public static double CorrectingAngle_YXtoXY(double angle)
    {
        double tempAngle = 90.0 - angle;
        if (tempAngle < -45.0)
        {
            tempAngle += 180.0;
        }
        else if (tempAngle > 135.0)
        {
            tempAngle -= 180.0;
        }

        return tempAngle;
    }

    public static MCvBox2D AvgBox(MCvBox2D box1,MCvBox2D box2)
    {
        float Width = (box1.size.Width + box2.size.Width) / 2;
        float Height = (box1.size.Height + box2.size.Height) / 2;
        float X = (box1.center.X + box2.center.X) / 2;
        float Y = (box1.center.Y + box2.center.Y) / 2;
        float A = (box1.angle + box2.angle) / 2;

        return new MCvBox2D(new System.Drawing.PointF(X, Y),
            new System.Drawing.SizeF(Width, Height), A);
    }

    public static PointF AvgPointF(PointF p1, PointF p2)
    {
        float x = (p1.X + p2.X) / 2;
        float y = (p1.Y + p2.Y) / 2;
        return new PointF(x, y);
    }
}

