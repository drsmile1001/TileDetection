using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;

static class Rank
{
    /// <summary>從磁磚組(MCvBox2D[,])獲得該磁磚組的各行列之趨勢線</summary>
    /// <param name="theTilesType">瓷磚類型</param>
    /// <param name="Tiles">磁磚組</param>
    /// <returns>各行列之趨勢線</returns>
    public static TrendLine TilesLineAngle(TilesType theTilesType, MCvBox2D[,] Tiles)
    {
        switch (theTilesType)
        {
            case TilesType.Square:
                TrendLine SquareTrendLine
                    = new TrendLine(SquareGrids.COLUMN_COUNT, SquareGrids.ROW_COUNT);
                #region 水平趨勢線
                for (int rowNum = 0; rowNum < SquareGrids.ROW_COUNT; rowNum++)
                {
                    double[] Xdata = new double[SquareGrids.COLUMN_COUNT];
                    double[] Ydata = new double[SquareGrids.COLUMN_COUNT];
                    for (int columnNum = 0; columnNum < SquareGrids.COLUMN_COUNT; columnNum++)
                    {
                        Xdata[columnNum] = Tiles[columnNum, rowNum].center.X;
                        Ydata[columnNum] = Tiles[columnNum, rowNum].center.Y * -1;
                    }
                    myStatistics.TrendLine TrendLine = new myStatistics.TrendLine(Xdata, Ydata);
                    if (TrendLine.Angle() < -45)
                    {
                        SquareTrendLine.Horizontal[rowNum] = TrendLine.Angle() + 180f;
                    }
                    else
                    {
                        SquareTrendLine.Horizontal[rowNum] = TrendLine.Angle();
                    }

                }
                #endregion
                #region 垂直趨勢線
                for (int columnNum = 0; columnNum < SquareGrids.COLUMN_COUNT; columnNum++)
                {
                    double[] Xdata = new double[SquareGrids.ROW_COUNT];
                    double[] Ydata = new double[SquareGrids.ROW_COUNT];
                    for (int rowNum = 0; rowNum < SquareGrids.ROW_COUNT; rowNum++)
                    {
                        Xdata[rowNum] = Tiles[columnNum, rowNum].center.X;
                        Ydata[rowNum] = Tiles[columnNum, rowNum].center.Y * -1;
                    }
                    myStatistics.TrendLine TrendLine = new myStatistics.TrendLine(Ydata, Xdata);
                    if (TrendLine.Angle() < -45)
                    {
                        SquareTrendLine.Vertical[columnNum] = myTool.CorrectingAngle_YXtoXY(TrendLine.Angle()) + 180f;
                    }
                    else
                    {
                        SquareTrendLine.Vertical[columnNum] = myTool.CorrectingAngle_YXtoXY(TrendLine.Angle());
                    }

                }
                #endregion



                return SquareTrendLine;
            case TilesType.Rectangle:
                TrendLine RectangleTrendLine
                    = new TrendLine(RectangleGrids.columnCount, RectangleGrids.ROW_COUNT);
                #region 水平趨勢線
                for (int rowNum = 0; rowNum < RectangleGrids.ROW_COUNT; rowNum++)
                {
                    double[] Xdata = new double[RectangleGrids.columnCount];
                    double[] Ydata = new double[RectangleGrids.columnCount];
                    for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
                    {
                        Xdata[columnNum] = Tiles[columnNum, rowNum].center.X;
                        Ydata[columnNum] = Tiles[columnNum, rowNum].center.Y * -1;
                    }
                    myStatistics.TrendLine TrendLine = new myStatistics.TrendLine(Xdata, Ydata);
                    if (TrendLine.Angle() < -45)
                    {
                        RectangleTrendLine.Horizontal[rowNum] = TrendLine.Angle() + 180f;
                    }
                    else
                    {
                        RectangleTrendLine.Horizontal[rowNum] = TrendLine.Angle();
                    }

                }
                #endregion
                #region 垂直趨勢線
                for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
                {
                    double[] Xdata = new double[RectangleGrids.ROW_COUNT];
                    double[] Ydata = new double[RectangleGrids.ROW_COUNT];
                    for (int rowNum = 0; rowNum < RectangleGrids.ROW_COUNT; rowNum++)
                    {
                        Xdata[rowNum] = Tiles[columnNum, rowNum].center.X;
                        Ydata[rowNum] = Tiles[columnNum, rowNum].center.Y * -1;
                    }
                    myStatistics.TrendLine TrendLine = new myStatistics.TrendLine(Ydata, Xdata);
                    if (TrendLine.Angle() < -45)
                    {
                        RectangleTrendLine.Vertical[columnNum] = myTool.CorrectingAngle_YXtoXY(TrendLine.Angle()) + 180f;
                    }
                    else
                    {
                        RectangleTrendLine.Vertical[columnNum] = myTool.CorrectingAngle_YXtoXY(TrendLine.Angle());
                    }

                }
                #endregion

                return RectangleTrendLine;
            default:
                return new TrendLine(0, 0);
        }
    }


}

/// <summary>磁磚組各行列之趨勢線</summary>
class TrendLine
{
    /// <summary>水平方向延伸的趨勢線角度，向右為0，逆時為正</summary>
    public double[] Horizontal;
    /// <summary>垂直方向延伸的趨勢線角度，向右為0，逆時為正</summary>
    public double[] Vertical;

    public TrendLine(int column, int row)
    {
        Horizontal = new double[row];
        Vertical = new double[column];
    }
}


