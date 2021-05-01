using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FileManager_C : MonoBehaviour
{
    public static string fname = "default";
    public CellManager_C cellManager;
    public GameObject playManager;

    public GameObject saveButton;
    public GameObject playButton;

    public GameObject dialog;
    public InputField inputField;
    public GameObject cancelButton;
    public GameObject submitButton;

    private Button _save;
    private Button _play;
    private Button _cancel;
    private Button _submit;
    private void Start()
    {
        _save = saveButton.GetComponent<Button>();
        _save.onClick.AddListener(delegate { DisplayDialog(); });

        _play = playButton.GetComponent<Button>();
        _play.onClick.AddListener(delegate { StartPlaying(); });

        _cancel = cancelButton.GetComponent<Button>();
        _cancel.onClick.AddListener(delegate { Cancel(); });

        _submit = submitButton.GetComponent<Button>();
        _submit.onClick.AddListener(delegate { Submit(); });
    }
    public void StartSaving(string name = "default")
    {

        int[,] cache; // ������ �迭 ����

        cache = (int[,])cellManager.GetSudokuValues().Clone();

        string arr2str = ""; // ���ڿ� ����

        for (int y = 0; y < 9; y++) // �迭�� ','�� �����ư��� tempStr�� ����
        {
            for (int x = 0; x < 9; x++)
            {
                arr2str += cache[y, x].ToString();
            }
        }

        PlayerPrefs.SetString(name, arr2str); // PlyerPrefs�� ���ڿ� ���·� ����
        //print(arr2str);
        print("Saved");
    }
    public void StartPlaying()
    {
        StartSaving();
        SceneManager.LoadScene("PlayScene");
    }

    private void DisplayDialog()
    {
        GameObject.Find("PlayManager").SetActive(false);
        dialog.SetActive(true);
    }


    private void Cancel()
    {
        dialog.SetActive(false);
        playManager.SetActive(true);
    }

    private void Submit()
    {
        Text _fname = inputField.transform.Find("Text").GetComponent<Text>();
        fname = _fname.text;

        StartSaving(fname);

        dialog.SetActive(false);
        playManager.SetActive(true);
    }
}