using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Runtime.InteropServices;
using Emgu.CV.CvEnum;

#region 存檔用
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;
#endregion

namespace 磁磚辨識評分
{

    public partial class FormUserDef : Form
    {
        #region 公用變數
        string fileName;
        Image<Bgr, Byte> ImageForShow;
        Image<Bgr, Byte> BaseImage;


        Image<Bgr, Byte> ThumbnailImage;
        Image<Bgr, Byte> imageWatchArea;
        List<MCvBox2D> BaseMcvBox2DList;

        double ScaleOfThumbnail = 1;
        Point centerOfWatchArea = new Point(0, 0);
        Point pointDragStart = new Point(0, 0);
        Point CenterOfThumbnailWatchArea = new Point(0, 0);
        Point CenterOfWatchAreaWhenDragStart = new Point(0, 0);
        bool mouseDownInPicboxThumbnail = false;
        bool mouseDownInPicboxWatchArea = false;
        int WatchAreaMode = WatchAreaModeNum.Browse;

        Dictionary<string, double> PixelPerCentimeter = new Dictionary<string, double>();




        List<Point> pointsOfAddingTile = new List<Point>();
        
        /// <summary>只評分上半</summary>
        bool RankTopOnly = true;

        #region 和定義邊界有關的變數
        Grid WorkspaceType = 0;
        Point WorkspaceLeftTop = Point.Empty;
        Point WorkspaceRightDown = Point.Empty;
        #endregion

        /// <summary>按下space</summary>
        bool spaceDown = false;

        #endregion

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
            lbxTileList.Items.Clear();
            foreach (MCvBox2D box in BaseMcvBox2DList)
            {
                ImageForShow.Draw(box, new Bgr(Color.Red), 1);

            }
            for (int index = 0; index < BaseMcvBox2DList.Count; index++)
            {
                ImageForShow.Draw(BaseMcvBox2DList[index], new Bgr(Color.Red), 1);
                string boxNum = "tile" + index.ToString();
                lbxTileList.Items.Add(boxNum);
            }
            ThumbnailImage = InputImage.Clone().Resize(160, 120, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR, true);
            ScaleOfThumbnail = (double)ThumbnailImage.Height / (double)InputImage.Height;
            picboxThumbnail.Image = ThumbnailImage.ToBitmap();
            picboxWatchArea.Image = ImageForShow.ToBitmap();

            imageWatchArea = new Image<Bgr, byte>(new Size(picboxWatchArea.Width, picboxWatchArea.Height));

            #region 初始化縮圖和觀看視窗
            drawThumbnailImage();
            drawWatchArea();
            lblTileCount.Text = "總數：" + BaseMcvBox2DList.Count.ToString();
            #endregion

            //設定比例參數
            SetPixelPerCentimeter(Path.GetFileNameWithoutExtension(fileName));
            //顯示檔名
            this.Text = fileName;
            this.fileName = fileName;

        }

        #endregion

