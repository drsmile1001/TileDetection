using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

class report
{
    string fileName;
    List<string> stringsToReport = new List<string>();
    /// <summary>水平勾縫標準差</summary>
    public double GapHSD = 0;
    /// <summary>垂直勾縫標準差</summary>
    public double GapVSD = 0;
    /// <summary>水平傾角標準差</summary>
    public double RowAngleSD = 0;
    /// <summary>垂直傾角標準差</summary>
    public double ColumnAngleSD = 0;
    /// <summary>平均Y方向離差</summary>
    public double RowResidualAvg = 0;
    /// <summary>平均X方向離差</summary>
    public double ColumnResidualAvg = 0;
    /// <summary>旋轉角標準差</summary>
    public double AngleSD = 0;

    /// <summary>建構式</summary>
    /// <param name="title">檔名</param>
    public report(string fileName)
    {
        this.fileName = Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName);
    }

    public void newLine(string input)
    {
#if false
		using (StreamWriter sw = new StreamWriter(fileName,true))
        {
            sw.WriteLine(input);
            sw.Close();
        }  
#endif
        stringsToReport.Add(input);
    }


    /// <summary>進行存檔</summary>
    public void SaveReport(bool rankTopOnly)
    {
        string txtFileName = rankTopOnly ? (fileName + "ReportT.txt") : (fileName + "Report.txt");
        using (StreamWriter sw = new StreamWriter(txtFileName, false))
        {
            string[] FileInfo = Path.GetFileName(fileName).Split('_');
            string dataLine = "";
            if (FileInfo.Length == 4)
            {
                sw.WriteLine("受測者\t受測狀態\t區塊\t水平勾縫標準差\t垂直勾縫標準差\t水平傾角標準差\t垂直傾角標準差\t平均Y方向離差\t平均X方向離差\t旋轉角標準差");
                dataLine = FileInfo[0] + FileInfo[1] + "\t" + FileInfo[2] + "\t" + FileInfo[3] + "\t";

            }
            else
            {
                sw.WriteLine("檔名\t水平勾縫標準差\t垂直勾縫標準差\t水平傾角標準差\t垂直傾角標準差\t平均Y方向離差\t平均X方向離差\t旋轉角標準差");
                dataLine = Path.GetFileName(fileName) + "\t";
            }
            dataLine +=
                GapHSD.ToString() + "\t" +
                GapVSD.ToString() + "\t" +
                RowAngleSD.ToString() + "\t" +
                ColumnAngleSD.ToString() + "\t" +
                RowResidualAvg.ToString() + "\t" +
                ColumnResidualAvg.ToString() + "\t" +
                AngleSD.ToString();
            sw.WriteLine(dataLine);
            System.Windows.Forms.Clipboard.SetText(dataLine);
            foreach (string line in stringsToReport)
            {
                sw.WriteLine(line);
            }
            sw.Close();
        }
        Console.Beep();
    }

    public string result(TilesType theTilesType)
    {
        double AvgGapSD = (GapHSD + GapVSD) / 2;
        double AvgLineAngleSD = (RowAngleSD + ColumnAngleSD) / 2;
        double AvgResidual = (RowResidualAvg + ColumnResidualAvg)/2;
        double ResultAngleSD = AngleSD;
        if (theTilesType == TilesType.Square)
        {
            
            double maxGapSD = 1.409306;
            double minGapSD = 0.391957;
            AvgGapSD = (maxGapSD - AvgGapSD) / (maxGapSD - minGapSD);
            double maxLineAngleSD = 0.509698;
            double minLineAngleSD = 0.030265;
            AvgLineAngleSD = (maxLineAngleSD - AvgLineAngleSD) / (maxLineAngleSD - minLineAngleSD);
            double maxResidual = 0.865798;
            double minResidual = 0.158384;
            AvgResidual = (maxResidual - AvgResidual) / (maxResidual - minResidual);
            double maxAngleSD = 1.330636;
            double minAngleSD = 0.262421;
            ResultAngleSD = (maxAngleSD - ResultAngleSD) / (maxAngleSD - minAngleSD);
        }
        else
        {
            double maxGapSD = 1.701376;
            double minGapSD = 0.355462;
            AvgGapSD = (maxGapSD - AvgGapSD) / (maxGapSD - minGapSD);
            double maxLineAngleSD = 0.314535;
            double minLineAngleSD = 0.037904;
            AvgLineAngleSD = (maxLineAngleSD - AvgLineAngleSD) / (maxLineAngleSD - minLineAngleSD);
            double maxResidual = 0.629412;
            double minResidual = 0.117022;
            AvgResidual = (maxResidual - AvgResidual) / (maxResidual - minResidual);
            double maxAngleSD = 0.359992;
            double minAngleSD = 0.053626;
            ResultAngleSD = (maxAngleSD - ResultAngleSD) / (maxAngleSD - minAngleSD);
        }
        string output = "";
        output += "平均勾縫標準差:" + AvgGapSD.ToString("0.000");
        output += "平均傾角標準差" + AvgLineAngleSD.ToString("0.000");
        output += "平均中心距"+ AvgResidual.ToString("0.000");
        output += "旋轉角" + ResultAngleSD.ToString("0.000");
        output += "綜合分數" + (AvgGapSD + AvgLineAngleSD + AvgResidual + ResultAngleSD) / 4;
        return output;
    }

}

