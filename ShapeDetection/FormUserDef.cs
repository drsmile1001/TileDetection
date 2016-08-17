
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using 磁磚辨識評分.資料結構;

#region 存檔用

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;

#endregion 存檔用

namespace 磁磚辨識評分
{
    public partial class FormUserDef : Form
    {
        #region 偵錯目的參數

        /// <summary>繪製誤差</summary>
        private bool DRAW_ERR = false;

        /// <summary>輸出磁磚位置</summary>
        private bool PRINT_TILES_LOCATION = false;

        #endregion 偵錯目的參數

        #region 公用變數

        /// <summary>目前開起來的檔案</summary>
        private string FileName { get; set; }
        private Image<Bgr, Byte> ImageForShow;
        private Image<Bgr, Byte> BaseImage;

        private Image<Bgr, Byte> ThumbnailImage;
        private Image<Bgr, Byte> imageWatchArea;
        private List<MCvBox2D> BaseMcvBox2DList;

        /// <summary>記錄需要繪製的誤差標記</summary>
        private ErrMark currentErrMark;

        private double ScaleOfThumbnail = 1;
        private Point centerOfWatchArea = new Point(0, 0);
        private Point pointDragStart = new Point(0, 0);
        private Point CenterOfThumbnailWatchArea = new Point(0, 0);
        private Point CenterOfWatchAreaWhenDragStart = new Point(0, 0);
        private bool mouseDownInPicboxThumbnail = false;
        private bool mouseDownInPicboxWatchArea = false;
        private int WatchAreaMode = WatchAreaModeNum.Browse;

        private Dictionary<string, double> PixelPerCentimeter = new Dictionary<string, double>();

        private List<Point> pointsOfAddingTile = new List<Point>();

        private RankArea rankArea = RankArea.Top;

        #region 和定義邊界有關的變數

        private Grid WorkspaceType = 0;
        private Point WorkspaceLeftTop = Point.Empty;
        private Point WorkspaceRightDown = Point.Empty;

        #endregion 和定義邊界有關的變數

        /// <summary>按下space</summary>
        private bool spaceDown = false;

        #endregion 公用變數

        #region 初始化

        /// <summary>FormUserDef 建構函式</summary>
        public FormUserDef()
        {
            InitializeComponent();
        }

        /// <summary>載入底圖、磁磚</summary>
        public void LoadIDTiles(Image<Bgr, Byte> InputImage, List<MCvBox2D> InputMcvBox2DList, string fileName)
        {
            BaseImage = InputImage.Clone();
            ImageForShow = InputImage.Clone();
            BaseMcvBox2DList = new List<MCvBox2D>(InputMcvBox2DList.ToArray());
            Invoke(new Action(() => { lbxTileList.Items.Clear(); }));

            foreach (MCvBox2D box in BaseMcvBox2DList)
            {
                ImageForShow.Draw(box, new Bgr(Color.Red), 1);
            }
            for (int index = 0; index < BaseMcvBox2DList.Count; index++)
            {
                ImageForShow.Draw(BaseMcvBox2DList[index], new Bgr(Color.Red), 1);
                string boxNum = "tile" + index.ToString();
                Invoke(new Action(() => { lbxTileList.Items.Add(boxNum); }));
            }
            ThumbnailImage = InputImage.Clone().Resize(160, 120, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR, true);
            ScaleOfThumbnail = (double)ThumbnailImage.Height / InputImage.Height;
            picboxThumbnail.Image = ThumbnailImage.ToBitmap();
            picboxWatchArea.Image = ImageForShow.ToBitmap();

            imageWatchArea = new Image<Bgr, byte>(new Size(picboxWatchArea.Width, picboxWatchArea.Height));

            #region 初始化縮圖和觀看視窗

            drawThumbnailImage();
            drawWatchArea();
            Invoke(new Action(() => { lblTileCount.Text = "總數：" + BaseMcvBox2DList.Count.ToString(); }));

            #endregion 初始化縮圖和觀看視窗

            //設定比例參數
            SetPixelPerCentimeter(Path.GetFileNameWithoutExtension(fileName));
            //顯示檔名
            Invoke(new Action(() => { Text = fileName; ; }));
            FileName = fileName;
        }

        #endregion 初始化

        #region 抓滑鼠座標方法

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Point lpPoint);

        public Point GetCursorPosition()
        {
            Point defPnt = new Point();
            GetCursorPos(ref defPnt);
            return defPnt;
        }

        #endregion 抓滑鼠座標方法

        #region 繪製縮圖的方框

        /// <summary>
        /// 繪製縮圖的方框
        /// </summary>
        private void drawThumbnailImage()
        {
            Image<Bgr, Byte> TempImage = ThumbnailImage.Clone();

            if (mouseDownInPicboxThumbnail)
            {
                CenterOfThumbnailWatchArea = getMouseLocationRelayToThumbnailImage();
            }
            else if (mouseDownInPicboxWatchArea)
            {
                #region 這裡以目前的WatchArea位置去繪製縮圖的方框

                CenterOfThumbnailWatchArea = new Point(
                    (int)((double)centerOfWatchArea.X * ScaleOfThumbnail),
                    (int)((double)centerOfWatchArea.Y * ScaleOfThumbnail));

                #endregion 這裡以目前的WatchArea位置去繪製縮圖的方框
            }
            else
            {
                #region 初始化或預設

                CenterOfThumbnailWatchArea = new Point(

                    ThumbnailImage.Width / 2,
                    ThumbnailImage.Height / 2
                    );

                #endregion 初始化或預設
            }

            TempImage.Draw(
                new Rectangle(
                    CenterOfThumbnailWatchArea.X - (int)(picboxWatchArea.Width * ScaleOfThumbnail / 2),
                    CenterOfThumbnailWatchArea.Y - (int)(picboxWatchArea.Height * ScaleOfThumbnail / 2),
                    (int)(picboxWatchArea.Width * ScaleOfThumbnail),
                    (int)(picboxWatchArea.Height * ScaleOfThumbnail)),
                new Bgr(Color.White), 1);

            picboxThumbnail.Image = TempImage.ToBitmap();
        }

        #region 指定繪製中心

        private void drawThumbnailImage(int x, int y)
        {
            Image<Bgr, Byte> TempImage = ThumbnailImage.Clone();

            CenterOfThumbnailWatchArea = new Point(x, y);

            TempImage.Draw(
                new Rectangle(
                    CenterOfThumbnailWatchArea.X - (int)(picboxWatchArea.Width * ScaleOfThumbnail / 2),
                    CenterOfThumbnailWatchArea.Y - (int)(picboxWatchArea.Height * ScaleOfThumbnail / 2),
                    (int)(picboxWatchArea.Width * ScaleOfThumbnail),
                    (int)(picboxWatchArea.Height * ScaleOfThumbnail)),
                new Bgr(Color.White), 1);

            picboxThumbnail.Image = TempImage.ToBitmap();
        }

        #endregion 指定繪製中心

        #endregion 繪製縮圖的方框

        #region 繪製觀看視窗

        private void drawWatchArea()
        {
            #region 找到觀看視窗的中心點

            if (mouseDownInPicboxThumbnail)
            {
                centerOfWatchArea = getMouseLocationRelayToThumbnailImage();
                centerOfWatchArea.X = (int)((double)centerOfWatchArea.X / ScaleOfThumbnail);
                centerOfWatchArea.Y = (int)((double)centerOfWatchArea.Y / ScaleOfThumbnail);
            }
            else if (mouseDownInPicboxWatchArea)
            {
                #region 透過紀錄拖曳開始時的WatchArea中心與滑鼠的座標位置，在拖曳時設定新的WatChArea

                centerOfWatchArea = new Point(
            CenterOfWatchAreaWhenDragStart.X + pointDragStart.X - getMouseLocationRelayTopicboxWatchArea().X,
            CenterOfWatchAreaWhenDragStart.Y + pointDragStart.Y - getMouseLocationRelayTopicboxWatchArea().Y);

                #endregion 透過紀錄拖曳開始時的WatchArea中心與滑鼠的座標位置，在拖曳時設定新的WatChArea
            }
            else
            {
                #region 初始化或預設

                centerOfWatchArea = new Point(
                    ImageForShow.Width / 2,
                    ImageForShow.Height / 2);

                #endregion 初始化或預設
            }

            #endregion 找到觀看視窗的中心點

            picboxWatchArea.Image = ImageForShow.Copy(new MCvBox2D((PointF)centerOfWatchArea, new SizeF((float)picboxWatchArea.Width, (float)picboxWatchArea.Height), 0.0f)).ToBitmap();
        }

