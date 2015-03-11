using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV.Structure;

class RectangleTiles
{
    /// <summary>代表整組磁磚</summary>
    public MCvBox2D[,] Tiles;

    /// <summary>錯誤訊息</summary>
    public int msg;

    public RectangleGrids myGrid;



    public RectangleTiles(Point theGridRD, Point theGridLT, List<MCvBox2D> BaseMcvBox2DList, string fileName,bool rankTopOnly)
    {
        myGrid = new RectangleGrids(theGridRD, theGridLT);
        Tiles = new MCvBox2D[RectangleGrids.columnCount, RectangleGrids.rowCount];

        bool[,] PositionOccupied = new bool[RectangleGrids.columnCount, RectangleGrids.rowCount];
        for (int columnIndex = 0; columnIndex < RectangleGrids.columnCount; columnIndex++)
        {
            for (int rowIndex = 0; rowIndex < RectangleGrids.rowCount; rowIndex++)
            {
                PositionOccupied[columnIndex, rowIndex] = false;
            }
        }

        #region 定位磁磚
        double halfHorizontalSpacing = myGrid.get_GridSpacingHorizontalInPixel / 2;

        for (int index = 0; index < BaseMcvBox2DList.Count; index++)
        {
            int columnOfBox = (int)Math.Round((BaseMcvBox2DList[index].center.X - myGrid.get_GridsLT.X) / halfHorizontalSpacing) - 1;

            int rowOfBox = (int)((BaseMcvBox2DList[index].center.Y - myGrid.get_GridsLT.Y) / myGrid.get_GridSpacingVerticalInPixel);

            if (PositionOccupied[columnOfBox, rowOfBox] == false)
            {
                Tiles[columnOfBox, rowOfBox] = BaseMcvBox2DList[index];
                PositionOccupied[columnOfBox, rowOfBox] = true;
            }
            else
            {
                Console.WriteLine("tile" + index + "位置與其他tile重複");
                msg = error_TilePosition;
            }
        }
        #endregion

        //評分
        rankRectangleTiles(fileName, Tiles, myGrid,rankTopOnly);
        

    }

    public static string rankRectangleTiles(string fileName, MCvBox2D[,] Tiles, RectangleGrids myGrid,bool rankTopOnly)
    {
        #region 評分

        report myReport = new report(fileName);
        int rowCount = rankTopOnly ? RectangleGrids.rowCountHalf : RectangleGrids.rowCount;
        #region 評分_絕對位置
        myReport.newLine("絕對位置：");
        double absoluteError = 0.0;
        for (int row = 0; row < rowCount; row++)
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

        #endregion

        #region 評分_溝縫
        myReport.newLine("");
        myReport.newLine("溝縫間隔：");
        #region 列與列間溝縫
        myReport.newLine("列與列間溝縫：");
        List<double> allRowError = new List<double>();
        double helfTileWidth = myGrid.PixelFormMm(RectangleGrids.tileWidth) / 2;
        
        #region 考慮第0條至第6條(共7條)水平溝縫，將溝縫視為上鄰接列的附屬，所以最後一列沒有溝縫
        for (int rowNum = 0; rowNum < rowCount -1; rowNum++)
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
                        #endregion
                        #region 溝縫差加上：左下磁磚右上點 與 本磁磚中心點與下邊交點 的y方向差
                        double disB = myGrid.mmFormPixel(myMath.GetDis<double>(
                                new Tile(Tiles[columnNum - 1, rowNum + 1]).conerRT.Y - helfTileWidth
                                    - Tiles[columnNum, rowNum].center.Y));
                        rowError.Add(disB);
                        allRowError.Add(disB);
                        #endregion
                    }
                    #endregion

