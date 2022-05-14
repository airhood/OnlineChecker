using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoticeUI : MonoBehaviour
{
    public static bool isShowing = false;

    [Header("SubNotice")]
    public GameObject subbox;
    public Text subintext;
    public Animator subani;

    [Header("Speed")]
    public float speed1 = 2.0f;
    public float speed2 = 0.3f;

    private WaitForSeconds _UIDelay1;
    private WaitForSeconds _UIDelay2;

    void Start()
    {
        subbox.SetActive(false);

        _UIDelay1 = new WaitForSeconds(speed1);
        _UIDelay2 = new WaitForSeconds(speed2);
    }

    public void SUB(string message)
    {
        isShowing = true;
        subintext.text = message;
        subbox.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(SUBDelay());
    }

    IEnumerator SUBDelay()
    {
        subbox.SetActive(true);
        subani.SetBool("isOn", true);
        yield return _UIDelay1;
        subani.SetBool("isOn", false);
        yield return _UIDelay2;
        subbox.SetActive(false);
        isShowing = false;
    }
}
