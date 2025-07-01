using UnityEngine;

[CreateAssetMenu(fileName = "New ItemData", menuName = "Naninovel/Item Data")]
public class ItemData : ScriptableObject
{
    [Tooltip("아이템의 고유 ID (나니노벨 변수명과 일치 권장)")]
    public string itemId;

    [Tooltip("아이템 이름 (UI 표시용)")]
    public string itemName;

    [Tooltip("아이템 설명 (툴팁 등에서 사용 가능)")]
    [TextArea]
    public string description;

    [Tooltip("UI에 표시될 아이콘 스프라이트")]
    public Sprite iconSprite;

    // 필요에 따라 추가 속성 정의 가능
    // 예: public AudioClip useSound;
    // 예: public GameObject vfxOnUse;
    // 예: public int sellPrice;
}