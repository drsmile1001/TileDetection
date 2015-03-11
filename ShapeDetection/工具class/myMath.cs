using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

#region 計算用class
static class myMath
{
    public static double GetDistance(double A_x, double A_y, double B_x, double B_y)
    {
        double x = System.Math.Abs(B_x - A_x);
        double y = System.Math.Abs(B_y - A_y);

        return Math.Sqrt(x * x + y * y);
    }
    public static double GetDistance(Point A, Point B)
    {
        double x = (double)System.Math.Abs(A.X - B.X);
        double y = (double)System.Math.Abs(A.Y - B.Y);
        return Math.Sqrt(x * x + y * y);
    }
    public static double GetDistance(PointF A, PointF B)
    {
        double x = (double)System.Math.Abs(A.X - B.X);
        double y = (double)System.Math.Abs(A.Y - B.Y);
        return Math.Sqrt(x * x + y * y);
    }
    public static double GetDistance(PointF A, Point B)
    {
        double x = (double)System.Math.Abs(A.X - B.X);
        double y = (double)System.Math.Abs(A.Y - B.Y);
        return Math.Sqrt(x * x + y * y);
    }
    public static double GetDistance(Point A, PointF B)
    {
        double x = (double)System.Math.Abs(A.X - B.X);
        double y = (double)System.Math.Abs(A.Y - B.Y);
        return Math.Sqrt(x * x + y * y);
    }

    public static double GetDis<T>(T nub)
    {
        double dis;
        try
        {
            dis = double.Parse(nub.ToString());
        }
        catch (Exception)
        {

            throw;
        }
        if (dis<0)
        {
            dis = 0;
        }
        return dis;
    }


    public static float getAngle(Point A, Point B)
    {
        float xDiff = (float)(B.X - A.X);
        float yDiff = (float)(B.Y - A.Y);
        return (float)Math.Atan2(yDiff, xDiff) * (float)(180 / Math.PI);

    }

    public static float getAngle(PointF A, PointF B)
    {
        float xDiff = B.X - A.X;
        float yDiff = B.Y - A.Y;
        return (float)Math.Atan2(yDiff, xDiff) * (float)(180 / Math.PI);

    }

    /// <summary>產生包含上下的亂數double</summary>
    public static double randNumber(double minValue, double maxValue)
    {
        Random rnd = new Random(Guid.NewGuid().GetHashCode());
        double rate = (double)rnd.Next(0, 1000000001) / (double)1000000000;
        return minValue + rate * (maxValue - minValue);
    }

    #region 角度轉換
    /// <summary>弧度轉度數</summary>
    public static double Rad2Deg(double RadAngle)
    {
        return RadAngle / Math.PI * 180;
    }
    /// <summary>度數轉弧度</summary>
    public static double Deg2Rad(double DegAngle)
    {
        return DegAngle / 180 * Math.PI;
    } 
    #endregion

    /// <summary>數學角度轉繪圖角度</summary>
    /// 數學角度：0度指向X+，逆時為正
    /// 繪圖角度：0度指向Y+，順時為正
    public static double MathDeg2DrawingDeg(double MathDeg)
    {
        return -MathDeg + 90;
    }

    /// <summary>繪圖角度轉數學角度</summary>
    /// 數學角度：0度指向X+，逆時為正
    /// 繪圖角度：0度指向Y+，順時為正
    public static double DrawingDeg2MathDeg(double DrawingDeg)
    {
        return -DrawingDeg + 90;
    }

}
#endregion