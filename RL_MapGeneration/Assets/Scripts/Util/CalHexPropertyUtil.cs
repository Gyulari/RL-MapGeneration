using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gyulari.HexSensor.Util
{
    public class CalHexPropertyUtil : MonoBehaviour
    {
        public static int GetRankByHexIdx(int hexIdx)
        {
            int targetRank = 1;

            if(hexIdx == 0) {
                return targetRank;
            }

            while(hexIdx > 0) {
                hexIdx -= 6 * targetRank;
                targetRank++;
            }

            return targetRank;
        }

        public static int GetHexNumberInRank(int rank, int hexIdx)
        {
            if (rank == 1)
                return 0;

            int hexNum_preRank = 1;

            for (int i = 1; i < rank - 1; i++) {
                hexNum_preRank += i * 6;
            }

            return hexIdx - hexNum_preRank;
        }

        public static int GetMaxHexCount(int maxRank)
        {
            int maxHexIdx = 1;

            for (int i = 1; i < maxRank; i++) {
                maxHexIdx += 6 * i;
            }

            return maxHexIdx;
        }
    }
}