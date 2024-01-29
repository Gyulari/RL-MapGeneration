using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Gyulari.HexSensor.Util
{
    public class IOUtil
    {
        // �Ű������� path �����Ͽ� �뵵�� �°� ��� �����ϵ��� ������ ��
        public static void ExtractDataByJson<T>(List<T> list)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            string path = Path.Combine(Application.dataPath, "save.json");
            string jsonString = JsonConvert.SerializeObject(list, settings);
            File.WriteAllText(path, jsonString);
        }

        public static List<T> ImportDataByJson<T>()
        {
            List<T> data = new List<T>();

            string path = Path.Combine(Application.dataPath, "save.json");
            string jsonString = File.ReadAllText(path);
            data = JsonConvert.DeserializeObject<List<T>>(jsonString);

            return data;
        }
    }
}