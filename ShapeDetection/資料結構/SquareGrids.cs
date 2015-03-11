using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

/// <summary>正方形網格</summary>
class SquareGrids
{
    public const int rowCount = 12;
    public const int rowCountHalf = 6;
    public const int columnCount = 12;

    

    
    /// <summary>正方形網格的斜邊長度(mm)</summary>
    public const double hypotenuse = 848.5281374;
    /// <summary>正方形磁磚長(mm)</summary>
    public const double tileLength = 46.78125;
    /// <summary>溝縫寬(mm)</summary>
    public const double gapWidth = 3;

  
    /// <summary>網格間距</summary>
    private float gridSpacingInPixel;
    /// <summary>取得網格間距(像素)</summary>
    public float get_gridSpacingInPixel
    {
        get { return gridSpacingInPixel; }
    }

    /// <summary>正方形磁磚邊長(像素)</summary>
    private float tileLengthInPixel;
    /// <summary>取得正方形磁磚邊長(像素)</summary>
    public float get_tileLengthInPixel
    {
        get { return tileLengthInPixel; }
    }

    /// <summary>網格左上角</summary>
    private Point GridsLT;
    public Point get_GridsLT
    {
        get { return GridsLT; }
    }
    /// <summary>網格右下角</summary>
    private Point GridsRD;
    public Point get_GridsRd
    {
        get { return GridsRD; }
    }

    private double mmPerPixel;
    /// <summary>依照網格，轉換長度(pixel->mm)</summary>
    public double mmFormPixel(double pixel)
    {
        return pixel * mmPerPixel;
    }

    /// <summary>正方形網格建構式</summary>
    public SquareGrids(Point theGridRD, Point theGridLT)
    {
        GridsLT = theGridLT;
        GridsRD = theGridRD;

        mmPerPixel = SquareGrids.hypotenuse / myMath.GetDistance(GridsLT, GridsRD);

        gridSpacingInPixel = (float)((SquareGrids.tileLength + SquareGrids.gapWidth) / mmPerPixel);
        tileLengthInPixel = (float)(SquareGrids.tileLength / mmPerPixel);
    }

    /// <summary>磁磚理論上的左上點</summary>
    public PointF TileModelLT(int column, int row)
    {
        float x = GridsLT.X + gridSpacingInPixel * column;
        float y = GridsLT.Y + gridSpacingInPixel * row;
        return new PointF(x, y);
    }

    /// <summary>磁磚理論上的左下點</summary>
    public PointF TileModelLD(int column, int row)
    {
        float x = GridsLT.X + gridSpacingInPixel * column;
        float y = GridsLT.Y + gridSpacingInPixel * row + tileLengthInPixel;
        return new PointF(x, y);
    }

    /// <summary>磁磚理論上的右上點</summary>
    public PointF TileModelRT(int column, int row)
    {
        float x = GridsLT.X + gridSpacingInPixel * column + tileLengthInPixel;
        float y = GridsLT.Y + gridSpacingInPixel * row;

        return new PointF(x, y);

    }

    /// <summary>磁磚理論上的右下點</summary>
    public PointF TileModelRD(int column, int row)
    {
        float x = GridsLT.X + gridSpacingInPixel * column + tileLengthInPixel;
        float y = GridsLT.Y + gridSpacingInPixel * row + tileLengthInPixel;
        return new PointF(x, y);
    }
}
