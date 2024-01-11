using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HexagonBufferDrawer
{
    public DebugChannelData ChannelData;
    public HexagonBuffer Buffer;
    public MonoBehaviour Context;
    public Editor Editor;
    public float HexagonSizeRatio;
    public bool IsEnabled;
    public bool IsStandby;

    private bool m_Repaint;
    private bool m_RepaintOnFirstUpdate;
    private IEnumerator m_StopRepaintCoroutine;
    private readonly YieldInstruction m_StopRepaintDelay = new WaitForSeconds(0.5f);

    public void Enable(MonoBehaviour context, DebugChannelData channelData, HexagonBuffer buffer)
    {
        ChannelData = channelData;
        Buffer = buffer;
        HexagonSizeRatio = buffer.Rank / (float)buffer.Rank;

        Context = context;
        m_RepaintOnFirstUpdate = true;
        m_StopRepaintCoroutine ??= StopRepaintCoroutine();

        IsEnabled = true;
        IsStandby = false;
    }

    public void Standby()
    {
        IsEnabled = false;
        IsStandby = true;
    }

    public void Disable()
    {
        IsEnabled = false;
        IsStandby = false;
        m_RepaintOnFirstUpdate = false;
    }

    public void OnSensorUpdate()
    {
        if (m_RepaintOnFirstUpdate) {
            EnableRepaint();
        }

        if(m_Repaint && Editor != null) {
            Editor.Repaint();
        }
    }

    public void EnableRepaint()
    {
        if(Context != null) {
            CoroutineUtil.Start(Context, m_StopRepaintCoroutine);
            m_Repaint = true;
        }
    }

    private IEnumerator StopRepaintCoroutine()
    {
        yield return m_StopRepaintDelay;
        m_Repaint = false;
    }
}
