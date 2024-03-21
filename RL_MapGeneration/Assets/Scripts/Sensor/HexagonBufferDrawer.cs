#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using Unity.Collections;
using Gyulari.HexSensor.Util;
using TMPro;
using JetBrains.Annotations;

namespace Gyulari.HexSensor
{
    [Serializable]
    public class HexagonBufferDrawer
    {
        public DebugChannelData ChannelData;
        public HexagonBuffer Buffer;
        public MonoBehaviour Context;
        public Editor Editor;
        // public float HexagonSizeRatio;
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
            // HexagonSizeRatio = buffer.Rank / (float)buffer.Rank;

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

            if (m_Repaint && Editor != null) {
                Editor.Repaint();
            }
        }

        public void EnableRepaint()
        {
            if (Context != null) {
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

    [CustomPropertyDrawer(typeof(HexagonBufferDrawer))]
    public class HexagonBufferDrawerGUI : PropertyDrawer
    {
        private static readonly GUIStyle s_Style = new GUIStyle()
        {
            richText = true,
            fontStyle = FontStyle.Bold
        };

        // Message shown when disabled state
        private static readonly GUIContent s_Msg = new GUIContent(
                    "<color=#CC6666>Available in Game Play Mode</color>");

        // Backgrounnd color
        private static readonly Color s_BGColor = new Color32(32, 32, 32, 255);

        private HexagonBufferDrawer m_Target;
        private NativeArray<Color32> m_Pixels;
        private Texture2D m_Texture;
        private Color32[] m_Black;
        private Rect m_HexagonRect;
        private Rect m_FullRect;
        private bool[] m_DrawChannel;
        private int pixelResolution = 2;

        List<HexCell_CenterPosInfoByRank> hexCellCenterPosInfo = IOUtil.ImportDataByJson<HexCell_CenterPosInfoByRank>("Config/HexCellCenterPosInfo.json");
        List<HexCell_Pixels> hexCellPixelsInfo = IOUtil.ImportDataByJson<HexCell_Pixels>("Config/HexCellPixelsInfo.json");
        private bool[] m_HexCell_Pixels;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return m_FullRect.height;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            // HexagonBufferDrawer 인스턴스인 m_Debug_HexagonBufferDrawer를 m_Target으로 저장
            m_Target ??= fieldInfo.GetValue(property.serializedObject.targetObject)
                as HexagonBufferDrawer;

            // m_Target을 활용해 현재 Editor를 가져옴
            if(m_Target.Editor == null) {
                m_Target.Editor = EditorUtil.GetEditor(property.serializedObject);
            }

            // 실행중인 경우 Repaint를 가능하게 설정
            if (Application.isPlaying && (m_Target.IsEnabled || m_Target.IsStandby)) { 
                m_Target.EnableRepaint();
            }
            else {
                return;
            }

            // 현재 이벤트가 Repaint 이벤트인지 여부
            bool draw = Event.current.type == EventType.Repaint;

            // Enable 상태인 경우
            if (m_Target.IsEnabled) {
                if (draw) {
                    // Rect 연산
                    CalcRects(rect);
                    // var tSizeTuple = GetTextureSizeByRank(m_Target.Buffer.Rank, pixelResolution);
                    var (width, height) = GetTextureSizeByRank(m_Target.Buffer.Rank, pixelResolution);
                    ValidateTexture(width, height);
                    m_HexCell_Pixels = hexCellPixelsInfo[pixelResolution - 1].pixels;

                    if (m_Target.ChannelData.HasHexagonPositions) {
                        // DrawGL(DrawBackground, DrawPartialGrid);
                    }
                    else {
                        DrawGL(DrawBackground, DrawFullGrid);
                    }

                    EditorGUI.DrawPreviewTexture(m_HexagonRect, m_Texture);
                }
                DrawLabels();
            }
            else if (m_Target.IsStandby) {
                if (draw) {
                    DrawGL(DrawBackground);
                }
            }
            else {
                m_FullRect.height = 16;
                rect.x += 2;
                rect.y += 1;
                EditorGUI.LabelField(rect, s_Msg, s_Style);
            }
        }

        // rect 연산
        private void CalcRects(Rect rect)
        {
            // 현재 에디터 뷰의 너비
            float width = EditorGUIUtility.currentViewWidth;

            m_HexagonRect = rect;
            m_HexagonRect.x = EditorGUIUtility.labelWidth;    // 인스펙터 좌측 여백(채널 정보가 출력되는 라벨 공간에 +20) 부여
            m_HexagonRect.width = width - m_HexagonRect.x - 20;    // 여백을 뺀 만큼을 너비로 지정
            // m_HexagonRect.height = m_HexagonRect.width * m_Target.HexagonSizeRatio;
            m_HexagonRect.height = m_HexagonRect.width;    // 높이는 정사각형으로 지정

            // 채널정보 라벨 공간과 디버그 버퍼 출력 공간을 감싸는 배경 rect 지정
            m_FullRect = rect;
            m_FullRect.x = 0;
            m_FullRect.width = width;
            int reqTextHeight = m_Target.Buffer.NumChannels * 16 + 6;    // 채널정보 라벨 공간의 높이를 (채널 수 x 16 + 6)로 연산
            m_FullRect.height = Mathf.Max(reqTextHeight, m_HexagonRect.height);    // 채널정보 라벨 공간의 높이와 디버그 버퍼 출력 공간 높이 중 큰 것을 배경 rect의 높이로 지정
        }

        // Texture 유효성 검사
        private void ValidateTexture(int w, int h)
        {
            // Texture가 null이거나 인자로 전달받은 w, h와 일치하지 않는다면
            if(m_Texture == null || m_Texture.width != w || m_Texture.height != h) {
                // 맞는 규격으로 새로운 texture를 생성해서 재할당
                m_Texture = new Texture2D(w, h, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point
                };
                m_Pixels = m_Texture.GetRawTextureData<Color32>();    // CPU 내의 텍스처 데이터를 직접 반환
                m_Black = Enumerable.Repeat(new Color32(0, 0, 0, 255), w * h).ToArray();    // m_Black을 전부 black 색상으로 채움
            }
        }

        private (int width, int height) GetTextureSizeByRank(int rank, int resolution)
        {
            int width = 7 * resolution * (2 * rank - 1);
            int height = 4 * resolution * (3 * rank - 1);

            return (width, height);
        }

        private void DrawGL(params Action[] jobs)
        {
            GUI.BeginClip(m_FullRect);    // 클립 영역 설정 (m_FullRect으로 그림 영역을 제한)
            GL.PushMatrix();    // GL 행렬 스택에 현재 모델뷰 행렬을 푸시
            GL.Clear(true, false, Color.black);    // GL에서 현재 렌더링 버퍼를 지움, 컬러 버퍼는 지우되(true) 깊이 버퍼는 지우지않음(false), Color.black은 클리어할 때 사용할 색상
            EditorUtil.GLMaterial.SetPass(0);    // 렌더링에 사용할 GLMaterial의 첫 번째 패스를 설정, GL에서 사용되는 Material을 설정하는 부분

            // 전달된 작업 목록에 대해 반복, 작업은 Action 형식으로 전달되며, 이 작업에서는 GL 명령들이 수행됨
            foreach(var job in jobs) {
                job.Invoke();
            }

            GL.PopMatrix();    // 이전에 PushMatrix()로 저장한 현재 모델뷰 행렬을 복원
            GUI.EndClip();    // BeginClip()으로 시작한 클립 영역 종료
        }

        private void DrawBackground()
        {
            // Full
            GL.Begin(GL.QUADS);
            GL.Color(s_BGColor);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(m_FullRect.width, 0, 0);
            GL.Vertex3(m_FullRect.width, m_FullRect.height, 0);
            GL.Vertex3(0, m_FullRect.height, 0);
            GL.End();
            // Grid
            GL.Begin(GL.QUADS);
            GL.Color(Color.black);
            GL.Vertex3(m_HexagonRect.x, 0, 0);
            GL.Vertex3(m_FullRect.width, 0, 0);
            GL.Vertex3(m_FullRect.width, m_FullRect.height, 0);
            GL.Vertex3(m_HexagonRect.x, m_FullRect.height, 0);
            GL.End();
        }

        /*
        private void DrawPartialGrid()
        {
            var channelData = m_Target.ChannelData;
            var buffer = m_Target.Buffer;
            int n = buffer.NumChannels;
            int w = buffer.Rank;

            m_Pixels.CopyFrom(m_Black);

            for (int c = 0; c < n; c++) {
                if (m_DrawChannel[c]) {
                    Color color = channelData.GetColor(c);
                    var positions = channelData.GetHexagonPositions(c);

                    foreach (var pos in positions) {
                        int i = pos.y * w + pos.x;
                        Color32 a = m_Pixels[i];
                        Color32 b = buffer.Read(c, i) * color;
                        // Add colors.
                        b.r = (byte)Math.Min(a.r + b.r, 255);
                        b.g = (byte)Math.Min(a.g + b.g, 255);
                        b.b = (byte)Math.Min(a.b + b.b, 255);
                        b.a = 255;
                        m_Pixels[i] = b;
                    }
                }
            }

            m_Texture.Apply();
        }
        */

        private void DrawFullGrid()
        {
            var channelData = m_Target.ChannelData;
            var buffer = m_Target.Buffer;
            var numChannels = buffer.NumChannels;
            var maxRank = buffer.Rank;
            var h = CalHexPropertyUtil.GetMaxHexCount(maxRank);

            m_Pixels.CopyFrom(m_Black);

            for (int hIdx = 0; hIdx < h; hIdx++) {
                for(int c = 0; c < numChannels; c++) {
                    // Draw Channel[c]는 channel label의 토글이 on 상태일 때 그릴지 말지 결정하는 것이지 채널별로 구별해주는 것이 아님
                    if (m_DrawChannel[c]) {
                        // 여기서 이번에 읽어들이는게 한 channel이도록 제한하지 않는 이상 마지막 채널의 색상으로 도배가 될 것임
                        if(buffer.Read(hIdx,c) == 1) {
                            DrawHexCell(m_Pixels, hIdx, channelData.GetColor(c), maxRank);
                        }
                        // m_Pixels[hIdx] = channelData.GetColor(c);
                    }
                }
            }

            m_Texture.Apply();
        }

        private void DrawHexCell(NativeArray<Color32> pixels, int hIdx, Color channelColor, int maxRank)
        {
            int cellRank = GetRankFromHexIndex(hIdx);
            int idxInRank = GetIndexInRank(cellRank, hIdx);
            // 7(rank-1)k + 6(rank-1)k x 7(2rank-1)k, k = pixelResolution
            // pixel 1D array 중 중앙 셀의 좌하단 첫 번째 픽셀의 index
            int originIdx = 7 * (maxRank - 1) * pixelResolution + (m_Texture.height / 2 - 4 * pixelResolution) * m_Texture.width;
            int cellOriginIdx = (int)(originIdx
                + hexCellCenterPosInfo[cellRank - 1].cell_Info[idxInRank].centerPos.x * pixelResolution
                + hexCellCenterPosInfo[cellRank - 1].cell_Info[idxInRank].centerPos.y * pixelResolution * m_Texture.width);

            for (int i = 0; i < 8 * pixelResolution; i++) {
                for (int j = 0; j < 7 * pixelResolution; j++) {
                    if (m_HexCell_Pixels[i * 7 * pixelResolution + j])
                        pixels[cellOriginIdx + m_Texture.width * i + j] = channelColor;
                }
            }

            /*
            for(int i=0; i < (maxRank-1) * pixelResolution; i++) {
                for(int j=0; j < (maxRank-2) * pixelResolution; j++) {
                    if (m_HexCell_Pixels[i * 7 * pixelResolution + j])
                        pixels[cellOriginIdx + 119 * pixelResolution * i + j] = channelColor;
                }
            }
            */

            /*
            int rank = GetRankFromHexIndex(hIdx);
            int hexIdx_InRank = GetHexIndexInRank(rank, hIdx);
            int originIdx = (int)(56f * pixelResolution + 48 * pixelResolution * 119 * pixelResolution);
            int hexCell_OriginIdx = (int)(originIdx 
                + (hexCellCenterPosInfo[rank-1].cell_Info[hexIdx_InRank].centerPos.x * pixelResolution)
                + (hexCellCenterPosInfo[rank-1].cell_Info[hexIdx_InRank].centerPos.y * pixelResolution * 119 * pixelResolution));
            // int hexCell_OriginIdx = (int)(59.5f * pixelResolution + hexCellCenterPosInfo[rank-1].cell_Info[hexIdx_InRank].centerPos.x * pixelResolution
               //  + ((52 * pixelResolution + hexCellCenterPosInfo[rank-1].cell_Info[hexIdx_InRank].centerPos.y * pixelResolution) * 7 * pixelResolution) * 119 * pixelResolution);

            for (int i=0; i < 8 * pixelResolution; i++) {
                for(int j=0; j < 7 * pixelResolution; j++) {
                    if (m_HexCell_Pixels[i * 7 * pixelResolution + j] == true)
                        pixels[hexCell_OriginIdx + 119 * pixelResolution * i + j] = channelColor;
                }
            }
            */
        }

        private int GetRankFromHexIndex(int hexIdx)
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

        private int GetIndexInRank(int rank, int hIdx)
        {
            if (rank == 1)
                return 0;

            int hexNum_preRank = 1;

            for(int i=1; i<rank-1; i++){
                hexNum_preRank += i * 6;
            }

            return hIdx - hexNum_preRank;
        }

        /*
        private void DrawFullGrid()
        {
            var channelData = m_Target.ChannelData;
            var buffer = m_Target.Buffer;
            int n = buffer.NumChannels;
            int w = buffer.Rank;
            int h = buffer.Rank;

            m_Pixels.CopyFrom(m_Black);

            for (int c = 0; c < n; c++) {
                if (m_DrawChannel[c]) {
                    Color color = channelData.GetColor(c);

                    for (int x = 0; x < w; x++) {
                        for (int y = 0; y < h; y++) {
                            int i = y * w + x;
                            Color32 a = m_Pixels[i];
                            Color32 b = buffer.Read(c, y) * color;
                            // Add colors.
                            b.r = (byte)Math.Min(a.r + b.r, 255);
                            b.g = (byte)Math.Min(a.g + b.g, 255);
                            b.b = (byte)Math.Min(a.b + b.b, 255);
                            b.a = 255;
                            m_Pixels[i] = b;
                        }
                    }
                }
            }

            m_Texture.Apply();
        }
        */

        private void DrawLabels()
        {
            var channelData = m_Target.ChannelData;
            int n = m_Target.Buffer.NumChannels;

            Rect rect = m_FullRect;
            rect.x = 2;
            rect.y += 2;
            rect.height = 16;

            if (m_DrawChannel == null || m_DrawChannel.Length != n) {
                m_DrawChannel = new bool[n];
                for(int i=0; i < n; i++) {
                    m_DrawChannel[i] = true;
                }
            }

            for(int i=0; i < n; i++) {
                bool draw = m_DrawChannel[i];
                string rgb = ColorUtility.ToHtmlStringRGB(
                    draw ? channelData.GetColor(i) : Color.grey);
                m_DrawChannel[i] = EditorGUI.ToggleLeft(rect,
                    $"<color=#{rgb}>{channelData.GetChannelName(i)}</color>",
                    draw, s_Style);
                rect.y += 16;
            }
        }
    }
}
#endif