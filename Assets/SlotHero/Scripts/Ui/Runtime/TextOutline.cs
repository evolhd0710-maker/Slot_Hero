using TMPro;
using UnityEngine;

namespace SlotHero.Ui
{
    /// <summary>
    /// 글자 칸(TextMeshPro)에 테두리를 두르거나 걷는다.
    ///
    /// 글꼴 재질이 모바일 거리장 셰이더(`TextMeshPro/Mobile/Distance Field`)라
    /// 테두리 두께만 넣으면 그려지지 않는다. 셰이더의 테두리 스위치(`OUTLINE_ON`)를 함께 켜야 한다.
    /// 인스펙터에서 고치면 TMP 편집기가 스위치를 켜 주지만 코드로 넣을 때는 직접 켠다.
    /// 2026년 10월 25일 태그 글자에 흰 테두리를 두르다가 알았다.
    /// 상단 표시줄 플레이 시간의 테두리도 같은 까닭으로 그려지지 않고 있었다.
    ///
    /// 두께를 넣는 순간 TMP 가 그 글자 칸만의 재질 사본을 만들므로 스위치를 켜도 다른 글자 칸은 바뀌지 않는다.
    ///
    /// TMP 테두리는 글자 가장자리를 가운데 두고 안팎으로 반씩 두른다. 그대로 두면 안쪽 글자 면이 가늘어져
    /// 30픽셀 태그 이름이 제 색을 잃고 흰 글자처럼 보였다. 그래서 글자 면을 테두리 두께만큼 부풀려(`_FaceDilate`)
    /// 글자 면은 본래 굵기를 지키고 테두리는 그 바깥에 붙게 한다.
    /// </summary>
    public static class TextOutline
    {
        private const string Keyword = "OUTLINE_ON";
        private const string FaceDilate = "_FaceDilate";

        /// <summary>테두리를 두른다. 두께가 0 이면 걷는다.</summary>
        public static void Apply(TMP_Text label, Color color, float width)
        {
            if (label == null)
            {
                return;
            }

            label.outlineColor = color;
            label.outlineWidth = width;

            Material material = label.fontMaterial;
            if (material == null)
            {
                return;
            }

            // 사본은 씬에 함께 저장되어 만든 컴퓨터의 아틀라스를 가리킨 채 남는다.
            // 글꼴 에셋을 컴퓨터마다 만들어 그 아틀라스가 없으면 첫 아틀라스의 글자가 흰 네모로 나왔다(2026년 10월 27일).
            // 지금 글꼴의 첫 아틀라스로 다시 잇는다.
            if (label.font != null && label.font.atlasTexture != null)
            {
                material.SetTexture(ShaderUtilities.ID_MainTex, label.font.atlasTexture);
            }

            if (width > 0f)
            {
                material.EnableKeyword(Keyword);
            }
            else
            {
                material.DisableKeyword(Keyword);
            }

            material.SetFloat(FaceDilate, width);
        }
    }
}
