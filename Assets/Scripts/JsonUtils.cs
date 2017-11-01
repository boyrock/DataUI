using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public class JsonUtils
{
    //public static void LoadJsonFile<T>(ref object overwriteTarget, string filePath = "appData.json")
    //{
    //    //var path = Path.Combine(Application.streamingAssetsPath, filePath);
    //    if (File.Exists(filePath))
    //    {
    //        if(overwriteTarget.GetType().IsArray)
    //        {
    //            GetJsonArray<T>(overwriteTarget, File.ReadAllText(filePath));
    //        }
    //        else
    //            JsonUtility.FromJsonOverwrite(File.ReadAllText(filePath), overwriteTarget);
    //    }
    //    else
    //        SaveJsonFile<T>(overwriteTarget, filePath);
    //}

    //public static T[] LoadJsonFileForArrayData<T>(string filePath = "appData.json")
    //{
    //    return JsonToArray<T>(File.ReadAllText(filePath));
    //}

    //public static void SaveJsonFileForArrayData<T>(object obj, string filePath = "appData.json")
    //{
    //    string json;

    //    IEnumerable enumerable = obj as IEnumerable;
    //    json = ArrayToJson<T>(enumerable.Cast<T>().ToArray<T>());

    //    var path = Path.Combine(Application.streamingAssetsPath, filePath);
    //    var dPath = Path.GetDirectoryName(path);
    //    if (!Directory.Exists(dPath))
    //        Directory.CreateDirectory(dPath);

    //    using (var writer = new StreamWriter(path))
    //        writer.Write(json);
    //    //#if UNITY_EDITOR
    //    //        UnityEditor.AssetDatabase.Refresh();
    //    //#endif
    //}

    public static void LoadJsonFile(object overwriteTarget, string filePath = "appData.json")
    {
        if (File.Exists(filePath))
            JsonUtility.FromJsonOverwrite(File.ReadAllText(filePath), overwriteTarget);
        else
            SaveJsonFile(overwriteTarget, filePath);
    }
    
    public static void SaveJsonFile(object obj, string filePath = "appData.json")
    {
        string json = JsonUtility.ToJson(obj);

        var path = Path.Combine(Application.streamingAssetsPath, filePath);
        var dPath = Path.GetDirectoryName(path);
        if (!Directory.Exists(dPath))
            Directory.CreateDirectory(dPath);

        using (var writer = new StreamWriter(path))
            writer.Write(json);
//#if UNITY_EDITOR
//        UnityEditor.AssetDatabase.Refresh();
//#endif
    }

    //static T[] JsonToArray<T>(string json)
    //{
    //    Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
    //    return wrapper.array;
    //}

    //static string ArrayToJson<T>(T[] array)
    //{
    //    Wrapper<T> wrapper = new Wrapper<T>();
    //    wrapper.array = array;
    //    return JsonUtility.ToJson(wrapper, true);
    //}

    //[System.Serializable]
    //private class Wrapper<T>
    //{
    //    public T[] array;
    //}
}