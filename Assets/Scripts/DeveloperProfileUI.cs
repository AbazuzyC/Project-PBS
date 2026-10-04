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
        if (developerList == null || developerList.Length == 0) return;
        _indexNow = Mathf.Clamp(_indexNow, 0, developerList.Length - 1);
        DeveloperProfile data = developerList[_indexNow];
        if (data == null) return;

        if (_image != null)
        {
            _image.sprite = data.profilePic;
            _image.enabled = (data.profilePic != null);
        }

        if (_txtName != null) _txtName.text = "Nama: " + data.developerName;
        if (_txtRegNumber != null)
        {
            string label = (!string.IsNullOrEmpty(data.regNumber) && data.regNumber.Trim().Length > 12) ? "NIP: " : "NIM: ";
            _txtRegNumber.text = label + data.regNumber;
        }
        if (_txtEmail != null) _txtEmail.text = "Email: " + data.email;
        if (_txtMajor != null) _txtMajor.text = data.major;
        if (_txtDescription != null) _txtDescription.text = data.description;

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