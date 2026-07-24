using UnityEngine;
using UnityEngine.UI;

public class TopicCategoryButton : MonoBehaviour 
{
    public TopicCategorySO topicData;      // Tarik asset Topic_KupuKupu ke sini
    public MaterialManager materialManager; // Tarik GameManager ke sini

    private void Start() 
    {
        GetComponent<Button>().onClick.AddListener(OnCategoryClicked);
    }

    private void OnCategoryClicked() 
    {
        materialManager.StartMaterial(topicData);
        // Tambahkan logic pindah panel UI (SetActive) di sini jika perlu
    }
}