        #region 抓滑鼠座標方法
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);
        public Point GetCursorPosition()
        {
            Point defPnt = new Point();
            GetCursorPos(ref defPnt);
            return defPnt;
        }
        #endregion

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
                #endregion

            }
            else
            {
                #region 初始化或預設
                CenterOfThumbnailWatchArea = new Point(

                    ThumbnailImage.Width / 2,
                    ThumbnailImage.Height / 2
                    );
                #endregion
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
        #endregion

        #endregion

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
                #endregion
            }
            else
            {
                #region 初始化或預設
                centerOfWatchArea = new Point(
                    ImageForShow.Width / 2,
                    ImageForShow.Height / 2);
                #endregion
            }
            #endregion


            picboxWatchArea.Image = ImageForShow.Copy(new MCvBox2D((PointF)centerOfWatchArea, new SizeF((float)picboxWatchArea.Width, (float)picboxWatchArea.Height), 0.0f)).ToBitmap();
        }

        #region 繪製觀看視窗在(x,y)位置
        private void drawWatchArea(int x, int y)
        {
            centerOfWatchArea = new Point(x, y);
            picboxWatchArea.Image = ImageForShow.Copy(new MCvBox2D((PointF)centerOfWatchArea, new SizeF((float)picboxWatchArea.Width, (float)picboxWatchArea.Height), 0.0f)).ToBitmap();
        }
        #endregion
        #endregion

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
        #endregion

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
        #endregion

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

        #endregion
        #endregion


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
                        #endregion

                        #region 畫水平線
                        for (double LineNum = 0; LineNum <= 12.0; LineNum += 1.0)
                        {
                            ImageForShow.Draw(
                                new LineSegment2D(
                                    new Point(WorkspaceLeftTop.X, (int)(WorkspaceLeftTop.Y + (WorkspaceRightDown.Y - WorkspaceLeftTop.Y) / 12.0 * LineNum)),
                                    new Point(WorkspaceRightDown.X, (int)(WorkspaceLeftTop.Y + (WorkspaceRightDown.Y - WorkspaceLeftTop.Y) / 12.0 * LineNum))),
                                new Bgr(Color.White), 1);
                        }
                        #endregion

                        break;
                    case Grid.RectangleGrid:
                        #region 畫長方形的網格，先建立暫時的RectangleGrid，然後利用之繪出格子
                        RectangleGrids tempRectangleGrids = new RectangleGrids(WorkspaceRightDown, WorkspaceLeftTop);

                        for (int column = 0; column < RectangleGrids.columnCount; column++)
                        {
                            for (int row = 0; row < RectangleGrids.rowCount; row++)
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
                                    #endregion

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

                                    #endregion
                                }
                            }
                        }
                        #endregion

                        break;
                    default:
                        MessageBox.Show("未預期：有定義網格邊界，卻無網格類型");
                        break;
                }

            }
            #endregion
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
                #endregion
                #region 標明選取的，重繪
                ImageForShow.Draw(BaseMcvBox2DList[lbxTileList.SelectedIndex], new Bgr(Color.Yellow), 1);
                drawWatchArea(
                    (int)BaseMcvBox2DList[lbxTileList.SelectedIndex].center.X,
                    (int)BaseMcvBox2DList[lbxTileList.SelectedIndex].center.Y);
                drawThumbnailImage(
                    (int)(BaseMcvBox2DList[lbxTileList.SelectedIndex].center.X * ScaleOfThumbnail),
                    (int)(BaseMcvBox2DList[lbxTileList.SelectedIndex].center.Y * ScaleOfThumbnail));
                #endregion
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
                            #endregion

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
                            #endregion

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
                            #endregion

                            #region 將新的MCvBox2D丟到BaseMcv2DList，重建UI清單
                            BaseMcvBox2DList.Add(tempBox);
                            lbxTileList.Items.Add("newtile");
                            for (int index = 0; index < BaseMcvBox2DList.Count; index++)
                            {
                                string boxNum = "tile" + index.ToString();
                                lbxTileList.Items[index] = boxNum;
                            }
                            pointsOfAddingTile.Clear();
                            #endregion

                            #region 重繪
                            lbxTileList.SelectedIndex = lbxTileList.Items.Count - 1;
                            drawImageForShow();
                            ImageForShow.Draw(BaseMcvBox2DList[BaseMcvBox2DList.Count - 1], new Bgr(Color.Yellow), 1);
                            #region true
                            drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                            drawThumbnailImage(CenterOfThumbnailWatchArea.X, CenterOfThumbnailWatchArea.Y);
                            #endregion
                            WatchAreaMode = WatchAreaModeNum.Browse;
                            #endregion

                            btnDelTile.Enabled = true;
                            lblTileCount.Text = "總數：" + BaseMcvBox2DList.Count.ToString();

                        }
                    }
                    #endregion
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
                                #endregion
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
                                #endregion

                                lblMsg.Text = "正方型工作區:" + WorkspaceLeftTop.ToString() + WorkspaceRightDown.ToString();

                                #region 重新繪製底圖，出現網格
                                drawImageForShow();
                                drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                                drawThumbnailImage(CenterOfThumbnailWatchArea.X, CenterOfThumbnailWatchArea.Y);
                                #endregion

                                WatchAreaMode = WatchAreaModeNum.Browse;

                                #endregion
                            }
                            #endregion
                            break;
                        case Grid.RectangleGrid:
                            #region 輸入長方形工作區邊界
                            if (WorkspaceLeftTop == Point.Empty)
                            {
                                #region 如果左上點尚未建立，則輸入座標為左上點，提醒使用者輸入右下點
                                WorkspaceLeftTop = getMouseLocationRelayToBaseImage();
                                lblMsg.Text = "請點擊工作區右下角";
                                #endregion
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
                                #endregion


                                lblMsg.Text = "長方形工作區:" + WorkspaceLeftTop.ToString() + WorkspaceRightDown.ToString();

                                #region 重新繪製底圖，出現網格
                                drawImageForShow();
                                drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
                                drawThumbnailImage(CenterOfThumbnailWatchArea.X, CenterOfThumbnailWatchArea.Y);
                                #endregion

                                WatchAreaMode = WatchAreaModeNum.Browse;
                                #endregion
                            }
                            #endregion

                            break;
                        default:
                            MessageBox.Show("未預期：沒有定義工作區類型卻執行定義工作區座標點");
                            break;
                    }
                    #endregion
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
                #endregion

                WatchAreaMode = WatchAreaModeNum.Browse;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
