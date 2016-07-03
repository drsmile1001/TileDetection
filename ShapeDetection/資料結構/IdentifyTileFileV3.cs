using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace 磁磚辨識評分.資料結構
{
    /// <summary>用來給JSON.Net轉換存檔用的類別</summary>
    public class IdentifyTileFileV3
    {
        /// <summary>底圖的位元組陣列</summary>
        public byte[] BaseImageBytes { get; set; }

        /// <summary>辨識到的磁磚資料</summary>
        public List<MCvBox2D> Boxes { get; set; }

        /// <summary>工作區左上角</summary>
        public Point WorkAreaLeftTop { get; set; }

        /// <summary>工作區右下角</summary>
        public Point WorkAreaRightDown { get; set; }

        /// <summary>磁磚類型</summary>
        public Grid WorkAreaType { get; set; }

        /// <summary>取得Emgu版 Image</summary>
        /// <returns></returns>
        public Image<Bgr,byte> GetImage()
        {
            var iamge = Image.FromStream(new MemoryStream(BaseImageBytes));
            var bitmap = new Bitmap(iamge);
            var resultImage = new Image<Bgr, byte>(bitmap);
            return resultImage;
        }
    }
}
