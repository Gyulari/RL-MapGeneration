using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gyulari.HexSensor.Util;
using Gyulari.HexSensor;

public class HexagonMapTest : MonoBehaviour
{
    public GameObject agent;
    public List<GameObject> tiles;
    
    int maxHexCount = 7; // tile ¼ö

    TestAgent _TestAgent;

    struct Node
    {
        public int channel;
        public int link;

        public Node (int channel, int link)
        {
            this.channel = channel;
            this.link = link;
        }
    }

    List<Node> nodes = new List<Node>();

    private void Start()
    {
        _TestAgent = agent.GetComponent<TestAgent>();

        nodes.Add(new Node(0, 1));
        nodes.Add(new Node(1, 1));
        nodes.Add(new Node(2, 1));
        nodes.Add(new Node(3, 2));
        nodes.Add(new Node(3, 2));
        nodes.Add(new Node(4, 3));
        nodes.Add(new Node(5, 3));

        List<HexCell_CenterPosInfoByRank> hexCellCenterPosInfo = IOUtil.ImportDataByJson<HexCell_CenterPosInfoByRank>("Config/HexCellCenterPosInfo.json");

        for (int i=0; i<maxHexCount; i++) {
            _TestAgent.WriteNode(nodes[i].channel, i, nodes[i].link);

            if (i == 0)
                Instantiate(tiles[nodes[i].channel], new Vector3(0f, 0.05f, 0f), Quaternion.Euler(90f, 30f, 0f));
            else
                Instantiate(tiles[nodes[i].channel],
                    new Vector3(hexCellCenterPosInfo[1].cell_Info[i - 1].centerPos.x / 10.0f,
                                0.05f,
                                hexCellCenterPosInfo[1].cell_Info[i - 1].centerPos.y / 10.0f),
                    Quaternion.Euler(90f, 30f, 0f));
        }
    }
}
