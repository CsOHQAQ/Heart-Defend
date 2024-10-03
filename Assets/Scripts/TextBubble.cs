using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBubble : MonoBehaviour
{
    public TextMeshProUGUI Tmp;
    public List<TextInfo> texts;
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        Tmp = GetComponent<TextMeshProUGUI>();
        Tmp.text = "";
        texts = new List<TextInfo>();
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Tmp.text == "")
        {
            if (texts.Count > 0)
            {
                Tmp.text=texts[0].text;
                timer = 0;
                Tmp.color = new Color(1, 1, 1, 0);
            }
        }
        else
        {
            timer += Time.deltaTime;
            if (timer >= texts[0].showTime)
            {                
                texts.RemoveAt(0);
                Tmp.text = "";
                return;
            }

            if (timer >= texts[0].showTime - 1f)
            {
                Tmp.color=new Color(1, 1,1, texts[0].showTime-timer);
            }
            else
            {
                Tmp.color = new Color(1, 1, 1, Mathf.Clamp01(timer));
            }

        }

    }

    public void AddText(string text,float showTime)
    {
        TextInfo info = new TextInfo();
        info.text = text;
        info.showTime = showTime;
        texts.Add(info);
    }

    public struct TextInfo
    {
        public string text;
        public float showTime;
    }
}
