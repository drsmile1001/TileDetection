using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;

internal static class ExtensionMethods
{
    public static Color GetColor(this Image<Bgr, Byte> image, int x, int y)
    {
        if (x > image.Width || y > image.Height)
        {
            return Color.Black;
        }

        return Color.FromArgb(image.Data[y, x, 2], image.Data[y, x, 1], image.Data[y, x, 0]);
    }

    public static Color GetColor(this Image<Bgr, Byte> image, float x, float y)
    {
        double px = (Math.Ceiling(x) - x);
        double py = (Math.Ceiling(y) - y);
        Color x0y0 = image.GetColor((int)Math.Floor(x), (int)Math.Floor(y));
        Color x1y0 = image.GetColor((int)Math.Ceiling(x), (int)Math.Floor(y));
        Color x0y1 = image.GetColor((int)Math.Floor(x), (int)Math.Ceiling(y));
        Color x1y1 = image.GetColor((int)Math.Ceiling(x), (int)Math.Ceiling(y));

        byte r = (byte)(x0y0.R * px * py + x0y1.R * px * (1 - py) + x1y0.R * (1 - px) * py + x1y1.R * (1 - px) * (1 - py));
        byte g = (byte)(x0y0.G * px * py + x0y1.G * px * (1 - py) + x1y0.G * (1 - px) * py + x1y1.G * (1 - px) * (1 - py));
        byte b = (byte)(x0y0.B * px * py + x0y1.B * px * (1 - py) + x1y0.B * (1 - px) * py + x1y1.B * (1 - px) * (1 - py));

        return Color.FromArgb(r, g, b);
    }

    public static double GetGray(this Image<Bgr, Byte> image, int x, int y)
    {
        Color c = image.GetColor(x, y);
        return GetGray(c);
    }

    public static double GetGray(this Image<Bgr, Byte> image, float x, float y)
    {
        Color c = image.GetColor(x, y);
        return GetGray(c);
    }

    private static double GetGray(Color c)
    {
        return 0.072169 * c.B + 0.715160 * c.G + 0.212671 * c.R;
    }

    public static void SetColor(this Image<Bgr, Byte> image, int x, int y, Color c)
    {
        image.Data[x, y, 0] = c.B;
        image.Data[x, y, 1] = c.G;
        image.Data[x, y, 2] = c.R;
    }

    public static void SetColor(this Image<Bgr, Byte> image, int x, int y, byte r, byte g, byte b)
    {
        image.Data[x, y, 0] = b;
        image.Data[x, y, 1] = g;
        image.Data[x, y, 2] = r;
    }

    public static Point ToPoint(this PointF p)
    {
        return new Point((int)p.X, (int)p.Y);
    }
}