/*using System.Text;
using TMPro;
using UnityEngine;

public class ResultPanelUI : MonoBehaviour
{
    public GameObject resultPanel;
    [Header("UI 텍스트 참조")]
    public TMP_Text tagCountText;
    public TMP_Text symbolLogText;
    public TMP_Text finalStatText;

    public void ShowResult(TurnContext context)
    {
        tagCountText.text = BuildTagCountText(context);
        symbolLogText.text = BuildSymbolLogText(context);
        finalStatText.text = BuildFinalStatText(context);
    }

    private string BuildTagCountText(TurnContext context)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var kv in context.tagCounts)
        {
            sb.AppendLine($"{kv.Key} {kv.Value}");
        }
        return sb.ToString();
    }

    private string BuildSymbolLogText(TurnContext context)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < context.symbolLogs.Count; i++)
        {
            var log = context.symbolLogs[i];
            sb.AppendLine($"문양{i + 1} ({log.symbolName}) 공격 {log.baseAttack}+{log.bonusAttack} 방어 {log.baseDefense}+{log.bonusDefense}");
        }
        return sb.ToString();
    }

    private string BuildFinalStatText(TurnContext context)
    {
        return $"최종 \n 단일 공격 {context.totalDamage} \n 방어 {context.totalDefense}";
    }

    public void ResultPanelToggle()
    {
        resultPanel.SetActive(!resultPanel.activeSelf);
    }
}*/

using System.Text;
using TMPro;
using UnityEngine;

public class ResultPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject resultPanel;

    [Header("UI 텍스트 참조")]
    [SerializeField] private TMP_Text tagCountText;
    [SerializeField] private TMP_Text symbolLogText;
    [SerializeField] private TMP_Text finalStatText;

    public bool IsOpen => resultPanel != null && resultPanel.activeSelf;

    public void Show()
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);
    }

    public void Hide()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    public void Refresh(TurnContext context)
    {
        if (context == null)
            return;

        if (tagCountText != null)
            tagCountText.text = BuildTagCountText(context);

        if (symbolLogText != null)
            symbolLogText.text = BuildSymbolLogText(context);

        if (finalStatText != null)
            finalStatText.text = BuildFinalStatText(context);
    }


    private string BuildTagCountText(TurnContext context)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var pair in context.tagCounts)
            sb.AppendLine($"{pair.Key} {pair.Value}");

        return sb.ToString();
    }

    private string BuildSymbolLogText(TurnContext context)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < context.symbolLogs.Count; i++)
        {
            SymbolResultLog log = context.symbolLogs[i];

            sb.AppendLine(
                $"문양 {i + 1} ({log.symbolName}) " +
                $"공격 {log.baseAttack}+{log.bonusAttack} " +
                $"방어 {log.baseDefense}+{log.bonusDefense}"
            );
        }

        return sb.ToString();
    }

    private string BuildFinalStatText(TurnContext context)
    {
        return $"최종\n단일 공격 {context.totalDamage}\n방어 {context.totalDefense}";
    }
}