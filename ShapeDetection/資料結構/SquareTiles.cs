﻿using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using 磁磚辨識評分;
using SmileLib;

internal class SquareTiles
{
    /// <summary>錯誤訊息</summary>
    public int msg;

    public SquareGrids myGrid;

    /// <summary>代表整組磁磚</summary>
    public MCvBox2D[,] Tiles;

    public ErrMark CurrentErrMark;

    public TilesReport Report { get; private set; }

    /// <summary>正方磁磚墻建構式</summary>
    public SquareTiles(Point theGridRD, Point theGridLT, List<MCvBox2D> BaseMcvBox2DList, string fileName, RankArea rankArea)
    {
        myGrid = new SquareGrids(theGridRD, theGridLT);
        Tiles = TileArrayToTile2DArray(theGridRD, theGridLT, BaseMcvBox2DList);
        CurrentErrMark = new ErrMark();
        //進行評分
        Report = RankSquareTile(fileName, Tiles, myGrid, rankArea,ref CurrentErrMark);
    }

    public static TilesReport RankSquareTile(string fileName, MCvBox2D[,] Tiles, SquareGrids myGrid, RankArea rankArea,ref ErrMark outPutErrMark)
    {
        #region 評分
        
        TilesReport myReport = new TilesReport(fileName);
        int rowStart;
        int rowEnd;
        switch (rankArea)
        {
            case RankArea.Top:
                rowStart = 0;
                rowEnd = SquareGrids.ROW_COUNT_HALF;
                break;
            case RankArea.Down:
                rowStart = SquareGrids.ROW_COUNT_HALF;
                rowEnd = SquareGrids.ROW_COUNT;
                break;
            default:
                throw SmileLib.EnumTool.OutOfEnum<RankArea>();
        }


        #region 評分_絕對位置
#if false

        myReport.newLine("絕對位置：");
        double absoluteError = 0.0;
        for (int row = 0; row < rowEnd; row++)
        {
            string rowReport = "";
            for (int column = 0; column < SquareGrids.columnCount; column++)
            {
                Tile tempTile = new Tile(Tiles[column, row]);
                double tempError = 0.0;
                tempError += myGrid.mmFormPixel(myMath.GetDistance(tempTile.conerLT, myGrid.TileModelLT(column, row)));
                tempError += myGrid.mmFormPixel(myMath.GetDistance(tempTile.conerLD, myGrid.TileModelLD(column, row)));
                tempError += myGrid.mmFormPixel(myMath.GetDistance(tempTile.conerRT, myGrid.TileModelRT(column, row)));
                tempError += myGrid.mmFormPixel(myMath.GetDistance(tempTile.conerRD, myGrid.TileModelRD(column, row)));

                rowReport += "\t" + tempError;
                absoluteError += tempError;
            }
            myReport.newLine(rowReport);
        }
        myReport.newLine("絕對位置總誤差：\t" + absoluteError);

#endif
        #endregion 評分_絕對位置

        #region 評分_溝縫

        myReport.newLine("");
        myReport.newLine("溝縫間隔：");

        List<double> gap = new List<double>();
        Dictionary<LineSegment2DF, double> gapAndDis = new Dictionary<LineSegment2DF, double>();
        #region 列與列間溝縫

        myReport.newLine("列與列間溝縫：");
        List<double> allRowError = new List<double>();
        for (int rowNum = rowStart; rowNum < rowEnd - 1; rowNum++)
        {
            string rowReport = "第" + rowNum.ToString("D2") + "條：";
            double[] rowError = new double[SquareGrids.COLUMN_COUNT * 2];
            for (int columnNum = 0; columnNum < SquareGrids.COLUMN_COUNT; columnNum++)
            {
                Tile tempTileUp = new Tile(Tiles[columnNum, rowNum]);
                Tile tempTileDown = new Tile(Tiles[columnNum, rowNum + 1]);
                rowError[columnNum * 2] = myGrid.mmFormPixel(myMath.GetDis<float>(tempTileDown.conerLT.Y - tempTileUp.conerLD.Y));
                LineSegment2DF leftLine = new LineSegment2DF(tempTileDown.conerLT, tempTileUp.conerLD);
                rowError[columnNum * 2 + 1] = myGrid.mmFormPixel(myMath.GetDis<float>(tempTileDown.conerRT.Y - tempTileUp.conerRD.Y));
                LineSegment2DF rightLine = new LineSegment2DF(tempTileDown.conerRT, tempTileUp.conerRD);
                rowReport += "\t" + rowError[columnNum * 2] + "\t" + rowError[columnNum * 2 + 1];
                double gapA = rowError[columnNum * 2];
                double gapB = rowError[columnNum * 2 + 1];
                gapAndDis.Add(leftLine, gapA);
                gapAndDis.Add(rightLine, gapB);
                allRowError.Add(gapA);
                allRowError.Add(gapB);
                gap.Add(gapA);
                gap.Add(gapB);
            }
            rowReport += "\t平均：\t" + rowError.Average() + "\t標準差：\t" + rowError.StandardDeviation();
            myReport.newLine(rowReport);
        }
        myReport.GapHSD = allRowError.ToArray().StandardDeviation();

        #endregion 列與列間溝縫

        #region 行與行間溝縫

        myReport.newLine("行與行間溝縫：");
        List<double> allColumnError = new List<double>();
        for (int columnNum = 0; columnNum < SquareGrids.COLUMN_COUNT - 1; columnNum++)
        {
            string columnReport = "第" + columnNum.ToString("D2") + "條：";
            List<double> columnError = new List<double>();

            for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
            {
                Tile tempTileLeft = new Tile(Tiles[columnNum, rowNum]);
                Tile tempTileRight = new Tile(Tiles[columnNum + 1, rowNum]);
                double gapC = myGrid.mmFormPixel(myMath.GetDis<float>(tempTileRight.conerLT.X - tempTileLeft.conerRT.X));
                columnError.Add(gapC);
                LineSegment2DF topLine = new LineSegment2DF(tempTileRight.conerLT, tempTileLeft.conerRT);
                double gapD = myGrid.mmFormPixel(myMath.GetDis<float>(tempTileRight.conerLD.X - tempTileLeft.conerRD.X));
                columnError.Add(gapD);
                LineSegment2DF downLine = new LineSegment2DF(tempTileRight.conerLD, tempTileLeft.conerRD);
                columnReport += "\t" + gapC + "\t" + gapD;
                gapAndDis.Add(topLine, gapC);
                gapAndDis.Add(downLine, gapD);
                allColumnError.Add(gapC);
                allColumnError.Add(gapD);
                gap.Add(gapC);
                gap.Add(gapD);
            }

            columnReport += "\t平均：\t" + columnError.Average() + "\t標準差\t" + columnError.ToArray().StandardDeviation();
            myReport.newLine(columnReport);
        }
        myReport.GapVSD = allColumnError.ToArray().StandardDeviation();

        #endregion 行與行間溝縫

        #region 不分方向評分

        double gapSD = gap.ToArray().StandardDeviation();
        double gapAvg = gap.Average();
        double gapMax = gap.Max();
        double gapMin = gap.Min();
        double gapRange = gapMax - gapMin;
        double gapRangeAvgRatio = gapRange / gapAvg;
        myReport.newLine("勾縫標準差:/t" + gapSD);
        myReport.GapSD = gapSD;
        myReport.newLine("勾縫極差:/t" + gapRange);
        myReport.GapRange = gapRange;
        myReport.newLine("平均勾縫寬:/t" + gapAvg);
        myReport.GapAvg = gapAvg;
        myReport.newLine("極差平均比/t" + gapAvg);
        myReport.GapRangeAvgRatio = gapRangeAvgRatio;

        var ErrGap = from outputGap in gapAndDis
                     where !outputGap.Value.IsBetween(gapAvg - ErrMark.GapSquareTh, gapAvg + ErrMark.GapSquareTh)
                     select outputGap.Key;
        outPutErrMark.GapErr = ErrGap.ToList();

        #endregion 不分方向評分

        #endregion 評分_溝縫

        #region 評分_磁磚角趨勢線
#if false

        myReport.newLine("");
        myReport.newLine("磁磚角趨勢線");

        #region 水平趨勢線

        myReport.newLine("水平趨勢線");
        for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
        {
            double[] tempTopXs = new double[SquareGrids.columnCount * 2];
            double[] tempTopYs = new double[SquareGrids.columnCount * 2];
            string HorizontalTrendLineX = "";
            string HorizontalTrendLineY = "";
            for (int columnNum = 0; columnNum < SquareGrids.columnCount; columnNum++)
            {
                Tile tempTile = new Tile(Tiles[columnNum, rowNum]);

                tempTopXs[columnNum * 2] = tempTile.conerLT.X;
                tempTopXs[columnNum * 2 + 1] = tempTile.conerRT.X;
                HorizontalTrendLineX += "\t" + tempTile.conerLT.X + "\t" + tempTile.conerRT.X;
                tempTopYs[columnNum * 2] = tempTile.conerLT.Y * -1;
                tempTopYs[columnNum * 2 + 1] = tempTile.conerRT.Y * -1;
                HorizontalTrendLineY += "\t" + tempTile.conerLT.Y + "\t" + tempTile.conerRT.Y;
            }
            myStatistics.TrendLine TrendLineTop = new myStatistics.TrendLine(tempTopXs, tempTopYs);
            myReport.newLine("第" + rowNum.ToString("D2") + "列上：");
            myReport.newLine("角度(度數)：\t" + TrendLineTop.Angle());
            myReport.newLine("Xdata:\t" + HorizontalTrendLineX);
            myReport.newLine("Ydata:\t" + HorizontalTrendLineY);

            double[] tempDownXs = new double[SquareGrids.columnCount * 2];
            double[] tempDownYs = new double[SquareGrids.columnCount * 2];
            HorizontalTrendLineX = "";
            HorizontalTrendLineY = "";
            for (int columnNum = 0; columnNum < SquareGrids.columnCount; columnNum++)
            {
                Tile tempTile = new Tile(Tiles[columnNum, rowNum]);

                tempTopXs[columnNum * 2] = tempTile.conerLD.X;
                tempTopXs[columnNum * 2 + 1] = tempTile.conerRD.X;
                HorizontalTrendLineX += "\t" + tempTile.conerLD.X + "\t" + tempTile.conerRD.X;
                tempTopYs[columnNum * 2] = tempTile.conerLD.Y * -1;
                tempTopYs[columnNum * 2 + 1] = tempTile.conerRD.Y * -1;
                HorizontalTrendLineY += "\t" + tempTile.conerLD.Y + "\t" + tempTile.conerRD.Y;
            }
            myStatistics.TrendLine TrendLineDown = new myStatistics.TrendLine(tempTopXs, tempTopYs);
            myReport.newLine("第" + rowNum.ToString("D2") + "列下：");
            myReport.newLine("角度(度數)：\t" + TrendLineDown.Angle());
            myReport.newLine("Xdata:\t" + HorizontalTrendLineX);
            myReport.newLine("Ydata:\t" + HorizontalTrendLineY);
        }

        #endregion 水平趨勢線

        #region 垂直趨勢線

        myReport.newLine("垂直趨勢線");
        for (int columnNum = 0; columnNum < SquareGrids.columnCount; columnNum++)
        {
            double[] tempTopXs = new double[rowEnd * 2];
            double[] tempTopYs = new double[rowEnd * 2];
            string VerticalTrendLineX = "";
            string VerticalTrendLineY = "";
            for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
            {
                Tile tempTile = new Tile(Tiles[columnNum, rowNum]);

                tempTopXs[rowNum * 2] = tempTile.conerLT.X;
                tempTopXs[rowNum * 2 + 1] = tempTile.conerLD.X;
                VerticalTrendLineX += "\t" + tempTile.conerLT.X + "\t" + tempTile.conerLD.X;
                tempTopYs[rowNum * 2] = tempTile.conerLT.Y * -1;
                tempTopYs[rowNum * 2 + 1] = tempTile.conerLD.Y * -1;
                VerticalTrendLineY += "\t" + tempTile.conerLT.Y + "\t" + tempTile.conerLD.Y;
            }
            myStatistics.TrendLine TrendLineTop = new myStatistics.TrendLine(tempTopYs, tempTopXs);
            myReport.newLine("第" + columnNum.ToString("D2") + "行左：");
            myReport.newLine("角度(度數)：\t" + myTool.CorrectingAngle_YXtoXY(TrendLineTop.Angle()));
            myReport.newLine("Xdata:\t" + VerticalTrendLineX);
            myReport.newLine("Ydata:\t" + VerticalTrendLineY);

            double[] tempDownXs = new double[rowEnd * 2];
            double[] tempDownYs = new double[rowEnd * 2];
            VerticalTrendLineX = "";
            VerticalTrendLineY = "";
            for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
            {
                Tile tempTile = new Tile(Tiles[columnNum, rowNum]);

                tempTopXs[rowNum * 2] = tempTile.conerRT.X;
                tempTopXs[rowNum * 2 + 1] = tempTile.conerRD.X;
                VerticalTrendLineX += "\t" + tempTile.conerRT.X + "\t" + tempTile.conerRD.X;
                tempTopYs[rowNum * 2] = tempTile.conerRT.Y * -1;
                tempTopYs[rowNum * 2 + 1] = tempTile.conerRD.Y * -1;
                VerticalTrendLineY += "\t" + tempTile.conerRT.Y + "\t" + tempTile.conerRD.Y;
            }
            myStatistics.TrendLine TrendLineDown = new myStatistics.TrendLine(tempTopYs, tempTopXs);
            myReport.newLine("第" + columnNum.ToString("D2") + "行右：");
            myReport.newLine("角度(度數)：\t" + myTool.CorrectingAngle_YXtoXY(TrendLineDown.Angle()));
            myReport.newLine("Xdata:\t" + VerticalTrendLineX);
            myReport.newLine("Ydata:\t" + VerticalTrendLineY);
        }

        #endregion 垂直趨勢線

#endif
        #endregion 評分_磁磚角趨勢線

        #region 評分_磁磚中心趨勢線

        myReport.newLine("");
        myReport.newLine("磁磚中心趨勢線夾角");

        #region 水平趨勢線

        myReport.newLine("水平趨勢線夾角");
        List<double> HorizontalTrendLineaAngle = new List<double>();
        for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
        {
            double[] Xdata = new double[SquareGrids.COLUMN_COUNT];
            double[] Ydata = new double[SquareGrids.COLUMN_COUNT];
            string HorizontalTrendLineX = "";
            string HorizontalTrendLineY = "";
            for (int columnNum = 0; columnNum < SquareGrids.COLUMN_COUNT; columnNum++)
            {
                Xdata[columnNum] = Tiles[columnNum, rowNum].center.X;
                HorizontalTrendLineX += "\t" + Xdata[columnNum];
                Ydata[columnNum] = Tiles[columnNum, rowNum].center.Y * -1;
                HorizontalTrendLineY += "\t" + Ydata[columnNum];
            }
            myStatistics.TrendLine TrendLine = new myStatistics.TrendLine(Xdata, Ydata);
            double angle = TrendLine.Angle();
            myReport.newLine("第" + rowNum.ToString("D2") + "列：");
            myReport.newLine("角度(度數)：\t" + angle);
            HorizontalTrendLineaAngle.Add(angle);
            myReport.newLine("Xdata:\t" + HorizontalTrendLineX);
            myReport.newLine("Ydata:\t" + HorizontalTrendLineY);
        }
        double rowAngleSD = HorizontalTrendLineaAngle.ToArray().StandardDeviation();
        myReport.RowAngleSD = rowAngleSD;
        myReport.newLine("水平趨勢線夾角標準差" + rowAngleSD);

        #endregion 水平趨勢線

        #region 垂直趨勢線

        myReport.newLine("垂直趨勢線");
        List<double> VerticalTrendLineAngle = new List<double>();
        for (int columnNum = 0; columnNum < SquareGrids.COLUMN_COUNT; columnNum++)
        {
            List<double> Xdata = new List<double>();
            List<double> Ydata = new List<double>();
            string VerticalTrendLineX = "";
            string VerticalTrendLineY = "";
            for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
            {
                float x = Tiles[columnNum, rowNum].center.X;
                Xdata.Add(x);
                VerticalTrendLineX += "\t" + x;
                float y = Tiles[columnNum, rowNum].center.Y * -1;
                Ydata.Add(y);
                VerticalTrendLineY += "\t" + y;
            }
            myStatistics.TrendLine TrendLine = new myStatistics.TrendLine(Ydata.ToArray(), Xdata.ToArray());
            double angle = myTool.CorrectingAngle_YXtoXY(TrendLine.Angle());
            VerticalTrendLineAngle.Add(angle);
            myReport.newLine("第" + columnNum.ToString("D2") + "行：");
            myReport.newLine("角度(度數)：\t" + angle);
            myReport.newLine("Xdata:\t" + VerticalTrendLineX);
            myReport.newLine("Ydata:\t" + VerticalTrendLineY);
        }
        double columnAngleSD = VerticalTrendLineAngle.ToArray().StandardDeviation();
        myReport.ColumnAngleSD = columnAngleSD;
        myReport.newLine("垂直趨勢線夾角標準差" + columnAngleSD);

        #endregion 垂直趨勢線

        #endregion 評分_磁磚中心趨勢線

        #region 評分_座標標準差
#if false

        myReport.newLine("");
        myReport.newLine("座標標準差");

        #region 列中磁磚的Y方向

        myReport.newLine("列中磁磚的Y方向");
        List<double> RowY = new List<double>();
        List<double> RowSpacing = new List<double>();

        for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
        {
            double[] TileYC = new double[SquareGrids.COLUMN_COUNT];
            double[] TileYE = new double[SquareGrids.COLUMN_COUNT];
            string TileYReport = "第" + rowNum.ToString("D2") + "列：";
            for (int columnNum = 0; columnNum < SquareGrids.COLUMN_COUNT; columnNum++)
            {
                TileYC[columnNum] = myGrid.mmFormPixel(Tiles[columnNum, rowNum].center.Y);
                TileYReport += "\t" + TileYC[columnNum];
            }

            double TileYC_avg = TileYC.Average();
            TileYReport +=
                "\t平均：\t" + TileYC_avg
                + "\t標準差\t" + TileYC.StandardDeviation() + "\t差：";

            for (int columnNum = 0; columnNum < SquareGrids.COLUMN_COUNT; columnNum++)
            {
                TileYE[columnNum] = TileYC[columnNum] - TileYC_avg;
                TileYReport += "\t" + TileYE[columnNum];
            }
            myReport.newLine(TileYReport);
            RowY.Add(TileYC.Average());
        }

        myReport.newLine("列與列間隔：");
        string RowSpacingReport = "\t";
        for (int rowNum = 0; rowNum < RowY.Count - 1; rowNum++)
        {
            double spacing = RowY[rowNum + 1] - RowY[rowNum];
            RowSpacing.Add(spacing);
            RowSpacingReport += "\t" + spacing;
        }
        myReport.newLine(RowSpacingReport
            + "\t平均\t" + RowSpacing.Average()
            + "\t標準差\t" + myStatistics.StandardDeviation(RowSpacing.ToArray()));

        #endregion 列中磁磚的Y方向

        #region 行中磁磚的X方向

        myReport.newLine("行中磁磚的X方向");
        double[] ColumnX = new double[SquareGrids.COLUMN_COUNT];
        double[] ColumnSpacing = new double[SquareGrids.COLUMN_COUNT - 1];

        for (int columnNum = 0; columnNum < SquareGrids.COLUMN_COUNT; columnNum++)
        {
            List<double> TileXC = new List<double>();
            List<double> TileXE = new List<double>();
            string TileXReport = "第" + columnNum.ToString("D2") + "行：";
            for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
            {
                double x = myGrid.mmFormPixel(Tiles[columnNum, rowNum].center.X);
                TileXC.Add(x);
                TileXReport += "\t" + x;
            }
            double TileXC_avg = TileXC.Average();
            TileXReport +=
                "\t平均：\t" + TileXC.Average()
                + "\t標準差\t" + TileXC.StandardDeviation() + "\t差：";
            for (int rowNum = 0; rowNum < TileXC.Count(); rowNum++)
            {
                double err = TileXC[rowNum] - TileXC_avg;
                TileXE.Add(err);
                TileXReport += "\t" + err;
            }
            myReport.newLine(TileXReport);

            ColumnX[columnNum] = TileXC.Average();
        }

        myReport.newLine("行與行間隔：");
        string ColumnSpacingReport = "\t";
        for (int columnNum = 0; columnNum < SquareGrids.COLUMN_COUNT - 1; columnNum++)
        {
            ColumnSpacing[columnNum] = ColumnX[columnNum + 1] - ColumnX[columnNum];
            ColumnSpacingReport += "\t" + ColumnSpacing[columnNum];
        }
        myReport.newLine(ColumnSpacingReport
            + "\t平均\t" + ColumnSpacing.Average()
            + "\t標準差\t" + myStatistics.StandardDeviation(ColumnSpacing));

        #endregion 行中磁磚的X方向

#endif
        #endregion 評分_座標標準差

        myReport.newLine("筆直度:");

        #region 平均Y方向離差

        myReport.newLine("Y方向離差:");
        double sumOfTileCenterToTrendLineH = 0;
        for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
        {
            string rowReport = "第" + rowNum + "列";
            double[] Xdata = new double[SquareGrids.COLUMN_COUNT];
            double[] Ydata = new double[SquareGrids.COLUMN_COUNT];
            for (int column = 0; column < SquareGrids.COLUMN_COUNT; column++)
            {
                Xdata[column] = Tiles[column, rowNum].center.X;
                Ydata[column] = Tiles[column, rowNum].center.Y * -1;
            }
            myStatistics.TrendLine theTrendLine = new myStatistics.TrendLine(Xdata, Ydata);
            for (int column = 0; column < SquareGrids.COLUMN_COUNT; column++)
            {
                double Residual = Math.Abs(theTrendLine.PointToLineDis(Xdata[column], Ydata[column]));
                Residual = myGrid.mmFormPixel(Residual);
                sumOfTileCenterToTrendLineH += Residual;
                rowReport += "\t" + Residual;
            }
            myReport.newLine(rowReport);
        }
        double rowResidualAvg = sumOfTileCenterToTrendLineH / (SquareGrids.ROW_COUNT_HALF * SquareGrids.COLUMN_COUNT);
        string TotalRowReport = "平均Y方向離差:" + rowResidualAvg.ToString("0.000");

        myReport.newLine(TotalRowReport);
        myReport.RowResidualAvg = rowResidualAvg;

        #endregion 平均Y方向離差

        #region 平均X方向離差

        myReport.newLine("X方向離差:");
        double sumOfTileCenterToTrendLineV = 0;
        for (int column = 0; column < SquareGrids.COLUMN_COUNT; column++)
        {
            string ColumnReport = "第" + column + "行";
            List<double> Xdata = new List<double>();
            List<double> Ydata = new List<double>();
            
            for (int row = rowStart; row < rowEnd; row++)
            {
                Xdata.Add(Tiles[column, row].center.X);
                Ydata.Add(Tiles[column, row].center.Y * -1);
            }
            myStatistics.TrendLine theTrendLine = new myStatistics.TrendLine(Ydata.ToArray(), Xdata.ToArray());
            for (int row = 0; row < Xdata.Count; row++)
            {
                double Residual = Math.Abs(theTrendLine.PointToLineDis(Ydata[row], Xdata[row]));
                Residual = myGrid.mmFormPixel(Residual);
                sumOfTileCenterToTrendLineV += Residual;
                ColumnReport += "\t" + Residual;
            }
            myReport.newLine(ColumnReport);
        }
        double columnResidualAvg = sumOfTileCenterToTrendLineV / (SquareGrids.ROW_COUNT_HALF * SquareGrids.COLUMN_COUNT);
        string TotalColumnReport = "平均X方向離差:" + columnResidualAvg.ToString("0.000");

        myReport.newLine(TotalColumnReport);
        myReport.ColumnResidualAvg = columnResidualAvg;

        #endregion 平均X方向離差

        #region 評分_磁磚旋轉角

        myReport.newLine("");
        myReport.newLine("磁磚旋轉角");

        #region 印出每列

        for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
        {
            string angleReport = "第" + rowNum.ToString("D2") + "列";
            double[] angleDataInRow = new double[SquareGrids.COLUMN_COUNT];
            for (int columnNum = 0; columnNum < SquareGrids.COLUMN_COUNT; columnNum++)
            {
                angleDataInRow[columnNum] = Tiles[columnNum, rowNum].angle;
                angleReport += "\t" + angleDataInRow[columnNum];
            }
            myReport.newLine(angleReport
                + "\t平均：\t" + angleDataInRow.Average()
                + "\t標準差：\t" + angleDataInRow.StandardDeviation());
        }

        #endregion 印出每列

        #region 統計每行

        string angleAvgInColumn = "平均：";
        string angleSDInColumn = "標準差：";
        for (int columnNum = 0; columnNum < SquareGrids.COLUMN_COUNT; columnNum++)
        {
            List<double> angleData = new List<double>();
            for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
            {
                angleData.Add(Tiles[columnNum, rowNum].angle);
            }
            angleAvgInColumn += "\t" + angleData.Average();
            angleSDInColumn += "\t" + angleData.StandardDeviation();
        }
        myReport.newLine(angleAvgInColumn);
        myReport.newLine(angleSDInColumn);

        #endregion 統計每行

        #region 統計整個

        List<double> wholeAngleData = new List<double>();
        for (int columnNum = 0; columnNum < SquareGrids.COLUMN_COUNT; columnNum++)
        {
            for (int row = rowStart; row < rowEnd; row++)
            {
                wholeAngleData.Add(Tiles[columnNum, row].angle);
            }
        }

        #endregion 統計整個

        myReport.newLine("總平均：\t" + wholeAngleData.Average());
        double AngleSD = wholeAngleData.StandardDeviation();
        myReport.newLine("總標準差：\t" + AngleSD);
        myReport.AngleSD = AngleSD;

        #endregion 評分_磁磚旋轉角

        #region 林國良法（磁磚角離差）

        #region 垂直離差
        myReport.newLine("磁磚角離差Y方向:");
        List<double> residualSetY = new List<double>();
        for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
        {
            List<PointF> pointSetInLineUpper = new List<PointF>();
            List<PointF> pointSetInLineLower = new List<PointF>();
            for (int column = 0; column < SquareGrids.COLUMN_COUNT; column++)
            {
                Tile currentTile = new Tile(Tiles[column, rowNum]);
                pointSetInLineUpper.Add(currentTile.conerLT);
                pointSetInLineUpper.Add(currentTile.conerRT);
                pointSetInLineLower.Add(currentTile.conerLD);
                pointSetInLineLower.Add(currentTile.conerRD);
            }
            myStatistics.TrendLine currentTrendLineUpper = new myStatistics.TrendLine(pointSetInLineUpper);
            myStatistics.TrendLine currentTrendLineLower = new myStatistics.TrendLine(pointSetInLineLower);
            foreach (var point in pointSetInLineUpper)
            {
                double residual = Math.Abs(currentTrendLineUpper.PointToLineDis(point.X, point.Y));
                residualSetY.Add(residual);
            }
            foreach (var point in pointSetInLineLower)
            {
                double residual = Math.Abs(currentTrendLineLower.PointToLineDis(point.X, point.Y));
                residualSetY.Add(residual);
            }
        }
        double residualAvgY = residualSetY.Average();
        double residualYPerImageWidth = myGrid.mmFormPixel(residualAvgY);
        myReport.newLine("平均垂直離差(mm)：" + residualYPerImageWidth);
        myReport.CornerResidualY = residualYPerImageWidth;

        double imageWidthInMm = myGrid.mmFormPixel(myGrid.WidthInPixel);
        double residualYPer3mInMm = residualYPerImageWidth / imageWidthInMm * 3000;
        myReport.newLine("3m垂直離差(mm)：" + residualYPer3mInMm);
        myReport.ResidualAvgY3m = residualYPer3mInMm;
        #endregion

        #region 水平離差
        myReport.newLine("磁磚角離差X方向:");
        List<double> residualSetX = new List<double>();
        for (int column = 0; column < SquareGrids.COLUMN_COUNT; column++)
        {
            List<PointF> pointSetInLineLeft = new List<PointF>();
            List<PointF> pointSetInLineRight = new List<PointF>();
            for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
            {
                Tile currentTile = new Tile(Tiles[column, rowNum]);
                pointSetInLineLeft.Add(currentTile.conerLT);
                pointSetInLineLeft.Add(currentTile.conerLD);
                pointSetInLineRight.Add(currentTile.conerRT);
                pointSetInLineRight.Add(currentTile.conerRD);
            }
            myStatistics.TrendLine currentTrendLineLeft = new myStatistics.TrendLine(pointSetInLineLeft,true);
            myStatistics.TrendLine currentTrendLineRight = new myStatistics.TrendLine(pointSetInLineRight, true);
            foreach (var point in pointSetInLineLeft)
            {
                double residual = Math.Abs(currentTrendLineLeft.PointToLineDis(point.Y, point.X));
                residualSetX.Add(residual);
            }
            foreach (var point in pointSetInLineRight)
            {
                double residual = Math.Abs(currentTrendLineRight.PointToLineDis(point.Y, point.X));
                residualSetX.Add(residual);
            }
        }
        double residualAvgX = residualSetX.Average();
        double residualXPerImageHeight = myGrid.mmFormPixel(residualAvgX);
        myReport.newLine("平均水平離差(mm)：" + residualXPerImageHeight);
        myReport.CornerResidualX = residualXPerImageHeight;
        double imageHeightInMm = myGrid.mmFormPixel(myGrid.HeightInPixel);
        double residualXPer3mInMm = residualXPerImageHeight / imageHeightInMm * 3000;
        myReport.newLine("3m水平離差(mm)：" + residualXPer3mInMm);
        myReport.ResidualAvgX3m = residualXPer3mInMm;
        #endregion

        #endregion

        myReport.SaveReport(rankArea);
        //return myReport.ScoringByVariance(TilesType.Square);
        return myReport;
        //myReport.doToReport("評分報告");

        //return myReport.SaveReport(rankTopOnly);

        #endregion 評分
    }

