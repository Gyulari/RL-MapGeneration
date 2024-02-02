using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Gyulari.HexSensor.Util
{
    public class IOUtil
    {
        public static void ExportDataByJson<T>(List<T> list, string path)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };

            string fPath = Path.Combine(Application.dataPath, path);
            string jsonString = JsonConvert.SerializeObject(list, settings);
            File.WriteAllText(fPath, jsonString);
        }

        public static List<T> ImportDataByJson<T>(string path)
        {
            List<T> data = new List<T>();

            string fPath = Path.Combine(Application.dataPath, path);
            string jsonString = File.ReadAllText(fPath);
            data = JsonConvert.DeserializeObject<List<T>>(jsonString);

            return data;
        }
    }
}