using UnityEngine;

[CreateAssetMenu(fileName = "NewDeveloperProfile", menuName = "Developer Profile/New Profile")]
public class DeveloperProfile : ScriptableObject
{
    public Sprite profilePic;
    public string developerName;
    public string regNumber;
    public string email;
    public string major;
    [TextArea(3, 6)]
    public string description;
}
