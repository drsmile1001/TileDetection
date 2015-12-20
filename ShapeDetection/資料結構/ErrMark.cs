using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;

namespace 磁磚辨識評分
{
    public class ErrMark
    {
        public const double GapSquareTh = 0.4336561900250077;
        public const double ParallelismSquareTh = 0.0607066722336596;
        public const double StraightnessSquareTh = 0.1656120655919259;
        public const double AngleSquareThe = 0.3521319618411498;
        public List<LineSegment2DF> GapErr = new List<LineSegment2DF>();
    }


}
