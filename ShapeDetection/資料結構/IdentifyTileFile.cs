using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.Structure;
using Emgu.CV;

#region 存讀檔用class
[Serializable]
public class IdentifyTileFile
{
    public Image<Bgr, byte> BaseImage;
    public myBox[] boxs;

    public void setBoxs(List<MCvBox2D> inputBoxs)
    {
        boxs = new myBox[inputBoxs.Count];
        for (int index = 0; index < inputBoxs.Count; index++)
        {
            boxs[index] = new myBox(inputBoxs[index]);
        }
    }

    public List<MCvBox2D> getMcvbox2DList()
    {
        List<MCvBox2D> tempList = new List<MCvBox2D>();
        for (int index = 0; index < boxs.Length; index++)
        {
            tempList.Add(boxs[index].getMcvBox2D());
        }
        return tempList;
    }
}
#endregion

