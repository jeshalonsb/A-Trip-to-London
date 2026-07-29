using UnityEngine;

public class FallingCoffeeItem : MonoBehaviour
{
    public enum ItemType
    {
        CoffeeBean,
        Milk,
        Sugar,
        Trash,
        Salt
    }

    [Header("Item")]
    [SerializeField] private ItemType itemType;

    [Header("Movement")]
    [SerializeField] private float fallSpeed = 220f;

    private RectTransform itemRect;
    private CoffeeCatchMinigame minigame;

    private bool finished;

    private void Awake()
    {
        itemRect = GetComponent<RectTransform>();
    }

    public void Initialize(
        CoffeeCatchMinigame game)
    {
        minigame = game;
        finished = false;
    }

    private void Update()
    {
        if (finished ||
            minigame == null ||
            !minigame.GameActive)
        {
            return;
        }

        itemRect.anchoredPosition +=
            Vector2.down *
            fallSpeed *
            Time.deltaTime;

        CheckCupCollision();
        CheckMissed();
    }

    private void CheckCupCollision()
    {
        if (minigame.Cup == null)
            return;

        if (!RectsOverlap(
            itemRect,
            minigame.Cup))
        {
            return;
        }

        finished = true;

        minigame.CatchItem(itemType);

        Destroy(gameObject);
    }

    private void CheckMissed()
    {
        if (minigame == null ||
            minigame.GameArea == null)
        {
            return;
        }

        Vector3[] itemCorners = new Vector3[4];
        Vector3[] areaCorners = new Vector3[4];

        itemRect.GetWorldCorners(itemCorners);
        minigame.GameArea.GetWorldCorners(areaCorners);

        // Corner 2 is the item's top-right.
        float itemTop = itemCorners[2].y;

        // Corner 0 is the game area's bottom-left.
        float gameAreaBottom = areaCorners[0].y;

        // Only despawn once the whole item passes below the area.
        if (itemTop >= gameAreaBottom)
            return;

        finished = true;

        minigame.MissItem(itemType);

        Destroy(gameObject);
    }

    private bool RectsOverlap(
        RectTransform first,
        RectTransform second)
    {
        Vector3[] firstCorners =
            new Vector3[4];

        Vector3[] secondCorners =
            new Vector3[4];

        first.GetWorldCorners(firstCorners);
        second.GetWorldCorners(secondCorners);

        Rect firstRect = new Rect(
            firstCorners[0].x,
            firstCorners[0].y,
            firstCorners[2].x -
            firstCorners[0].x,
            firstCorners[2].y -
            firstCorners[0].y
        );

        Rect secondRect = new Rect(
            secondCorners[0].x,
            secondCorners[0].y,
            secondCorners[2].x -
            secondCorners[0].x,
            secondCorners[2].y -
            secondCorners[0].y
        );

        return firstRect.Overlaps(secondRect);
    }
}