using UnityEngine;

public class CombatValuePopupSpawner : MonoBehaviour
{
    [Header("전투 유닛")]
    [SerializeField] private NewPlayer player;
    [SerializeField] private NewEnemy enemy;

    [Header("팝업 생성 위치")]
    [SerializeField] private RectTransform playerPopupAnchor;
    [SerializeField] private RectTransform enemyPopupAnchor;

    [Header("팝업 프리팹")]
    [SerializeField] private CombatValuePopup popupPrefab;

    [Header("팝업 색상")]
    [SerializeField]
    private Color damageColor =
        new Color(1f, 0.2f, 0.2f, 1f);

    [SerializeField]
    private Color shieldColor =
        new Color(0.2f, 0.55f, 1f, 1f);

    private bool isSubscribed;

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (isSubscribed)
            return;

        if (player != null)
        {
            // OnDamageTaken이 아니라 OnDamageReceived를 구독한다.
            // 따라서 실드가 전부 막아도 숫자가 표시된다.
            player.OnDamageReceived +=
                HandlePlayerDamageReceived;

            player.OnShieldGained +=
                HandlePlayerShieldGained;
        }

        if (enemy != null)
        {
            enemy.OnDamageReceived +=
                HandleEnemyDamageReceived;

            enemy.OnShieldGained +=
                HandleEnemyShieldGained;
        }

        isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!isSubscribed)
            return;

        if (player != null)
        {
            player.OnDamageReceived -=
                HandlePlayerDamageReceived;

            player.OnShieldGained -=
                HandlePlayerShieldGained;
        }

        if (enemy != null)
        {
            enemy.OnDamageReceived -=
                HandleEnemyDamageReceived;

            enemy.OnShieldGained -=
                HandleEnemyShieldGained;
        }

        isSubscribed = false;
    }

    private void HandlePlayerDamageReceived(
        int damage
    )
    {
        SpawnPopup(
            playerPopupAnchor,
            $"-{damage}",
            damageColor
        );
    }

    private void HandleEnemyDamageReceived(
        int damage
    )
    {
        SpawnPopup(
            enemyPopupAnchor,
            $"-{damage}",
            damageColor
        );
    }

    private void HandlePlayerShieldGained(
        int shieldAmount
    )
    {
        SpawnPopup(
            playerPopupAnchor,
            $"+{shieldAmount}",
            shieldColor
        );
    }

    private void HandleEnemyShieldGained(
        int shieldAmount
    )
    {
        SpawnPopup(
            enemyPopupAnchor,
            $"+{shieldAmount}",
            shieldColor
        );
    }

    private void SpawnPopup(
        RectTransform targetAnchor,
        string value,
        Color color
    )
    {
        if (popupPrefab == null)
        {
            Debug.LogError(
                "CombatValuePopupSpawner에 Popup Prefab이 연결되지 않았습니다.",
                this
            );

            return;
        }

        if (targetAnchor == null)
        {
            Debug.LogError(
                "CombatValuePopupSpawner에 Popup Anchor가 연결되지 않았습니다.",
                this
            );

            return;
        }

        CombatValuePopup popup = Instantiate(
            popupPrefab,
            targetAnchor,
            false
        );

        RectTransform popupRect =
            popup.GetComponent<RectTransform>();

        if (popupRect != null)
        {
            popupRect.anchorMin =
                new Vector2(0.5f, 0.5f);

            popupRect.anchorMax =
                new Vector2(0.5f, 0.5f);

            popupRect.pivot =
                new Vector2(0.5f, 0.5f);

            popupRect.anchoredPosition =
                Vector2.zero;

            popupRect.localRotation =
                Quaternion.identity;

            popupRect.localScale =
                Vector3.one;
        }

        popup.transform.SetAsLastSibling();

        popup.Show(
            value,
            color
        );
    }
}