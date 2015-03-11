using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace 磁磚辨識評分
{
    public partial class SampleGen : Form
    {
        /// <summary>背景圖，用以顯示</summary>
        Image<Bgr, Byte> DrawZone;
        /// <summary>1mm長用多少pixel表示</summary>
        const double pixelpermm = 10;
        /// <summary>產生的磁磚</summary>
        MCvBox2D[,] genTiles;
        /// <summary>產生磁磚的類型</summary>
        TilesType genTilesType;

        static SizeF SquareTileSize = new SizeF(470f, 470f);
        static SizeF RectangleTileSize = new SizeF((float)RectangleGrids.tileLength * 10f, (float)RectangleGrids.tileWidth * 10f);
        static Size RectangleDrawZone = new Size(11200, 5000);

        static Image<Bgr, byte> RTile = new Image<Bgr, byte>("RectangularTileSample.bmp").Resize((int)RectangleTileSize.Width, (int)RectangleTileSize.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
        static Image<Bgr, byte> SWTile = new Image<Bgr, byte>("SquareWhileTileSample.bmp").Resize(470, 470, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
        static Image<Bgr, byte> SBTile = new Image<Bgr, byte>("SquareBeigeTileSample.bmp").Resize(470, 470, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
        //static Image<Bgr, byte> RTileSample = new Image<Bgr, byte>("RectangularTileSample.bmp")

        static Bgr DrawSolidColor = new Bgr(Color.White);
        static Bgr DrawEdgeColor = new Bgr(Color.Red);

        /// <summary>只評分上半</summary>
        bool RankTopOnly = true;

        public SampleGen()
        {
            InitializeComponent();
        }

        private void btnGen_Click(object sender, EventArgs e)
        {
            genSample(genTilesType);
            showSample(genTilesType);
            RankSample(genTilesType);
        }

        private void RankSample(TilesType theTilesType)
        {
            if (theTilesType == TilesType.Square)
            {
                Tile LT = new Tile(genTiles[0, 0]);
                Point RDPoint = LT.conerLT.ToPoint();
                RDPoint.Offset((int)(50 * 12 * pixelpermm), (int)(50 * 12 * pixelpermm));
                SquareGrids myGrid = new SquareGrids(RDPoint, LT.conerLT.ToPoint());
                toolStripStatusLabel1.Text = SquareTiles.RankSquareTile(Directory.GetCurrentDirectory() + "\\genSquareTile.jpg", genTiles, myGrid,RankTopOnly);
            }
            else
            {
                Tile LT = new Tile(genTiles[0, 0]);
                Point RDPoint = LT.conerLT.ToPoint();
                RDPoint.Offset((int)(RectangleGrids.Length*pixelpermm), (int)(RectangleGrids.Width*pixelpermm));
                RectangleGrids myGrid = new RectangleGrids(RDPoint, LT.conerLT.ToPoint());
                toolStripStatusLabel1.Text = RectangleTiles.rankRectangleTiles(Directory.GetCurrentDirectory() + "\\genRecTile", genTiles, myGrid,RankTopOnly);
                
            }
        }

        /// <summary>產生樣本</summary>
        private void genSample(TilesType theTilesType)
        {
            Random rd = new Random();
            switch (theTilesType)
            {
                case TilesType.Square:
                    /*
                     * 正方型磁磚為47mm，溝縫為3mm
                     * 故1磁磚需要50mm*50mm
                     * 邊緣各保留一磁磚寬
                     * 顧14*14 = 700mm*700mm
                     * 像素即為7000*7000
                     */
                    DrawZone = new Image<Bgr, byte>(7000, 7000, new Bgr(Color.DarkBlue));


                    //依據行列的間距，產生磁磚
                    genColumnRowDis(theTilesType);

                    //將整組磁磚移回中心
                    moveTilesToCenter(theTilesType);

                    //依據行列的角度，移動磁磚
                    genColumnRowAngle(theTilesType);

                    //依據設定的誤差，挑整磁磚到趨勢線的距離
                    genTileLineErr(theTilesType);

                    //設定磁磚的起始角度
                    setTileBaseAngle(theTilesType);

                    //設定磁磚角度
                    setTileAngle(theTilesType);

                    break;
                case TilesType.Rectangle:
                    /*
                     * 長方型磁磚為240*51mm，溝縫為4.7mm
                     * 故1磁磚需要244.7mm*55.7mm
                     * 上下保留一磁磚寬，左右保留0.2磁磚
                     * 左右寬 244.7*4.2 = 1027.74 上下 55.7*9 = 501.3
                     * 取像素10200*5000
                     */
                    DrawZone = new Image<Bgr, byte>(RectangleDrawZone.Width, RectangleDrawZone.Height, new Bgr(Color.DarkBlue));

                    //依據行列的間距，產生磁磚
                    genColumnRowDis(theTilesType);

                    //將整組磁磚移回中心
                    moveTilesToCenter(theTilesType);

                    //依據行列的角度，移動磁磚
                    genColumnRowAngle(theTilesType);

                    //依據設定的誤差，挑整磁磚到趨勢線的距離
                    genTileLineErr(theTilesType);

                    //設定磁磚的起始角度
                    setTileBaseAngle(theTilesType);

                    //設定磁磚角度
                    setTileAngle(theTilesType);

                    break;
                default:
                    break;
            }



        }

        /// <summary>產生行列間距位置</summary>
        private void genColumnRowDis(TilesType theTilesType)
        {

            PointF TileCenterPosition = new PointF(0f, 0f);

            float[] rowErr;
            float[] columnErr;
            switch (theTilesType)
            {
                case TilesType.Square:
                    #region 產生每一行、每一列的間距
                    rowErr = new float[SquareGrids.rowCount];
                    columnErr = new float[SquareGrids.columnCount];


                    for (int index = 0; index < rowErr.Length; index++)
                    {
                        rowErr[index] = 47f * (float)pixelpermm + (float)nudRowDis.Value * (float)pixelpermm
                            + (float)myMath.randNumber((double)nudRowErr.Value * -1, (double)nudRowErr.Value) * (float)pixelpermm;
                    }
                    for (int index = 0; index < columnErr.Length; index++)
                    {
                        columnErr[index] = 47f * (float)pixelpermm + (float)nudCoulumnDis.Value * (float)pixelpermm
                            + (float)myMath.randNumber((double)nudCoulumnErr.Value * -1, (double)nudCoulumnErr.Value) * (float)pixelpermm;
                    }
                    #endregion

                    #region 依據間距產生磁磚
                    genTiles = new MCvBox2D[12, 12];

                    for (int column = 0; column < SquareGrids.columnCount; column++)
                    {
                        TileCenterPosition.Y = 0f;
                        TileCenterPosition.X += columnErr[column];

                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            TileCenterPosition.Y += rowErr[row];

                            genTiles[column, row] = new MCvBox2D(TileCenterPosition, SquareTileSize, 0f);

                        }
                    }
                    #endregion
                    break;
                case TilesType.Rectangle:
                    #region 產生每一行、每一列的間距
                    rowErr = new float[RectangleGrids.rowCount];
                    columnErr = new float[RectangleGrids.columnCount];

                    for (int index = 0; index < rowErr.Length; index++)
                    {
                        rowErr[index] = ((float)RectangleGrids.tileWidth
                            + (float)nudRowDis.Value
                            + (float)myMath.randNumber((double)nudRowErr.Value * -1, (double)nudRowErr.Value)) * (float)pixelpermm;
                    }
                    for (int index = 0; index < columnErr.Length; index++)
                    {
                        columnErr[index] = ((float)RectangleGrids.tileLength / 2f
                            + (float)nudCoulumnDis.Value / 2f
                            + (float)myMath.randNumber((double)nudCoulumnErr.Value * -1, (double)nudCoulumnErr.Value)) * (float)pixelpermm;
                    }
                    #endregion

                    #region 依據間距產生磁磚
                    genTiles = new MCvBox2D[RectangleGrids.columnCount, RectangleGrids.rowCount];

                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        TileCenterPosition.Y = 0f;
                        TileCenterPosition.X += columnErr[column];

                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            TileCenterPosition.Y += rowErr[row];

                            genTiles[column, row] = new MCvBox2D(TileCenterPosition, RectangleTileSize, 0f);

                        }
                    }
                    #endregion
                    break;
                default:
                    break;
            }

        }

        /// <summary>產生行列的角度</summary>
        private void genColumnRowAngle(TilesType theTilesType)
        {
            float[] rowAngle;
            float[] columnAngle;
            float[] avgColumn;
            float[] avgRow;
            switch (theTilesType)
            {
                case TilesType.Square:

                    #region 產生每一行、每一列的角度
                    rowAngle = new float[SquareGrids.rowCount];
                    columnAngle = new float[SquareGrids.columnCount];

                    for (int index = 0; index < rowAngle.Length; index++)
                    {
                        rowAngle[index] = (float)nudRowAngle.Value
                            + (float)myMath.randNumber((double)nudRowAngleErr.Value * -1, (double)nudRowAngleErr.Value);
                    }
                    for (int index = 0; index < columnAngle.Length; index++)
                    {
                        columnAngle[index] = (float)nudCoulumnAngle.Value
                            + (float)myMath.randNumber((double)nudCoulumnAngleErr.Value * -1, (double)nudCoulumnAngleErr.Value);
                    }
                    #endregion

                    #region 依據行列角度移動磁磚

                    #region 取得每行每列的平均座標(中心線的X,Y值)
                    avgColumn = new float[SquareGrids.columnCount];
                    avgRow = new float[SquareGrids.rowCount];

                    for (int column = 0; column < SquareGrids.columnCount; column++)
                    {
                        avgColumn[column] = 0;
                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            avgColumn[column] += genTiles[column, row].center.X;
                        }
                        avgColumn[column] /= SquareGrids.rowCount;

                    }

                    for (int row = 0; row < SquareGrids.rowCount; row++)
                    {
                        avgRow[row] = 0;
                        for (int column = 0; column < SquareGrids.columnCount; column++)
                        {
                            avgRow[row] += genTiles[column, row].center.Y;
                        }
                        avgRow[row] /= SquareGrids.columnCount;

                    }
                    #endregion

                    for (int column = 0; column < SquareGrids.columnCount; column++)
                    {
                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            #region 產生該行列磁磚該移動的量
                            float tileShiftX = 0;
                            float tileShiftY = 0;
                            float newTileShiftX = 0;
                            float newTileShiftY = 0;

                            do
                            {
                                tileShiftX = newTileShiftX;
                                tileShiftY = newTileShiftY;
                                newTileShiftX = (avgRow[row] - 3500 - tileShiftY) * -1 * (float)Math.Tan(myMath.Deg2Rad(columnAngle[column]));
                                newTileShiftY = (avgColumn[column] - 3500 + newTileShiftX) * (float)Math.Tan(myMath.Deg2Rad(rowAngle[row]));
                            } while (Math.Abs(newTileShiftX - tileShiftX) > 0.5 || Math.Abs(newTileShiftY - tileShiftY) > 0.5);
                            #endregion

                            #region 移動磁磚
                            genTiles[column, row].center.X -= newTileShiftX;
                            genTiles[column, row].center.Y -= newTileShiftY;
                            #endregion
                        }
                    }
                    #endregion

                    break;
                case TilesType.Rectangle:

                    #region 產生每一行、每一列的角度
                    rowAngle = new float[RectangleGrids.rowCount];
                    columnAngle = new float[RectangleGrids.columnCount];

                    for (int index = 0; index < rowAngle.Length; index++)
                    {
                        rowAngle[index] = (float)nudRowAngle.Value
                            + (float)myMath.randNumber((double)nudRowAngleErr.Value * -1, (double)nudRowAngleErr.Value);
                    }
                    for (int index = 0; index < columnAngle.Length; index++)
                    {
                        columnAngle[index] = (float)nudCoulumnAngle.Value
                            + (float)myMath.randNumber((double)nudCoulumnAngleErr.Value * -1, (double)nudCoulumnAngleErr.Value);
                    }
                    #endregion

                    #region 依據行列角度移動磁磚

                    #region 取得每行每列的平均座標(中心線的X,Y值)
                    avgColumn = new float[RectangleGrids.columnCount];
                    avgRow = new float[RectangleGrids.rowCount];

                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        avgColumn[column] = 0;
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            avgColumn[column] += genTiles[column, row].center.X;
                        }
                        avgColumn[column] /= RectangleGrids.rowCount;

                    }

                    for (int row = 0; row < RectangleGrids.rowCount; row++)
                    {
                        avgRow[row] = 0;
                        for (int column = 0; column < RectangleGrids.columnCount; column++)
                        {
                            avgRow[row] += genTiles[column, row].center.Y;
                        }
                        avgRow[row] /= RectangleGrids.columnCount;

                    }
                    #endregion

                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            #region 產生該行列磁磚該移動的量
                            float tileShiftX = 0;
                            float tileShiftY = 0;
                            float newTileShiftX = 0;
                            float newTileShiftY = 0;
                            //12235, 5013
                            do
                            {
                                tileShiftX = newTileShiftX;
                                tileShiftY = newTileShiftY;
                                newTileShiftX = (avgRow[row] - RectangleDrawZone.Height / 2 - tileShiftY) * -1 * (float)Math.Tan(myMath.Deg2Rad(columnAngle[column]));
                                newTileShiftY = (avgColumn[column] - RectangleDrawZone.Width / 2 + newTileShiftX) * (float)Math.Tan(myMath.Deg2Rad(rowAngle[row]));
                            } while (Math.Abs(newTileShiftX - tileShiftX) > 0.5 || Math.Abs(newTileShiftY - tileShiftY) > 0.5);
                            #endregion

                            #region 移動磁磚
                            genTiles[column, row].center.X -= newTileShiftX;
                            genTiles[column, row].center.Y -= newTileShiftY;
                            #endregion
                        }
                    }
                    #endregion
                    break;
                default:
                    break;
            }

        }

        /// <summary>設定磁磚到行列的距離</summary>
        private void genTileLineErr(TilesType theTilesType)
        {
            switch (theTilesType)
            {
                case TilesType.Square:
                    #region 調整一行磁磚當中，x的變量
                    for (int column = 0; column < SquareGrids.columnCount; column++)
                    {
                        float SumXErr = 0;
                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            float XErr = (float)myMath.randNumber((double)nudTileDisX.Value * -1, (double)nudTileDisX.Value) * (float)pixelpermm;
                            genTiles[column, row].center.X += XErr;
                            SumXErr += XErr;
                        }
                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            genTiles[column, row].center.X -= SumXErr / SquareGrids.rowCount;
                        }
                    }
                    #endregion

                    #region 調整一列磁磚當中，y的變量
                    for (int row = 0; row < SquareGrids.rowCount; row++)
                    {
                        float SumYErr = 0;
                        for (int column = 0; column < SquareGrids.columnCount; column++)
                        {
                            float YErr = (float)myMath.randNumber((double)nudTileDisY.Value * -1, (double)nudTileDisY.Value) * (float)pixelpermm;
                            genTiles[column, row].center.Y += YErr;
                            SumYErr += YErr;
                        }
                        for (int column = 0; column < SquareGrids.columnCount; column++)
                        {
                            genTiles[column, row].center.X -= SumYErr / SquareGrids.columnCount;
                        }
                    }
                    #endregion
                    break;
                case TilesType.Rectangle:
                    #region 調整一行磁磚當中，x的變量
                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        float SumXErr = 0;
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            float XErr = (float)myMath.randNumber((double)nudTileDisX.Value * -1, (double)nudTileDisX.Value) * (float)pixelpermm;
                            genTiles[column, row].center.X += XErr;
                            SumXErr += XErr;
                        }
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            genTiles[column, row].center.X -= SumXErr / RectangleGrids.rowCount;
                        }
                    }
                    #endregion

                    #region 調整一列磁磚當中，y的變量
                    for (int row = 0; row < RectangleGrids.rowCount; row++)
                    {
                        float SumYErr = 0;
                        for (int column = 0; column < RectangleGrids.columnCount; column++)
                        {
                            float YErr = (float)myMath.randNumber((double)nudTileDisY.Value * -1, (double)nudTileDisY.Value) * (float)pixelpermm;
                            genTiles[column, row].center.Y += YErr;
                            SumYErr += YErr;
                        }
                        for (int column = 0; column < RectangleGrids.columnCount; column++)
                        {
                            genTiles[column, row].center.X -= SumYErr / RectangleGrids.columnCount;
                        }
                    }
                    #endregion
                    break;
                default:
                    break;
            }

        }

        /// <summary>將整個磁磚搬回中心</summary>
        private void moveTilesToCenter(TilesType theTilesType)
        {

            float TilesCenterX = 0;
            float TilesCenterY = 0;
            float TilesCenterXErr;
            float TilesCenterYErr;

            switch (theTilesType)
            {
                case TilesType.Square:
                    #region 將整個磁磚搬回中心

                    for (int column = 0; column < SquareGrids.columnCount; column++)
                    {
                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            TilesCenterX += genTiles[column, row].center.X;
                            TilesCenterY += genTiles[column, row].center.Y;
                        }

                    }
                    TilesCenterX /= 144;
                    TilesCenterY /= 144;
                    TilesCenterXErr = TilesCenterX - 3500;
                    TilesCenterYErr = TilesCenterY - 3500;

                    for (int column = 0; column < SquareGrids.columnCount; column++)
                    {
                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            genTiles[column, row].center.X -= TilesCenterXErr;
                            genTiles[column, row].center.Y -= TilesCenterYErr;
                        }

                    }

                    #endregion
                    break;
                case TilesType.Rectangle:
                    #region 將整個磁磚搬回中心

                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            TilesCenterX += genTiles[column, row].center.X;
                            TilesCenterY += genTiles[column, row].center.Y;
                        }

                    }
                    TilesCenterX /= RectangleGrids.columnCount * RectangleGrids.rowCount;
                    TilesCenterY /= RectangleGrids.columnCount * RectangleGrids.rowCount;
                    TilesCenterXErr = TilesCenterX - RectangleDrawZone.Width / 2;
                    TilesCenterYErr = TilesCenterY - RectangleDrawZone.Height / 2;

                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            genTiles[column, row].center.X -= TilesCenterXErr;
                            genTiles[column, row].center.Y -= TilesCenterYErr;
                        }

                    }

                    #endregion
                    break;
                default:
                    break;
            }

        }

        /// <summary>設定磁磚的起始角度</summary>
        private void setTileBaseAngle(TilesType theTilesType)
        {
            TrendLine theTrendLine;
            switch (theTilesType)
            {
                case TilesType.Square:
                    theTrendLine = Rank.TilesLineAngle(theTilesType, genTiles);

                    #region 依據不同的base調整磁磚指向
                    if (rdbAngleBaseUp.Checked)
                    {
                        for (int column = 0; column < SquareGrids.columnCount; column++)
                        {
                            for (int row = 0; row < SquareGrids.rowCount; row++)
                            {
                                genTiles[column, row].angle = 0f;
                            }

                        }
                    }
                    else if (rdbAngleBaseRow.Checked)
                    {
                        for (int column = 0; column < SquareGrids.columnCount; column++)
                        {
                            for (int row = 0; row < SquareGrids.rowCount; row++)
                            {
                                genTiles[column, row].angle = (float)myMath.MathDeg2DrawingDeg(theTrendLine.Horizontal[row] + 90f);
                            }

                        }
                    }
                    else if (rdbAngleBaseColumn.Checked)
                    {
                        for (int column = 0; column < SquareGrids.columnCount; column++)
                        {
                            for (int row = 0; row < SquareGrids.rowCount; row++)
                            {
                                genTiles[column, row].angle
                                    = (float)(myMath.MathDeg2DrawingDeg(theTrendLine.Vertical[column]));
                            }

                        }
                    }
                    else if (rdbAngleBaseCRAvg.Checked)
                    {
                        for (int column = 0; column < SquareGrids.columnCount; column++)
                        {
                            for (int row = 0; row < SquareGrids.rowCount; row++)
                            {
                                genTiles[column, row].angle
                                    = (float)myMath.MathDeg2DrawingDeg((theTrendLine.Horizontal[row] + 90f) / 2
                                    + theTrendLine.Vertical[column] / 2);

                            }

                        }
                    }
                    #endregion

                    break;
                case TilesType.Rectangle:
                    theTrendLine = Rank.TilesLineAngle(theTilesType, genTiles);

                    #region 依據不同的base調整磁磚指向
                    if (rdbAngleBaseUp.Checked)
                    {
                        for (int column = 0; column < RectangleGrids.columnCount; column++)
                        {
                            for (int row = 0; row < RectangleGrids.rowCount; row++)
                            {
                                genTiles[column, row].angle = 0f;
                            }

                        }
                    }
                    else if (rdbAngleBaseRow.Checked)
                    {
                        for (int column = 0; column < RectangleGrids.columnCount; column++)
                        {
                            for (int row = 0; row < RectangleGrids.rowCount; row++)
                            {
                                genTiles[column, row].angle = (float)myMath.MathDeg2DrawingDeg(theTrendLine.Horizontal[row] + 90f);
                            }

                        }
                    }
                    else if (rdbAngleBaseColumn.Checked)
                    {
                        for (int column = 0; column < RectangleGrids.columnCount; column++)
                        {
                            for (int row = 0; row < RectangleGrids.rowCount; row++)
                            {
                                genTiles[column, row].angle
                                    = (float)(myMath.MathDeg2DrawingDeg(theTrendLine.Vertical[column]));
                            }

                        }
                    }
                    else if (rdbAngleBaseCRAvg.Checked)
                    {
                        for (int column = 0; column < RectangleGrids.columnCount; column++)
                        {
                            for (int row = 0; row < RectangleGrids.rowCount; row++)
                            {
                                genTiles[column, row].angle
                                    = (float)myMath.MathDeg2DrawingDeg((theTrendLine.Horizontal[row] + 90f) / 2
                                    + theTrendLine.Vertical[column] / 2);

                            }

                        }
                    }
                    #endregion
                    break;
                default:
                    break;
            }
        }

        /// <summary>設定磁磚的角度</summary>
        private void setTileAngle(TilesType theTileType)
        {
            switch (theTileType)
            {
                case TilesType.Square:
                    #region 處理整個圖版
                    for (int column = 0; column < SquareGrids.columnCount; column++)
                    {
                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            genTiles[column, row].angle +=
                                -(float)nudTileAngle.Value + (float)myMath.randNumber((double)nudTileAngleErr.Value * -1, (double)nudTileAngleErr.Value);
                        }
                    }
                    #endregion
                    #region 處理行方向的
                    for (int column = 0; column < SquareGrids.columnCount; column++)
                    {
                        float columnAngleErr = -(float)nudColumnTileAngle.Value
                            + (float)myMath.randNumber((double)nudColumnTileAngleErr.Value * -1, (double)nudColumnTileAngleErr.Value);
                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            genTiles[column, row].angle += columnAngleErr;
                        }
                    }
                    #endregion
                    #region 處理列方向的
                    for (int row = 0; row < SquareGrids.rowCount; row++)
                    {
                        float rowAngleErr = -(float)nudRowTileAngle.Value
                            + (float)myMath.randNumber((double)nudRowTileAngleErr.Value * -1, (double)nudRowTileAngleErr.Value);
                        for (int column = 0; column < SquareGrids.columnCount; column++)
                        {
                            genTiles[column, row].angle += rowAngleErr;
                        }
                    }
                    #endregion

                    break;
                case TilesType.Rectangle:
                    #region 處理整個圖版
                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            genTiles[column, row].angle +=
                                -(float)nudTileAngle.Value + (float)myMath.randNumber((double)nudTileAngleErr.Value * -1, (double)nudTileAngleErr.Value);
                        }
                    }
                    #endregion
                    #region 處理行方向的
                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        float columnAngleErr = -(float)nudColumnTileAngle.Value
                            + (float)myMath.randNumber((double)nudColumnTileAngleErr.Value * -1, (double)nudColumnTileAngleErr.Value);
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            genTiles[column, row].angle += columnAngleErr;
                        }
                    }
                    #endregion
                    #region 處理列方向的
                    for (int row = 0; row < RectangleGrids.rowCount; row++)
                    {
                        float rowAngleErr = -(float)nudRowTileAngle.Value
                            + (float)myMath.randNumber((double)nudRowTileAngleErr.Value * -1, (double)nudRowTileAngleErr.Value);
                        for (int column = 0; column < RectangleGrids.columnCount; column++)
                        {
                            genTiles[column, row].angle += rowAngleErr;
                        }
                    }
                    #endregion
                    break;
                default:
                    break;
            }
        }



        /// <summary>將目前的結果繪製到UI</summary>
        private void showSample(TilesType theTilesType)
        {
            switch (theTilesType)
            {
                case TilesType.Square:
                    DrawZone = new Image<Bgr, byte>(7000, 7000, new Bgr(Color.DarkBlue));
                    for (int column = 0; column < SquareGrids.columnCount; column++)
                    {
                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            if (ckbDrawTexture.Checked)
                            {
                                Image<Bgr, byte> tempTile;
                                if ((column + row) % 2 == 0)
                                {
                                    tempTile = SWTile.Copy();

                                    tempTile = tempTile.Rotate((double)genTiles[column, row].angle, new Bgr(Color.Black), false);
                                    
                                }
                                else
                                {
                                    tempTile = SBTile.Copy();

                                    tempTile = tempTile.Rotate((double)genTiles[column, row].angle, new Bgr(Color.Black), false);
                                    
                                }

                                Image<Bgr, byte> tempBackGrand = new Image<Bgr, byte>(7000, 7000,new Bgr(Color.Black));
                                Point Anchor = genTiles[column, row].center.ToPoint();
                                Anchor.Offset(-tempTile.Width / 2, -tempTile.Height / 2);

                                tempBackGrand.ROI = new Rectangle(Anchor, tempTile.Size);

                                tempTile.CopyTo(tempBackGrand);
                                tempBackGrand.ROI = new Rectangle();

                                Image<Gray, byte> mask = tempBackGrand.Convert<Gray, byte>();

                                tempBackGrand.Copy(DrawZone, mask);
                            }
                            else
                            {
                                DrawZone.Draw(genTiles[column, row], DrawSolidColor, 0);
                            }
                        }
                    }
                    break;
                case TilesType.Rectangle:
                    DrawZone = new Image<Bgr, byte>(RectangleDrawZone.Width, RectangleDrawZone.Height, new Bgr(Color.DarkBlue));
                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            if ((column + row) % 2 == 0)
                            {
                                if (ckbDrawTexture.Checked)
                                {
                                    Image<Bgr, byte> tempTile = RTile.Rotate((double)genTiles[column, row].angle, new Bgr(Color.Black),false);
                                    Image<Bgr, byte> tempBackGrand = new Image<Bgr, byte>(RectangleDrawZone.Width, RectangleDrawZone.Height);
                                    Point Anchor = genTiles[column, row].center.ToPoint();
                                    Anchor.Offset(-tempTile.Width / 2, -tempTile.Height / 2);

                                    tempBackGrand.ROI = new Rectangle(Anchor, tempTile.Size);

                                    tempTile.CopyTo(tempBackGrand);
                                    tempBackGrand.ROI = new Rectangle();

                                    Image<Gray, byte> mask = tempBackGrand.Convert<Gray, byte>();

                                    tempBackGrand.Copy(DrawZone, mask);
                                }
                                else
                                {
                                    DrawZone.Draw(genTiles[column, row], DrawSolidColor, 0);
                                }

                                //DrawZone.Draw(genTiles[column, row], DrawEdgeColor, 1);
                            }

                        }
                    }
                    break;
                default:
                    break;
            }


            pictureBox1.Image = DrawZone.ToBitmap();
        }

        private void btnSaveSample_Click(object sender, EventArgs e)
        {
            RankSample(genTilesType);
            saveSample();
        }

        private void btnLoadSample_Click(object sender, EventArgs e)
        {
            loadSample();
            showSample(genTilesType);
        }

        #region 存讀檔
        /// <summary>存檔</summary>
        private void saveSample()
        {
            SampleFile savefile = new SampleFile(genTiles, genTilesType);
            BinaryFormatter formatter = new BinaryFormatter();
            string time = DateTime.Now.ToString(@"yyyyMMdd-HHmmss");
            Stream output = File.Create(time + "data");
            formatter.Serialize(output, savefile);
            output.Close();
            DrawZone.ToBitmap().Save(time + "sample.png", ImageFormat.Png);
            using (StreamWriter sw = new StreamWriter(time + "TileData.txt"))
            {
                for (int column = 0; column < genTiles.GetLength(0); column++)
                {
                    for (int row = 0; row < genTiles.GetLength(1); row++)
                    {
                        string line = column.ToString() + "," + row.ToString();
                        line += "\t" + genTiles[column, row].center.X.ToString();
                        line += "\t" + genTiles[column, row].center.Y.ToString();
                        line += "\t" + genTiles[column, row].angle.ToString();
                        sw.WriteLine(line);
                    }
                }
            }


        }

        /// <summary>讀檔</summary>
        private void loadSample()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog1.FileName;
                BinaryFormatter formatter = new BinaryFormatter();
                Stream input = File.OpenRead(filename);
                SampleFile tempTileFile = (SampleFile)formatter.Deserialize(input);
                input.Close();
                genTiles = new MCvBox2D[tempTileFile.boxs.GetLength(0), tempTileFile.boxs.GetLength(1)];
                for (int i = 0; i < tempTileFile.boxs.GetLength(0); i++)
                {
                    for (int j = 0; j < tempTileFile.boxs.GetLength(1); j++)
                    {
                        genTiles[i, j] = tempTileFile.boxs[i, j].getMcvBox2D();
                    }
                }
                genTilesType = tempTileFile.TilesType;
                if (genTilesType == TilesType.Square)
                {
                    DrawZone = new Image<Bgr, byte>(7000, 7000);
                }
                else
                {
                    DrawZone = new Image<Bgr, byte>(RectangleDrawZone.Width, RectangleDrawZone.Height);
                }

            }
        }
        #endregion

        private void SampleGen_Load(object sender, EventArgs e)
        {

        }

        private void rdbGenTileTypeSquare_CheckedChanged(object sender, EventArgs e)
        {
            genTilesType = TilesType.Square;
        }

        private void rdbGenTileTypeRectangle_CheckedChanged(object sender, EventArgs e)
        {
            genTilesType = TilesType.Rectangle;
        }

        private void btnReDraw_Click(object sender, EventArgs e)
        {
            showSample(genTilesType);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Image<Bgr, byte> tempTile;
            tempTile = SWTile.Copy();

            tempTile = tempTile.Rotate(-30, new Bgr(Color.Black),false);
            pictureBox1.Image = tempTile.ToBitmap();
        }

        private void ckbRankTopOnly_CheckedChanged(object sender, EventArgs e)
        {

        }




    }
}
