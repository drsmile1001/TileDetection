
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace 磁磚辨識評分
{


    public partial class IdentificationForm : Form
    {
        Image<Bgr, Byte> imgOrg;
        Image<Bgr, Byte> imgBin;
        Image<Bgr, Byte> imgCatch;
        Image<Bgr, Byte> imgOrgPlusCatch;
        List<MCvBox2D> boxListDone;


        public IdentificationForm()
        {
            InitializeComponent();

            fileNameTextBox.Text = "pic3.png";
        }

        public Image<Gray, Byte> Sharpen(Image<Gray, Byte> image)
        {
            Image<Gray, Byte> result = image.CopyBlank(); //copy a blank image

            MIplImage MIpImg = (MIplImage)System.Runtime.InteropServices.Marshal.PtrToStructure(image.Ptr, typeof(MIplImage));
            MIplImage MIpImgResult = (MIplImage)System.Runtime.InteropServices.Marshal.PtrToStructure(result.Ptr, typeof(MIplImage));

            int imageHeight = MIpImg.height;
            int imageWidth = MIpImg.widthStep;

            unsafe
            {
                for (int height = 1; height < imageHeight - 1; height++)
                {
                    //current_pixel line
                    byte* currentPixel = (byte*)MIpImg.imageData + imageWidth * height;
                    //up_pixel line
                    byte* uplinePixel = currentPixel - MIpImg.widthStep;
                    //down_pixel line
                    byte* downlinePixel = currentPixel + MIpImg.widthStep;
                    //result current_pixel line
                    byte* resultPixel = (byte*)MIpImgResult.imageData + imageWidth * height;

                    for (int width = 1; width < imageWidth - 1; width++)
                    {
                        //5*current_pixel-left_pixel-right_pixel-up_pixel-down_pixel
                        int sharpValue = 5 * currentPixel[width] - currentPixel[width - 1]
                                            - currentPixel[width + 1] - uplinePixel[width]
                                            - downlinePixel[width];

                        if (sharpValue < 0) sharpValue = 0;     //Gray level 0~255
                        if (sharpValue > 255) sharpValue = 255; //Gray level 0~255

                        resultPixel[width] = (byte)sharpValue;
                    }
                }
            }

            return result;
        }

        private double GetDistance(double A_x, double A_y, double B_x, double B_y)
        {
            double x = System.Math.Abs(B_x - A_x);
            double y = System.Math.Abs(B_y - A_y);
            return Math.Sqrt(x * x + y * y);
        }

        public void PerformShapeDetection(int mode)
        {
            if (fileNameTextBox.Text != String.Empty)
            {
                StringBuilder msgBuilder = new StringBuilder("Performance: ");

                //Load the image from file and resize it for display
                /*原始會resize
               Image<Bgr, Byte> img = 
                  new Image<Bgr, byte>(fileNameTextBox.Text)
                  .Resize(800, 800, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR, true);
                */
                //自己修改 不resize
                Image<Bgr, Byte> img =
                  new Image<Bgr, byte>(fileNameTextBox.Text);

                //Convert the image to grayscale and filter out the noise

                Image<Gray, Byte> gray = img.Convert<Gray, Byte>();
                //gray = Sharpen(gray);
                //gray._Erode(2);
                if (mode == 1)
                {
                    int maxCount = 0;
                    int bestThresholdNum = 0;
                    for (int i = 0; i < 255; i++)
                    {
                        Image<Gray, Byte> testgray = img.Convert<Gray, Byte>();
                        testgray = testgray.ThresholdBinary(new Gray(i), new Gray(255));
                        int testCount = watching(testgray);
                        if (testCount>maxCount)
                        {
                            maxCount = testCount;
                            bestThresholdNum = i;
                        }
                    }
                    gray = gray.ThresholdBinary(new Gray(bestThresholdNum), new Gray(255));
                    trackBarThresholding.Value = bestThresholdNum;
                    lblThreshold.Text = bestThresholdNum.ToString();
                }
                else
                {
                    gray = gray.ThresholdBinary(new Gray(trackBarThresholding.Value), new Gray(255));
                }
                
                
                double cannyThreshold = 180.0;
                Stopwatch watch = Stopwatch.StartNew();
                
                double cannyThresholdLinking = 120.0;
                Image<Gray, Byte> cannyEdges = gray.Canny(cannyThreshold, cannyThresholdLinking);
                
                watch.Reset(); watch.Start();
                List<Triangle2DF> triangleList = new List<Triangle2DF>();
                List<MCvBox2D> boxList = new List<MCvBox2D>(); //a box is a rotated rectangle
                using (MemStorage storage = new MemStorage()) //allocate storage for contour approximation
                    for (
                       Contour<Point> contours = cannyEdges.FindContours(
                          Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                          Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST,
                          storage);
                       contours != null;
                       contours = contours.HNext)
                    {
                        Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.05, storage);

                        if (currentContour.Area > 250) //only consider contours with area greater than 250
                        {

                                
                                #region determine if all the angles in the contour are within [85, 95] degree
                                bool isRectangle = true;
                                Point[] pts = currentContour.ToArray();
                                LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

                                for (int i = 0; i < edges.Length; i++)
                                {
                                    double angle = Math.Abs(
                                       edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
                                    if (angle < 70 || angle > 110)
                                    {
                                        isRectangle = false;
                                        break;
                                    }
                                }
                                #endregion
                                 

                                if (isRectangle) boxList.Add(currentContour.GetMinAreaRect());
                                
                            

                        }
                    }
                
                #region 清除重複偵測
                
                //經過上面的操作，boxList裡面應該會有所有偵測到的長方形，而且同一個磁磚有機會被偵測多次。
                //預計建立兩個新的boxList 一個暫存中心座標相近的box，一個放處理過沒有重複的box
                //預計作法：在舊的boxList取出一box，比對有相近座標的box也都取出放進tempBoxList，找完後平均tempbox裡的結果，放進boxListFinish
                const double shourtDistince = 10.0;
                List<MCvBox2D> boxListNew = new List<MCvBox2D>();
                bool[] boxListTag = new bool[boxList.Count];

                foreach (MCvBox2D boxItemSample in boxList)
                {
                    int indexOfSample = boxList.IndexOf(boxItemSample);
                    if (boxListTag[indexOfSample] == true)
                    {
                        continue;
                    }
                    //取出新樣本
                    List<MCvBox2D> boxListTemp = new List<MCvBox2D>();
                    boxListTemp.Add(boxItemSample);
                    boxListTag[indexOfSample] = true;
                    

                    //找到和樣本接近的box，放進boxListTemp
                    foreach (MCvBox2D boxItemCompared in boxList)
                    {

                        int indexOfcompared = boxList.IndexOf(boxItemCompared);
                        if (boxListTag[indexOfcompared] == true)
                        {
                            continue;
                        }
                        if (GetDistance
                            (
                            (double)boxItemCompared.center.X,
                            (double)boxItemCompared.center.Y,
                            (double)boxItemSample.center.X,
                            (double)boxItemSample.center.Y
                            )
                            < shourtDistince)
                        {
                            boxListTemp.Add(boxItemCompared);
                            boxListTag[indexOfcompared] = true;
                        }
                    }
                    //計算boxListTemp裡面的box數值
                    double[] locationX = new double[boxListTemp.Count];
                    double[] locationY = new double[boxListTemp.Count];
                    double[] angle = new double[boxListTemp.Count];
                    double[] height = new double[boxListTemp.Count];
                    double[] width = new double[boxListTemp.Count];

                    for (int index = 0; index < boxListTemp.Count; index++)
                    {
                        locationX[index] = (double)boxListTemp[index].center.X;
                        locationY[index] = (double)boxListTemp[index].center.Y;
                        if (boxListTemp[index].angle < 45.0 && boxListTemp[index].angle > -45.0)
                        {
                            angle[index] = (double)boxListTemp[index].angle;

                            height[index] = (double)boxListTemp[index].size.Height;
                            width[index] = (double)boxListTemp[index].size.Width;
                        }
                        else
                        {
                            if (boxListTemp[index].angle > 45.0)
                            {
                                angle[index] = (double)boxListTemp[index].angle - 90.0;

                            }
                            else
                            {
                                angle[index] = (double)boxListTemp[index].angle + 90.0;
                            }
                            height[index] = (double)boxListTemp[index].size.Width;
                            width[index] = (double)boxListTemp[index].size.Height;
                        }

                    }
                    double avgLocationX = locationX.Average();
                    double avgLocationY = locationY.Average();
                    double avgAngle = angle.Average();
                    double avgHeight = height.Average();
                    double avgWidth = width.Average();
                    boxListNew.Add(new MCvBox2D(new PointF((float)avgLocationX,(float)avgLocationY),new SizeF((float)avgWidth,(float)avgHeight),(float)avgAngle));
                }
                boxList = boxListNew;
    

                #endregion
                



                
                lblBoxFind.Text = "找到：" + boxList.Count.ToString();
                watch.Stop();
                msgBuilder.Append(String.Format("Triangles & Rectangles - {0} ms; ", watch.ElapsedMilliseconds));
              

                originalImageBox.Image = img;
                imgOrg = img;



                
                this.Text = msgBuilder.ToString();

                #region draw triangles and rectangles
                imgCatch = img.CopyBlank();
                //原始碼
                /*  
               foreach (Triangle2DF triangle in triangleList)
                  triangleRectangleImage.Draw(triangle, new Bgr(Color.DarkBlue), 2);
              
               foreach (MCvBox2D box in boxList)
                  triangleRectangleImage.Draw(box, new Bgr(Color.DarkOrange), 2);
               triangleRectangleImageBox.Image = triangleRectangleImage;
                 */
                //自己修改
                int boxNum = 0;
                foreach (MCvBox2D box in boxList)
                {

                    imgCatch.Draw(box, new Bgr(Color.Orange), 2);
                    imgCatch[(int)box.center.Y, (int)box.center.X] = new Bgr(0.0, 0.0, 255.0);
                    boxNum++;
                }
                CatchImageBox.Image = imgCatch;
                imgCatch.ToBitmap().Save("imgCatch.bmp");

                imgOrgPlusCatch = new Image<Bgr, byte>(fileNameTextBox.Text);
                imgBin = gray.Convert<Bgr, byte>();

                foreach (MCvBox2D box in boxList)
                {
                    imgOrgPlusCatch.Draw(box, new Bgr(Color.Red), 2);
                    imgBin.Draw(box, new Bgr(Color.Red), 2);
                }
                OrgPlusCatchBox.Image = imgOrgPlusCatch;


                BinImageBox.Image = imgBin;
                //BinImageBox.Image = cannyEdges;
                //boxListDone = new MCvBox2D[boxList.Count];
                //boxList.CopyTo(boxListDone);
                boxListDone = new List<MCvBox2D>(boxList.ToArray());

                imgOrgPlusCatch.ToBitmap().Save("imgOrgPlusCatch.bmp");
                imgBin.ToBitmap().Save("imgBin.bmp");
                cannyEdges.ToBitmap().Save("cannyEdges.bmp");
                #region 評分部分
#if false

                double[] anglelist = new double[boxList.Count];
                for (int i = 0; i < boxList.Count; i++)
                {

                    if (boxList[i].angle < -45.0)
                    {
                        anglelist[i] = boxList[i].angle + 90.0f;
                    }
                    else
                    {
                        anglelist[i] = boxList[i].angle;
                    }
                }
                string str = "";
                for (int i = 0; i < anglelist.Length; i++)
                {
                    str += "box" + i.ToString() + ":" + anglelist[i].ToString() + "\n";
                }
                lblMsg.Text = str;

                lblAngleErr.Text = StandardDeviation(anglelist).ToString(); 
#endif
                #endregion

                
                #endregion
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            PerformShapeDetection(0);
        }

        private void loadImageButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                fileNameTextBox.Text = openFileDialog1.FileName;
            }
        }

        private void trackBarThreshold_Scroll(object sender, EventArgs e)
        {
            lblThreshold.Text = trackBarThresholding.Value.ToString();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            PerformShapeDetection(0);
        }

        private void originalImageBox_OnZoomScaleChange(object sender, EventArgs e)
        {
            // triangleRectangleImageBox.SetZoomScale(originalImageBox.ZoomScale,
        }

        private int watching(Image<Gray, Byte> gray)
        {
            gray = gray.ThresholdBinary(new Gray(trackBarThresholding.Value), new Gray(255));

            double cannyThreshold = 180.0;
            Stopwatch watch = Stopwatch.StartNew();

            double cannyThresholdLinking = 120.0;
            Image<Gray, Byte> cannyEdges = gray.Canny(cannyThreshold, cannyThresholdLinking);
            watch.Reset(); watch.Start();
            List<Triangle2DF> triangleList = new List<Triangle2DF>();
            List<MCvBox2D> boxList = new List<MCvBox2D>(); //a box is a rotated rectangle
            using (MemStorage storage = new MemStorage()) //allocate storage for contour approximation
                for (
                   Contour<Point> contours = cannyEdges.FindContours(
                      Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                      Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST,
                      storage);
                   contours != null;
                   contours = contours.HNext)
                {
                    Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.05, storage);

                    if (currentContour.Area > 250) //only consider contours with area greater than 250
                    {
                        if (currentContour.Total == 3) //The contour has 3 vertices, it is a triangle
                        {
                            Point[] pts = currentContour.ToArray();
                            triangleList.Add(new Triangle2DF(
                               pts[0],
                               pts[1],
                               pts[2]
                               ));
                        }
                        else if (currentContour.Total == 4) //The contour has 4 vertices.
                        {
                            #region determine if all the angles in the contour are within [85, 95] degree
                            bool isRectangle = true;
                            Point[] pts = currentContour.ToArray();
                            LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

                            for (int i = 0; i < edges.Length; i++)
                            {
                                double angle = Math.Abs(
                                   edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
                                if (angle < 85 || angle > 95)
                                {
                                    isRectangle = false;
                                    break;
                                }
                            }
                            #endregion

                            if (isRectangle) boxList.Add(currentContour.GetMinAreaRect());
                            for (int i = 0; i < boxList.Count - 1; i++)
                            {
                                if (GetDistance
                                    ((int)boxList[i].center.X,
                                    (int)boxList[i].center.Y,
                                    (int)boxList[boxList.Count - 1].center.X,
                                    (int)boxList[boxList.Count - 1].center.Y)
                                     < 10.0)
                                {
                                    boxList.RemoveAt(boxList.Count - 1);

                                    break;
                                }
                            }
                        }

                    }
                }
            return boxList.Count;
        }

        private void btnFindBest_Click(object sender, EventArgs e)
        {
            PerformShapeDetection(1);
        }



        private void btnGotoUserDef_Click(object sender, EventArgs e)
        {
//            FormUserDef frm = new FormUserDef(imgOrg, boxListDone, openFileDialog1.FileName);
            Program.myUserDefForm.Invoke(
                new Action<Image<Bgr, Byte>, List<MCvBox2D>,string>(Program.myUserDefForm.LoadIDTiles),
                new object[] { imgOrg, boxListDone, openFileDialog1.FileName });
            this.Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string now = DateTime.Now.ToString("MM-dd_H-mm-ss");
            Bitmap tempbmpOrg = imgOrg.ToBitmap();
            tempbmpOrg.Save(now + "ImageOrg.png", ImageFormat.Png);
            Bitmap tempbmpBin = imgBin.ToBitmap();
            tempbmpBin.Save(now + "ImageBin.png", ImageFormat.Png);
            Bitmap tempbmpCatch = imgCatch.ToBitmap(); 
            tempbmpCatch.Save(now + "ImageCatch.png", ImageFormat.Png);
            Bitmap tempbmpOrgPlusCatch = imgOrgPlusCatch.ToBitmap();
            tempbmpOrgPlusCatch.Save(now + "ImageOrgPlusCatch.png", ImageFormat.Png);

            tempbmpOrg.Dispose();
            tempbmpCatch.Dispose();
            tempbmpBin.Dispose();
            tempbmpOrgPlusCatch.Dispose();
        }

        private void btnGotoUserDef_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

                        
        }
    }
}
