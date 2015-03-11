using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV.Structure;

class SquareTiles
{
    /// <summary>代表整組磁磚</summary>
    public MCvBox2D[,] Tiles;

    /// <summary>錯誤訊息</summary>
    public int msg;

    public SquareGrids myGrid;



    public SquareTiles(Point theGridRD, Point theGridLT, List<MCvBox2D> BaseMcvBox2DList, string fileName, bool rankTopOnly)
    {
        myGrid = new SquareGrids(theGridRD, theGridLT);
        Tiles = new MCvBox2D[SquareGrids.columnCount, SquareGrids.rowCount];

        bool[,] PositionOccupied = new bool[SquareGrids.rowCount, SquareGrids.columnCount];
        for (int rowIndex = 0; rowIndex < SquareGrids.rowCount; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < SquareGrids.columnCount; columnIndex++)
            {
                PositionOccupied[columnIndex, rowIndex] = false;
            }
        }

        #region 定位磁磚
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
        double avgGridSpacingH_Pixel = (RightDown.X - LeftTop.X) / (SquareGrids.columnCount - 1);
        double avgGridSpacingV_Pixel = (RightDown.Y - LeftTop.Y) / (SquareGrids.rowCount - 1);

        for (int index = 0; index < BaseMcvBox2DList.Count; index++)
        {
            //int columnOfBox = (int)((BaseMcvBox2DList[index].center.X - myGrid.get_GridsLT.X) / myGrid.get_gridSpacingInPixel);
            int columnOfBox = (int)((BaseMcvBox2DList[index].center.X - myGrid.get_GridsLT.X) / avgGridSpacingH_Pixel);

            //int rowOfBox = (int)((BaseMcvBox2DList[index].center.Y - myGrid.get_GridsLT.Y) / myGrid.get_gridSpacingInPixel);
            int rowOfBox = (int)((BaseMcvBox2DList[index].center.Y - myGrid.get_GridsLT.Y) / avgGridSpacingV_Pixel);

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

        //進行評分
        RankSquareTile(fileName, Tiles, myGrid, rankTopOnly);

    }