#if false
            IdentifyTileFile tempTileFile = new IdentifyTileFile();
            tempTileFile.BaseImage = BaseImage.Clone();
            tempTileFile.setBoxs(BaseMcvBox2DList);


            string filename = "";
            saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(this.Text);

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filename = saveFileDialog1.FileName;
                BinaryFormatter formatter = new BinaryFormatter();
                Stream output = File.Create(filename);
                formatter.Serialize(output, tempTileFile);
                output.Close();
            }

#endif
            IdentifyTileFileV2 tempTileFileV2 = new IdentifyTileFileV2();
            tempTileFileV2.BaseImage = BaseImage.Clone();
            tempTileFileV2.setBoxs(BaseMcvBox2DList);
            tempTileFileV2.WorkAreaType = WorkspaceType;
            tempTileFileV2.WorkAreaLeftTop = WorkspaceLeftTop;
            tempTileFileV2.WorkAreaRightDown = WorkspaceRightDown;

            string fileName = "";
            saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(this.Text);
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = saveFileDialog1.FileName;
                BinaryFormatter formatter = new BinaryFormatter();
                Stream output = File.Create(fileName);
                formatter.Serialize(output, tempTileFileV2);
                output.Close();
            }

        }

        /// <summary>載入舊檔</summary>
        private void btnLoad_Click(object sender, EventArgs e)
        {
#if false
            fileName = "";

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            fileName = openFileDialog1.FileName;

            BinaryFormatter formatter = new BinaryFormatter();
            Stream input = File.OpenRead(fileName);
            IdentifyTileFile tempTileFile = (IdentifyTileFile)formatter.Deserialize(input);
            input.Close();

            LoadIDTiles(tempTileFile.BaseImage, tempTileFile.getMcvbox2DList(), fileName); 
#endif
            fileName = "";

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            fileName = openFileDialog1.FileName;
            BinaryFormatter formatter = new BinaryFormatter();
            Stream input = File.OpenRead(fileName);
            object inputDeserialize = formatter.Deserialize(input);
            input.Close();
            IdentifyTileFile tempTileFile;
            try
            {
                tempTileFile = (IdentifyTileFileV2)inputDeserialize;
                WorkspaceType = (tempTileFile as IdentifyTileFileV2).WorkAreaType;
                WorkspaceLeftTop = (tempTileFile as IdentifyTileFileV2).WorkAreaLeftTop;
                WorkspaceRightDown = (tempTileFile as IdentifyTileFileV2).WorkAreaRightDown;
                Console.WriteLine("讀到新版本");
                lblFileEdition.Text = "新版本";
            }
            catch (Exception)
            {
                tempTileFile = (IdentifyTileFile)inputDeserialize;
                Console.WriteLine("讀到舊版本");
                lblFileEdition.Text = "舊版本";
            }
            LoadIDTiles(tempTileFile.BaseImage, tempTileFile.getMcvbox2DList(), fileName); 

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
            nudPixelPerCentimeter.Value = (decimal)value;
        }

        /// <summary>對正方形磁磚評分</summary>
        private void btnGiveGradeSquare_Click(object sender, EventArgs e)
        {
            if (WorkspaceLeftTop == Point.Empty || WorkspaceRightDown == Point.Empty)
            {
                lblMsg.Text = "請先設定工作區域";
            }
            else
            {
                SquareTiles mySquareTiles;

                if (BaseMcvBox2DList.Count != SquareGrids.rowCount * SquareGrids.columnCount)
                {
                    lblMsg.Text = "磁磚數量不正確，無法評分";
                }
                else
                {
                    mySquareTiles = new SquareTiles(WorkspaceRightDown, WorkspaceLeftTop, BaseMcvBox2DList, fileName,RankTopOnly);
                    if (mySquareTiles.msg == SquareTiles.error_TilePosition)
                    {
                        lblMsg.Text = "磁磚位置有誤，請檢查";
                        return;
                    }
                    else
                    {
                        drawImageForShow();
                        MCvFont f = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX, 1, 1);
                        for (int columnNum = 0; columnNum < SquareGrids.columnCount; columnNum++)
                        {
                            for (int rowNum = 0; rowNum < SquareGrids.rowCount; rowNum++)
                            {
                                string output = "(" + columnNum + "," + rowNum + ")";
                                ImageForShow.Draw(output, ref f, Point.Round(mySquareTiles.Tiles[columnNum, rowNum].center), new Bgr(Color.Black));
                            }
                        }

                        #region 印出磁磚
#if false
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
#endif
                        #endregion
                    }
                }
            }
        }

        /// <summary>對長方形磁磚評分</summary>
        private void btnGiveGradeRectangle_Click(object sender, EventArgs e)
        {
            if (WorkspaceLeftTop == Point.Empty || WorkspaceRightDown == Point.Empty)
            {
                lblMsg.Text = "請先設定工作區域";
            }
            else
            {
                RectangleTiles myRetangleTiles;

                if (BaseMcvBox2DList.Count != RectangleGrids.tileCount)
                {
                    lblMsg.Text = "磁磚數量不正確，無法評分";
                    Console.WriteLine("磁磚數量不正確，無法評分");
                }
                else
                {
                    myRetangleTiles = new RectangleTiles(WorkspaceRightDown, WorkspaceLeftTop, BaseMcvBox2DList, fileName,RankTopOnly);
                    if (myRetangleTiles.msg == RectangleTiles.error_TilePosition)
                    {
                        lblMsg.Text = "磁磚位置有誤，請檢查";
                        Console.WriteLine("磁磚位置有誤，請檢查");
                        return;
                    }
                    else
                    {
                        drawImageForShow();
                        MCvFont f = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX, 1, 1);
                        for (int columnNum = 0; columnNum < RectangleGrids.columnCount; columnNum++)
                        {
                            for (int rowNum = 0; rowNum < RectangleGrids.rowCount; rowNum++)
                            {
                                if ((columnNum + rowNum) % 2 != 1)
                                {
                                    string output = "(" + columnNum + "," + rowNum + ")";
                                    ImageForShow.Draw(output, ref f, Point.Round(myRetangleTiles.Tiles[columnNum, rowNum].center), new Bgr(Color.Black));
                                }
                            }
                        }
                        #region 印出磁磚
#if false
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
#endif
                        #endregion
                    }
                }
            }
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
        #endregion

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
            #endregion

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
                        temp.center.Y -= 1;
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
                        temp.center.Y += 1;
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
                        temp.center.X -= 1;
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
                        temp.center.X += 1;
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
        private double getTileEdgeGray(Tile tempTile)
        {
            double gray = 0.0;
            foreach (PointF p in tempTile.TopEdge)
            {
                gray += BaseImage.GetGray(p.Y, p.X);
            }
            foreach (PointF p in tempTile.RightEdge)
            {
                gray += BaseImage.GetGray(p.Y, p.X);
            }
            foreach (PointF p in tempTile.DownEdge)
            {
                gray += BaseImage.GetGray(p.Y, p.X);
            }
            foreach (PointF p in tempTile.LeftEdge)
            {
                gray += BaseImage.GetGray(p.Y, p.X);
            }
            gray /= 80;
            return gray;

        }

        /// <summary>自動旋轉磁磚</summary>
        private void AutoTurnTile(int tileIndex)
        {
            double GrayNow = getTileEdgeGray(new Tile(BaseMcvBox2DList[tileIndex]));
            double ClockwiseMaxGray = GrayNow;
            double CounterclockwiseMaxGray = GrayNow;
            MCvBox2D tempClockwiseTile = BaseMcvBox2DList[tileIndex];
            MCvBox2D tempCounterclockwiseTile = BaseMcvBox2DList[tileIndex];
            while (true)
            {
                tempClockwiseTile.angle += 0.01f;
                double tempGray = getTileEdgeGray(new Tile(tempClockwiseTile));
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
                double tempGray = getTileEdgeGray(new Tile(tempCounterclockwiseTile));
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
        }

        /// <summary>自動微調磁磚位置</summary>
        private void AutoShiftTile(int tileIndex)
        {
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
        #endregion

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
                    tempSize = new SizeF((float)(SquareGrids.tileLength * (double)nudPixelPerCentimeter.Value), (float)(SquareGrids.tileLength * (double)nudPixelPerCentimeter.Value));
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
        #endregion

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
            for (int i = 0; i < BaseMcvBox2DList.Count; i++)
            {
                MCvBox2D temp = BaseMcvBox2DList[i];
                temp.size.Width -= 1;
                temp.size.Height -= 1;
                BaseMcvBox2DList[i] = temp;
                AutoShiftTile(i);
                AutoTurnTile(i);
                AutoShiftTile(i);
                AutoTurnTile(i);
                temp = BaseMcvBox2DList[i];
                temp.size.Width += 1;
                temp.size.Height += 1;
                BaseMcvBox2DList[i] = temp;
            }
            drawImageForShow();
            drawWatchArea(centerOfWatchArea.X, centerOfWatchArea.Y);
        }

        private void ckbRankTopOnly_CheckedChanged(object sender, EventArgs e)
        {
            RankTopOnly = ckbRankTopOnly.Checked;
            
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
    #endregion







}
