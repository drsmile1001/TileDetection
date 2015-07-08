using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

internal class RectangleTiles
{
    /// <summary>錯誤訊息</summary>
    public int msg;

    public RectangleGrids myGrid;

    /// <summary>代表整組磁磚</summary>
    public MCvBox2D[,] Tiles;

    public string RankResult { get; private set; }

    public RectangleTiles(Point theGridRD, Point theGridLT, List<MCvBox2D> BaseMcvBox2DList, string fileName, RankArea rankArea)
    {
        myGrid = new RectangleGrids(theGridRD, theGridLT);
        Tiles = RectangleTiles.TileArrayToTile2DArray(theGridRD, theGridLT, BaseMcvBox2DList);

        //評分
        RankResult = rankRectangleTiles(fileName, Tiles, myGrid, rankArea);
    }

    public static string rankRectangleTiles(string fileName, MCvBox2D[,] Tiles, RectangleGrids myGrid, RankArea rankArea)
    {
        #region 評分

        report myReport = new report(fileName);
        int rowEnd;
        int rowStart;
        switch (rankArea)
        {
            case RankArea.Top:
                rowStart = 0;
                rowEnd = RectangleGrids.rowCountHalf;
                break;

            case RankArea.Down:
                rowStart = RectangleGrids.rowCountHalf;
                rowEnd = RectangleGrids.rowCount;
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
            for (int column = 0; column < RectangleGrids.columnCount; column++)
            {
                if ((row + column) % 2 != 1)
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
                else
                {
                    rowReport += "\t";
                }
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

        #region 列與列間溝縫

        myReport.newLine("列與列間溝縫：");
        List<double> allRowError = new List<double>();
        double helfTileWidth = myGrid.PixelFormMm(RectangleGrids.tileWidth) / 2;

        #region 考慮第0條至第6條(共7條)水平溝縫，將溝縫視為上鄰接列的附屬，所以最後一列沒有溝縫

        for (int rowNum = rowStart; rowNum < rowEnd - 1; rowNum++)
        {
            string rowReport = "第" + rowNum.ToString("D2") + "條：";
            List<double> rowError = new List<double>();

            #region 依序考慮每一行磁磚要計算的溝縫

            for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
            {
                #region 對照行列，只計算"存在"磁磚位置

                if ((columnNum + rowNum) % 2 == 0)
                {
                    #region 只看奇數列中有存在的磁磚

                    #region 如果相對於本磁磚的左下磁磚存在的話

                    if (columnNum - 1 >= 0)
                    {
                        #region 溝縫差加上：左下磁磚中心點與上邊交點 與 本磁磚左下點 的y方向差

                        double disA = myGrid.mmFormPixel(myMath.GetDis<double>(
                                Tiles[columnNum - 1, rowNum + 1].center.Y - helfTileWidth
                                    - new Tile(Tiles[columnNum, rowNum]).conerLD.Y));
                        rowError.Add(disA);
                        allRowError.Add(disA);
                        gap.Add(disA);

                        #endregion 溝縫差加上：左下磁磚中心點與上邊交點 與 本磁磚左下點 的y方向差

                        #region 溝縫差加上：左下磁磚右上點 與 本磁磚中心點與下邊交點 的y方向差

                        double disB = myGrid.mmFormPixel(myMath.GetDis<double>(
                                new Tile(Tiles[columnNum - 1, rowNum + 1]).conerRT.Y - helfTileWidth
                                    - Tiles[columnNum, rowNum].center.Y));
                        rowError.Add(disB);
                        allRowError.Add(disB);
                        gap.Add(disB);

                        #endregion 溝縫差加上：左下磁磚右上點 與 本磁磚中心點與下邊交點 的y方向差
                    }

                    #endregion 如果相對於本磁磚的左下磁磚存在的話

                    #region 如果相對於本磁磚的右下磁磚存在的話

                    if (columnNum + 1 < RectangleGrids.columnCount)
                    {
                        #region 溝縫差加上：右下磁磚左上點 與 本磁磚中心點與下邊交點 的y方向差

                        double disC = myGrid.mmFormPixel(myMath.GetDis<double>(
                                new Tile(Tiles[columnNum + 1, rowNum + 1]).conerLT.Y - helfTileWidth
                                    - Tiles[columnNum, rowNum].center.Y));
                        rowError.Add(disC);
                        allRowError.Add(disC);
                        gap.Add(disC);

                        #endregion 溝縫差加上：右下磁磚左上點 與 本磁磚中心點與下邊交點 的y方向差

                        #region 溝縫差加上：右下磁磚中心點與上邊交點 與 本磁磚右下點 的y方向差

                        double disD = myGrid.mmFormPixel(myMath.GetDis<double>(
                                Tiles[columnNum + 1, rowNum + 1].center.Y - helfTileWidth
                                    - new Tile(Tiles[columnNum, rowNum]).conerRD.Y));
                        rowError.Add(disD);
                        allRowError.Add(disD);
                        gap.Add(disD);

                        #endregion 溝縫差加上：右下磁磚中心點與上邊交點 與 本磁磚右下點 的y方向差
                    }

                    #endregion 如果相對於本磁磚的右下磁磚存在的話

                    #endregion 只看奇數列中有存在的磁磚
                }

                #endregion 對照行列，只計算"存在"磁磚位置
            }

            #endregion 依序考慮每一行磁磚要計算的溝縫

            foreach (double err in rowError)
            {
                rowReport += "\t" + err;
            }
            rowReport += "\t平均：\t" + rowError.Average() + "\t標準差：\t" + rowError.ToArray().StandardDeviation();
            myReport.newLine(rowReport);
        }

        #endregion 考慮第0條至第6條(共7條)水平溝縫，將溝縫視為上鄰接列的附屬，所以最後一列沒有溝縫

        myReport.GapHSD = allRowError.ToArray().StandardDeviation();

        #endregion 列與列間溝縫

        #region 行與行間溝縫

        myReport.newLine("行與行間溝縫：");
        List<double> allColumnError = new List<double>();
        for (int columnNum = 0; columnNum < RectangleGrids.gapCountInColumn; columnNum++)
        {
            string columnReport = "第" + columnNum.ToString("D2") + "條：";
            double[] columnError = new double[rowEnd];

            for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
            {
                if ((columnNum + rowNum) % 2 == 0)
                {
                    Tile tempTileLeft = new Tile(Tiles[columnNum, rowNum]);
                    Tile tempTileRight = new Tile(Tiles[columnNum + 2, rowNum]);
                    double disA = myGrid.mmFormPixel(myMath.GetDis<float>(tempTileRight.conerLT.X - tempTileLeft.conerRT.X));
                    double disB = myGrid.mmFormPixel(myMath.GetDis<float>(tempTileRight.conerLD.X - tempTileLeft.conerRD.X));
                    columnError[rowNum - (columnNum % 2)] = disA;
                    columnError[rowNum - (columnNum % 2) + 1] = disB;
                    columnReport += "\t" + columnError[rowNum - (columnNum % 2)] + "\t" + columnError[rowNum - (columnNum % 2) + 1];

                    allColumnError.Add(disA);
                    allColumnError.Add(disB);
                    gap.Add(disA);
                    gap.Add(disB);
                }
            }
            columnReport += "\t平均：\t" + columnError.Average() + "\t標準差\t" + columnError.StandardDeviation();
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

        #endregion 不分方向評分

        #endregion 評分_溝縫

        #region 評分_磁磚角趨勢線

        myReport.newLine("");
        myReport.newLine("磁磚角趨勢線");

        #region 水平趨勢線

        myReport.newLine("水平趨勢線");
        for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
        {
            double[] tempTopXs = new double[RectangleGrids.ColumnCountInRow(rowNum) * 2];
            double[] tempTopYs = new double[RectangleGrids.ColumnCountInRow(rowNum) * 2];
            string HorizontalTrendLineTopX = "";
            string HorizontalTrendLineTopY = "";

            double[] tempDownXs = new double[RectangleGrids.ColumnCountInRow(rowNum) * 2];
            double[] tempDownYs = new double[RectangleGrids.ColumnCountInRow(rowNum) * 2];
            string HorizontalTrendLineDownX = "";
            string HorizontalTrendLineDownY = "";

            for (int columnNum = 0; columnNum < RectangleGrids.ColumnCountInRow(rowNum); columnNum++)
            {
                Tile tempTile = new Tile(Tiles[columnNum * 2 + (rowNum % 2), rowNum]);

                tempTopXs[columnNum * 2] = tempTile.conerLT.X;
                tempTopXs[columnNum * 2 + 1] = tempTile.conerRT.X;
                HorizontalTrendLineTopX += "\t" + tempTile.conerLT.X + "\t" + tempTile.conerRT.X;
                tempTopYs[columnNum * 2] = tempTile.conerLT.Y * -1;
                tempTopYs[columnNum * 2 + 1] = tempTile.conerRT.Y * -1;
                HorizontalTrendLineTopY += "\t" + tempTile.conerLT.Y + "\t" + tempTile.conerRT.Y;

                tempDownXs[columnNum * 2] = tempTile.conerLD.X;
                tempDownXs[columnNum * 2 + 1] = tempTile.conerRD.X;
                HorizontalTrendLineDownX += "\t" + tempTile.conerLD.X + "\t" + tempTile.conerRD.X;
                tempDownYs[columnNum * 2] = tempTile.conerLD.Y * -1;
                tempDownYs[columnNum * 2 + 1] = tempTile.conerRD.Y * -1;
                HorizontalTrendLineDownY += "\t" + tempTile.conerLD.Y + "\t" + tempTile.conerRD.Y;
            }
            myStatistics.TrendLine TrendLineTop = new myStatistics.TrendLine(tempTopXs, tempTopYs);
            myReport.newLine("第" + rowNum.ToString("D2") + "列上：");
            myReport.newLine("角度(度數)：\t" + TrendLineTop.Angle());
            myReport.newLine("Xdata:\t" + HorizontalTrendLineTopX);
            myReport.newLine("Ydata:\t" + HorizontalTrendLineTopY);

            myStatistics.TrendLine TrendLineDown = new myStatistics.TrendLine(tempDownXs, tempDownYs);
            myReport.newLine("第" + rowNum.ToString("D2") + "列下：");
            myReport.newLine("角度(度數)：\t" + TrendLineDown.Angle());
            myReport.newLine("Xdata:\t" + HorizontalTrendLineDownX);
            myReport.newLine("Ydata:\t" + HorizontalTrendLineDownY);
        }

        #endregion 水平趨勢線

        #region 垂直趨勢線

        myReport.newLine("垂直趨勢線");
        for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
        {
            double[] tempLeftXs = new double[rowEnd];
            double[] tempLeftYs = new double[rowEnd];
            string VerticalTrendLineLeftX = "";
            string VerticalTrendLineLeftY = "";

            double[] tempRightXs = new double[rowEnd];
            double[] tempRightYs = new double[rowEnd];
            string VerticalTrendLineRightX = "";
            string VerticalTrendLineRightY = "";

            for (int rowNum = rowStart; rowNum < rowEnd / 2; rowNum++)
            {
                Tile tempTile = new Tile(Tiles[columnNum, rowNum * 2 + (columnNum % 2)]);

                tempLeftXs[rowNum * 2] = tempTile.conerLT.X;
                tempLeftXs[rowNum * 2 + 1] = tempTile.conerLD.X;
                VerticalTrendLineLeftX += "\t" + tempTile.conerLT.X + "\t" + tempTile.conerLD.X;
                tempLeftYs[rowNum * 2] = tempTile.conerLT.Y * -1;
                tempLeftYs[rowNum * 2 + 1] = tempTile.conerLD.Y * -1;
                VerticalTrendLineLeftY += "\t" + tempTile.conerLT.Y + "\t" + tempTile.conerLD.Y;

                tempRightXs[rowNum * 2] = tempTile.conerRT.X;
                tempRightXs[rowNum * 2 + 1] = tempTile.conerRD.X;
                VerticalTrendLineRightX += "\t" + tempTile.conerRT.X + "\t" + tempTile.conerRD.X;
                tempRightYs[rowNum * 2] = tempTile.conerRT.Y * -1;
                tempRightYs[rowNum * 2 + 1] = tempTile.conerRD.Y * -1;
                VerticalTrendLineRightY += "\t" + tempTile.conerRT.Y + "\t" + tempTile.conerRD.Y;
            }
            myStatistics.TrendLine TrendLineTop = new myStatistics.TrendLine(tempLeftYs, tempLeftXs);
            myReport.newLine("第" + columnNum.ToString("D2") + "行左：");
            myReport.newLine("角度(度數)：\t" + myTool.CorrectingAngle_YXtoXY(TrendLineTop.Angle()));
            myReport.newLine("Xdata:\t" + VerticalTrendLineLeftX);
            myReport.newLine("Ydata:\t" + VerticalTrendLineLeftY);

            myStatistics.TrendLine TrendLineRight = new myStatistics.TrendLine(tempRightYs, tempRightXs);
            myReport.newLine("第" + columnNum.ToString("D2") + "行右：");
            myReport.newLine("角度(度數)：\t" + myTool.CorrectingAngle_YXtoXY(TrendLineRight.Angle()));
            myReport.newLine("Xdata:\t" + VerticalTrendLineRightX);
            myReport.newLine("Ydata:\t" + VerticalTrendLineRightY);
        }

        #endregion 垂直趨勢線

        #endregion 評分_磁磚角趨勢線

        #region 評分_磁磚中心趨勢線

        myReport.newLine("");
        myReport.newLine("磁磚中心趨勢線");

        #region 水平趨勢線

        myReport.newLine("水平趨勢線");
        List<double> HorizontalTrendLineAngle = new List<double>();
        for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
        {
            double[] Xdata = new double[RectangleGrids.ColumnCountInRow(rowNum)];
            double[] Ydata = new double[RectangleGrids.ColumnCountInRow(rowNum)];
            string HorizontalTrendLineX = "";
            string HorizontalTrendLineY = "";

            for (int columnNum = 0; columnNum < RectangleGrids.ColumnCountInRow(rowNum); columnNum++)
            {
                Xdata[columnNum] = Tiles[columnNum * 2 + (rowNum % 2), rowNum].center.X;
                HorizontalTrendLineX += "\t" + Xdata[columnNum];
                Ydata[columnNum] = Tiles[columnNum * 2 + (rowNum % 2), rowNum].center.Y * -1;
                HorizontalTrendLineY += "\t" + Ydata[columnNum];
            }
            myStatistics.TrendLine TrendLine = new myStatistics.TrendLine(Xdata, Ydata);
            double angle = TrendLine.Angle();
            myReport.newLine("第" + rowNum.ToString("D2") + "列：");
            myReport.newLine("角度(度數)：\t" + TrendLine.Angle());
            HorizontalTrendLineAngle.Add(angle);

            myReport.newLine("Xdata:\t" + HorizontalTrendLineX);
            myReport.newLine("Ydata:\t" + HorizontalTrendLineY);
        }
        double rowAngleSD = HorizontalTrendLineAngle.ToArray().StandardDeviation();
        myReport.RowAngleSD = rowAngleSD;
        myReport.newLine("水平趨勢線夾角標準差" + rowAngleSD);

        #endregion 水平趨勢線

        #region 垂直趨勢線

        myReport.newLine("垂直趨勢線");
        List<double> VerticalTrendLineAngle = new List<double>();
        for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
        {
            double[] Xdata = new double[rowEnd / 2];
            double[] Ydata = new double[rowEnd / 2];
            string VerticalTrendLineX = "";
            string VerticalTrendLineY = "";
            for (int rowNum = rowStart; rowNum < rowEnd / 2; rowNum++)
            {
                Xdata[rowNum] = Tiles[columnNum, rowNum * 2 + (columnNum % 2)].center.X;
                VerticalTrendLineX += "\t" + Xdata[rowNum];
                Ydata[rowNum] = Tiles[columnNum, rowNum * 2 + (columnNum % 2)].center.Y * -1;
                VerticalTrendLineY += "\t" + Ydata[rowNum];
            }
            myStatistics.TrendLine TrendLine = new myStatistics.TrendLine(Ydata, Xdata);
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

        myReport.newLine("");
        myReport.newLine("座標標準差");

        #region 列中磁磚的Y方向

        myReport.newLine("列中磁磚的Y方向");
        double[] RowY = new double[rowEnd];
        double[] RowSpacing = new double[rowEnd - 1];

        for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
        {
            double[] TileYC = new double[RectangleGrids.ColumnCountInRow(rowNum)];
            double[] TileYE = new double[RectangleGrids.ColumnCountInRow(rowNum)];
            string TileYReport = "第" + rowNum.ToString("D2") + "列：";
            for (int columnNum = 0; columnNum < RectangleGrids.ColumnCountInRow(rowNum); columnNum++)
            {
                TileYC[columnNum] = myGrid.mmFormPixel(Tiles[columnNum * 2 + (rowNum % 2), rowNum].center.Y);
                TileYReport += "\t" + TileYC[columnNum];
            }

            double TileYC_avg = TileYC.Average();
            if (rowNum % 2 == 1)
            {
                TileYReport += "\t";
            }
            TileYReport +=
                "\t平均：\t" + TileYC_avg
                + "\t標準差\t" + myStatistics.StandardDeviation(TileYC) + "\t差：";

            for (int columnNum = 0; columnNum < RectangleGrids.ColumnCountInRow(rowNum); columnNum++)
            {
                TileYE[columnNum] = TileYC[columnNum] - TileYC_avg;
                TileYReport += "\t" + TileYE[columnNum];
            }
            myReport.newLine(TileYReport);
            RowY[rowNum] = TileYC.Average();
        }

        myReport.newLine("列與列間隔：");
        string RowSpacingReport = "\t";
        for (int rowNum = rowStart; rowNum < rowEnd - 1; rowNum++)
        {
            RowSpacing[rowNum] = RowY[rowNum + 1] - RowY[rowNum];
            RowSpacingReport += "\t" + RowSpacing[rowNum];
        }
        myReport.newLine(RowSpacingReport
            + "\t平均\t" + RowSpacing.Average()
            + "\t標準差\t" + myStatistics.StandardDeviation(RowSpacing));

        #endregion 列中磁磚的Y方向

        #region 行中磁磚的X方向

        myReport.newLine("行中磁磚的X方向");
        double[] ColumnX = new double[RectangleGrids.columnCount];
        double[] ColumnSpacing = new double[RectangleGrids.columnCount - 1];

        for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
        {
            double[] TileXC = new double[rowEnd / 2];
            double[] TileXE = new double[rowEnd / 2];
            string TileXReport = "第" + columnNum.ToString("D2") + "行：";
            for (int rowNum = rowStart; rowNum < rowEnd / 2; rowNum++)
            {
                TileXC[rowNum] = myGrid.mmFormPixel(Tiles[columnNum, rowNum * 2 + (columnNum % 2)].center.X);
                TileXReport += "\t" + TileXC[rowNum];
            }
            double TileXC_avg = TileXC.Average();
            if (columnNum % 2 == 1)
            {
                TileXReport += "\t";
            }
            TileXReport +=
                "\t平均：\t" + TileXC.Average()
                + "\t標準差\t" + myStatistics.StandardDeviation(TileXC) + "\t差：";
            for (int rowNum = rowStart; rowNum < rowEnd / 2; rowNum++)
            {
                TileXE[rowNum] = TileXC[rowNum] - TileXC_avg;
                TileXReport += "\t" + TileXE[rowNum];
            }
            myReport.newLine(TileXReport);

            ColumnX[columnNum] = TileXC.Average();
        }

        myReport.newLine("行與行間隔：");
        string ColumnSpacingReport = "\t";
        for (int columnNum = 0; columnNum < RectangleGrids.columnCount - 1; columnNum++)
        {
            ColumnSpacing[columnNum] = ColumnX[columnNum + 1] - ColumnX[columnNum];
            ColumnSpacingReport += "\t" + ColumnSpacing[columnNum];
        }
        myReport.newLine(ColumnSpacingReport
            + "\t平均\t" + ColumnSpacing.Average()
            + "\t標準差\t" + myStatistics.StandardDeviation(ColumnSpacing));

        #endregion 行中磁磚的X方向

        #endregion 評分_座標標準差

        #region 評分_磁磚中心與趨勢線距離和

        myReport.newLine("筆直度:");

        #region 平均Y方向離差

        myReport.newLine("Y方向離差:");
        double sumOfTileCenterToTrendLineH = 0;
        for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
        {
            string rowReport = "第" + rowNum + "列";
            double[] Xdata = new double[RectangleGrids.ColumnCountInRow(rowNum)];
            double[] Ydata = new double[RectangleGrids.ColumnCountInRow(rowNum)];
            for (int columnNum = 0; columnNum < RectangleGrids.ColumnCountInRow(rowNum); columnNum++)
            {
                Xdata[columnNum] = Tiles[columnNum * 2 + (rowNum % 2), rowNum].center.X;
                Ydata[columnNum] = Tiles[columnNum * 2 + (rowNum % 2), rowNum].center.Y * -1;
            }
            myStatistics.TrendLine theTrendLine = new myStatistics.TrendLine(Xdata, Ydata);
            for (int columnNum = 0; columnNum < RectangleGrids.ColumnCountInRow(rowNum); columnNum++)
            {
                double Residual = Math.Abs(theTrendLine.PointToLineDis(Xdata[columnNum], Ydata[columnNum]));
                Residual = myGrid.mmFormPixel(Residual);
                sumOfTileCenterToTrendLineH += Residual;
                rowReport += "\t" + Residual;
            }
            myReport.newLine(rowReport);
        }
        int numberOfTile = RectangleGrids.tileCount / 2;
        double rowResidualAvg = sumOfTileCenterToTrendLineH / numberOfTile;
        string TotalRowReport = "平均y方向離差:" + rowResidualAvg.ToString("0.000");
        myReport.newLine(TotalRowReport);
        myReport.RowResidualAvg = rowResidualAvg;

        #endregion 平均Y方向離差

        #region 平均X方向離差

        myReport.newLine("平均X方向離差");
        double sumOfTileCenterToTrendLineV = 0;
        for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
        {
            string ColumnReport = "第" + columnNum + "行";
            double[] Xdata = new double[rowEnd / 2];
            double[] Ydata = new double[rowEnd / 2];
            for (int rowNum = rowStart; rowNum < rowEnd / 2; rowNum++)
            {
                Xdata[rowNum] = Tiles[columnNum, rowNum * 2 + (columnNum % 2)].center.X;
                Ydata[rowNum] = Tiles[columnNum, rowNum * 2 + (columnNum % 2)].center.Y * -1;
            }
            myStatistics.TrendLine theTrendLine = new myStatistics.TrendLine(Ydata, Xdata);
            for (int rowNum = rowStart; rowNum < rowEnd / 2; rowNum++)
            {
                double Residual = Math.Abs(theTrendLine.PointToLineDis(Ydata[rowNum], Xdata[rowNum]));
                Residual = myGrid.mmFormPixel(Residual);
                sumOfTileCenterToTrendLineV += Residual;
                ColumnReport += "\t" + Residual;
            }
            myReport.newLine(ColumnReport);
        }
        double columnResidualAvg = sumOfTileCenterToTrendLineV / numberOfTile;
        string TotalColumnReport = "平均X方向離差:" + columnResidualAvg.ToString("0.000");
        myReport.newLine(TotalColumnReport);
        myReport.ColumnResidualAvg = columnResidualAvg;

        #endregion 平均X方向離差

        #endregion 評分_磁磚中心與趨勢線距離和

        #region 評分_磁磚旋轉角

        myReport.newLine("");
        myReport.newLine("磁磚旋轉角");
        string TileAngleFirstRow = "";
        for (int columnNUm = 0; columnNUm < RectangleGrids.columnCount; columnNUm++)
        {
            TileAngleFirstRow += "\t第" + columnNUm.ToString("D2") + "行";
        }
        TileAngleFirstRow += "\t平均\t標準差";
        myReport.newLine(TileAngleFirstRow);

        #region 印出每列

        for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
        {
            string angleReport = "第" + rowNum.ToString("D2") + "列";
            if (rowNum % 2 == 1)
            {
                angleReport += "\t";
            }
            double[] angleDataInRow = new double[RectangleGrids.ColumnCountInRow(rowNum)];
            for (int columnNum = 0; columnNum < RectangleGrids.ColumnCountInRow(rowNum); columnNum++)
            {
                angleDataInRow[columnNum] = Tiles[columnNum * 2 + (rowNum % 2), rowNum].angle;
                if (columnNum != 0)
                {
                    angleReport += "\t";
                }
                angleReport += "\t" + angleDataInRow[columnNum];
            }
            if (rowNum % 2 == 1)
            {
                angleReport += "\t";
            }
            myReport.newLine(angleReport
                + "\t" + angleDataInRow.Average()
                + "\t" + myStatistics.StandardDeviation(angleDataInRow));
        }

        #endregion 印出每列

        #region 統計每行

        string angleAvgInColumn = "平均";
        string angleSDInColumn = "標準差";
        for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
        {
            double[] angleData = new double[rowEnd / 2];
            for (int rowNum = rowStart; rowNum < rowEnd / 2; rowNum++)
            {
                angleData[rowNum] = Tiles[columnNum, rowNum * 2 + (columnNum % 2)].angle;
            }
            angleAvgInColumn += "\t" + angleData.Average();
            angleSDInColumn += "\t" + myStatistics.StandardDeviation(angleData);
        }
        myReport.newLine(angleAvgInColumn);
        myReport.newLine(angleSDInColumn);

        #endregion 統計每行

        #region 統計整個

        double[] wholeAngleData = new double[rowEnd * RectangleGrids.columnCount];
        for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
        {
            for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
            {
                if ((rowNum + columnNum) % 2 == 0)
                {
                    wholeAngleData[rowNum * RectangleGrids.columnCount + columnNum] = Tiles[columnNum, rowNum].angle;
                }
            }
        }

        #endregion 統計整個

        myReport.newLine("總平均\t" + wholeAngleData.Average());

        myReport.AngleSD = myStatistics.StandardDeviation(wholeAngleData);
        myReport.newLine("總標準差\t" + myReport.AngleSD);

        #endregion 評分_磁磚旋轉角

        #region 林國良法（磁磚角離差）

        #region 垂直離差

        List<double> residualSetY = new List<double>();
        for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
        {
            List<PointF> pointSetInLineUpper = new List<PointF>();
            List<PointF> pointSetInLineLower = new List<PointF>();
            for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
            {
                if ((rowNum + columnNum) % 2 != 0)
                {
                    continue;
                }
                Tile currentTile = new Tile(Tiles[columnNum, rowNum]);
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

        #endregion 垂直離差

        #region 水平離差

        myReport.newLine("磁磚角離差X方向:");
        List<double> residualSetX = new List<double>();
        for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
        {
            List<PointF> pointSetInLineLeft = new List<PointF>();
            List<PointF> pointSetInLineRight = new List<PointF>();
            for (int rowNum = rowStart; rowNum < rowEnd; rowNum++)
            {
                if ((rowNum + columnNum) % 2 != 0)
                {
                    continue;
                }
                Tile currentTile = new Tile(Tiles[columnNum, rowNum]);
                pointSetInLineLeft.Add(currentTile.conerLT);
                pointSetInLineLeft.Add(currentTile.conerLD);
                pointSetInLineRight.Add(currentTile.conerRT);
                pointSetInLineRight.Add(currentTile.conerRD);
            }
            myStatistics.TrendLine currentTrendLineLeft = new myStatistics.TrendLine(pointSetInLineLeft, true);
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

        #endregion 水平離差

        #endregion 林國良法（磁磚角離差）

        //string output = myReport.result(TilesType.Rectangle);
        //myReport.doToReport("評分報告");
        myReport.SaveReport(rankArea);
        return myReport.ScoringByVariance(TilesType.Rectangle);
        //return myReport.SaveReport(rankTopOnly);

        #endregion 評分
    }

    /// <summary>將1D的磁磚陣列轉為2D陣列</summary>
    public static MCvBox2D[,] TileArrayToTile2DArray(Point theGridRD, Point theGridLT, List<MCvBox2D> BaseMcvBox2DList)
    {
        #region 定位磁磚

        RectangleGrids myGrid = new RectangleGrids(theGridRD, theGridLT);
        MCvBox2D[,] tiles = new MCvBox2D[RectangleGrids.columnCount, RectangleGrids.rowCount];

        bool[,] PositionOccupied = new bool[RectangleGrids.columnCount, RectangleGrids.rowCount];
        for (int columnIndex = 0; columnIndex < RectangleGrids.columnCount; columnIndex++)
        {
            for (int rowIndex = 0; rowIndex < RectangleGrids.rowCount; rowIndex++)
            {
                PositionOccupied[columnIndex, rowIndex] = false;
            }
        }

        double halfHorizontalSpacing = myGrid.get_GridSpacingHorizontalInPixel / 2;

        for (int index = 0; index < BaseMcvBox2DList.Count; index++)
        {
            int columnOfBox = (int)Math.Round((BaseMcvBox2DList[index].center.X - myGrid.GridsLT.X) / halfHorizontalSpacing) - 1;

            int rowOfBox = (int)((BaseMcvBox2DList[index].center.Y - myGrid.GridsLT.Y) / myGrid.get_GridSpacingVerticalInPixel);

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