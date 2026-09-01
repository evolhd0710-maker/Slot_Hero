using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitEffectIconUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text stackText;

    private UnitEffect boundEffect;
    private bool showStackCount;

    public UnitEffect BoundEffect => boundEffect;

    public void Bind(
        UnitEffect effect,
        Sprite icon,
        bool shouldShowStackCount)
    {
        boundEffect = effect;
        showStackCount = shouldShowStackCount;
        print(effect.DisplayName + "아이콘 적용됨");
        iconImage.sprite = icon;
        iconImage.enabled = icon != null;

        Refresh();
        gameObject.SetActive(true);
    }

    public void Refresh()
    {
        if (boundEffect == null)
        {
            stackText.gameObject.SetActive(false);
            return;
        }

        bool showStack =
            showStackCount &&
            boundEffect.Stacks > 0;

        stackText.gameObject.SetActive(showStack);

        if (showStack)
            stackText.text = boundEffect.Stacks.ToString();

        gameObject.name =
            $"EffectIcon_{boundEffect.EffectId}";
    }

    public void Release()
    {
        boundEffect = null;

        iconImage.sprite = null;
        stackText.text = string.Empty;

        gameObject.SetActive(false);
    }
}