    public static string RankSquareTile(string fileName, MCvBox2D[,] Tiles, SquareGrids myGrid, bool rankTopOnly)
    {
        #region 評分
        report myReport = new report(fileName);
        int rowCount = rankTopOnly ? SquareGrids.rowCountHalf : SquareGrids.rowCount;
        #region 評分_絕對位置
        myReport.newLine("絕對位置：");
        double absoluteError = 0.0;
        for (int row = 0; row < rowCount; row++)
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

        #endregion

        #region 評分_溝縫

        myReport.newLine("");
        myReport.newLine("溝縫間隔：");
        #region 列與列間溝縫
        myReport.newLine("列與列間溝縫：");
        List<double> allRowError = new List<double>();
        for (int rowNum = 0; rowNum < rowCount -1; rowNum++)
        {
            string rowReport = "第" + rowNum.ToString("D2") + "條：";
            double[] rowError = new double[SquareGrids.columnCount * 2];
            for (int columnNum = 0; columnNum < SquareGrids.columnCount; columnNum++)
            {
                Tile tempTileUp = new Tile(Tiles[columnNum, rowNum]);
                Tile tempTileDown = new Tile(Tiles[columnNum, rowNum + 1]);
                rowError[columnNum * 2] = myGrid.mmFormPixel(myMath.GetDis<float>(tempTileDown.conerLT.Y - tempTileUp.conerLD.Y));
                rowError[columnNum * 2 + 1] = myGrid.mmFormPixel(myMath.GetDis<float>(tempTileDown.conerRT.Y - tempTileUp.conerRD.Y));
                rowReport += "\t" + rowError[columnNum * 2] + "\t" + rowError[columnNum * 2 + 1];
                allRowError.Add(rowError[columnNum * 2]);
                allRowError.Add(rowError[columnNum * 2 + 1]);
            }
            rowReport += "\t平均：\t" + rowError.Average() + "\t標準差：\t" + rowError.StandardDeviation();
            myReport.newLine(rowReport);
        }
        myReport.GapHSD = allRowError.ToArray().StandardDeviation();
        
        #endregion
        #region 行與行間溝縫
        myReport.newLine("行與行間溝縫：");
        List<double> allColumnError = new List<double>();
        for (int columnNum = 0; columnNum < SquareGrids.columnCount - 1; columnNum++)
        {
            string columnReport = "第" + columnNum.ToString("D2") + "條：";
            double[] columnError = new double[rowCount * 2];

            for (int rowNum = 0; rowNum < rowCount; rowNum++)
            {
                Tile tempTileLeft = new Tile(Tiles[columnNum, rowNum]);
                Tile tempTileRight = new Tile(Tiles[columnNum + 1, rowNum]);
                columnError[rowNum * 2] = myGrid.mmFormPixel(myMath.GetDis<float>(tempTileRight.conerLT.X - tempTileLeft.conerRT.X));
                columnError[rowNum * 2 + 1] = myGrid.mmFormPixel(myMath.GetDis<float>(tempTileRight.conerLD.X - tempTileLeft.conerRD.X));
                columnReport += "\t" + columnError[rowNum * 2] + "\t" + columnError[rowNum * 2 + 1];
                allColumnError.Add(columnError[rowNum * 2]);
                allColumnError.Add(columnError[rowNum * 2 + 1]);
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

        #endregion

        #region 垂直趨勢線
        myReport.newLine("垂直趨勢線");
        for (int columnNum = 0; columnNum < SquareGrids.columnCount; columnNum++)
        {
            double[] tempTopXs = new double[rowCount * 2];
            double[] tempTopYs = new double[rowCount * 2];
            string VerticalTrendLineX = "";
            string VerticalTrendLineY = "";
            for (int rowNum = 0; rowNum < rowCount; rowNum++)
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


            double[] tempDownXs = new double[rowCount * 2];
            double[] tempDownYs = new double[rowCount * 2];
            VerticalTrendLineX = "";
            VerticalTrendLineY = "";
            for (int rowNum = 0; rowNum < rowCount; rowNum++)
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

        #endregion

        #endregion

        #region 評分_磁磚中心趨勢線
        myReport.newLine("");
        myReport.newLine("磁磚中心趨勢線夾角");
        #region 水平趨勢線
        myReport.newLine("水平趨勢線夾角");
        List<double> HorizontalTrendLineaAngle = new List<double>();
        for (int rowNum = 0; rowNum < rowCount; rowNum++)
        {
            double[] Xdata = new double[SquareGrids.columnCount];
            double[] Ydata = new double[SquareGrids.columnCount];
            string HorizontalTrendLineX = "";
            string HorizontalTrendLineY = "";
            for (int columnNum = 0; columnNum < SquareGrids.columnCount; columnNum++)
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

        #endregion

        #region 垂直趨勢線
        myReport.newLine("垂直趨勢線");
        List<double> VerticalTrendLineAngle = new List<double>();
        for (int columnNum = 0; columnNum < SquareGrids.columnCount; columnNum++)
        {
            double[] Xdata = new double[rowCount];
            double[] Ydata = new double[rowCount];
            string VerticalTrendLineX = "";
            string VerticalTrendLineY = "";
            for (int rowNum = 0; rowNum < rowCount; rowNum++)
            {
                Xdata[rowNum] = Tiles[columnNum, rowNum].center.X;
                VerticalTrendLineX += "\t" + Xdata[rowNum];
                Ydata[rowNum] = Tiles[columnNum, rowNum].center.Y * -1;
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
            double[] TileYC = new double[SquareGrids.columnCount];
            double[] TileYE = new double[SquareGrids.columnCount];
            string TileYReport = "第" + rowNum.ToString("D2") + "列：";
            for (int columnNum = 0; columnNum < SquareGrids.columnCount; columnNum++)
            {
                TileYC[columnNum] = myGrid.mmFormPixel(Tiles[columnNum, rowNum].center.Y);
                TileYReport += "\t" + TileYC[columnNum];
            }

            double TileYC_avg = TileYC.Average();
            TileYReport +=
                "\t平均：\t" + TileYC_avg
                + "\t標準差\t" + TileYC.StandardDeviation() +"\t差：";

            for (int columnNum = 0; columnNum < SquareGrids.columnCount; columnNum++)
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
        double[] ColumnX = new double[SquareGrids.columnCount];
        double[] ColumnSpacing = new double[SquareGrids.columnCount - 1];

        for (int columnNum = 0; columnNum < SquareGrids.columnCount; columnNum++)
        {
            double[] TileXC = new double[rowCount];
            double[] TileXE = new double[rowCount];
            string TileXReport = "第" + columnNum.ToString("D2") + "行：";
            for (int rowNum = 0; rowNum < rowCount; rowNum++)
            {
                TileXC[rowNum] = myGrid.mmFormPixel(Tiles[columnNum, rowNum].center.X);
                TileXReport += "\t" + TileXC[rowNum];
            }
            double TileXC_avg = TileXC.Average();
            TileXReport +=
                "\t平均：\t" + TileXC.Average()
                + "\t標準差\t" + TileXC.StandardDeviation() + "\t差：";
            for (int rowNum = 0; rowNum < rowCount; rowNum++)
            {
                TileXE[rowNum] = TileXC[rowNum] - TileXC_avg;
                TileXReport += "\t" + TileXE[rowNum];
            }
            myReport.newLine(TileXReport);


            ColumnX[columnNum] = TileXC.Average();
        }

        myReport.newLine("行與行間隔：");
        string ColumnSpacingReport = "\t";
        for (int columnNum = 0; columnNum < SquareGrids.columnCount - 1; columnNum++)
        {
            ColumnSpacing[columnNum] = ColumnX[columnNum + 1] - ColumnX[columnNum];
            ColumnSpacingReport += "\t" + ColumnSpacing[columnNum];
        }
        myReport.newLine(ColumnSpacingReport
            + "\t平均\t" + ColumnSpacing.Average()
            + "\t標準差\t" + myStatistics.StandardDeviation(ColumnSpacing));



        #endregion

        #endregion
        myReport.newLine("筆直度:");
        #region 平均Y方向離差
        myReport.newLine("Y方向離差:");
        double sumOfTileCenterToTrendLineH = 0;
        for (int rowNum = 0; rowNum < rowCount; rowNum++)
        {
            string rowReport = "第" + rowNum + "列";
            double[] Xdata = new double[SquareGrids.columnCount];
            double[] Ydata = new double[SquareGrids.columnCount];
            for (int column = 0; column < SquareGrids.columnCount; column++)
            {
                Xdata[column] = Tiles[column, rowNum].center.X;
                Ydata[column] = Tiles[column, rowNum].center.Y * -1;
            }
            myStatistics.TrendLine theTrendLine = new myStatistics.TrendLine(Xdata, Ydata);
            for (int column = 0; column < SquareGrids.columnCount; column++)
            {
                double Residual = Math.Abs(theTrendLine.PointToLineDis(Xdata[column], Ydata[column]));
                Residual = myGrid.mmFormPixel(Residual);
                sumOfTileCenterToTrendLineH += Residual;
                rowReport += "\t" + Residual;
            }
            myReport.newLine(rowReport);
        }
        double rowResidualAvg = sumOfTileCenterToTrendLineH / (rowCount * SquareGrids.columnCount);
        string TotalRowReport = "平均Y方向離差:" + rowResidualAvg.ToString("0.000") ;

        myReport.newLine(TotalRowReport);
        myReport.RowResidualAvg = rowResidualAvg;
        #endregion

        #region 平均X方向離差
        myReport.newLine("X方向離差:");
        double sumOfTileCenterToTrendLineV = 0;
        for (int column = 0; column < SquareGrids.columnCount; column++)
        {
            string ColumnReport = "第" + column + "行";
            double[] Xdata = new double[rowCount];
            double[] Ydata = new double[rowCount];
            for (int row = 0; row < rowCount; row++)
            {
                Xdata[row] = Tiles[column, row].center.X;
                Ydata[row] = Tiles[column, row].center.Y * -1;
            }
            myStatistics.TrendLine theTrendLine = new myStatistics.TrendLine(Ydata, Xdata);
            for (int row = 0; row < rowCount; row++)
            {
                double Residual =Math.Abs(theTrendLine.PointToLineDis(Ydata[row], Xdata[row]));
                Residual = myGrid.mmFormPixel(Residual);
                sumOfTileCenterToTrendLineV += Residual;
                ColumnReport += "\t" + Residual;
            }
            myReport.newLine(ColumnReport);
        }
        double columnResidualAvg = sumOfTileCenterToTrendLineV / (rowCount * SquareGrids.columnCount);
        string TotalColumnReport = "平均X方向離差:" + columnResidualAvg.ToString("0.000");

        myReport.newLine(TotalColumnReport);
        myReport.ColumnResidualAvg = columnResidualAvg;
        #endregion

        #region 評分_磁磚旋轉角
        myReport.newLine("");
        myReport.newLine("磁磚旋轉角");

        #region 印出每列
        for (int rowNum = 0; rowNum < rowCount; rowNum++)
        {
            string angleReport = "第" + rowNum.ToString("D2") + "列";
            double[] angleDataInRow = new double[SquareGrids.columnCount];
            for (int columnNum = 0; columnNum < SquareGrids.columnCount; columnNum++)
            {
                angleDataInRow[columnNum] = Tiles[columnNum, rowNum].angle;
                angleReport += "\t" + angleDataInRow[columnNum];
            }
            myReport.newLine(angleReport
                + "\t平均：\t" + angleDataInRow.Average()
                + "\t標準差：\t" + myStatistics.StandardDeviation(angleDataInRow));
        }
        #endregion
        #region 統計每行
        string angleAvgInColumn = "平均：";
        string angleSDInColumn = "標準差：";
        for (int columnNum = 0; columnNum < SquareGrids.columnCount; columnNum++)
        {
            double[] angleData = new double[rowCount];
            for (int rowNum = 0; rowNum < rowCount; rowNum++)
            {
                angleData[rowNum] = Tiles[columnNum, rowNum].angle;
            }
            angleAvgInColumn += "\t" + angleData.Average();
            angleSDInColumn += "\t" + myStatistics.StandardDeviation(angleData);
        }
        myReport.newLine(angleAvgInColumn);
        myReport.newLine(angleSDInColumn);
        #endregion

        #region 統計整個

        double[] wholeAngleData = new double[rowCount * SquareGrids.columnCount];
        for (int columnNum = 0; columnNum < SquareGrids.columnCount; columnNum++)
        {
            for (int row = 0; row < rowCount; row++)
            {
                wholeAngleData[row * SquareGrids.columnCount + columnNum] = Tiles[columnNum, row].angle;
            }
        }
        #endregion
        myReport.newLine("總平均：\t" + wholeAngleData.Average());
        double AngleSD = myStatistics.StandardDeviation(wholeAngleData);
        myReport.newLine("總標準差：\t" + AngleSD);
        myReport.AngleSD = AngleSD;
        #endregion

        string output = myReport.result(TilesType.Square);
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