    /// <summary>將1D的磁磚陣列轉為2D陣列</summary>
    public static MCvBox2D[,] TileArrayToTile2DArray(Point theGridRD, Point theGridLT, List<MCvBox2D> BaseMcvBox2DList)
    {
        #region 定位磁磚

        MCvBox2D[,] tiles = new MCvBox2D[SquareGrids.COLUMN_COUNT, SquareGrids.ROW_COUNT];
        bool[,] PositionOccupied = new bool[SquareGrids.ROW_COUNT, SquareGrids.COLUMN_COUNT];
        for (int rowIndex = 0; rowIndex < SquareGrids.ROW_COUNT; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < SquareGrids.COLUMN_COUNT; columnIndex++)
            {
                PositionOccupied[columnIndex, rowIndex] = false;
            }
        }

        PointF LeftTop = PointF.Empty;
        double minDis = double.MaxValue;
        PointF RightDown = PointF.Empty;
        double maxDis = double.MinValue;
        foreach (var item in BaseMcvBox2DList)
        {
            if (myMath.GetDistance(item.center, new PointF(0, 0)) < minDis)
            {
                minDis = myMath.GetDistance(item.center, new PointF(0, 0));
                LeftTop = item.center;
            }
            if (myMath.GetDistance(item.center, new PointF(0, 0)) > maxDis)
            {
                maxDis = myMath.GetDistance(item.center, new PointF(0, 0));
                RightDown = item.center;
            }
        }
        double avgGridSpacingH_Pixel = (RightDown.X - LeftTop.X) / (SquareGrids.COLUMN_COUNT - 1);
        double avgGridSpacingV_Pixel = (RightDown.Y - LeftTop.Y) / (SquareGrids.ROW_COUNT - 1);

        for (int index = 0; index < BaseMcvBox2DList.Count; index++)
        {
            //int columnOfBox = (int)((BaseMcvBox2DList[index].center.X - myGrid.get_GridsLT.X) / myGrid.get_gridSpacingInPixel);
            int columnOfBox = (int)((BaseMcvBox2DList[index].center.X - theGridLT.X) / avgGridSpacingH_Pixel);

            //int rowOfBox = (int)((BaseMcvBox2DList[index].center.Y - myGrid.get_GridsLT.Y) / myGrid.get_gridSpacingInPixel);
            int rowOfBox = (int)((BaseMcvBox2DList[index].center.Y - theGridLT.Y) / avgGridSpacingV_Pixel);

            if (PositionOccupied[columnOfBox, rowOfBox] == false)
            {
                tiles[columnOfBox, rowOfBox] = BaseMcvBox2DList[index];
                PositionOccupied[columnOfBox, rowOfBox] = true;
            }
            else
            {
                throw new Exception("tile" + index + "位置與其他tile重複");
            }
        }
        return tiles;

        #endregion 定位磁磚
    }

    #region 錯誤訊息

    public const int error_TilePosition = 1;
    public const int safe = 0;

    #endregion 錯誤訊息
}