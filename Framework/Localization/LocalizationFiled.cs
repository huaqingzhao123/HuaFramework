using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationFiled : MonoBehaviour
{
    [Header("是否本地化Text")]
    public bool IsText;
    [Header("是否本地化Sprite图片")]
    public bool IsSprite;
    [Header("是否本地化AudioClip")]
    public bool IsAudioClip;
    [Header("新的文本内容")]
    public string NewText;
    [Header("新的Sprite图片")]
    public Sprite NewSprite;
    [Header("新的AudioClip")]
    public AudioClip NewAudio;
    private void Awake()
    {
        StartCoroutine(Localization());
    }

    private IEnumerator Localization()
    {
        yield return new WaitForEndOfFrame();
        LocalizationLogic();
    }
    public void LocalizationLogic()
    {
        if (IsText)
        {
            transform.GetComponent<Text>().text = NewText;
        }
        if (IsSprite)
            transform.GetComponent<Image>().sprite = NewSprite;
        if (IsAudioClip)
            transform.GetComponent<AudioSource>().clip = NewAudio;
    }
}
