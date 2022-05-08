using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Threading;
using UnityEngine.UI;


public class GoogleNormalData
{
    public string order, result, msg;
}

public class GoogleAccountData
{
    public string order, result, msg;
    public string id, nickname;
}

public class GoogleNicknameData
{
    public string order, result, msg;
    public string nickname;
}

public class GoogleLevelData
{
    public string order, result, msg;
    public int level;
}

public enum RequestType
{
    Register, Login, Logout, ChangeNickname, GetNickname, AddLevel, GetLevel
}

public class AccountManager : MonoBehaviour
{
    public static bool isLogined = false;

    const string URL = "";

    GoogleAccountData GAD;
    GoogleNicknameData GNND;
    GoogleLevelData GLD;

    public InputField SignUpIDInput;
    public InputField SignUpPWInput;
    public InputField SignUpPWConfirmInput;
    public InputField SignUpNicknameInput;

    public InputField LoginIDInput;
    public InputField LoginPWInput;

    public NoticeUI notice;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RequestRegisterToServer(string id, string pw, string pwConfirm, string nickname)
    {
        if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(pw) && !string.IsNullOrEmpty(pwConfirm) && !string.IsNullOrEmpty(nickname))
        {
            notice.SUB("입력란이 비어있습니다");
            return;
        }

        WWWForm form = new WWWForm();
        form.AddField("order", "register");
        form.AddField("id", id);
        form.AddField("pw", pw);

        print("Register called");
        StartCoroutine(Post(form, RequestType.Register));
    }

    public void RequestLoginToServer(string id, string pw)
    {
        if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(pw))
        {
            notice.SUB("입력란이 비어있습니다");
            return;
        }

        WWWForm form = new WWWForm();
        form.AddField("order", "login");
        form.AddField("id", id);
        form.AddField("pw", pw);

        print("Login called");
        StartCoroutine(Post(form, RequestType.Login));
    }

    public void RequestLogoutToServer()
    {
        WWWForm form = new WWWForm();
        form.AddField("order", "logout");

        print("Logout called");
        StartCoroutine(Post(form, RequestType.Logout));
    }

    public void RequestChangeNicknameToServer(string newNickname)
    {
        WWWForm form = new WWWForm();
        form.AddField("order", "changeNickname");
        form.AddField("nickname", newNickname);

        print("ChangeNickname called");
        StartCoroutine(Post(form, RequestType.ChangeNickname));
    }

    public void RequestGetNicknameToServer()
    {
        WWWForm form = new WWWForm();
        form.AddField("order", "getNickname");

        print("GetNickname called");
        StartCoroutine(Post(form, RequestType.GetNickname));
    }

    public void RequestAddLevelToServer(int amount)
    {
        WWWForm form = new WWWForm();
        form.AddField("order", "addLevel");
        form.AddField("amount", amount);

        print("AddLevel called");
        StartCoroutine(Post(form, RequestType.AddLevel));
    }

    public void RequestGetLevelToServer()
    {
        WWWForm form = new WWWForm();
        form.AddField("order", "getLevel");

        print("GetLevel called");
        StartCoroutine(Post(form, RequestType.GetLevel));
    }

    IEnumerator Post(WWWForm form, RequestType requestType)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            yield return www.SendWebRequest();

            if (www.isDone)
            {
                string json = www.downloadHandler.text;
                switch (requestType)
                {
                    case RequestType.Register:
                        AccountResponse(json);
                        break;
                    case RequestType.Login:
                        AccountResponse(json);
                        break;
                    case RequestType.Logout:
                        break;
                    case RequestType.ChangeNickname:
                        ChangeNicknameResponse(json);
                        break;
                    case RequestType.GetNickname:
                        GetNicknameResponse(json);
                        break;
                    case RequestType.AddLevel:
                        AddLevelResponse(json);
                        break;
                    case RequestType.GetLevel:
                        GetLevelResponse(json);
                        break;
                }
            }
        }
    }

    void AccountResponse(string json)
    {
        if (string.IsNullOrEmpty(json)) return;

        GAD = JsonUtility.FromJson<GoogleAccountData>(json);

        if (GAD.result == "ERROR")
        {
            if (GAD.msg == "같은 닉네임인 사용자가 있습니다")
            {
                notice.SUB(GAD.msg);
                return;
            }
            else
            {
                notice.SUB(GAD.order + "을 실행할 수 없습니다. 에러 메세지: " + GAD.msg + " | json: " + json);
                return;
            }
        }
        else if (GAD.result == "OK")
        {
            print(GAD.order + "을 실행했습니다. 메세지: " + GAD.msg + " | json: " + json);

            if (GAD.order == "register")
            {
                
            }
            else if (GAD.order == "login")
            {
                
            }
            else
            {
                notice.SUB("Error: order not found in the order index. Order: " + GAD.order);
            }

        }
        else
        {
            notice.SUB("알 수 없는 에러로 인해 " + GAD.order + "을 실행할 수 없습니다. 에러 메세지: " + GAD.msg + " | json: " + json);
        }
    }

    void ChangeNicknameResponse(string json)
    {
        if (string.IsNullOrEmpty(json)) return;
    }

    void GetNicknameResponse(string json)
    {
        
    }

    void AddLevelResponse(string json)
    {

    }

    void GetLevelResponse(string json)
    {
        
    }

    void OnLogin(string id)
    {
        print("Login success. id: " + id);
        SaveLastLoginID(id);
    }

    void OnLoginFailed(string errorMessage)
    {
        print("Login failed. Error message: " + errorMessage);
    }

    public void SaveLastLoginID(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        PlayerPrefs.SetString("LastLoginID", id);
        PlayerPrefs.Save();
    }

    public string LoadLastLoginID()
    {
        if (!PlayerPrefs.HasKey("LastLoginID"))
        {
            return null;
        }

        return PlayerPrefs.GetString("LastLoginID");
    }
}