                    #region 如果相對於本磁磚的右下磁磚存在的話
                    if (columnNum + 1 < RectangleGrids.columnCount)
                    {
                        #region 溝縫差加上：右下磁磚左上點 與 本磁磚中心點與下邊交點 的y方向差
                        double disC = myGrid.mmFormPixel(myMath.GetDis<double>(
                                new Tile(Tiles[columnNum + 1, rowNum + 1]).conerLT.Y - helfTileWidth
                                    - Tiles[columnNum, rowNum].center.Y));
                        rowError.Add(disC);
                        allRowError.Add(disC);
                        #endregion

                        #region 溝縫差加上：右下磁磚中心點與上邊交點 與 本磁磚右下點 的y方向差
                        double disD = myGrid.mmFormPixel(myMath.GetDis<double>(
                                Tiles[columnNum + 1, rowNum + 1].center.Y - helfTileWidth
                                    - new Tile(Tiles[columnNum, rowNum]).conerRD.Y));
                        rowError.Add(disD);
                        allRowError.Add(disD);
                        #endregion
                    }
                    #endregion
                    #endregion
                }
                #endregion
            }
            #endregion

            foreach (double err in rowError)
            {
                rowReport += "\t" + err;
            }
            rowReport += "\t平均：\t" + rowError.Average() + "\t標準差：\t" + rowError.ToArray().StandardDeviation();
            myReport.newLine(rowReport);
        }
        myReport.GapHSD = allRowError.ToArray().StandardDeviation();

        #endregion
        #endregion

        #region 行與行間溝縫
        myReport.newLine("行與行間溝縫：");
        List<double> allColumnError = new List<double>();
        for (int columnNum = 0; columnNum < RectangleGrids.gapCountInColumn; columnNum++)
        {
            string columnReport = "第" + columnNum.ToString("D2") + "條：";
            double[] columnError = new double[rowCount];

            for (int rowNum = 0; rowNum < rowCount; rowNum++)
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
                }
            }
            columnReport += "\t平均：\t" + columnError.Average() + "\t標準差\t" + columnError.StandardDeviation();
            myReport.newLine(columnReport);
        }
        myReport.GapVSD = allColumnError.ToArray().StandardDeviation();
        #endregion



        #endregion

        #region 評分_磁磚角趨勢線
        myReport.newLine("");
        myReport.newLine("磁磚角趨勢線");
        #region 水平趨勢線
        myReport.newLine("水平趨勢線");
        for (int rowNum = 0; rowNum < rowCount; rowNum++)
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

        #endregion

        #region 垂直趨勢線
        myReport.newLine("垂直趨勢線");
        for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
        {
            double[] tempLeftXs = new double[rowCount];
            double[] tempLeftYs = new double[rowCount];
            string VerticalTrendLineLeftX = "";
            string VerticalTrendLineLeftY = "";

            double[] tempRightXs = new double[rowCount];
            double[] tempRightYs = new double[rowCount];
            string VerticalTrendLineRightX = "";
            string VerticalTrendLineRightY = "";

            for (int rowNum = 0; rowNum < rowCount / 2 ; rowNum++)
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

        #endregion

        #endregion

        #region 評分_磁磚中心趨勢線
        myReport.newLine("");
        myReport.newLine("磁磚中心趨勢線");
        #region 水平趨勢線
        myReport.newLine("水平趨勢線");
        List<double> HorizontalTrendLineaAngle = new List<double>();
        for (int rowNum = 0; rowNum < rowCount; rowNum++)
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
            HorizontalTrendLineaAngle.Add(angle);
            myReport.newLine("Xdata:\t" + HorizontalTrendLineX);
            myReport.newLine("Ydata:\t" + HorizontalTrendLineY);
        }
        double rowAngleSD = HorizontalTrendLineaAngle.ToArray().StandardDeviation();
        myReport.RowAngleSD = rowAngleSD;
        myReport.newLine("水平趨勢線夾角標準差" + rowAngleSD);

        #endregion

        #region 垂直趨勢線
        myReport.newLine("垂直趨勢線");
        List<double> VerticalTrendLineAngle = new List<double>();
        for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
        {
            double[] Xdata = new double[rowCount/2];
            double[] Ydata = new double[rowCount / 2];
            string VerticalTrendLineX = "";
            string VerticalTrendLineY = "";
            for (int rowNum = 0; rowNum < rowCount / 2; rowNum++)
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
            myReport.newLine("角度(度數)：\t" + myTool.CorrectingAngle_YXtoXY(TrendLine.Angle()));
            myReport.newLine("Xdata:\t" + VerticalTrendLineX);
            myReport.newLine("Ydata:\t" + VerticalTrendLineY);
        }
        double columnAngleSD = VerticalTrendLineAngle.ToArray().StandardDeviation();
        myReport.ColumnAngleSD = columnAngleSD;
        myReport.newLine("垂直趨勢線夾角標準差" + columnAngleSD);

        #endregion

        #endregion

        #region 評分_座標標準差
        myReport.newLine("");
        myReport.newLine("座標標準差");
        #region 列中磁磚的Y方向
        myReport.newLine("列中磁磚的Y方向");
        double[] RowY = new double[rowCount];
        double[] RowSpacing = new double[rowCount - 1];

        for (int rowNum = 0; rowNum < rowCount; rowNum++)
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
        for (int rowNum = 0; rowNum < rowCount - 1; rowNum++)
        {
            RowSpacing[rowNum] = RowY[rowNum + 1] - RowY[rowNum];
            RowSpacingReport += "\t" + RowSpacing[rowNum];
        }
        myReport.newLine(RowSpacingReport
            + "\t平均\t" + RowSpacing.Average()
            + "\t標準差\t" + myStatistics.StandardDeviation(RowSpacing));
        #endregion

        #region 行中磁磚的X方向
        myReport.newLine("行中磁磚的X方向");
        double[] ColumnX = new double[RectangleGrids.columnCount];
        double[] ColumnSpacing = new double[RectangleGrids.columnCount - 1];

        for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
        {
            double[] TileXC = new double[rowCount / 2];
            double[] TileXE = new double[rowCount / 2];
            string TileXReport = "第" + columnNum.ToString("D2") + "行：";
            for (int rowNum = 0; rowNum < rowCount / 2; rowNum++)
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
            for (int rowNum = 0; rowNum < rowCount / 2; rowNum++)
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



        #endregion

        #endregion

        #region 評分_磁磚中心與趨勢線距離和

        myReport.newLine("筆直度:");
        #region 平均Y方向離差
        myReport.newLine("Y方向離差:");
        double sumOfTileCenterToTrendLineH = 0;
        for (int rowNum = 0; rowNum < rowCount; rowNum++)
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
        int numberOfTile = rankTopOnly ? RectangleGrids.tileCount /2 : RectangleGrids.tileCount;
        double rowResidualAvg = sumOfTileCenterToTrendLineH / numberOfTile;
        string TotalRowReport = "平均y方向離差:" + rowResidualAvg.ToString("0.000");
        myReport.newLine(TotalRowReport);
        myReport.RowResidualAvg = rowResidualAvg;
        #endregion

        #region 平均X方向離差
        myReport.newLine("平均X方向離差");
        double sumOfTileCenterToTrendLineV = 0;
        for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
        {
            string ColumnReport = "第" + columnNum + "行";
            double[] Xdata = new double[rowCount / 2];
            double[] Ydata = new double[rowCount / 2];
            for (int rowNum = 0; rowNum < rowCount / 2; rowNum++)
            {
                Xdata[rowNum] = Tiles[columnNum, rowNum * 2 + (columnNum % 2)].center.X;
                Ydata[rowNum] = Tiles[columnNum, rowNum * 2 + (columnNum % 2)].center.Y * -1;
            }
            myStatistics.TrendLine theTrendLine = new myStatistics.TrendLine(Ydata, Xdata);
            for (int rowNum = 0; rowNum < rowCount / 2; rowNum++)
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
        
        #endregion

        #endregion

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
        for (int rowNum = 0; rowNum < rowCount; rowNum++)
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
        #endregion
        #region 統計每行
        string angleAvgInColumn = "平均";
        string angleSDInColumn = "標準差";
        for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
        {
            double[] angleData = new double[rowCount / 2];
            for (int rowNum = 0; rowNum < rowCount / 2; rowNum++)
            {
                angleData[rowNum] = Tiles[columnNum, rowNum * 2 + (columnNum % 2)].angle;
            }
            angleAvgInColumn += "\t" + angleData.Average();
            angleSDInColumn += "\t" + myStatistics.StandardDeviation(angleData);
        }
        myReport.newLine(angleAvgInColumn);
        myReport.newLine(angleSDInColumn);
        #endregion

        #region 統計整個

        double[] wholeAngleData = new double[rowCount * RectangleGrids.columnCount];
        for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
        {
            for (int row = 0; row < rowCount; row++)
            {
                if ((row + columnNum) % 2 == 0)
                {
                    wholeAngleData[row * RectangleGrids.columnCount + columnNum] = Tiles[columnNum, row].angle;
                }
            }
        }
        #endregion
        myReport.newLine("總平均\t" + wholeAngleData.Average());

        myReport.AngleSD = myStatistics.StandardDeviation(wholeAngleData);
        myReport.newLine("總標準差\t" + myReport.AngleSD);
        #endregion

        string output = myReport.result(TilesType.Rectangle);
        //myReport.doToReport("評分報告");
        myReport.SaveReport(rankTopOnly);
        return output;
        #endregion
    }

    #region 錯誤訊息
    public const int safe = 0;
    public const int error_TilePosition = 1;
    #endregion

}