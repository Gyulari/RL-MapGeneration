using Gyulari.HexSensor.Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class HexCellPixelsTest : MonoBehaviour
{
    public int resolution;
    public GameObject pixel;

    private void Start()
    {
        List<HexagonCellPixels> hexCellPixelsInfo = IOUtil.ImportDataByJson<HexagonCellPixels>("Config/HexCellPixelsInfo.json");

        int width = resolution * 7;

        for(int i=0; i < hexCellPixelsInfo[resolution-1].pixels.Length; i++) {
            if (hexCellPixelsInfo[resolution-1].pixels[i] == true) {
                Vector3 pos = new Vector3(i % width, (i / width), 0);
                Instantiate(pixel, pos, Quaternion.identity);
            }
        }
    }
}
