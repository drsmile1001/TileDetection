using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class LoadFileTool
{
    public static object Fail = new object();

    public static object TryLoadFile<T>(string filename)
    {
        object newObject;
        BinaryFormatter formatter = new BinaryFormatter();
        try
        {
            Stream input = File.OpenRead(filename);
            newObject = (T)formatter.Deserialize(input);
            input.Close();
        }
        catch (Exception)
        {
            return Fail;
        }
        return newObject;
    }
}

