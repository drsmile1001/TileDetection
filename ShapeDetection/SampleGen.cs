using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

namespace 磁磚辨識評分
{
    public partial class SampleGen : Form
    {
        /// <summary>背景圖，用以顯示</summary>
        private Image<Bgr, Byte> DrawZone;

        /// <summary>1mm長用多少pixel表示</summary>
        private const double pixelpermm = 10;

        /// <summary>產生的磁磚</summary>
        private MCvBox2D[,] genTiles;

        /// <summary>產生磁磚的類型</summary>
        private TilesType genTilesType;

        /// <summary>正方磚大小</summary>
        private static SizeF SquareTileSize = new SizeF(47f * (float)pixelpermm, 47f * (float)pixelpermm);

        /// <summary>二丁掛大小</summary>
        private static SizeF RectangleTileSize = new SizeF((float)RectangleGrids.tileLength * (float)pixelpermm, (float)RectangleGrids.tileWidth * (float)pixelpermm);

        /// <summary>二丁掛畫布</summary>
        private static Size RectangleDrawZone = new Size(1120 * (int)pixelpermm, 500 * (int)pixelpermm);

        /// <summary>正方磚畫布</summary>
        private static Size SquareDrawZone = new Size(700 * (int)pixelpermm, 700 * (int)pixelpermm);

