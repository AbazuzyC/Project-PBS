using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DeveloperProfileUI : MonoBehaviour
{
    [Header("Data")]
    public DeveloperProfile[] developerList;
    private int _indexNow = 0;

    [Header("UI Refrence")]
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _txtName;
    [SerializeField] private TextMeshProUGUI _txtRegNumber;
    [SerializeField] private TextMeshProUGUI _txtEmail;
    [SerializeField] private TextMeshProUGUI _txtMajor;
    [SerializeField] private TextMeshProUGUI _txtDescription;

    [Header("Panel Switch")]
    public GameObject aboutPanel;
    public GameObject profilePanel;
    public GameObject nextButton;

    void OnEnable()
    {
        _indexNow = 0;
        ShowProfile();
    }
    void OnDisable() 
    {
        if(profilePanel.activeSelf)
            profilePanel.SetActive(false);    
    }
    void ShowProfile()
    {
        DeveloperProfile data = developerList[_indexNow];
        _image.sprite = data.profilePic;
        _txtName.text = "Nama: " + data.developerName;
        _txtRegNumber.text = "NIP: " + data.regNumber;
        _txtEmail.text = "Email: " + data.email;
        _txtMajor.text = data.major;
        _txtDescription.text = data.description;

        if(nextButton != null)
            nextButton.SetActive(_indexNow < developerList.Length - 1);
    }
    void SwitchPanel()
    {
        aboutPanel.SetActive(true);
        profilePanel.SetActive(false);
        _indexNow = 0;
    }
    public void Next()
    {
        if(_indexNow < developerList.Length - 1)
        {
            _indexNow++;
            ShowProfile();
        }
    }
    public void Prev()
    {
        if(_indexNow == 0)
        {
            SwitchPanel();
            return;
        }

        _indexNow--;
        ShowProfile();
    }
}