using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewSlotManager : MonoBehaviour
{
    [Header("덱 데이터")]
    public List<Symbol> masterDeck = new List<Symbol>();
    private List<Symbol> runtimeDeck = new List<Symbol>();
    public List<Symbol> RolledResults { get; private set; } = new List<Symbol>();

    [Header("UI 컴포넌트 배열 (각 5개씩 1:1 매칭)")]
    public RawImage[] blurRawImages; // 무한 UV 스크롤을 회전 연출용 RawImage
    public Image[] reelImages;       // 최종 결과 문양이 떨어질 정적 Image

    [Header("연출 세팅")]
    [SerializeField] private float scrollSpeed = 5f;     // 셔플 이미지 롤링 속도
    [SerializeField] private float dropDuration = 0.2f;   // 최종 문양 착지 시간
    [SerializeField] private float spawnYOffset = 150f;   // 최종 문양 시작 오프셋

    private RectTransform[] reelRects;
    private Vector2[] originPositions;
    private bool[] isReelRolling; // 각 릴이 현재 회전 중인지 체크하는 플래그
    public bool isRollEnd;

    private void Awake()
    {
        isRollEnd = true;
        reelRects = new RectTransform[reelImages.Length];
        originPositions = new Vector2[reelImages.Length];
        isReelRolling = new bool[reelImages.Length];

        for (int i = 0; i < reelImages.Length; i++)
        {
            reelRects[i] = reelImages[i].GetComponent<RectTransform>();
            originPositions[i] = reelRects[i].anchoredPosition;

            // 시작할 때는 최종 문양 이미지를 숨겨둡니다.
            reelImages[i].gameObject.SetActive(false);
            blurRawImages[i].gameObject.SetActive(false);
        }
    }

    public void RollWrapper()
    {
        StopAllCoroutines();
        StartCoroutine(Roll());
    }

    private IEnumerator Roll()
    {
        isRollEnd = false;
        RolledResults.Clear();
        runtimeDeck = new List<Symbol>(masterDeck);
        ShuffleDeck(runtimeDeck);

        // 1. 모든 슬롯의 '무한 셔플 롤링'을 동시에 가동합니다.
        for (int i = 0; i < reelImages.Length; i++)
        {
            reelImages[i].gameObject.SetActive(false);
            blurRawImages[i].gameObject.SetActive(true);
            isReelRolling[i] = true;
        }

        int maxSlots = Mathf.Min(5, reelImages.Length, runtimeDeck.Count);

        // 2. 왼쪽 슬롯부터 0.5초 간격으로 하나씩 롤링을 멈추고 최종 문양을 떨어뜨립니다.
        for (int i = 0; i < maxSlots; i++)
        {
            yield return new WaitForSeconds(0.5f);

            // 해당 칸의 무한 셔플을 정지시키고 화면에서 끕니다.
            isReelRolling[i] = false;
            blurRawImages[i].gameObject.SetActive(false);

            // 최종 문양 데이터를 확정하고 활성화합니다.
            Symbol chosenSymbol = runtimeDeck[i];
            RolledResults.Add(chosenSymbol);

            reelImages[i].sprite = chosenSymbol.symbolSprite;
            reelImages[i].gameObject.SetActive(true);

            // 이전에 만든 '위에서 아래로 착! 떨어지는 연출' 실행
            StartCoroutine(Co_PlayDropAnimation(reelRects[i], originPositions[i]));
        }
        isRollEnd = true;
    }

    /// <summary>
    /// 매 프레임 호출되며 롤링 정지 신호가 오기 전까지 셔플 이미지를 무한 스크롤합니다.
    /// </summary>
    private void Update()
    {
        for (int i = 0; i < blurRawImages.Length; i++)
        {
            // 해당 릴이 회전 중일 때만 UV 좌표를 아래로 굴립니다.
            if (isReelRolling[i])
            {
                Rect currentRect = blurRawImages[i].uvRect;

                // Y축 좌표를 시간에 따라 증가시켜 위에서 아래로 흐르는 연출을 만듭니다.
                currentRect.y += Time.deltaTime * scrollSpeed;

                blurRawImages[i].uvRect = currentRect;
            }
        }
    }

    private IEnumerator Co_PlayDropAnimation(RectTransform targetRect, Vector2 originPos)
    {
        float elapsedTime = 0f;
        Vector2 startPos = originPos + new Vector2(0, spawnYOffset);
        targetRect.anchoredPosition = startPos;

        while (elapsedTime < dropDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dropDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-Out 감속 연산

            targetRect.anchoredPosition = Vector2.Lerp(startPos, originPos, t);
            yield return null;
        }
        targetRect.anchoredPosition = originPos;
    }

    private void ShuffleDeck(List<Symbol> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Symbol temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }
}