        private static Image<Bgr, byte> RTile = new Image<Bgr, byte>("RectangularTileSample.bmp").Resize((int)RectangleTileSize.Width, (int)RectangleTileSize.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
        private static Image<Bgr, byte> SWTile = new Image<Bgr, byte>("SquareWhileTileSample.bmp").Resize((int)SquareTileSize.Width, (int)SquareTileSize.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
        private static Image<Bgr, byte> SBTile = new Image<Bgr, byte>("SquareBeigeTileSample.bmp").Resize((int)SquareTileSize.Width, (int)SquareTileSize.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);

        private static Bgr DrawSolidColor = new Bgr(Color.White);
        private static Bgr DrawEdgeColor = new Bgr(Color.Red);

        /// <summary>資料夾選擇對話視窗</summary>
        private FolderSelect.FolderSelectDialog fsd = new FolderSelect.FolderSelectDialog();

        /// <summary>批次處理</summary>
        //bool IsBatchProcess = false;
        /// <summary>要批次處理的檔案</summary>
        private List<string> BatchFiles = new List<string>();

        private List<string> ScoreList = new List<string>();

        /// <summary>只評分上半</summary>
        private bool RankTopOnly = true;

        public SampleGen()
        {
            InitializeComponent();
        }

        private void btnGen_Click(object sender, EventArgs e)
        {
            genSample(genTilesType);
            showSample();
            RankSample("");
        }

        /// <summary>評分</summary>
        private string RankSample(string fullFilePath)
        {
            string result;
            if (fullFilePath == "") fullFilePath = Directory.GetCurrentDirectory() + "\\genTile.jpg";
            if (genTilesType == TilesType.Square)
            {
                Tile LT = new Tile(genTiles[0, 0]);
                Point RDPoint = LT.conerLT.ToPoint();
                RDPoint.Offset((int)(50 * 12 * pixelpermm), (int)(50 * 12 * pixelpermm));
                SquareGrids myGrid = new SquareGrids(RDPoint, LT.conerLT.ToPoint());
                result = SquareTiles.RankSquareTile(fullFilePath, genTiles, myGrid, RankTopOnly);
                toolStripStatusLabel1.Text = result;
            }
            else
            {
                Tile LT = new Tile(genTiles[0, 0]);
                Point RDPoint = LT.conerLT.ToPoint();
                RDPoint.Offset((int)(RectangleGrids.Length * pixelpermm), (int)(RectangleGrids.Width * pixelpermm));
                RectangleGrids myGrid = new RectangleGrids(RDPoint, LT.conerLT.ToPoint());
                result = RectangleTiles.rankRectangleTiles(fullFilePath, genTiles, myGrid, RankTopOnly);
                toolStripStatusLabel1.Text = result;
            }
            return result;
        }

        /// <summary>產生樣本</summary>
        private void genSample(TilesType theTilesType)
        {
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

                    DrawZone = new Image<Bgr, byte>(SquareDrawZone.Width, SquareDrawZone.Height, new Bgr(Color.Gray));

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

                    //依據行列的角度，移動磁磚
                    genColumnRowAngle(theTilesType);

                    //依據設定的誤差，挑整磁磚到趨勢線的距離
                    genTileLineErr(theTilesType);

                    //設定磁磚的起始角度
                    setTileBaseAngle(theTilesType);

                    //設定磁磚角度
                    setTileAngle(theTilesType);

                    //將整組磁磚移回中心
                    moveTilesToCenter(theTilesType);

                    break;

                default:
                    break;
            }
        }

        /// <summary>產生行列間距位置</summary>
        private void genColumnRowDis(TilesType theTilesType)
        {
            PointF TileCenterPosition = new PointF(0f, 0f);
            GaussianRandom gr = new GaussianRandom();
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
                        double thisRowErr = 47 * pixelpermm + (double)nudRowDis.Value * pixelpermm
                            + (gr.NextGaussian() / 2 * (double)nudRowErr.Value * pixelpermm);
                        rowErr[index] = (float)thisRowErr;
                        //+ (float)myMath.randNumber((double)nudRowErr.Value * -1, (double)nudRowErr.Value) * (float)pixelpermm;
                    }
                    for (int index = 0; index < columnErr.Length; index++)
                    {
                        double thisColumnErr = 47 * pixelpermm + (double)nudCoulumnDis.Value * pixelpermm
                            + (gr.NextGaussian() / 2 * (double)nudCoulumnErr.Value * pixelpermm);
                        columnErr[index] = (float)thisColumnErr;
                        //+ (float)myMath.randNumber((double)nudCoulumnErr.Value * -1, (double)nudCoulumnErr.Value) * (float)pixelpermm;
                    }

                    #endregion 產生每一行、每一列的間距

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

                    #endregion 依據間距產生磁磚

                    break;

                case TilesType.Rectangle:

                    #region 產生每一行、每一列的間距

                    rowErr = new float[RectangleGrids.rowCount];
                    columnErr = new float[RectangleGrids.columnCount];

                    for (int index = 0; index < rowErr.Length; index++)
                    {
                        double thisRowErr = RectangleGrids.tileWidth * pixelpermm + (double)nudRowDis.Value * pixelpermm
                            + (gr.NextGaussian() / 2 * (double)nudRowErr.Value * pixelpermm);
                        rowErr[index] = (float)thisRowErr;
                        //+ (float)myMath.randNumber((double)nudRowErr.Value * -1, (double)nudRowErr.Value)) * (float)pixelpermm;
                    }
                    for (int index = 0; index < columnErr.Length; index++)
                    {
                        double thisColumnErr = (RectangleGrids.tileLength / 2) * pixelpermm + (double)nudCoulumnDis.Value / 2 * pixelpermm
                            + (gr.NextGaussian() / 2 * (double)nudRowErr.Value * pixelpermm);
                        columnErr[index] = (float)thisColumnErr;
                        //+ (float)myMath.randNumber((double)nudCoulumnErr.Value * -1, (double)nudCoulumnErr.Value)) * (float)pixelpermm;
                    }

                    #endregion 產生每一行、每一列的間距

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

                    #endregion 依據間距產生磁磚

                    break;

                default:
                    break;
            }
        }

        /// <summary>產生行列的角度</summary>
        private void genColumnRowAngle(TilesType theTilesType)
        {
            GaussianRandom gr = new GaussianRandom();
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
                        double thisRowAngle = (double)nudRowAngle.Value + gr.NextGaussian() / 2 * (double)nudRowAngleErr.Value;
                        rowAngle[index] = (float)thisRowAngle;
                        //+ (float)myMath.randNumber((double)nudRowAngleErr.Value * -1, (double)nudRowAngleErr.Value);
                    }
                    for (int index = 0; index < columnAngle.Length; index++)
                    {
                        double thisColumnAngle = (double)nudCoulumnAngle.Value + gr.NextGaussian() / 2 * (double)nudCoulumnAngleErr.Value;
                        columnAngle[index] = (float)thisColumnAngle;
                        //+ (float)myMath.randNumber((double)nudCoulumnAngleErr.Value * -1, (double)nudCoulumnAngleErr.Value);
                    }

