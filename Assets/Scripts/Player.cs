using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Player Info")]
    public Sprite ProfileImage;
    public string name;
    public int level;

    [Space(10)]
    [Header("UI Objects")]
    [SerializeField] Image ProfileImageComponent;
    [SerializeField] Text NameComponent;
    [SerializeField] Text LevelComponent;

    public void SetInformation(Sprite ProfileImage, string name, int level)
    {
        ProfileImageComponent.sprite = ProfileImage;
        this.name = name;
        this.level = level;
    }

    public void SetName(string name)
    {
        this.name = name;
    }

    public void SetLevel(int level)
    {
        this.level = level;
    }

    public void AddLevel(int level)
    {
        this.level += level;
    }
}
