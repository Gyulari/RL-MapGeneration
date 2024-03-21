using UnityEngine;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine.AI;
using Unity.MLAgents;
using Grpc.Core;
using System.Linq.Expressions;
using Gyulari.HexSensor.Util;
using Gyulari.HexSensor;

public class HexagonSensor : ISensor
{
    // Sensor 구성을 BufferSensor에서 몇 가지 채택하여 변경 중

    private readonly string m_Name;
    private readonly HexagonBuffer m_HexagonBuffer;
    private int m_MaxNumObs;
    private int m_ObsSize;
    float[] m_ObservationBuffer;
    int m_CurrentNumObservables;
    ObservationSpec m_ObservationSpec;

    public HexagonSensor(
        string name,
        HexagonBuffer buffer,
        int maxNumberObs,
        int obsSize,
        ObservationType observationType)
    {
        m_Name = name;
        m_MaxNumObs = maxNumberObs;
        m_ObsSize = obsSize;
        m_ObservationBuffer = new float[m_ObsSize * m_MaxNumObs];
        m_CurrentNumObservables = 0;
        m_ObservationSpec = ObservationSpec.VariableLength(m_MaxNumObs, m_ObsSize);

        buffer.GetShape().Validate();
        m_HexagonBuffer = buffer;
    }

    public ObservationSpec GetObservationSpec()
    {
        return m_ObservationSpec;
    }

    public int Write(ObservationWriter writer)
    {
        for(int i=0; i < m_MaxNumObs; i++) {
            for (int ch=0; ch < m_HexagonBuffer.NumChannels; ch++) {
                if(m_HexagonBuffer.Read(i, ch) != -1) {
                    writer[m_ObsSize * i] = ch;
                    writer[m_ObsSize * i + 1] = m_HexagonBuffer.Read(i, ch);
                }
            }
        }
        /*
        int numWritten = 0;
        int rank = m_HexagonBuffer.Rank;
        int numChannels = m_HexagonBuffer.NumChannels;
        int maxHexNum = CalHexPropertyUtil.GetMaxHexCount(rank);

        float[] writerBuffer = new float[2];
        writerBuffer[0] = 

        writer[index] = channel;

        for (int c = 0; c < numChannels; c++) {
            for (int h = 0; h < maxHexNum; h++) {
                writer[h, c] = m_HexagonBuffer.Read(h, c);
                numWritten++;
            }
        }
        */

        return 0;
    }

    public virtual byte[] GetCompressedObservation()
    {
        return null;
    }

    public virtual void Update()
    {
        Reset();
    }

    public virtual void Reset()
    {
        m_CurrentNumObservables = 0;
        Array.Clear(m_ObservationBuffer, 0, m_ObservationBuffer.Length);
    }

    public CompressionSpec GetCompressionSpec()
    {
        return CompressionSpec.Default();
    }

    public string GetName()
    {
        return m_Name;
    }

    public BuiltInSensorType GetBuiltInSensorType()
    {
        return BuiltInSensorType.BufferSensor;
    }
}