                    #endregion 產生每一行、每一列的角度

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

                    #endregion 取得每行每列的平均座標(中心線的X,Y值)

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
                                newTileShiftX = (avgRow[row] - SquareDrawZone.Width / 2 - tileShiftY) * -1 * (float)Math.Tan(myMath.Deg2Rad(columnAngle[column]));
                                newTileShiftY = (avgColumn[column] - SquareDrawZone.Height / 2 + newTileShiftX) * (float)Math.Tan(myMath.Deg2Rad(rowAngle[row]));
                            } while (Math.Abs(newTileShiftX - tileShiftX) > 0.5 || Math.Abs(newTileShiftY - tileShiftY) > 0.5);

                            #endregion 產生該行列磁磚該移動的量

                            #region 移動磁磚

                            genTiles[column, row].center.X -= newTileShiftX;
                            genTiles[column, row].center.Y -= newTileShiftY;

                            #endregion 移動磁磚
                        }
                    }

                    #endregion 依據行列角度移動磁磚

                    break;

                case TilesType.Rectangle:

                    #region 產生每一行、每一列的角度

                    rowAngle = new float[RectangleGrids.rowCount];
                    columnAngle = new float[RectangleGrids.columnCount];

                    for (int index = 0; index < rowAngle.Length; index++)
                    {
                        double thisRowAngle = (double)nudRowAngle.Value + gr.NextGaussian() / 2 * (double)nudRowAngleErr.Value;
                        rowAngle[index] = (float)thisRowAngle;
                        //+ (float)myMath.randNumber((double)nudRowAngleErr.Value * -1, (double)nudRowAngleErr.Value);
                    }
                    for (int index = 0; index < columnAngle.Length; index++)
                    {
                        double thisColumnAngle = (double)nudCoulumnAngle.Value + gr.NextGaussian() / 2 * (double)nudCoulumnAngleErr.Value;
                        columnAngle[index] = (float)thisColumnAngle;
                        //+ (float)myMath.randNumber((double)nudCoulumnAngleErr.Value * -1, (double)nudCoulumnAngleErr.Value);
                    }

                    #endregion 產生每一行、每一列的角度

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

                    #endregion 取得每行每列的平均座標(中心線的X,Y值)

                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
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
                                newTileShiftX = (avgRow[row] - RectangleDrawZone.Height / 2 - tileShiftY) * -1 * (float)Math.Tan(myMath.Deg2Rad(columnAngle[column]));
                                newTileShiftY = (avgColumn[column] - RectangleDrawZone.Width / 2 + newTileShiftX) * (float)Math.Tan(myMath.Deg2Rad(rowAngle[row]));
                            } while (Math.Abs(newTileShiftX - tileShiftX) > 0.5 || Math.Abs(newTileShiftY - tileShiftY) > 0.5);

                            #endregion 產生該行列磁磚該移動的量

                            #region 移動磁磚

                            genTiles[column, row].center.X -= newTileShiftX;
                            genTiles[column, row].center.Y -= newTileShiftY;

                            #endregion 移動磁磚
                        }
                    }

                    #endregion 依據行列角度移動磁磚

                    break;

                default:
                    break;
            }
        }

        /// <summary>設定磁磚到行列的距離</summary>
        private void genTileLineErr(TilesType theTilesType)
        {
            GaussianRandom gr = new GaussianRandom();
            switch (theTilesType)
            {
                case TilesType.Square:

                    #region 調整一行磁磚當中，x的變量

                    for (int column = 0; column < SquareGrids.columnCount; column++)
                    {
                        float SumXErr = 0;
                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            float XErr = (float)(gr.NextGaussian() / 2 * (double)nudTileDisX.Value * pixelpermm);
                            genTiles[column, row].center.X += XErr;
                            SumXErr += XErr;
                        }
                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            genTiles[column, row].center.X -= SumXErr / SquareGrids.rowCount;
                        }
                    }

                    #endregion 調整一行磁磚當中，x的變量

                    #region 調整一列磁磚當中，y的變量

                    for (int row = 0; row < SquareGrids.rowCount; row++)
                    {
                        float SumYErr = 0;
                        for (int column = 0; column < SquareGrids.columnCount; column++)
                        {
                            float YErr = (float)(gr.NextGaussian() / 2 * (double)nudTileDisY.Value * pixelpermm);
                            genTiles[column, row].center.Y += YErr;
                            SumYErr += YErr;
                        }
                        for (int column = 0; column < SquareGrids.columnCount; column++)
                            genTiles[column, row].center.X -= SumYErr / SquareGrids.columnCount;
                        {
                        }
                    }

                    #endregion 調整一列磁磚當中，y的變量

                    break;

                case TilesType.Rectangle:

                    #region 調整一行磁磚當中，x的變量

                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        float SumXErr = 0;
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            float XErr = (float)(gr.NextGaussian() / 2 * (double)nudTileDisX.Value * pixelpermm);
                            genTiles[column, row].center.X += XErr;
                            SumXErr += XErr;
                        }
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            genTiles[column, row].center.X -= SumXErr / RectangleGrids.rowCount;
                        }
                    }

                    #endregion 調整一行磁磚當中，x的變量

                    #region 調整一列磁磚當中，y的變量

                    for (int row = 0; row < RectangleGrids.rowCount; row++)
                    {
                        float SumYErr = 0;
                        for (int column = 0; column < RectangleGrids.columnCount; column++)
                        {
                            float YErr = (float)(gr.NextGaussian() / 2 * (double)nudTileDisY.Value * pixelpermm);
                            genTiles[column, row].center.Y += YErr;
                            SumYErr += YErr;
                        }
                        for (int column = 0; column < RectangleGrids.columnCount; column++)
                        {
                            genTiles[column, row].center.X -= SumYErr / RectangleGrids.columnCount;
                        }
                    }

                    #endregion 調整一列磁磚當中，y的變量

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
                        for (int row = 0; row < SquareGrids.rowCount / 2; row++)
                        {
                            TilesCenterX += genTiles[column, row].center.X;
                            TilesCenterY += genTiles[column, row].center.Y;
                        }
                    }
                    TilesCenterX /= 144 / 2;
                    TilesCenterY /= 144 / 2;
                    TilesCenterXErr = TilesCenterX - SquareDrawZone.Width / 2;
                    TilesCenterYErr = TilesCenterY - SquareDrawZone.Height / 2;

                    for (int column = 0; column < SquareGrids.columnCount; column++)
                    {
                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            genTiles[column, row].center.X -= TilesCenterXErr;
                            genTiles[column, row].center.Y -= TilesCenterYErr;
                        }
                    }

                    #endregion 將整個磁磚搬回中心

                    break;

                case TilesType.Rectangle:

                    #region 將整個磁磚搬回中心

                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        for (int row = 0; row < RectangleGrids.rowCount / 2; row++)
                        {
                            if ((row + column) % 2 != 0)
                            {
                                continue;
                            }
                            TilesCenterX += genTiles[column, row].center.X;
                            TilesCenterY += genTiles[column, row].center.Y;
                        }
                    }
                    TilesCenterX /= RectangleGrids.tileCount / 2;
                    TilesCenterY /= RectangleGrids.tileCount / 2;
                    TilesCenterXErr = TilesCenterX - RectangleDrawZone.Width / 2;
                    TilesCenterYErr = TilesCenterY - RectangleDrawZone.Height / 2;

                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            if ((row + column) % 2 != 0)
                            {
                                continue;
                            }
                            genTiles[column, row].center.X -= TilesCenterXErr;
                            genTiles[column, row].center.Y -= TilesCenterYErr;
                        }
                    }

                    #endregion 將整個磁磚搬回中心

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

                    #endregion 依據不同的base調整磁磚指向

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

                    #endregion 依據不同的base調整磁磚指向

                    break;

                default:
                    break;
            }
        }

        /// <summary>設定磁磚的角度</summary>
        private void setTileAngle(TilesType theTileType)
        {
            GaussianRandom gr = new GaussianRandom();
            switch (theTileType)
            {
                case TilesType.Square:

                    #region 處理整個圖版
                    Random rd = new Random();
                    for (int column = 0; column < SquareGrids.columnCount; column++)
                    {
                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            
                            double angle = -(double)nudTileAngle.Value + gr.NextGaussian() / 2 * (double)nudTileAngleErr.Value;
                            //double angle = -(double)nudTileAngle.Value + (double)rd.Next(0, 1000) / (double)1000 * (double)nudTileAngleErr.Value;
                            genTiles[column, row].angle += (float)angle;
                            //-(float)nudTileAngle.Value + (float)myMath.randNumber((double)nudTileAngleErr.Value * -1, (double)nudTileAngleErr.Value);
                        }
                    }

                    #endregion 處理整個圖版

                    #region 處理行方向的

                    for (int column = 0; column < SquareGrids.columnCount; column++)
                    {
                        double angle = -(double)nudColumnTileAngle.Value + gr.NextGaussian() / 2 * (double)nudColumnTileAngleErr.Value;
                        //float columnAngleErr = -(float)nudColumnTileAngle.Value
                        //   + (float)myMath.randNumber((double)nudColumnTileAngleErr.Value * -1, (double)nudColumnTileAngleErr.Value);
                        for (int row = 0; row < SquareGrids.rowCount; row++)
                        {
                            genTiles[column, row].angle += (float)angle;
                        }
                    }

                    #endregion 處理行方向的

                    #region 處理列方向的

                    for (int row = 0; row < SquareGrids.rowCount; row++)
                    {
                        double angle = -(double)nudRowTileAngle.Value + gr.NextGaussian() / 2 * (double)nudRowTileAngleErr.Value;
                        //float rowAngleErr = -(float)nudRowTileAngle.Value
                        //    + (float)myMath.randNumber((double)nudRowTileAngleErr.Value * -1, (double)nudRowTileAngleErr.Value);
                        for (int column = 0; column < SquareGrids.columnCount; column++)
                        {
                            genTiles[column, row].angle += (float)angle;
                        }
                    }

                    #endregion 處理列方向的

                    break;

                case TilesType.Rectangle:

                    #region 處理整個圖版

                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            double angle = -(double)nudTileAngle.Value + gr.NextGaussian() / 2 * (double)nudTileAngleErr.Value;
                            genTiles[column, row].angle += (float)angle;
                            //-(float)nudTileAngle.Value + (float)myMath.randNumber((double)nudTileAngleErr.Value * -1, (double)nudTileAngleErr.Value);
                        }
                    }

                    #endregion 處理整個圖版

                    #region 處理行方向的

                    for (int column = 0; column < RectangleGrids.columnCount; column++)
                    {
                        double angle = -(double)nudColumnTileAngle.Value + gr.NextGaussian() / 2 * (double)nudColumnTileAngleErr.Value;
                        //float columnAngleErr = -(float)nudColumnTileAngle.Value
                        //   + (float)myMath.randNumber((double)nudColumnTileAngleErr.Value * -1, (double)nudColumnTileAngleErr.Value);
                        for (int row = 0; row < RectangleGrids.rowCount; row++)
                        {
                            genTiles[column, row].angle += (float)angle;
                        }
                    }

                    #endregion 處理行方向的

                    #region 處理列方向的

                    for (int row = 0; row < RectangleGrids.rowCount; row++)
                    {
                        double angle = -(double)nudRowTileAngle.Value + gr.NextGaussian() / 2 * (double)nudRowTileAngleErr.Value;
                        //float rowAngleErr = -(float)nudRowTileAngle.Value
                        //    + (float)myMath.randNumber((double)nudRowTileAngleErr.Value * -1, (double)nudRowTileAngleErr.Value);
                        for (int column = 0; column < RectangleGrids.columnCount; column++)
                        {
                            genTiles[column, row].angle += (float)angle;
                        }
                    }

                    #endregion 處理列方向的

                    break;

                default:
                    break;
            }
        }

        /// <summary>將目前的結果繪製到UI</summary>
        private void showSample()
        {
            ThreadStart ts = new ThreadStart(drawSampleProcess);
            Thread t = new Thread(ts);
            t.Start();
        }

        /// <summary>顯示圖片</summary>
        private void showImage(Bitmap image)
        {
            pictureBox1.Image = image;
        }

        /// <summary>繪製過程</summary>
        private void drawSampleProcess()
        {
            moveTilesToCenter(genTilesType);
            switch (genTilesType)
            {
                case TilesType.Square:
                    DrawZone = new Image<Bgr, byte>(SquareDrawZone.Width, SquareDrawZone.Height, new Bgr(Color.DarkBlue));
                    for (int column = 0; column < SquareGrids.columnCount; column++)
                    {
                        for (int row = 0; row < SquareGrids.rowCount / 2; row++)
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

                                Image<Bgr, byte> tempBackGrand = new Image<Bgr, byte>(SquareDrawZone.Width, SquareDrawZone.Height, new Bgr(Color.Black));
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
                        for (int row = 0; row < RectangleGrids.rowCount / 2; row++)
                        {
                            if ((column + row) % 2 == 0)
                            {
                                if (ckbDrawTexture.Checked)
                                {
                                    Image<Bgr, byte> tempTile = RTile.Rotate((double)genTiles[column, row].angle, new Bgr(Color.Black), false);
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
            if (ckbCanny.Checked)
            {
                 var bmp = DrawZone.Canny(128, 120).ToBitmap();
                 this.Invoke(new Action<Bitmap>(showImage), new object[] { bmp});
            }
            else
            {
                this.Invoke(new Action<Bitmap>(showImage), new object[] { DrawZone.ToBitmap() });
            }
            
        }

        private void btnSaveSample_Click(object sender, EventArgs e)
        {
            string fileName = "";
            switch (genTilesType)
            {
                case TilesType.Square:
                    fileName = "SquareSample";
                    break;

                case TilesType.Rectangle:
                    fileName = "Rectangle";
                    break;

                default:
                    throw new Exception("未設定磁磚類型");
            }
            SFD.FileName = fileName;
            if (SFD.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            fileName = SFD.FileName;
            RankSample(fileName);
            saveSample(fileName);
        }

        private void btnLoadSample_Click(object sender, EventArgs e)
        {
            loadSampleWithDialog();
            showSample();
        }

        #region 存讀檔

        /// <summary>存檔</summary>
        private void saveSample(string fileName)
        {
            string dataName;
            string ImageName;
            if (fileName == "")
            {
                string time = DateTime.Now.ToString(@"yyyyMMdd-HHmmss");
                dataName = time + ".sdata";
                ImageName = time + ".png";
            }
            else
            {
                dataName = fileName + ".sdata";
                ImageName = fileName + ".png";
            }
            SampleFile savefile = new SampleFile(genTiles, genTilesType);
            BinaryFormatter formatter = new BinaryFormatter();

            Stream output = File.Create(dataName);
            formatter.Serialize(output, savefile);
            output.Close();
            DrawZone.ToBitmap().Save(ImageName, ImageFormat.Png);
#if false
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
#endif
        }

        /// <summary>開啟對話視窗讀檔</summary>
        private void loadSampleWithDialog()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadSample(openFileDialog1.FileName);
            }
        }

        /// <summary>透過檔名讀檔</summary>
        private void LoadSample(string filename)
        {
            object inputFile;

            #region 嘗試以SampleFile讀入檔案

            inputFile = LoadFileTool.TryLoadFile<SampleFile>(filename);
            if (inputFile != LoadFileTool.Fail)
            {
                LoadSampleFile((SampleFile)inputFile);
                return;
            }

            #endregion 嘗試以SampleFile讀入檔案

            #region 嘗試以IdentifyTileFileV2讀入檔案

            inputFile = LoadFileTool.TryLoadFile<IdentifyTileFileV2>(filename);
            if (inputFile != LoadFileTool.Fail)
            {
                LoadIdentifyTileFileV2((IdentifyTileFileV2)inputFile);
                return;
            }

            #endregion 嘗試以IdentifyTileFileV2讀入檔案
        }

        /// <summary>讀取SampleFile檔案類型</summary>
        private void LoadSampleFile(SampleFile tempTileFile)
        {
            genTiles = new MCvBox2D[tempTileFile.boxs.GetLength(0), tempTileFile.boxs.GetLength(1)];
            for (int i = 0; i < tempTileFile.boxs.GetLength(0); i++)
            {
                for (int j = 0; j < tempTileFile.boxs.GetLength(1); j++)
                {
                    genTiles[i, j] = tempTileFile.boxs[i, j].getMcvBox2D();
                }
            }
            genTilesType = tempTileFile.TilesType;
            SetDrawZone();
        }

        /// <summary>讀取LoadIdentifyTileFileV2檔案類型</summary>
        private void LoadIdentifyTileFileV2(IdentifyTileFileV2 tempTileFile)
        {
            MCvBox2D[,] tiles;
            Point RD = tempTileFile.WorkAreaRightDown;
            Point LT = tempTileFile.WorkAreaLeftTop;
            List<MCvBox2D> tileList = tempTileFile.getMcvbox2DList();
            SizeF normalizedTileSize;
            double mmFormPixel;

            #region 讀入磁磚類型與2D磁磚

            switch (tempTileFile.WorkAreaType)
            {
                case Grid.SquareGrid:
                    genTilesType = TilesType.Square;
                    tiles = SquareTiles.TileArrayToTile2DArray(RD, LT, tileList);
                    SquareGrids SGrid = new SquareGrids(RD, LT);
                    mmFormPixel = SGrid.mmFormPixel(1);
                    normalizedTileSize = SquareTileSize;
                    break;

                case Grid.RectangleGrid:
                    genTilesType = TilesType.Rectangle;
                    tiles = RectangleTiles.TileArrayToTile2DArray(RD, LT, tileList);
                    RectangleGrids RGrid = new RectangleGrids(RD, LT);
                    mmFormPixel = RGrid.mmFormPixel(1);
                    normalizedTileSize = RectangleTileSize;
                    break;

                default:
                    throw new Exception("讀入的IdentifyTileFileV2未定義磁磚類型");
            }

            #endregion 讀入磁磚類型與2D磁磚

            #region 正規化所有磁磚

            for (int column = 0; column < tiles.GetLength(0); column++)
            {
                for (int row = 0; row < tiles.GetLength(1); row++)
                {
                    if (tempTileFile.WorkAreaType == Grid.RectangleGrid &&
                        (column + row) % 2 == 1) continue;

                    tiles[column, row].size = normalizedTileSize;
                    double xDiff = tiles[column, row].center.X - tiles[0, 0].center.X;
                    double yDiff = tiles[column, row].center.Y - tiles[0, 0].center.Y;
                    xDiff *= mmFormPixel * 10;
                    yDiff *= mmFormPixel * 10;
                    tiles[column, row].center.X = tiles[0, 0].center.X + (float)xDiff;
                    tiles[column, row].center.Y = tiles[0, 0].center.Y + (float)yDiff;
                }
            }

            #endregion 正規化所有磁磚

            #region 移動整個磁磚到中心

            PointF tilesCenter = new PointF(0, 0);
            int columnCount = (tempTileFile.WorkAreaType == Grid.SquareGrid) ? SquareGrids.columnCount : RectangleGrids.columnCount;
            int rowCount = (tempTileFile.WorkAreaType == Grid.SquareGrid) ? SquareGrids.rowCount : RectangleGrids.rowCount;

            for (int column = 0; column < columnCount; column++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    if (tempTileFile.WorkAreaType == Grid.RectangleGrid &&
                        (column + row) % 2 == 1) continue;
                    tilesCenter.X += tiles[column, row].center.X;
                    tilesCenter.Y += tiles[column, row].center.Y;
                }
            }
            int tileCount = (tempTileFile.WorkAreaType == Grid.SquareGrid) ? 144 : 28;
            tilesCenter.X /= tileCount;
            tilesCenter.Y /= tileCount;
            float targetCenterX = (tempTileFile.WorkAreaType == Grid.SquareGrid) ? SquareDrawZone.Width / 2 : RectangleDrawZone.Width / 2;
            float targetCenterY = (tempTileFile.WorkAreaType == Grid.SquareGrid) ? SquareDrawZone.Height / 2 : RectangleDrawZone.Height / 2;
            float centerXDiff = tilesCenter.X - targetCenterX;
            float centerYDiff = tilesCenter.Y - targetCenterY;

            for (int column = 0; column < columnCount; column++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    if (tempTileFile.WorkAreaType == Grid.RectangleGrid &&
                        (column + row) % 2 == 1) continue;
                    tiles[column, row].center.X -= centerXDiff;
                    tiles[column, row].center.Y -= centerYDiff;
                }
            }

            #endregion 移動整個磁磚到中心

            #region 設定資料

            genTiles = tiles;
            SetDrawZone();

            #endregion 設定資料
        }

        /// <summary>設定DrawZone</summary>
        private void SetDrawZone()
        {
            if (genTilesType == TilesType.Square)
            {
                DrawZone = new Image<Bgr, byte>(SquareDrawZone.Width, SquareDrawZone.Height);
            }
            else
            {
                DrawZone = new Image<Bgr, byte>(RectangleDrawZone.Width, RectangleDrawZone.Height);
            }
        }

        #endregion 存讀檔

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
            showSample();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Image<Bgr, byte> tempTile;
            tempTile = SWTile.Copy();

            tempTile = tempTile.Rotate(-30, new Bgr(Color.Black), false);
            pictureBox1.Image = tempTile.ToBitmap();
        }

        private void ckbRankTopOnly_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void btnBatchProcess_Click(object sender, EventArgs e)
        {
            ThreadStart ts = new ThreadStart(BatchProcess);
            Thread t = new Thread(ts);
            t.Start();
        }

        /// <summary>設定批次處理訊息</summary>
        private void SetBtnBatchProcessText(string msg)
        {
            btnBatchProcess.Text = msg;
        }

        /// <summary>批次處理照片To電腦繪製</summary>
        private void BatchProcess()
        {
            BatchFiles = GetBatchFiles(new string[]{".sdata"});
            int fileCount = BatchFiles.Count;
            int now = 0;

            foreach (string fileName in BatchFiles)
            {
                now += 1;
                this.Invoke(new Action<string>(SetBtnBatchProcessText), new object[] { now + "/" + fileCount });
                LoadSample(fileName);
                drawSampleProcess();

                saveSample(Path.GetDirectoryName(fileName) + "\\output\\" + Path.GetFileNameWithoutExtension(fileName));
            }
            this.Invoke(new Action<string>(SetBtnBatchProcessText), new object[] { "照片To電腦繪製" });
        }

        /// <summary>取得目錄</summary>
        private List<string> GetBatchFiles(string[] fileType)
        {
            List<string> fileList = new List<string>();
            fsd.Title = "開啟目錄";
            fsd.InitialDirectory = Environment.CurrentDirectory;
            if (!fsd.ShowDialog(IntPtr.Zero))
            {
                return fileList;
            }
            try
            {
                IEnumerable<string> files = Directory.EnumerateFiles(fsd.FileName);
                files = files.OrderBy(s => s);
                if (files.Count() < 1)
                {
                    return fileList;
                }
                foreach (string file in files)
                {
                    string ext = Path.GetExtension(file);
                    
                    if (Array.FindIndex(fileType,s => s.Equals(ext) )== -1)
                    {
                        continue;
                    }
                    fileList.Add(file);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
            return fileList;
        }

        private void btnBatchScore_Click(object sender, EventArgs e)
        {
            ThreadStart ts = new ThreadStart(BatchScoring);
            Thread t = new Thread(ts);
            t.Start();
        }

        private void SetBtnBatchScore(string msg)
        {
            btnBatchScore.Text = msg;
        }

        /// <summary>批次評分</summary>
        private void BatchScoring()
        {
            BatchFiles = GetBatchFiles(new string[]{".sdata",".data"});
            ScoreList.Clear();
            int fileCount = BatchFiles.Count;
            int now = 0;

            foreach (string fileName in BatchFiles)
            {
                now += 1;
                this.Invoke(new Action<string>(SetBtnBatchScore), new object[] { now + "/" + fileCount });
                LoadSample(fileName);
                ScoreList.Add(RankSample(fileName));
            }
            using (StreamWriter sw = new StreamWriter("BatchScoring.txt", false))
            {
                foreach (string line in ScoreList)
                {
                    sw.WriteLine(line);
                }
                sw.Close();
            }
            Process.Start("BatchScoring.txt");
            this.Invoke(new Action<string>(SetBtnBatchScore), new object[] { "批次評分" });
        }
    }
}