public static class BattleSelectionData
{
    public static EnemyDefinitionData SelectedEnemy { get; private set; }

    public static bool HasSelectedEnemy => SelectedEnemy != null;

    public static void SelectEnemy(EnemyDefinitionData enemyDefinition)
    {
        SelectedEnemy = enemyDefinition;
    }

    public static void Clear()
    {
        SelectedEnemy = null;
    }
}