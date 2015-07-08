using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;

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
}