        #region 繪製觀看視窗在(x,y)位置

        private void drawWatchArea(int x, int y)
        {
            centerOfWatchArea = new Point(x, y);
            picboxWatchArea.Image = ImageForShow.Copy(new MCvBox2D((PointF)centerOfWatchArea, new SizeF((float)picboxWatchArea.Width, (float)picboxWatchArea.Height), 0.0f)).ToBitmap();
        }

        #endregion 繪製觀看視窗在(x,y)位置

        #endregion 繪製觀看視窗

        #region 取得座標函式

        #region 取得滑鼠相對於縮圖左上角的座標

        /// <summary>
        /// 取得滑鼠相對於縮圖左上角的座標
        /// </summary>
        /// <returns></returns>
        private Point getMouseLocationRelayToThumbnailImage()
        {
            Point pointClickRelateToThumbnailImage = picboxThumbnail.PointToClient(GetCursorPosition());
            pointClickRelateToThumbnailImage.Offset(
                ThumbnailImage.Width / 2 - picboxThumbnail.Width / 2,
                ThumbnailImage.Height / 2 - picboxThumbnail.Height / 2);
            return pointClickRelateToThumbnailImage;
        }

        #endregion 取得滑鼠相對於縮圖左上角的座標

        #region 取得滑鼠相對於原圖左上角的座標

        /// <summary>
        /// 取得滑鼠相對於原圖左上角的座標
        /// </summary>
        /// <returns></returns>
        private Point getMouseLocationRelayToBaseImage()
        {
            Point pointClickRelateToBaseImage = picboxWatchArea.PointToClient(GetCursorPosition());
            pointClickRelateToBaseImage.Offset(
                centerOfWatchArea.X - picboxWatchArea.Width / 2,
                centerOfWatchArea.Y - picboxWatchArea.Height / 2);
            return pointClickRelateToBaseImage;
        }

        #endregion 取得滑鼠相對於原圖左上角的座標

        #region 取得滑鼠相對於picboxWatchArea左上角的座標

        /// <summary>
        /// 取得滑鼠相對於picboxWatchArea左上角的座標
        /// </summary>
        /// <returns></returns>
        private Point getMouseLocationRelayTopicboxWatchArea()
        {
            Point pointClickRelateToPicboxWatchArea = picboxWatchArea.PointToClient(GetCursorPosition());

            return pointClickRelateToPicboxWatchArea;
        }

        #endregion 取得滑鼠相對於picboxWatchArea左上角的座標

        #endregion 取得座標函式

        /// <summary>將目前的ImageForShow廢棄，依據BaseImage、BaseMcvBox2DList、工作區繪製新的</summary>
        private void drawImageForShow()
        {
            ImageForShow = BaseImage.Clone();

            #region 畫網格

            if (WorkspaceLeftTop != Point.Empty && WorkspaceRightDown != Point.Empty && ckbShowWeb.Checked)
            {
                switch (WorkspaceType)
                {
                    case Grid.SquareGrid:

                        #region 畫鉛錘線

                        for (double LineNum = 0; LineNum <= 12.0; LineNum += 1.0)
                        {
                            ImageForShow.Draw(
                                new LineSegment2D(
                                    new Point((int)(WorkspaceLeftTop.X + (WorkspaceRightDown.X - WorkspaceLeftTop.X) / 12.0 * LineNum), WorkspaceLeftTop.Y),
                                    new Point((int)(WorkspaceLeftTop.X + (WorkspaceRightDown.X - WorkspaceLeftTop.X) / 12.0 * LineNum), WorkspaceRightDown.Y)),
                                new Bgr(Color.White), 1);
                        }

                        #endregion 畫鉛錘線

                        #region 畫水平線

                        for (double LineNum = 0; LineNum <= 12.0; LineNum += 1.0)
                        {
                            ImageForShow.Draw(
                                new LineSegment2D(
                                    new Point(WorkspaceLeftTop.X, (int)(WorkspaceLeftTop.Y + (WorkspaceRightDown.Y - WorkspaceLeftTop.Y) / 12.0 * LineNum)),
                                    new Point(WorkspaceRightDown.X, (int)(WorkspaceLeftTop.Y + (WorkspaceRightDown.Y - WorkspaceLeftTop.Y) / 12.0 * LineNum))),
                                new Bgr(Color.White), 1);
                        }

                        #endregion 畫水平線

                        break;

                    case Grid.RectangleGrid:

                        #region 畫長方形的網格，先建立暫時的RectangleGrid，然後利用之繪出格子

                        RectangleGrids tempRectangleGrids = new RectangleGrids(WorkspaceRightDown, WorkspaceLeftTop);

                        for (int column = 0; column < RectangleGrids.columnCount; column++)
                        {
                            for (int row = 0; row < RectangleGrids.ROW_COUNT; row++)
                            {
                                if ((column + row) % 2 != 1)
                                {
                                    #region 如果是梅花座上的座標，將這個格子繪製出來

                                    #region 找出該格子的四角座標

                                    PointF GridLT = tempRectangleGrids.TileModelLT(column, row);
                                    PointF GridRT = tempRectangleGrids.TileModelRT(column, row);
                                    GridRT.X += (float)tempRectangleGrids.PixelFormMm(RectangleGrids.gapWidth);
                                    PointF GridLD = tempRectangleGrids.TileModelLD(column, row);
                                    GridLD.Y += (float)tempRectangleGrids.PixelFormMm(RectangleGrids.gapWidth);
                                    PointF GridRD = tempRectangleGrids.TileModelRD(column, row);
                                    GridRD.X += (float)tempRectangleGrids.PixelFormMm(RectangleGrids.gapWidth);
                                    GridRD.Y += (float)tempRectangleGrids.PixelFormMm(RectangleGrids.gapWidth);

                                    #endregion 找出該格子的四角座標

                                    ImageForShow.Draw(
                                        new LineSegment2DF(GridLT, GridRT),
                                            new Bgr(Color.White),
                                            1);

                                    ImageForShow.Draw(
                                        new LineSegment2DF(GridRT, GridRD),
                                            new Bgr(Color.White),
                                            1);

                                    ImageForShow.Draw(
                                        new LineSegment2DF(GridRD, GridLD),
                                            new Bgr(Color.White),
                                            1);

                                    ImageForShow.Draw(
                                        new LineSegment2DF(GridLT, GridLD),
                                            new Bgr(Color.White),
                                            1);

                                    #endregion 如果是梅花座上的座標，將這個格子繪製出來
                                }
                            }
                        }

                        #endregion 畫長方形的網格，先建立暫時的RectangleGrid，然後利用之繪出格子

                        break;

                    default:
                        MessageBox.Show("未預期：有定義網格邊界，卻無網格類型");
                        break;
                }
            }

            #endregion 畫網格

            int EdgeWide = 1;
            if (ckbDrawBold.Checked)
            {
                EdgeWide = 0;
            }
            foreach (MCvBox2D box in BaseMcvBox2DList)
            {
                ImageForShow.Draw(box, new Bgr(Color.Red), EdgeWide);
            }
        }

        private void picboxBorse_Click(object sender, EventArgs e)
        {
            drawThumbnailImage();
            drawWatchArea();
        }

