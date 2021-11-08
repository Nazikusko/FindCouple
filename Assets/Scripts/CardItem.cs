using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CardData", menuName = "Card Data", order = 51)]
public class CardItem : ScriptableObject
{
    public string IconName
    {
        get
        {
            return iconName;
        }
    }
    public Sprite Icon
    {
        get
        {
            return icon;
        }
    }
    public int ScorePoints
    {
        get
        {
            return scorePoints;
        }
    }

    [SerializeField]
    private string iconName;

    [SerializeField]
    private Sprite icon;

    [SerializeField]
    private int scorePoints;
}
