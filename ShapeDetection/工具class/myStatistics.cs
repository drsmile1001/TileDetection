using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class myStatistics
{
    /// <summary>
    /// 線性趨勢線or線性回歸方程式
    /// </summary>
    public class TrendLine
    {
        private double[] TrendLineEquationCoefficients;
        private double TrendLineAngle;


        /// <summary>建構趨勢線，需要避免輸入垂直線</summary>
        /// <param name="inputXData">自變數</param>
        /// <param name="inputYData">依變數</param>
        public TrendLine(double[] inputXData, double[] inputYData)
        {
            getTrendLineEquation(inputXData, inputYData);
            TrendLineAngle = Math.Atan(TrendLineEquationCoefficients[1]) * 180 / Math.PI;
        }

        /// <summary>取得趨勢線角度，X+為0，逆時鐘為正
        /// -180≤θ≤180
        /// </summary>
        public double Angle()
        {
            
            return TrendLineAngle;
        }
        /// <summary>取得趨勢線方程式的係數,index 0 為常數 1為1次方項</summary>
        /// <returns></returns>
        public double[] Coefficients()
        {
            return TrendLineEquationCoefficients;
        }

        private void getTrendLineEquation(double[] x, double[] y)
        {
            int m = 2;
            int info;
            alglib.barycentricinterpolant p;
            alglib.polynomialfitreport rep;
            alglib.polynomialfit(x, y, m, out info, out p, out rep);
            alglib.polynomialbar2pow(p, out TrendLineEquationCoefficients);
        }

        /// <summary>點到趨勢線的距離，正表示點在趨勢線的x+y+方向，負是x-y-方向</summary>
        public double PointToLineDis(double x,double y)
        { 
            /* 如果直線表示式：ax + by + c =0
             * 點是X,Y
             * 點到直線距離為： abs(aX +bY + c) / pow(pow(a,2) + pow(b,2) ,0.5)
             * 
             * 趨勢線方程：y=Ax+B => Ax-y+B=0
             * 故：
             * a = TrendLineEquationCoefficients[1]
             * b = -1
             * c = TrendLineEquationCoefficients[0]
             * 
             * 點到趨勢線方程為：
             * abs(TrendLineEquationCoefficients[1] * X - Y + TrendLineEquationCoefficients[0]) / pow(pow(TrendLineEquationCoefficients[1],2) + 1,0.5)
             * 
             */
            return -(TrendLineEquationCoefficients[1] * x - y + TrendLineEquationCoefficients[0]) 
                / Math.Pow(Math.Pow(TrendLineEquationCoefficients[1],2) + 1,0.5);
        }
    }


    /// <summary>取得標準差</summary>
    public static double StandardDeviation(this double[] num)
    {
        double avg = num.Average();
        double SumOfSqrs = 0.0;

        foreach (double d in num)
        {
            SumOfSqrs += Math.Pow(d - avg, 2);
        }
        return Math.Sqrt((SumOfSqrs / (num.Length - 1)));
    }

    
}


