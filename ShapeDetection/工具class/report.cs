using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

internal class report
{
    /// <summary>旋轉角標準差</summary>
    public double AngleSD = 0;

    /// <summary>垂直傾角標準差</summary>
    public double ColumnAngleSD = 0;

    /// <summary>平均X方向離差</summary>
    public double ColumnResidualAvg = 0;

    /// <summary>平均勾縫寬</summary>
    public double GapAvg = 0;

    /// <summary>水平勾縫標準差</summary>
    public double GapHSD = 0;

    /// <summary>勾縫極差</summary>
    public double GapRange = 0;

    /// <summary>極差平均比</summary>
    public double GapRangeAvgRatio = 0;

    /// <summary>勾縫標準差</summary>
    public double GapSD = 0;

    /// <summary>垂直勾縫標準差</summary>
    public double GapVSD = 0;

    /// <summary>水平傾角標準差</summary>
    public double RowAngleSD = 0;

    /// <summary>平均Y方向離差</summary>
    public double RowResidualAvg = 0;

    /// <summary>3m磁磚角Y方向離差</summary>
    public double ResidualAvgY3m = 0;
    /// <summary>3m磁磚角X方向離差</summary>
    public double ResidualAvgX3m = 0;

    /// <summary>磁磚角Y方向離差</summary>
    public double CornerResidualY = 0;

    /// <summary>磁磚角X方向離差</summary>
    public double CornerResidualX = 0;

    private string fileName;
    private List<string> stringsToReport = new List<string>();

