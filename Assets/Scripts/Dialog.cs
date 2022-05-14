using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Dialog", menuName = "Scriptable Object/Dialog", order = int.MaxValue)]
public class Dialog : ScriptableObject
{
    [SerializeField]
    private DialogPanel dialogPanel;

    [SerializeField]
    private string DialogTitle;

    [System.Serializable]
    public class DialogPanel
    {
        public GameObject PanelObject;
        public GameObject TitleObject;
        public GameObject MessageObject;
    }

    [System.Serializable]
    public class DialogContent
    {
        public string title;
        public string message;
        public Vector2 position;
    }

    [SerializeField]
    public List<DialogContent> DialogContentList = new List<DialogContent>();

    public static void ShowDialog(List<int> showRange)
    {
        if (showRange.Count <= 0) return;

        if (showRange.Count == 1)
        {
            if (showRange[0] == -1)
            {

            }
        }
        else
        {

        }
    }

    private void ShowDialogUI(DialogContent dialog)
    {
        string title = dialog.title;
        string message = dialog.message;
        Vector2 position = dialog.position;

        dialogPanel.TitleObject.GetComponent<Text>().text = title;
        dialogPanel.MessageObject.GetComponent<Text>().text = message;
    }
}
