using System.Collections.Generic;
using UnityEngine;

public class TurnContext
{
    public int totalDamage = 0;
    public int totalDefense = 0;

    public bool actionCancelled = false;

    public List<SymbolResultLog> symbolLogs = new List<SymbolResultLog>();
    public Dictionary<SymbolTag, int> tagCounts = new Dictionary<SymbolTag, int>();

    private readonly Stack<SymbolExecutionContext> symbolExecutionStack = new Stack<SymbolExecutionContext>();

    public SymbolExecutionContext CurrentSymbolExecution => symbolExecutionStack.Count > 0 ? symbolExecutionStack.Peek() : null;
    public int SymbolExecutionDepth => symbolExecutionStack.Count;

    private readonly Dictionary<Symbol, int> symbolReplayRequests = new Dictionary<Symbol, int>();

    public void BeginSymbolExecution(SymbolExecutionContext context)
    {
        if (context == null)
            return;

        symbolExecutionStack.Push(context);
    }

    public void EndSymbolExecution(SymbolExecutionContext context)
    {
        if (context == null || symbolExecutionStack.Count == 0)
            return;

        if (symbolExecutionStack.Peek() != context)
        {
            Debug.LogError("심볼 실행 종료 순서가 올바르지 않습니다.");
            return;
        }

        symbolExecutionStack.Pop();
    }

    public void AddTagCount(SymbolTag tag)
    {
        if (tagCounts.ContainsKey(tag))
            tagCounts[tag]++;
        else
            tagCounts[tag] = 1;
    }

    public void RequestSymbolReplay(Symbol symbol, int count = 1)
    {
        if (symbol == null || count <= 0)
            return;

        if (symbolReplayRequests.TryGetValue(symbol, out int currentCount))
            symbolReplayRequests[symbol] = currentCount + count;
        else
            symbolReplayRequests.Add(symbol, count);
    }

    public int ConsumeSymbolReplay(Symbol symbol)
    {
        if (symbol == null)
            return 0;

        if (!symbolReplayRequests.TryGetValue(symbol, out int replayCount))
            return 0;

        symbolReplayRequests.Remove(symbol);
        return replayCount;
    }
}