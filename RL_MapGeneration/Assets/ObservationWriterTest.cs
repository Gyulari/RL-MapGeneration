using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Barracuda;
using Unity.MLAgents.Inference;
using UnityEngine;
using Unity.MLAgents.Sensors;

public class ObservationWriterTest
{
    IList<float> m_Data;
    int m_Offset;

    Tensor data = null;
    int m_Batch;

    TensorShape m_TensorShape;


    public float this[int h, int w, int ch]
    {
        set
        {
            Debug.Log("1");
            if (m_Data != null) {
                Debug.Log("2");
                if (h < 0 || h >= m_TensorShape.height) {
                    throw new IndexOutOfRangeException($"height value {h} must be in range [0, {m_TensorShape.height - 1}]");
                }
                if (w < 0 || w >= m_TensorShape.width) {
                    throw new IndexOutOfRangeException($"width value {w} must be in range [0, {m_TensorShape.width - 1}]");
                }
                if (ch < 0 || ch >= m_TensorShape.channels) {
                    throw new IndexOutOfRangeException($"channel value {ch} must be in range [0, {m_TensorShape.channels - 1}]");
                }

                var index = m_TensorShape.Index(m_Batch, h, w, ch + m_Offset);
                Debug.Log(index);
                m_Data[index] = value;
                Debug.Log(m_Data[index]);
            }
            else {
                Debug.Log(data);
                data[m_Batch, h, w, ch + m_Offset] = value;
                Debug.Log("m_Batch : " + m_Batch);
                Debug.Log("h : " + h);
                Debug.Log("w : " + w);
                Debug.Log("ch : " + ch);
                Debug.Log("m_Offset : " + m_Offset);
                Debug.Log("data : " + data[m_Batch, h, w, ch + m_Offset]);
            }
        }
    }
}