#if PLATFORM_ZERO_YARN
using TMPro;
using UnityEngine;

namespace PlatformZero
{
    public sealed class StationSign : MonoBehaviour
    {
        [SerializeField] private string text;
        [SerializeField] private float size = 4;
        [SerializeField] private Color color = Color.white;
        public void Configure(string value,float fontSize,Color tint) { text=value;size=fontSize;color=tint; }
        public void Initialize(TMP_FontAsset font)
        {
            var label=gameObject.AddComponent<TextMeshPro>();
            label.font=font; label.text=text; label.fontSize=size; label.color=color;
            label.alignment=TextAlignmentOptions.Center;
            label.rectTransform.sizeDelta=new Vector2(10,3);
        }
    }
}

#endif
