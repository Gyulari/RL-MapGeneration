using Gyulari.HexSensor.Util;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class HexCenterPosTest : MonoBehaviour
{
    public List<GameObject> dot = new List<GameObject>();

    private void Start()
    {
        List<HexCell_CenterPosInfoByRank> hccpi = IOUtil.ImportDataByJson<HexCell_CenterPosInfoByRank>("Config/HexCellCenterPosInfo.json");
        List<MaterialInfo> mList = IOUtil.ImportDataByJson<MaterialInfo>("Config/MaterialInfos.json");

        int i = 0;

        foreach(var hci in hccpi) {
            if(i == 8) {
                for(int k=0; k<hci.cell_Info.Count; k++) {
                    dot[0].GetComponent<MeshRenderer>().material.color = mList[0].color;
                    Instantiate(dot[0], new Vector2(hci.cell_Info[k].centerPos.x/10f, hci.cell_Info[k].centerPos.y/10f), Quaternion.identity);
                }
            }
            else {
                for(int j=0; j<hci.cell_Info.Count; j++) {
                    dot[i].GetComponent<MeshRenderer>().material.color = mList[i].color;
                    Instantiate(dot[i], new Vector2(hci.cell_Info[j].centerPos.x/10f, hci.cell_Info[j].centerPos.y / 10f), Quaternion.identity);
                }
            }
            i++;
        }

        /*
        foreach(var cell in cellList) {
             List<HexCell_Info> hexCell = cell.cell_Info;
            for(int j=0; j < 6; j++) {
                if (i == 8) {
                    Instantiate(dot[0], new Vector2(cellInfo[0].centerPos.x / 10f, cellInfo.centerPos.y / 10f), Quaternion.identity);
                    dot[0].GetComponent<MeshRenderer>().material.color = mList[0].color;
                    break;
                }
                else {
                    Instantiate(dot[i], new Vector2(cellInfo.centerPos.x / 10f, cellInfo.centerPos.y / 10f), Quaternion.identity);
                    dot[i].GetComponent<MeshRenderer>().material.color = mList[i].color;
                }
                i++;
            }
        }
        */
    }
}
