using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct PageData 
{
    public string pageTitle; // Contoh: "TAHAP 1: TELUR"
    
    [TextArea(5, 15)] // Bikin kolom teks di Inspector lebih besar
    public string contentText; // Isi materi
    
    public Sprite pageImage; // Kosongkan jika materi ini tidak pakai gambar
}

[CreateAssetMenu(fileName = "New Topic Category", menuName = "Material System/Topic Category")]
public class TopicCategorySO : ScriptableObject 
{
    public string topicName; // Contoh: "MATERI KUPU KUPU"
    public List<PageData> pages;
}