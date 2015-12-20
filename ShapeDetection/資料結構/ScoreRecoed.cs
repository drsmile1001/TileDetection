namespace 磁磚辨識評分
{
    public class ScoreRecord
    {
        /// <summary>受測者</summary>
        public string Subject { get; set; }

        /// <summary>受測狀態</summary>
        public string SubjectState { get; set; }

        /// <summary>區塊</summary>
        public int RegionId { get; set; }

        /// <summary>上下半部</summary>
        public string TopDown { get; set; }

        /// <summary>勾縫指標量</summary>
        public double GI { get; set; }

        /// <summary>勾縫分數</summary>
        public double GS { get; set; }

        /// <summary>平行指標量</summary>
        public double LPI { get; set; }

        /// <summary>平行分數</summary>
        public double LPS { get; set; }

        /// <summary>筆直指標量</summary>
        public double LSI { get; set; }

        /// <summary>筆直分數</summary>
        public double LSS { get; set; }

        /// <summary>旋轉角指標量</summary>
        public double AI { get; set; }

        /// <summary>旋轉角分數</summary>
        public double AS { get; set; }

        /// <summary>綜合分數</summary>
        public double CompositeScore { get { return (GS + LPS + LSS + AS) / 4; } }

        /// <summary>方林法用的指標</summary>
        public double FangLingIndex { get; set; }

        /// <summary>方林法分數</summary>
        public double FangLingScore { get {return 100 - 20 * FangLingIndex; } }

        /// <summary>輸出用於MessageBox之類臨時顯示的字串</summary>
        /// <returns></returns>
        public string ToScoreLine()
        {
            string output = "";
            output += Subject + "_" + SubjectState + "_" + RegionId + "_" + TopDown;
            output += "\t勾縫分數:\t" + GS.ToString("00.0");
            output += "\t平行分數:\t" + LPS.ToString("00.0");
            output += "\t筆直分數:\t" + LSS.ToString("00.0");
            output += "\t轉角分數:\t" + AS.ToString("00.0");
            output += "\t綜合分數:\t" + CompositeScore.ToString("00.0");
            output += "\t方林分數:\t" + FangLingScore.ToString("00.0");
            //output += "\t林的分數(修):\t" + scoreOfLinM.ToString("00.0");
            //output += "\t角Y方向離差:\t" + CornerResidualY.ToString();
            //output += "\t角X方向離差:\t" + CornerResidualX.ToString();
            return output;
        }

        /// <summary>輸出用於存檔的字串</summary>
        /// <returns></returns>
        public string ToDataLine()
        {
            string output = "";
            output += Subject + "\t" + SubjectState + "\t" + RegionId + "\t" + TopDown + "\t";
            output += GI + "\t" + LPI + "\t" + LSI + "\t" + AI + "\t" + FangLingIndex+"\t";
            output += GS + "\t" + LPS + "\t" + LSS + "\t" + AS + "\t" + CompositeScore + "\t" + FangLingScore;
            return output;
        }
    }

}
