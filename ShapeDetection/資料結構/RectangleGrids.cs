using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

/// <summary>長方型網格</summary>
class RectangleGrids
{
    /// <summary>長方型網格中的列數:8</summary>
    public const int ROW_COUNT = 8;

    /// <summary>長方型網格中的一半列數:4</summary>
    public const int ROW_COUNT_HALF = 4;

    /// <summary>長方型網格中的行數:7</summary>
    public const int columnCount = 7;

    /// <summary>長方形網格裡有的磁磚數目:28</summary>
    public const int tileCount = 28;

    /// <summary>垂直勾縫數量:5</summary>
    public const int gapCountInColumn = 5;

    /// <summary>某列有的磁磚數</summary>
    public static int ColumnCountInRow(int row)
    {
        if (row % 2 == 0)
        {
            return 4;
        }
        else
        {
            return 3;
        }
    }

    /// <summary>某行有的磁磚數</summary>
    public static int RowCountInColumn(int Column)
    {
        if (Column % 2 == 0)
        {
            return 4;
        }
        else
        {
            return 4;
        }
    }

    /// <summary>長方形網格的長(左右,mm)</summary>
    public const double Length = 980.0;
    /// <summary>長方形網格的寬(上下,mm)</summary>
    public const double Width = 448.0;

    /// <summary>長方形網格的斜邊長度(mm)</summary>
    public const double hypotenuse = 1077.545359;
    /// <summary>長方形磁磚長(mm)</summary>
    public const double tileLength = 240.133;
    /// <summary>長方形磁磚寬(mm)</summary>
    public const double tileWidth = 52.1875;
    /// <summary>溝縫寬(mm)</summary>
    public const double gapWidth = 4.7;

    /// <summary>水平網格間距(像素)</summary>
    private float gridSpacingHorizontalInPixel;
    /// <summary>取得水平網格間距(像素)</summary>
    public float get_GridSpacingHorizontalInPixel
    {
        get { return gridSpacingHorizontalInPixel; }
    }

    /// <summary>垂直網格間距(像素)</summary>
    private float GridSpacingVerticalInPixel;
    /// <summary>取得垂直網格間距(像素)</summary>
    public float get_GridSpacingVerticalInPixel
    {
        get { return GridSpacingVerticalInPixel; }
    }

    /// <summary>長方形磁磚長(像素)</summary>
    private float tileLengthInPixel;
    /// <summary>取得長方形磁磚長(像素)</summary>
    public float get_tileLengthInPixel
    {
        get { return tileLengthInPixel; }
    }

    /// <summary>長方形磁磚寬(像素)</summary>
    private float tileWidthInPixel;
    /// <summary>取得長方形磁磚寬(像素)</summary>
    public float get_tileWidthInPixel
    {
        get { return tileWidthInPixel; }
    }

    /// <summary>網格左上角</summary>
    public Point GridsLT { get; private set; }
    
    /// <summary>網格右下角</summary>
    public Point GridsRD { get; private set; }

    /// <summary>網格寬</summary>
    public double WidthInPixel { get { return Math.Abs(GridsRD.X - GridsLT.X); } }

    /// <summary>網格高</summary>
    public double HeightInPixel { get { return Math.Abs(GridsRD.Y - GridsLT.Y); } }
    


    private double mmPerPixel;
    /// <summary>依照網格比例，轉換長度(pixel->mm)</summary>
    public double mmFormPixel(double pixel)
    {
        return pixel * mmPerPixel;
    }
    /// <summary>依照網格比例，轉換長度(mm->pixel)</summary>
    public double PixelFormMm(double mm)
    {
        return mm / mmPerPixel;
    }

    /// <summary>長磁磚網格建構式</summary>
    public RectangleGrids(Point theGridRD, Point theGridLT)
    {
        GridsLT = theGridLT;
        GridsRD = theGridRD;

        mmPerPixel = RectangleGrids.hypotenuse / myMath.GetDistance(GridsLT, GridsRD);

        gridSpacingHorizontalInPixel = (float)((RectangleGrids.tileLength + RectangleGrids.gapWidth) / mmPerPixel);
        GridSpacingVerticalInPixel = (float)((RectangleGrids.tileWidth + RectangleGrids.gapWidth) / mmPerPixel);

        tileLengthInPixel = (float)(RectangleGrids.tileLength / mmPerPixel);
        tileWidthInPixel = (float)(RectangleGrids.tileWidth / mmPerPixel);
    }

    /// <summary>磁磚理論上的左上點</summary>
    public PointF TileModelLT(int column, int row)
    {
        float x = GridsLT.X + gridSpacingHorizontalInPixel * column / 2;
        float y = GridsLT.Y + GridSpacingVerticalInPixel * row;
        return new PointF(x, y);
    }

    /// <summary>磁磚理論上的左下點</summary>
    public PointF TileModelLD(int column, int row)
    {
        float x = GridsLT.X + gridSpacingHorizontalInPixel * column / 2;
        float y = GridsLT.Y + GridSpacingVerticalInPixel * row + tileWidthInPixel;
        return new PointF(x, y);
    }

    /// <summary>磁磚理論上的右上點</summary>
    public PointF TileModelRT(int column, int row)
    {
        float x = GridsLT.X + gridSpacingHorizontalInPixel * column / 2 + tileLengthInPixel;
        float y = GridsLT.Y + GridSpacingVerticalInPixel * row;
        return new PointF(x, y);
    }

    /// <summary>磁磚理論上的右下點</summary>
    public PointF TileModelRD(int column, int row)
    {
        float x = GridsLT.X + gridSpacingHorizontalInPixel * column / 2 + tileLengthInPixel;
        float y = GridsLT.Y + GridSpacingVerticalInPixel * row + tileWidthInPixel;
        return new PointF(x, y);
    }
}