    /// <summary>建構式</summary>
    /// <param name="title">檔名</param>
    public report(string fileName)
    {
        this.fileName = Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName);
    }

    public void newLine(string input)
    {
        stringsToReport.Add(input);
    }

    /// <summary>進行存檔</summary>
    public string SaveReport(RankArea rankArea)
    {
        string txtFileName;
        switch (rankArea)
        {
            case RankArea.Top:
                txtFileName = fileName + "ReportT.txt";
                break;
            case RankArea.Down:
                txtFileName = fileName + "ReportD.txt";
                break;
            default:
                throw SmileLib.EnumTool.OutOfEnum<RankArea>();
        }

        string dataLine = "";
        using (StreamWriter sw = new StreamWriter(txtFileName, false))
        {
            string[] FileInfo = Path.GetFileName(fileName).Split('_');

            if (FileInfo.Length == 4)
            {
                sw.WriteLine("受測者\t受測狀態\t區塊\t水平勾縫標準差\t垂直勾縫標準差\t水平傾角標準差\t垂直傾角標準差\t平均Y方向離差\t平均X方向離差\t旋轉角標準差\t3M角Y方向離差\t3M角X方向離差\t角Y方向離差\t角X方向離差");
                dataLine = FileInfo[0] + FileInfo[1] + "\t" + FileInfo[2] + "\t" + FileInfo[3] + "\t";
            }
            else
            {
                sw.WriteLine("檔名\t水平勾縫標準差\t垂直勾縫標準差\t水平傾角標準差\t垂直傾角標準差\t平均Y方向離差\t平均X方向離差\t旋轉角標準差\t3M角Y方向離差\t3M角X方向離差\t角Y方向離差\t角X方向離差");
                dataLine = Path.GetFileName(fileName) + "\t";
            }
            dataLine +=
                GapHSD.ToString() + "\t" +
                GapVSD.ToString() + "\t" +
                RowAngleSD.ToString() + "\t" +
                ColumnAngleSD.ToString() + "\t" +
                RowResidualAvg.ToString() + "\t" +
                ColumnResidualAvg.ToString() + "\t" +
                AngleSD.ToString() + "\t" + 
                ResidualAvgY3m.ToString()+ "\t" + 
                ResidualAvgX3m.ToString()+ "\t" +
                CornerResidualY.ToString() + "\t" + 
                CornerResidualX.ToString();
            sw.WriteLine(dataLine);

            foreach (string line in stringsToReport)
            {
                sw.WriteLine(line);
            }
            sw.Close();
        }
        return dataLine;
    }

    #region 各面向誤差量門檻

    /// <summary>正方磚的誤差門檻</summary>
    private VarianceThreshold SquareTileVH = new VarianceThreshold(0.3669, 0.052, 0.1732, 0.3575);

    /// <summary>二丁掛的誤差門檻</summary>
    private VarianceThreshold RectangleTileVH = new VarianceThreshold(0.5468, 0.1751, 0.0949, 0.0996);

    /// <summary>正方磚分數斜率</summary>
    private ScoreSlope SquareTileSS = new ScoreSlope(-70.3322140939373, -502.415943384373, -184.165325702495, -86.6152559413475, -140);

    /// <summary>二丁掛分數斜率</summary>
    private ScoreSlope RectangleTileSS = new ScoreSlope(-63.6634155371259, -170.958735555665, -453.596710992925, -336.349870960398, -138.70734257);



    /// <summary>各面向誤差量門檻</summary>
    private class VarianceThreshold
    {
        public VarianceThreshold(double gap, double parallel, double straight, double angle)
        {
            Gap = gap;
            Parallel = parallel;
            Straight = straight;
            Angle = angle;
        }

        /// <summary>旋轉角</summary>
        public double Angle { get; private set; }

        /// <summary>勾縫</summary>
        public double Gap { get; private set; }

        /// <summary>平行</summary>
        public double Parallel { get; private set; }

        /// <summary>筆直</summary>
        public double Straight { get; private set; }

        
    }

    private class ScoreSlope : VarianceThreshold
    {
        public double FangAndLing { get; private set; }

        public ScoreSlope(double gap, double parallel, double straight, double angle,double fl)
            :base(gap,parallel,straight,angle)
        {
            FangAndLing = fl;
        }
    }

    #endregion 各面向誤差量門檻

    /// <summary>計算分數</summary>
    public string ScoringByVariance(TilesType theTilesType)
    {
        ScoreSlope selectSS = theTilesType == TilesType.Square? SquareTileSS:RectangleTileSS;
        double AvgGapSD = (GapHSD + GapVSD) / 2;
        double AvgLineAngleSD = (RowAngleSD + ColumnAngleSD) / 2;
        double AvgResidual = (RowResidualAvg + ColumnResidualAvg) / 2;
        double ResultAngleSD = AngleSD;
        double scoreOfGap = 0;
        double scoreOfParallel = 0;
        double scoreOfStraight = 0;
        double scoreOfAngle = 0;
        double LinResidualAvg = (ResidualAvgX3m +ResidualAvgY3m ) /2;
        double scoreOfLin = 100 - 20 * LinResidualAvg;
        double scoreOfLinM = 100 + selectSS.FangAndLing * ((CornerResidualX+CornerResidualY)/2);
        CalScore(theTilesType, AvgGapSD, AvgLineAngleSD, AvgResidual
            , ref scoreOfGap, ref scoreOfParallel, ref scoreOfStraight, ref scoreOfAngle);
        string output = "";
        output += fileName;
        output += "\t勾縫分數:\t" + scoreOfGap.ToString("00.0");
        output += "\t平行分數:\t" + scoreOfParallel.ToString("00.0");
        output += "\t筆直分數:\t" + scoreOfStraight.ToString("00.0");
        output += "\t轉角分數:\t" + scoreOfAngle.ToString("00.0");
        output += "\t綜合分數:\t" + ((scoreOfGap +scoreOfParallel+scoreOfStraight+ scoreOfAngle) / 4).ToString("00.0");
        output += "\t林的分數:\t" + scoreOfLin.ToString("00.0");
        //output += "\t林的分數(修):\t" + scoreOfLinM.ToString("00.0");
        //output += "\t角Y方向離差:\t" + CornerResidualY.ToString();
        //output += "\t角X方向離差:\t" + CornerResidualX.ToString();
        return output;
    }

    private void CalScore(TilesType theTilesType, double AvgGapSD, double AvgLineAngleSD, double AvgResidual, ref double scoreOfGap, ref double scoreOfParallel, ref double scoreOfStraight, ref double scoreOfAngle)
    {
#if false
        VarianceThreshold VTH;
        if (theTilesType == TilesType.Square)
        {
            VTH = SquareTileVH;
        }
        else
        {
            VTH = RectangleTileVH;
        }
        scoreOfGap = ScoreOfVariance(AvgGapSD, VTH.Gap);
        scoreOfParallel = ScoreOfVariance(AvgLineAngleSD, VTH.Parallel);
        scoreOfStraight = ScoreOfVariance(AvgResidual, VTH.Straight);
        scoreOfAngle = ScoreOfVariance(AngleSD, VTH.Angle); 
#endif
        ScoreSlope ss;
        if (theTilesType == TilesType.Square)
        {
            ss = SquareTileSS;
        }
        else
        {
            ss = RectangleTileSS;
        }
        scoreOfGap = ScoreOfVariance(AvgGapSD, ss.Gap,true);
        scoreOfParallel = ScoreOfVariance(AvgLineAngleSD, ss.Parallel, true);
        scoreOfStraight = ScoreOfVariance(AvgResidual, ss.Straight, true);
        scoreOfAngle = ScoreOfVariance(AngleSD, ss.Angle, true); 

    }

    /// <summary>變異量線性評分公式</summary>
    /// <param name="variance"></param>
    /// <param name="ThresholdOrSlope"></param>
    /// <returns></returns>
    private double ScoreOfVariance(double variance, double ThresholdOrSlope ,bool useSlope = false)
    {
        if (!useSlope)
        {
            return (ThresholdOrSlope - variance) / ThresholdOrSlope * 30 + 70; 
        }
        else
        {
            return 100+ ThresholdOrSlope * variance; 
        }
    }


}