        private void picboxBorse_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownInPicboxThumbnail = true;
        }

        private void picboxBorse_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDownInPicboxThumbnail = false;
        }

        private void picboxBorse_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDownInPicboxThumbnail)
            {
                drawThumbnailImage();
                drawWatchArea();
            }
        }

        private void picboxWatchArea_MouseDown(object sender, MouseEventArgs e)
        {
            if (WatchAreaMode == WatchAreaModeNum.Browse)
            {
                mouseDownInPicboxWatchArea = true;
                pointDragStart = getMouseLocationRelayTopicboxWatchArea();
                CenterOfWatchAreaWhenDragStart = centerOfWatchArea;
            }
        }

        private void picboxWatchArea_MouseUp(object sender, MouseEventArgs e)
        {
            if (WatchAreaMode == WatchAreaModeNum.Browse)
            {
                mouseDownInPicboxWatchArea = false;
            }
        }

        private void picboxWatchArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (WatchAreaMode == WatchAreaModeNum.Browse && mouseDownInPicboxWatchArea)
            {
                drawWatchArea();
                drawThumbnailImage();
            }
        }

        private void lbxTileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxTileList.SelectedIndex != -1 && WatchAreaMode == WatchAreaModeNum.Browse) //沒有選取的時候是-1
            {
                #region 先洗掉之前選取的

                drawImageForShow();

                #endregion 先洗掉之前選取的

                #region 標明選取的，重繪

                ImageForShow.Draw(BaseMcvBox2DList[lbxTileList.SelectedIndex], new Bgr(Color.Yellow), 1);
                drawWatchArea(
                    (int)BaseMcvBox2DList[lbxTileList.SelectedIndex].center.X,
                    (int)BaseMcvBox2DList[lbxTileList.SelectedIndex].center.Y);
                drawThumbnailImage(
                    (int)(BaseMcvBox2DList[lbxTileList.SelectedIndex].center.X * ScaleOfThumbnail),
                    (int)(BaseMcvBox2DList[lbxTileList.SelectedIndex].center.Y * ScaleOfThumbnail));

                #endregion 標明選取的，重繪
            }
        }

        private void btnDelTile_Click(object sender, EventArgs e)
        {
            if (lbxTileList.SelectedIndex != -1)
            {
                BaseMcvBox2DList.RemoveAt(lbxTileList.SelectedIndex);
                lbxTileList.Items.RemoveAt(lbxTileList.SelectedIndex);
                drawImageForShow();
                drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                for (int index = 0; index < BaseMcvBox2DList.Count; index++)
                {
                    string boxNum = "tile" + index.ToString();
                    lbxTileList.Items[index] = boxNum;
                }
                lblTileCount.Text = "總數：" + BaseMcvBox2DList.Count.ToString();
            }
        }

        private void btnCreatTile_Click(object sender, EventArgs e)
        {
            WatchAreaMode = WatchAreaModeNum.AddNewBox;
            btnDelTile.Enabled = false;
            lblTileCount.Text = "正在新增第1個點";
        }

        private void picboxWatchArea_Click(object sender, EventArgs e)
        {
            switch (WatchAreaMode)
            {
                case WatchAreaModeNum.AddNewBox:

                    #region 新增新的磁磚

                    if (pointsOfAddingTile.Count < 4)
                    {
                        Point inputPoint = getMouseLocationRelayToBaseImage();

                        pointsOfAddingTile.Add(inputPoint);
                        lblTileCount.Text = "正在新增第" + (pointsOfAddingTile.Count + 1).ToString() + "個點";
                        if (pointsOfAddingTile.Count == 4)
                        {
                            #region 找到四邊型中心點

                            int CenterX = 0;
                            int CenterY = 0;
                            for (int index = 0; index < 4; index++)
                            {
                                CenterX += pointsOfAddingTile[index].X;
                                CenterY += pointsOfAddingTile[index].Y;
                            }
                            Point centerOfTile = new Point(CenterX / 4, CenterY / 4);

                            #endregion 找到四邊型中心點

                            #region 區分出四個座標點分別會是四邊型的那個角落

                            Tile tempTile = new Tile();
                            for (int index = 0; index < 4; index++)
                            {
                                if (pointsOfAddingTile[index].X < centerOfTile.X &&
                                    pointsOfAddingTile[index].Y < centerOfTile.Y)
                                {
                                    tempTile.conerLT = pointsOfAddingTile[index];
                                }
                                if (pointsOfAddingTile[index].X < centerOfTile.X &&
                                    pointsOfAddingTile[index].Y > centerOfTile.Y)
                                {
                                    tempTile.conerLD = pointsOfAddingTile[index];
                                }
                                if (pointsOfAddingTile[index].X > centerOfTile.X &&
                                    pointsOfAddingTile[index].Y < centerOfTile.Y)
                                {
                                    tempTile.conerRT = pointsOfAddingTile[index];
                                }
                                if (pointsOfAddingTile[index].X > centerOfTile.X &&
                                    pointsOfAddingTile[index].Y > centerOfTile.Y)
                                {
                                    tempTile.conerRD = pointsOfAddingTile[index];
                                }
                            }

                            #endregion 區分出四個座標點分別會是四邊型的那個角落

                            #region 產生MCvBox2D對應到使用者輸入的四個點

                            float height, width, angle;
                            height = (float)(
                                myMath.GetDistance(tempTile.conerLT, tempTile.conerLD) +
                                myMath.GetDistance(tempTile.conerRT, tempTile.conerRD)) / 2;
                            width = (float)(
                                myMath.GetDistance(tempTile.conerLT, tempTile.conerRT) +
                                myMath.GetDistance(tempTile.conerLD, tempTile.conerRD)) / 2;
                            angle = (
                                myMath.getAngle(tempTile.conerLT, tempTile.conerRT) +
                                myMath.getAngle(tempTile.conerLD, tempTile.conerRD)) / 2;

                            MCvBox2D tempBox = new MCvBox2D((PointF)centerOfTile, new SizeF(width, height), angle);

                            #endregion 產生MCvBox2D對應到使用者輸入的四個點

                            #region 將新的MCvBox2D丟到BaseMcv2DList，重建UI清單

                            BaseMcvBox2DList.Add(tempBox);
                            lbxTileList.Items.Add("newtile");
                            for (int index = 0; index < BaseMcvBox2DList.Count; index++)
                            {
                                string boxNum = "tile" + index.ToString();
                                lbxTileList.Items[index] = boxNum;
                            }
                            pointsOfAddingTile.Clear();

                            #endregion 將新的MCvBox2D丟到BaseMcv2DList，重建UI清單

                            #region 重繪

                            lbxTileList.SelectedIndex = lbxTileList.Items.Count - 1;
                            drawImageForShow();
                            ImageForShow.Draw(BaseMcvBox2DList[BaseMcvBox2DList.Count - 1], new Bgr(Color.Yellow), 1);

                            #region true

                            drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                            drawThumbnailImage(CenterOfThumbnailWatchArea.X, CenterOfThumbnailWatchArea.Y);

                            #endregion true

                            WatchAreaMode = WatchAreaModeNum.Browse;

                            #endregion 重繪

                            btnDelTile.Enabled = true;
                            lblTileCount.Text = "總數：" + BaseMcvBox2DList.Count.ToString();
                        }
                    }

                    #endregion 新增新的磁磚

                    break;

                case WatchAreaModeNum.defWorkingspace:

                    #region 輸入工作區邊界

                    switch (WorkspaceType)
                    {
                        case Grid.SquareGrid:

                            #region 輸入正方形工作區邊界

                            if (WorkspaceLeftTop == Point.Empty)
                            {
                                #region 如果左上點尚未建立，則輸入座標為左上點，提醒使用者輸入右下點

                                WorkspaceLeftTop = getMouseLocationRelayToBaseImage();
                                lblMsg.Text = "請點擊工作區右下角";

                                #endregion 如果左上點尚未建立，則輸入座標為左上點，提醒使用者輸入右下點
                            }
                            else
                            {
                                #region 輸入右下點

                                WorkspaceRightDown = getMouseLocationRelayToBaseImage();

                                #region 依據數學模型，將右下點輸入點修正到理論上的位置

                                int xError = WorkspaceRightDown.X - WorkspaceLeftTop.X;
                                int yError = WorkspaceRightDown.Y - WorkspaceLeftTop.X;
                                double avgError = xError * 0.5 + yError * 0.5;
                                WorkspaceRightDown = new Point(WorkspaceLeftTop.X + (int)avgError, WorkspaceLeftTop.Y + (int)avgError);

                                #endregion 依據數學模型，將右下點輸入點修正到理論上的位置

                                lblMsg.Text = "正方型工作區:" + WorkspaceLeftTop.ToString() + WorkspaceRightDown.ToString();

                                #region 重新繪製底圖，出現網格

                                drawImageForShow();
                                drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                                drawThumbnailImage(CenterOfThumbnailWatchArea.X, CenterOfThumbnailWatchArea.Y);

                                #endregion 重新繪製底圖，出現網格

                                WatchAreaMode = WatchAreaModeNum.Browse;

                                #endregion 輸入右下點
                            }

                            #endregion 輸入正方形工作區邊界

                            break;

                        case Grid.RectangleGrid:

                            #region 輸入長方形工作區邊界

                            if (WorkspaceLeftTop == Point.Empty)
                            {
                                #region 如果左上點尚未建立，則輸入座標為左上點，提醒使用者輸入右下點

                                WorkspaceLeftTop = getMouseLocationRelayToBaseImage();
                                lblMsg.Text = "請點擊工作區右下角";

                                #endregion 如果左上點尚未建立，則輸入座標為左上點，提醒使用者輸入右下點
                            }
                            else
                            {
                                #region 輸入右下點

                                WorkspaceRightDown = getMouseLocationRelayToBaseImage();

                                #region 依據數學模型，將右下點輸入點修正到理論上的位置

#if false
                                double hypotenuseInput = myMath.GetDistance(WorkspaceLeftTop, WorkspaceRightDown);
                                WorkspaceRightDown = new Point(
                                    (int)(RectangleGrids.Length / RectangleGrids.hypotenuse * hypotenuseInput) + WorkspaceLeftTop.X,
                                    (int)(RectangleGrids.Width / RectangleGrids.hypotenuse * hypotenuseInput) + WorkspaceLeftTop.Y);
#endif
                                double hypotenuseInput = myMath.GetDistance(WorkspaceLeftTop, WorkspaceRightDown);
                                WorkspaceRightDown = new Point(
                                    (int)(RectangleGrids.Length / RectangleGrids.hypotenuse * hypotenuseInput) + WorkspaceLeftTop.X,
                                    (int)(RectangleGrids.Width / RectangleGrids.hypotenuse * hypotenuseInput) + WorkspaceLeftTop.Y);

                                #endregion 依據數學模型，將右下點輸入點修正到理論上的位置

                                lblMsg.Text = "長方形工作區:" + WorkspaceLeftTop.ToString() + WorkspaceRightDown.ToString();

                                #region 重新繪製底圖，出現網格

                                drawImageForShow();
                                drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                                drawThumbnailImage(CenterOfThumbnailWatchArea.X, CenterOfThumbnailWatchArea.Y);

                                #endregion 重新繪製底圖，出現網格

                                WatchAreaMode = WatchAreaModeNum.Browse;

                                #endregion 輸入右下點
                            }

                            #endregion 輸入長方形工作區邊界

                            break;

                        default:
                            MessageBox.Show("未預期：沒有定義工作區類型卻執行定義工作區座標點");
                            break;
                    }

                    #endregion 輸入工作區邊界

                    break;

                default:
                    // MessageBox.Show("未預期：watchAreaMode未被定義，click事件被觸發");
                    break;
            }
        }

        private void picboxWatchArea_DoubleClick(object sender, EventArgs e)
        {
            if (WatchAreaMode == WatchAreaModeNum.Browse)
            {
                WatchAreaMode = WatchAreaModeNum.doubleClicking;
                Point pointMousePosition = getMouseLocationRelayToBaseImage();
                int indexOfNerestTile = 0;
                double distanceLog = myMath.GetDistance(BaseMcvBox2DList[0].center, pointMousePosition);

                for (int index = 1; index < BaseMcvBox2DList.Count; index++)
                {
                    if (myMath.GetDistance(BaseMcvBox2DList[index].center, pointMousePosition) < distanceLog)
                    {
                        distanceLog = myMath.GetDistance(BaseMcvBox2DList[index].center, pointMousePosition);
                        indexOfNerestTile = index;
                    }
                }

                #region 繪製使用者雙擊的tile

                lbxTileList.SelectedIndex = indexOfNerestTile;
                drawImageForShow();

                ImageForShow.Draw(BaseMcvBox2DList[indexOfNerestTile], new Bgr(Color.Yellow), 1);
                drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                drawThumbnailImage(CenterOfThumbnailWatchArea.X, CenterOfThumbnailWatchArea.Y);

                #endregion 繪製使用者雙擊的tile

                WatchAreaMode = WatchAreaModeNum.Browse;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                Image image = BaseImage.Bitmap;
                image.Save(ms, ImageFormat.Png);
                ms.Close();
                imageBytes = ms.ToArray();
            }

            IdentifyTileFileV3 v3File = new IdentifyTileFileV3
            {
                BaseImageBytes = imageBytes,
                Boxes = BaseMcvBox2DList,
                WorkAreaLeftTop = WorkspaceLeftTop,
                WorkAreaRightDown = WorkspaceRightDown,
                WorkAreaType = WorkspaceType
            };
            var newFileName = $"{Path.GetDirectoryName(FileName)}\\{Path.GetFileNameWithoutExtension(FileName)}.json";
            v3File.Save(newFileName);
            Invoke(new Action(() => { lblMsg.Text = $"已儲存為{newFileName}"; }));
        }

        /// <summary>載入舊檔</summary>
        private void btnLoad_Click(object sender, EventArgs e)
        {
            FileName = "";
            openFileDialog1.Filter = "data|*.data";
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            FileName = openFileDialog1.FileName;
            var v2File = OpenV2File(FileName);
            WorkspaceType = v2File.WorkAreaType;
            WorkspaceLeftTop = v2File.WorkAreaLeftTop;
            WorkspaceRightDown = v2File.WorkAreaRightDown;
            LoadIDTiles(v2File.GetImage(), v2File.Boxes, FileName);
        }

        /// <summary>打開第二版檔案</summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static IdentifyTileFileV3 OpenV2File(string fileName)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            Stream input = File.OpenRead(fileName);
            object inputDeserialize = formatter.Deserialize(input);
            input.Close();
            IdentifyTileFileV2 tempTileFile = (IdentifyTileFileV2) inputDeserialize;
            var v3File = IdentifyTileFileV3.FromIdentifyTileFileV2(tempTileFile);
            
            return v3File;
        }

        /// <summary>依據給予的檔名，設定比例參數</summary>
        private void SetPixelPerCentimeter(string fileName)
        {
            double value;
            try
            {
                value = PixelPerCentimeter[fileName];
            }
            catch (KeyNotFoundException)
            {
                //MessageBox.Show("無可用比例參數");
                return;
            }
            Invoke(new Action(() => { nudPixelPerCentimeter.Value = (decimal)value; }));
        }

        /// <summary>對正方形磁磚評分</summary>
        private void btnGiveGradeSquare_Click(object sender, EventArgs e)
        {
            try
            {
                ScoreSquareSample();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        /// <summary>評分正方磚</summary>
        private TilesReport ScoreSquareSample(bool drawImage = false,bool isTop = true)
        {
            if (WorkspaceLeftTop == Point.Empty || WorkspaceRightDown == Point.Empty)
            {
                throw new Exception("請先設定工作區域");
            }
            if (BaseMcvBox2DList.Count != SquareGrids.ROW_COUNT * SquareGrids.COLUMN_COUNT)
            {
                throw new Exception("磁磚數量不正確，無法評分");
            }

            SquareTiles mySquareTiles = new SquareTiles(WorkspaceRightDown, WorkspaceLeftTop, BaseMcvBox2DList, FileName, rankArea);
            mySquareTiles.Report.ScoringByVariance(TilesType.Square,isTop);
            if (mySquareTiles.msg == SquareTiles.error_TilePosition)
            {
                throw new Exception("磁磚位置有誤，請檢查");
            }

            //繪製磁磚
            if (drawImage)
            {
                drawImageForShow();
                MCvFont f = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX, 1, 1);
                for (int columnNum = 0; columnNum < SquareGrids.COLUMN_COUNT; columnNum++)
                {
                    for (int rowNum = 0; rowNum < SquareGrids.ROW_COUNT; rowNum++)
                    {
                        string output = "(" + columnNum + "," + rowNum + ")";
                        ImageForShow.Draw(output, ref f, Point.Round(mySquareTiles.Tiles[columnNum, rowNum].center), new Bgr(Color.Black));
                    }
                }
            }

            //繪製誤差標記
            if (DRAW_ERR)
            {
                foreach (var item in mySquareTiles.CurrentErrMark.GapErr)
                {
                    ImageForShow.Draw(item, new Bgr(0, 0, 255), 3);
                }
            }

            //輸出磁磚位置
            if (PRINT_TILES_LOCATION)
            {
                using (StreamWriter sw = new StreamWriter("TilesLocation.txt"))
                {
                    for (int column = 0; column < mySquareTiles.Tiles.GetLength(0); column++)
                    {
                        for (int row = 0; row < mySquareTiles.Tiles.GetLength(1); row++)
                        {
                            string line = column.ToString() + "," + row.ToString();
                            line += "\t" + mySquareTiles.Tiles[column, row].center.X.ToString();
                            line += "\t" + mySquareTiles.Tiles[column, row].center.Y.ToString();
                            line += "\t" + mySquareTiles.Tiles[column, row].angle.ToString();
                            sw.WriteLine(line);
                        }
                    }
                    sw.Close();
                }
                Process.Start("TilesLocation.txt");
            }

            return mySquareTiles.Report;
        }

        /// <summary>對長方形磁磚評分</summary>
        private void btnGiveGradeRectangle_Click(object sender, EventArgs e)
        {
            try
            {
                ScoreRectangleSample();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        /// <summary>評分丁掛磚</summary>
        private TilesReport ScoreRectangleSample(bool drawImage = false,bool isTop = true)
        {
            if (WorkspaceLeftTop == Point.Empty || WorkspaceRightDown == Point.Empty)
            {
                throw new Exception("請先設定工作區域");
            }
            if (BaseMcvBox2DList.Count != RectangleGrids.tileCount)
            {
                throw new Exception("磁磚數量不正確，無法評分");
            }

            RectangleTiles myRetangleTiles = new RectangleTiles(WorkspaceRightDown, WorkspaceLeftTop, BaseMcvBox2DList, FileName, rankArea);
            myRetangleTiles.Report.ScoringByVariance(TilesType.Rectangle,isTop);
            if (myRetangleTiles.msg == RectangleTiles.error_TilePosition)
            {
                throw new Exception("磁磚位置有誤，請檢查");
            }

            //繪製磁磚
            if (drawImage)
            {
                drawImageForShow();
                MCvFont f = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX, 1, 1);
                for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
                {
                    for (int rowNum = 0; rowNum < RectangleGrids.ROW_COUNT; rowNum++)
                    {
                        if ((columnNum + rowNum) % 2 != 1)
                        {
                            string output = "(" + columnNum + "," + rowNum + ")";
                            ImageForShow.Draw(output, ref f, Point.Round(myRetangleTiles.Tiles[columnNum, rowNum].center), new Bgr(Color.Black));
                        }
                    }
                }
            }

            //輸出磁磚位置
            if (PRINT_TILES_LOCATION)
            {
                using (StreamWriter sw = new StreamWriter("TilesLocation.txt"))
                {
                    for (int column = 0; column < myRetangleTiles.Tiles.GetLength(0); column++)
                    {
                        for (int row = 0; row < myRetangleTiles.Tiles.GetLength(1); row++)
                        {
                            if ((column + row) % 2 != 1)
                            {
                                string line = column.ToString() + "," + row.ToString();
                                line += "\t" + myRetangleTiles.Tiles[column, row].center.X.ToString();
                                line += "\t" + myRetangleTiles.Tiles[column, row].center.Y.ToString();
                                line += "\t" + myRetangleTiles.Tiles[column, row].angle.ToString();
                                sw.WriteLine(line);
                            }
                        }
                    }
                    sw.Close();
                }
                Process.Start("TilesLocation.txt");
            }

            return myRetangleTiles.Report;
        }

        #region 定義工作區按鈕

        private void btnDimSquareWorkingspace_Click(object sender, EventArgs e)
        {
            WatchAreaMode = WatchAreaModeNum.defWorkingspace;
            lblMsg.Text = "請點擊工作區左上角";
            WorkspaceLeftTop = Point.Empty;
            WorkspaceRightDown = Point.Empty;
            WorkspaceType = Grid.SquareGrid;
        }

        private void btnDimRectangleWorkingspace_Click(object sender, EventArgs e)
        {
            WatchAreaMode = WatchAreaModeNum.defWorkingspace;
            lblMsg.Text = "請點擊工作區左上角";
            WorkspaceLeftTop = Point.Empty;
            WorkspaceRightDown = Point.Empty;
            WorkspaceType = Grid.RectangleGrid;
        }

        #endregion 定義工作區按鈕

        private void FormUserDef_Load(object sender, EventArgs e)
        {
            #region 載入比例參數

            if (File.Exists(@"Z:\實驗數據\磁磚照輸出\比例參數.txt"))
            {
                LoadPixelPerCentimeter(@"Z:\實驗數據\磁磚照輸出\比例參數.txt");
                MessageBox.Show(@"採用 Z:\實驗數據\磁磚照輸出\比例參數.txt");
            }
            else if (File.Exists(@"比例參數.txt"))
            {
                LoadPixelPerCentimeter(@"比例參數.txt");
                MessageBox.Show(@"採用 比例參數.txt");
            }
            else
            {
                MessageBox.Show("無可用的比例參數文件");
            }

            #endregion 載入比例參數
        }

        /// <summary>載入比例參數</summary>
        private void LoadPixelPerCentimeter(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    string[] newLine = sr.ReadLine().Split('\t');
                    double value;
                    try
                    {
                        value = Convert.ToDouble(newLine[1]);
                    }
                    catch
                    {
                        MessageBox.Show("載入預設比例參數文件失敗");
                        return;
                    }
                    PixelPerCentimeter.Add(newLine[0], value);
                }
                sr.Close();
            }
        }

        private void FormUserDef_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        #region 鍵盤事件

        private void FormUserDef_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    spaceDown = true;
                    break;

                case Keys.P:
                    if (lbxTileList.SelectedIndex != -1 && lbxTileList.SelectedIndex < lbxTileList.Items.Count - 1)
                    {
                        lbxTileList.SelectedIndex += 1;
                    }
                    e.Handled = true;
                    break;

                case Keys.I:
                    if (lbxTileList.SelectedIndex != -1 && lbxTileList.SelectedIndex > 0)
                    {
                        lbxTileList.SelectedIndex -= 1;
                    }
                    e.Handled = true;
                    break;

                case Keys.O:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.size.Height += 1;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;
                    break;

                case Keys.L:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.size.Height -= 1;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;
                    break;

                case Keys.K:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.size.Width -= 1;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }

                    e.Handled = true;
                    break;

                case Keys.OemSemicolon:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.size.Width += 1;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }

                    e.Handled = true;
                    break;

                case Keys.Up:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.center.Y -= 1;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;
                    break;

                case Keys.Down:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.center.Y += 1;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;
                    break;

                case Keys.Left:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.center.X -= 1;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;
                    break;

                case Keys.Right:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.center.X += 1;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;
                    break;

                case Keys.PageUp:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.angle -= 0.1f;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;
                    break;

                case Keys.PageDown:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.angle += 0.1f;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;
                    break;

                case Keys.Home:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.size.Height += 1;
                        temp.size.Width += 1;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;
                    break;

                case Keys.End:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.size.Height -= 1;
                        temp.size.Width -= 1;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;
                    break;

                case Keys.W:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.center.Y -= 0.2f;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;

                    break;

                case Keys.S:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.center.Y += 0.2f;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;

                    break;

                case Keys.A:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.center.X -= 0.2f;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;

                    break;

                case Keys.D:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.center.X += 0.2f;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;

                    break;

                case Keys.Q:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];

                        temp.angle -= 0.02f;

                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;

                    break;

                case Keys.E:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];

                        temp.angle += 0.02f;

                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;

                    break;

                case Keys.X:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];

                        temp.angle = 0.00f;

                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;
                    break;

                case Keys.R:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.size.Height += 1;
                        temp.size.Width += 1;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;

                    break;

                case Keys.F:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        MCvBox2D temp = BaseMcvBox2DList[lbxTileList.SelectedIndex];
                        temp.size.Height -= 1;
                        temp.size.Width -= 1;
                        BaseMcvBox2DList[lbxTileList.SelectedIndex] = temp;
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;

                    break;

                case Keys.C:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        AutoTurnTile(lbxTileList.SelectedIndex);

                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;
                    break;

                case Keys.Z:
                    if (lbxTileList.SelectedIndex != -1)
                    {
                        tileEditTimeUpDate();
                        AutoShiftTile(lbxTileList.SelectedIndex);
                        drawImageForShow();
                        drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                    }
                    e.Handled = true;
                    break;

                default:
                    break;
            }
        }

        /// <summary>取得磁磚邊緣灰度</summary>
        private double GetTileEdgeBrightness(Tile tempTile)
        {
            return tempTile.AllEdge.Select(p => BaseImage.GetGray(p.X, p.Y)).Average();
        }

        /// <summary>取得點組平均亮度</summary>
        private double GetBrightness(IEnumerable<PointF> pointSet)
        {
            return pointSet.Select(p => BaseImage.GetGray(p.X, p.Y)).Average();
        }

        /// <summary>自動旋轉磁磚</summary>
        private void AutoTurnTile(int tileIndex)
        {
#if false
            double GrayNow = getTileEdgeBrightness(new Tile(BaseMcvBox2DList[tileIndex]));
            double ClockwiseMaxGray = GrayNow;
            double CounterclockwiseMaxGray = GrayNow;
            MCvBox2D tempClockwiseTile = BaseMcvBox2DList[tileIndex];
            MCvBox2D tempCounterclockwiseTile = BaseMcvBox2DList[tileIndex];
            while (true)
            {
                tempClockwiseTile.angle += 0.01f;
                double tempGray = getTileEdgeBrightness(new Tile(tempClockwiseTile));
                if (tempGray > ClockwiseMaxGray)
                {
                    ClockwiseMaxGray = tempGray;
                }
                else if (tempGray < ClockwiseMaxGray)
                {
                    tempClockwiseTile.angle -= 0.01f;
                    break;
                }
            }
            while (true)
            {
                tempCounterclockwiseTile.angle -= 0.01f;
                double tempGray = getTileEdgeBrightness(new Tile(tempCounterclockwiseTile));
                if (tempGray > CounterclockwiseMaxGray)
                {
                    CounterclockwiseMaxGray = tempGray;
                }
                else if (tempGray < CounterclockwiseMaxGray)
                {
                    tempClockwiseTile.angle += 0.01f;
                    break;
                }
            }
            if (ClockwiseMaxGray > CounterclockwiseMaxGray && ClockwiseMaxGray > GrayNow)
            {
                BaseMcvBox2DList[tileIndex] = tempClockwiseTile;
            }
            else if (CounterclockwiseMaxGray > ClockwiseMaxGray && CounterclockwiseMaxGray > GrayNow)
            {
                BaseMcvBox2DList[tileIndex] = tempCounterclockwiseTile;
            }
#endif
            Dictionary<MCvBox2D, double> shiftBoxAndBrightness = new Dictionary<MCvBox2D, double>();
            MCvBox2D initialBox = BaseMcvBox2DList[tileIndex];
            double initialBrightness = GetTileEdgeBrightness(new Tile(initialBox));
            shiftBoxAndBrightness.Add(initialBox, initialBrightness);
            for (float rotate = -0.01f; rotate <= 0.01f; rotate += 0.02f)
            {
                MCvBox2D tempBox = initialBox;
                double tempMaxBrightness = initialBrightness;
                for (int step = 1; ; step++)
                {
                    tempBox.angle += rotate * step;
                    double tempBrightness = GetTileEdgeBrightness(new Tile(tempBox));
                    if (tempBrightness > tempMaxBrightness)
                    {
                        tempMaxBrightness = tempBrightness;
                        shiftBoxAndBrightness.Add(tempBox, tempBrightness);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            var max = shiftBoxAndBrightness.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            if (max.Equals(initialBox))
            {
                return;
            }
            BaseMcvBox2DList[tileIndex] = max;
        }

        /// <summary>自動微調磁磚位置</summary>
        private void AutoShiftTile(int tileIndex)
        {
            while (true)
            {
                Dictionary<MCvBox2D, double> shiftAndBrightness = new Dictionary<MCvBox2D, double>();
                double initialBrightness = GetTileEdgeBrightness(new Tile(BaseMcvBox2DList[tileIndex]));
                MCvBox2D initialbox = BaseMcvBox2DList[tileIndex];
                shiftAndBrightness.Add(initialbox, initialBrightness);
                PointF[] directSet = new PointF[] {
                    new PointF(0, 0.2f),
                    new PointF(0.2f, 0),
                    new PointF(0, -0.2f),
                    new PointF(-0.2f, 0),};
                foreach (var direct in directSet)
                {
                    double tempMaxBrightness = initialBrightness;
                    MCvBox2D tempBox = initialbox;
                    for (int step = 1; ; step++)
                    {
                        tempBox.center.X += direct.X * step;
                        tempBox.center.Y += direct.Y * step;
                        double tempBrightness = GetTileEdgeBrightness(new Tile(tempBox));
                        if (tempBrightness > tempMaxBrightness)
                        {
                            tempMaxBrightness = tempBrightness;
                            shiftAndBrightness.Add(tempBox, tempBrightness);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                MCvBox2D selectBox = shiftAndBrightness.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                if (selectBox.Equals(initialbox))
                {
                    break;
                }
                BaseMcvBox2DList[tileIndex] = selectBox;
            }
#if false
            while (true)
            {
                double GrayNow = getTileEdgeGray(new Tile(BaseMcvBox2DList[tileIndex]));
                double RightMaxGray = GrayNow;
                double LeftMaxGray = GrayNow;
                double UpMaxGray = GrayNow;
                double DownMaxGray = GrayNow;
                MCvBox2D tempRightTile = BaseMcvBox2DList[tileIndex];
                MCvBox2D tempLeftTile = BaseMcvBox2DList[tileIndex];
                MCvBox2D tempUpTile = BaseMcvBox2DList[tileIndex];
                MCvBox2D tempDownTile = BaseMcvBox2DList[tileIndex];
                while (true)
                {
                    tempRightTile.center.X += 1;
                    double tempGray = getTileEdgeGray(new Tile(tempRightTile));
                    if (tempGray > RightMaxGray)
                    {
                        RightMaxGray = tempGray;
                    }
                    else if (tempGray < RightMaxGray)
                    {
                        tempRightTile.center.X -= 1;
                        break;
                    }
                }
                while (true)
                {
                    tempLeftTile.center.X -= 1;
                    double tempGray = getTileEdgeGray(new Tile(tempLeftTile));
                    if (tempGray > LeftMaxGray)
                    {
                        LeftMaxGray = tempGray;
                    }
                    else if (tempGray < LeftMaxGray)
                    {
                        tempLeftTile.center.X += 1;
                        break;
                    }
                }
                while (true)
                {
                    tempUpTile.center.Y -= 1;
                    double tempGray = getTileEdgeGray(new Tile(tempUpTile));
                    if (tempGray > UpMaxGray)
                    {
                        UpMaxGray = tempGray;
                    }
                    else if (tempGray < UpMaxGray)
                    {
                        tempUpTile.center.Y += 1;
                        break;
                    }
                }
                while (true)
                {
                    tempDownTile.center.Y += 1;
                    double tempGray = getTileEdgeGray(new Tile(tempDownTile));
                    if (tempGray > DownMaxGray)
                    {
                        DownMaxGray = tempGray;
                    }
                    else if (tempGray < DownMaxGray)
                    {
                        tempDownTile.center.Y -= 1;
                        break;
                    }
                }
                double[] MaxGrays = new double[] { RightMaxGray, LeftMaxGray, UpMaxGray, DownMaxGray };
                if (GrayNow >= MaxGrays.Max())
                {
                    break;
                }
                else
                {
                    if (RightMaxGray == MaxGrays.Max())
                    {
                        BaseMcvBox2DList[tileIndex] = tempRightTile;
                    }
                    else if (LeftMaxGray == MaxGrays.Max())
                    {
                        BaseMcvBox2DList[tileIndex] = tempLeftTile;
                    }
                    else if (UpMaxGray == MaxGrays.Max())
                    {
                        BaseMcvBox2DList[tileIndex] = tempUpTile;
                    }
                    else if (DownMaxGray == MaxGrays.Max())
                    {
                        BaseMcvBox2DList[tileIndex] = tempDownTile;
                    }
                    else
                    {
                        MessageBox.Show("調整磁磚時發生未預期的狀況");
                    }
                }
            }
#endif
        }

        /// <summary>自動調整磁磚大小</summary>
        /// <param name="tileIndex">Index of the tile.</param>
        private void AutoResizeTile(int tileIndex)
        {
            #region 本函式使用常數

            //搜尋精度
            const float adjustAmount = 0.2f;
            //搜尋數量
            const int searchStep = 20;

            #endregion 本函式使用常數

            Tile initialTile = new Tile(BaseMcvBox2DList[tileIndex]);

            #region 準備調整向量

            Dictionary<EdgeDirection, PointF> adjustUnitVector = new Dictionary<EdgeDirection, PointF>();
            adjustUnitVector.Add(EdgeDirection.Top, new PointF(0, -adjustAmount));
            adjustUnitVector.Add(EdgeDirection.Down, new PointF(0, adjustAmount));
            adjustUnitVector.Add(EdgeDirection.Right, new PointF(adjustAmount, 0));
            adjustUnitVector.Add(EdgeDirection.Left, new PointF(-adjustAmount, 0));

            #endregion 準備調整向量

            #region 準備資料庫

            Dictionary<EdgeDirection, PointF[]> edgeData = new Dictionary<EdgeDirection, PointF[]>();
            Dictionary<EdgeDirection, Dictionary<PointF, double>> brightnessDic = new Dictionary<EdgeDirection, Dictionary<PointF, double>>();
            Dictionary<EdgeDirection, Dictionary<PointF, double>> brightnessDiffDic = new Dictionary<EdgeDirection, Dictionary<PointF, double>>();
            Dictionary<EdgeDirection, PointF> selectedEdgeAdjust = new Dictionary<EdgeDirection, PointF>();
            foreach (var direct in SmileLib.EnumTool.GetMembers<EdgeDirection>())
            {
                edgeData.Add(direct, initialTile.HalfEdge[direct]);
                brightnessDic.Add(direct, new Dictionary<PointF, double>());
                brightnessDiffDic.Add(direct, new Dictionary<PointF, double>());
            }

            #endregion 準備資料庫

            #region 依四個方向處理四邊

            foreach (var direct in SmileLib.EnumTool.GetMembers<EdgeDirection>())
            {
                PointF unitVector = adjustUnitVector[direct];
                for (int step = -searchStep; step <= searchStep; step++)
                {
                    PointF vector = new PointF(step * unitVector.X, step * unitVector.Y);
                    var adjustedPointFSet = from point in edgeData[direct]
                                            select new PointF(
                                                point.X + vector.X,
                                                point.Y + vector.Y);
                    double adjustedBrightness = GetBrightness(adjustedPointFSet);
                    brightnessDic[direct].Add(vector, adjustedBrightness);
                }
                for (int step = 0; step < brightnessDic[direct].Count - 1; step++)
                {
                    PointF vectorA = brightnessDic[direct].ElementAt(step).Key;
                    double brightnessA = brightnessDic[direct].ElementAt(step).Value;
                    PointF vectorB = brightnessDic[direct].ElementAt(step + 1).Key;
                    double brightnessB = brightnessDic[direct].ElementAt(step + 1).Value;
                    PointF vectorAvg = myTool.AvgPointF(vectorA, vectorB);
                    double brightnessDiff = brightnessB - brightnessA;
                    brightnessDiffDic[direct].Add(vectorAvg, brightnessDiff);
                }
                PointF selectVector = brightnessDiffDic[direct].
                    Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                selectedEdgeAdjust.Add(direct, selectVector);
            }

            Console.WriteLine();

            #endregion 依四個方向處理四邊
        }

        private void FormUserDef_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    spaceDown = false;
                    break;

                default:
                    break;
            }
        }

        private void tileEditTimeUpDate()
        {
            string title = lbxTileList.SelectedItem.ToString();

            char[] splitChar = { ' ' };
            string[] tempTitle = title.Split(splitChar);
            title = tempTitle[0] + " " + DateTime.Now.ToString(@"HH:mm:ss.ff");
            //            lbxTileList.SelectedItem = title;
            lbxTileList.Items[lbxTileList.SelectedIndex] = title;
            lblEditMsg.Text = title;
        }

        #endregion 鍵盤事件

        #region 正規化磁磚

        private void btnSquareTileRegularize_Click(object sender, EventArgs e)
        {
            TileRegularize(TilesType.Square);
        }

        private void btnRectangleTileRegularize_Click(object sender, EventArgs e)
        {
            TileRegularize(TilesType.Rectangle);
        }

        /// <summary>正規化磁磚</summary>
        private void TileRegularize(TilesType theTilesType)
        {
            SizeF tempSize;
            switch (theTilesType)
            {
                case TilesType.Square:
                    tempSize = new SizeF((float)(SquareGrids.TILE_LENGTH * (double)nudPixelPerCentimeter.Value), (float)(SquareGrids.TILE_LENGTH * (double)nudPixelPerCentimeter.Value));
                    break;

                case TilesType.Rectangle:
                    tempSize = new SizeF((float)((RectangleGrids.tileLength + (double)nudRectangularTileAdj.Value) * (double)nudPixelPerCentimeter.Value),
                        (float)(RectangleGrids.tileWidth * (double)nudPixelPerCentimeter.Value));
                    break;

                default:
                    tempSize = new SizeF(0, 0);
                    break;
            }
            for (int index = 0; index < BaseMcvBox2DList.Count; index++)
            {
                MCvBox2D tempBox = BaseMcvBox2DList[index];
                tempBox.size = tempSize;
                BaseMcvBox2DList[index] = tempBox;
            }

            drawImageForShow();
            drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
            drawThumbnailImage(CenterOfThumbnailWatchArea.X, CenterOfThumbnailWatchArea.Y);
        }

        #endregion 正規化磁磚

        private void ckbShowWeb_CheckedChanged(object sender, EventArgs e)
        {
            drawImageForShow();
            drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
            drawThumbnailImage(CenterOfThumbnailWatchArea.X, CenterOfThumbnailWatchArea.Y);
        }

        private void btnIdentification_Click(object sender, EventArgs e)
        {
            if (Program.myIDForm == null || Program.myIDForm.IsDisposed)
            {
                Program.myIDForm = new IdentificationForm();
                Program.myIDForm.Show();
            }
        }

        private void btnAutoTurnAllTile_Click(object sender, EventArgs e)
        {
            AutoAdjustAllTile();
        }

        private void AutoAdjustAllTile()
        {
            for (int i = 0; i < BaseMcvBox2DList.Count; i++)
            {
                MCvBox2D temp = BaseMcvBox2DList[i];
                temp.size.Width -= 2;
                temp.size.Height -= 2;
                BaseMcvBox2DList[i] = temp;
                AutoShiftTile(i);
                AutoTurnTile(i);
                AutoShiftTile(i);
                AutoTurnTile(i);
                AutoShiftTile(i);
                AutoTurnTile(i);
                //AutoResizeTile(i);
                temp = BaseMcvBox2DList[i];
                temp.size.Width += 2;
                temp.size.Height += 2;
                BaseMcvBox2DList[i] = temp;
            }
            drawImageForShow();
            drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
        }

        private void rdbRankArea_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbRankTop.Checked)
            {
                rankArea = RankArea.Top;
            }
            else if (rdbRankDown.Checked)
            {
                rankArea = RankArea.Down;
            }
            else
            {
                throw SmileLib.EnumTool.OutOfEnum<RankArea>();
            }
        }

        private void btnBatchScore_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(BatchScore);
            t.Start();
        }

        /// <summary>針對辨識完成檔進行批次評分</summary>
        private void BatchScore()
        {
            //取得評分的檔案列表
            List<string> sourceFiles = GetBatchFiles(new[] { ".json" });
            SmileLib.MessageListBox myMessageListBox = new SmileLib.MessageListBox();
            var result = myMessageListBox.ShowDialog(sourceFiles, "查找到的檔案");
            if (result != DialogResult.OK)
            {
                return;
            }

            var fileList = myMessageListBox.SelectResult.ToList();

            //評分結果
            List<ScoreRecord> scoreRecords = new List<ScoreRecord>();

            //轉換網格類型為磁磚類型
            var gridToTileTypeMap = new Dictionary<Grid, TilesType>
            {
                [Grid.RectangleGrid] = TilesType.Rectangle,
                [Grid.SquareGrid] = TilesType.Square
            };

            //依據給定的網格類型，給予評分方法
            var scoreSampleMap = new Dictionary<Grid, Func<bool, bool, TilesReport>>
            {
                [Grid.RectangleGrid] = ScoreRectangleSample,
                [Grid.SquareGrid] = ScoreSquareSample
            };
            int fileCount = fileList.Count;
            Invoke(new Action(() => myProgressBar.Maximum = fileCount));
            int doneCount = 0;
            //對每個選定的檔案進行處理
            foreach (var item in myMessageListBox.SelectResult)
            {
                var doneCountNow = doneCount;
                Invoke(new Action(() =>
                {
                    lblMsg.Text = $"進度{doneCountNow}/{fileCount}";
                    myProgressBar.Value = doneCountNow;
                }));
                FileName = item;
                
                var tempTileFile = IdentifyTileFileV3.OpenFromFile(FileName);
                WorkspaceLeftTop = tempTileFile.WorkAreaLeftTop;
                WorkspaceRightDown = tempTileFile.WorkAreaRightDown;
                WorkspaceType = tempTileFile.WorkAreaType;
                LoadIDTiles(tempTileFile.GetImage(), tempTileFile.Boxes, FileName);
                
                //進行正規化
                //TileRegularize(gridToTileTypeMap[WorkspaceType]);

                //自動微調位置
                AutoAdjustAllTile();
                //切換到評分上半部
                Invoke(new Action(() => { rdbRankTop.Checked = true; }));
                scoreRecords.Add(scoreSampleMap[WorkspaceType](false, true).Record);
                //切換到評分下半部
                Invoke(new Action(() => { rdbRankDown.Checked = true; }));
                scoreRecords.Add(scoreSampleMap[WorkspaceType](false, false).Record);
                doneCount += 1;
            }
            
            //輸出結果
            using (StreamWriter sw = new StreamWriter("BatchScoringResult.txt", false))
            {
                sw.WriteLine("受測者\t狀態\t區塊\t上下\t勾縫指標量\t平行指標量\t筆直指標量\t旋轉角指標量\t方林法殘差量\t勾縫分數\t平行分數\t筆直分數\t旋轉角分數\t綜合分數\t方林法分數");
                foreach (var item in scoreRecords)
                {
                    sw.WriteLine(item.ToDataLine());
                }
                sw.Close();
            }
            Process.Start("BatchScoringResult.txt");
            Invoke(new Action(() =>
            {
                lblMsg.Text = "完成";
                myProgressBar.Value = 0;
            }));
        }

        /// <summary>針對辨識完成檔進行批次評分</summary>
        private void BatchConvertV2ToV3()
        {
            //取得評分的檔案列表
            List<string> sourceFiles = GetBatchFiles(new [] { ".data" });
            SmileLib.MessageListBox myMessageListBox = new SmileLib.MessageListBox();
            var result = myMessageListBox.ShowDialog(sourceFiles, "查找到的檔案");
            if (result != DialogResult.OK)
            {
                return;
            }

            var fileList = myMessageListBox.SelectResult.ToList();
            int fileCount = fileList.Count;
            Invoke(new Action(() => myProgressBar.Maximum = fileCount));
            int doneCount = 0;
            //對每個選定的檔案進行處理
            foreach (var item in myMessageListBox.SelectResult)
            {
                var doneCountNow = doneCount;
                Invoke(new Action(() =>
                {
                    lblMsg.Text = $"進度{doneCountNow}/{fileCount}";
                    myProgressBar.Value = doneCountNow;
                }));
                FileName = item;
                WorkspaceLeftTop = new Point(-1, -1);
                WorkspaceRightDown = new Point(-1, -1);
                var tempTileFile = OpenV2File(FileName);
                
                var ext = Path.GetExtension(FileName);
                if(ext == null) throw new ArgumentException("字串無法解析得到附檔名",FileName);
                var newFileName = $"{FileName.Substring(0, FileName.Length - ext.Length)}.json";
                tempTileFile.Save(newFileName);
                doneCount += 1;
            }
            Invoke(new Action(() =>
            {
                lblMsg.Text = "完成";
                myProgressBar.Value = 0;
            }));
        }

        /// <summary>取得目錄</summary>
        private static List<string> GetBatchFiles(IEnumerable<string> fileType)
        {
            //資料夾選擇對話視窗
            FolderSelect.FolderSelectDialog fsd = new FolderSelect.FolderSelectDialog();

            List<string> fileList = new List<string>();
            fsd.Title = "開啟目錄";
            fsd.InitialDirectory = Environment.CurrentDirectory;
            if (!fsd.ShowDialog(IntPtr.Zero))
            {
                return fileList;
            }
            try
            {
                List<string> files = Directory.EnumerateFiles(fsd.FileName, "*", SearchOption.AllDirectories)
                    .OrderBy(s => s).ToList();
                return files.Where(fileName =>
                {
                    var ext = Path.GetExtension(fileName);
                    return fileType.Contains(ext);
                }).ToList();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
            return fileList;
        }

        /// <summary>測試</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTest_Click(object sender, EventArgs e)
        {
            
            
        }

        private void lblMsg_Click(object sender, EventArgs e)
        {
            
        }

        private void btnLoadV3_Click(object sender, EventArgs e)
        {
            FileName = "";
            openFileDialog1.Filter = "JSON檔|*.json";
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            FileName = openFileDialog1.FileName;
            var v3File = IdentifyTileFileV3.OpenFromFile(FileName);
            WorkspaceType = v3File.WorkAreaType;
            WorkspaceLeftTop = v3File.WorkAreaLeftTop;
            WorkspaceRightDown = v3File.WorkAreaRightDown;
            LoadIDTiles(v3File.GetImage(), v3File.Boxes, FileName);
        }
    }

    #region 程式狀態變數_WatchArea_class

    //picboxWatchArea可能要看操作mode來決定要輸出什麼資料或是觸發什麼方法
    //這裡保管mode字典
    /// <summary>
    /// 程式狀態變數_WatchArea
    /// </summary>
    public class WatchAreaModeNum
    {
        /// <summary>
        /// 使用者瀏覽中
        /// </summary>
        public const int Browse = 0;

        /// <summary>
        /// 使用者要新增新的Tile
        /// </summary>
        public const int AddNewBox = 1;

        public const int doubleClicking = 2;

        /// <summary>
        /// 定義工作區
        /// </summary>
        public const int defWorkingspace = 3;
    }

    #endregion 程式狀態變數_WatchArea_class
}