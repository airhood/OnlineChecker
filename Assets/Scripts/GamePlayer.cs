using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayer : MonoBehaviour
{
    [Header("Player Info")]
    public Sprite ProfileImage;
    public string Nickname;
    public int Level;

    [Space(10)]
    [Header("UI Objects")]
    [SerializeField] Image ProfileImageComponent;
    [SerializeField] Text NicknameComponent;
    [SerializeField] Text LevelComponent;

    public void Set(GamePlayer gamePlayer, bool updateUI)
    {
        bool isUpdated = false;

        if (gamePlayer.ProfileImage != null)
        {
            ProfileImage = gamePlayer.ProfileImage;
            isUpdated = true;
        }

        if (gamePlayer.Nickname != null)
        {
            Nickname = gamePlayer.Nickname;
            isUpdated = true;
        }

        if (gamePlayer.Level != -1)
        {
            Level = gamePlayer.Level;
            isUpdated = true;
        }

        if (updateUI && isUpdated)
        {
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        ProfileImageComponent.sprite = ProfileImage;
        NicknameComponent.text = Nickname;
        LevelComponent.text = Level.ToString();
    }

    public void SetInformation(Sprite ProfileImage, string name, int level)
    {
        ProfileImageComponent.sprite = ProfileImage;
        this.Nickname = name;
        this.Level = level;
    }

    public void SetName(string name)
    {
        this.Nickname = name;
    }

    public void SetLevel(int level)
    {
        this.Level = level;
    }

    public void AddLevel(int level)
    {
        this.Level += level;
    }
}
