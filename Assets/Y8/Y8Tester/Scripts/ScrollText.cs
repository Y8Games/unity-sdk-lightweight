using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;



public class ScrollText : MonoBehaviour
{
    private Text textObject;


    void Start()
    {
        textObject = gameObject.GetComponent<Text>();
    }


    void Update()
    {
        string s = textObject.text;
        string[] lines = s.Split('\n');
        lines = lines.Skip(lines.Length - 16).ToArray();
        textObject.text = string.Join("\n", lines);
    }
}
