﻿using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Newtonsoft.Json.Linq;

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
        public Image<Bgr, byte> GetImage()
        {
            var iamge = Image.FromStream(new MemoryStream(BaseImageBytes));
            var bitmap = new Bitmap(iamge);
            var resultImage = new Image<Bgr, byte>(bitmap);
            return resultImage;
        }

        /// <summary>以JSON格式存到特定路徑</summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            JObject jObject = JObject.FromObject(this);
            File.WriteAllText(fileName, jObject.ToString());
        }

        /// <summary>打開第三版檔案</summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IdentifyTileFileV3 OpenFromFile(string fileName)
        {
            var jsonString = File.ReadAllText(fileName);
            var newFile = JObject.Parse(jsonString).ToObject<IdentifyTileFileV3>();
            return newFile;
        }

        /// <summary>將第二版檔案轉成第三版</summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IdentifyTileFileV3 FromIdentifyTileFileV2(IdentifyTileFileV2 source)
        {
            //轉成成V3
            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                Image image = source.BaseImage.Bitmap;
                image.Save(ms, ImageFormat.Png);
                ms.Close();
                imageBytes = ms.ToArray();
            }

            IdentifyTileFileV3 v3File = new IdentifyTileFileV3
            {
                BaseImageBytes = imageBytes,
                Boxes = source.getMcvbox2DList(),
                WorkAreaLeftTop = source.WorkAreaLeftTop,
                WorkAreaRightDown = source.WorkAreaRightDown,
                WorkAreaType = source.WorkAreaType
            };

            return v3File;
        }
    }
}
