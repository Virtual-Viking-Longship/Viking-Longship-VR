using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using Vuplex.WebView;

/*
This class handles the display of the information of inspected objects
It interprets a txt file and translates into a vertical layout
This class is expected to attached to a Info panel prefab, which has a vertical layout and templates for the elements that are spawned in it
The info panel this class is attached to should also be a child of the player object
*/
public class FormattedDocumentDisplay : MonoBehaviour
{
    public WebBrowserButtons webBrowserButtons;
    private GameObject imageBlock, textBlock, linkBlock, audioPlayerBlock;
    private Transform verticalLayout;
    void Start()
    {
        imageBlock = transform.GetChild(0).gameObject;
        textBlock = transform.GetChild(1).gameObject;
        linkBlock = transform.GetChild(2).gameObject;
        audioPlayerBlock = transform.GetChild(3).gameObject;
        verticalLayout = transform.GetChild(4);
    }

    // This function is called by the ObjectInspector class
    public void DisplayDocument(TextAsset document)
    {
        foreach(Transform child in verticalLayout) Destroy(child.gameObject);

        List<String> lines = new List<String>(document.text.Split('\n'));
        for(int i = 0; i < lines.Count; i++)
        {
            if(lines[i][0] == '$')
            {
                switch(lines[i][1])
                {
                    case 'T':
                    i = DisplayText(lines, i);
                    break;

                    case 'I': DisplayImage(lines[i]);
                    break;
                    
                    case 'L': DisplayLink(lines[i]);
                    break;

                    case 'A': DisplayAudioPlayer(lines[i]);
                    break;
                }
            }
        }
    }

    private int DisplayText(List<String> lines, int currentLine)
    {
        String[] param = lines[currentLine].Split('(', ')')[1].Split(',');

        TextMeshProUGUI block = Instantiate(textBlock, verticalLayout).GetComponent<TextMeshProUGUI>();
        
        for(int i = 0; i < param.Length; i++) param[i] = param[i].Trim();

        block.gameObject.SetActive(true);

        SetTMPParameters(block, int.Parse(param[0]), param[1], param[2]);

        int line, lastLine = currentLine;
        for(line = currentLine + 1; line < lines.Count && lines[line][0] != '$'; line++)
        {
            if(lines[line].Length > 1) lastLine = line;
        }

        block.text = "";
        for(line = currentLine + 1; line <= lastLine; line++)
        {
            string textLine = lines[line].Trim();
            if(textLine.Length == 0) block.text += "\n\n";
            else block.text += textLine + " ";
        } 
        return line - 1;
    }

    private void SetTMPParameters(TextMeshProUGUI tmp, int fSize, String fStyle, String pos)
    {
        tmp.fontSize = fSize;

        switch(fStyle)
        {
            case "b": tmp.fontStyle = FontStyles.Bold;
            break;

            case "i": tmp.fontStyle = FontStyles.Italic;
            break;

            case "u": tmp.fontStyle = FontStyles.Underline;
            break;

            default: tmp.fontStyle = FontStyles.Normal;
            break;
        }

        switch(pos)
        {
            case "l": tmp.alignment = TextAlignmentOptions.Left;
            break;

            case "c": tmp.alignment = TextAlignmentOptions.Center;
            break;

            case "r": tmp.alignment = TextAlignmentOptions.Right;
            break;

            case "j": tmp.alignment = TextAlignmentOptions.Justified;
            break;
        }
    }

    private void DisplayImage(String param)
    {
        String imgPath = param.Split('(', ')')[1].Trim();
        GameObject block = Instantiate(imageBlock, verticalLayout);
        block.SetActive(true);
        Sprite image = Resources.Load<Sprite>(imgPath);
        StartCoroutine(PlaceImageBlock(block, image));
    }

    // In order to get the width of the image inside the vertical layout, we need to wait one frame for it to update
    private IEnumerator PlaceImageBlock(GameObject block, Sprite image)
    {
        yield return null;
        block.GetComponent<Image>().sprite = image;
        block.GetComponent<LayoutElement>().preferredHeight = (image.texture.height / (float) image.texture.width) * block.GetComponent<RectTransform>().rect.width;
    }

    private void DisplayAudioPlayer(String param)
    {
        GameObject block = Instantiate(audioPlayerBlock, verticalLayout);
        block.gameObject.SetActive(true);
        string[] parameters = param.Split('(', ')')[1].Split(',');
        for(int i = 0; i < parameters.Length; i++) parameters[i] = parameters[i].Trim();

        TextMeshProUGUI tmp = block.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        tmp.text = parameters[0];
        SetTMPParameters(tmp, int.Parse(parameters[1]), parameters[2], parameters[3]);
        GetComponent<AudioSource>().clip = Resources.Load<AudioClip>(parameters[4]);
    }

    private void DisplayLink(String param)
    {
        GameObject block = Instantiate(linkBlock, verticalLayout);
        block.gameObject.SetActive(true);
        string[] parameters = param.Split('(', ')')[1].Split(',');
        for(int i = 0; i < parameters.Length; i++) parameters[i] = parameters[i].Trim();

        block.GetComponent<TextMeshProUGUI>().text = parameters[0];
        SetTMPParameters(block.GetComponent<TextMeshProUGUI>(), int.Parse(parameters[1]), "u", parameters[2]);
        block.GetComponent<Button>().onClick.AddListener(() => {webBrowserButtons.LoadURL(parameters[3]);});
    }

    public void DisplayFormattedDocument(FormattedDocument document)
    {
        foreach(Transform child in verticalLayout) Destroy(child);

        foreach(FormattedDocument.ContentBlock contentBlock in document.contentBlocks)
        {
            if(contentBlock.image != null)
            {
                GameObject block = Instantiate(imageBlock, verticalLayout);
                block.SetActive(true);
                block.GetComponent<Image>().sprite = contentBlock.image;
            }

            if(contentBlock.text.Length > 0)
            {
                TextMeshProUGUI block = Instantiate(textBlock, verticalLayout).GetComponent<TextMeshProUGUI>();
                block.gameObject.SetActive(true);
                block.text = contentBlock.text;
                block.fontSize = contentBlock.fontSize;
                block.fontStyle = contentBlock.fontStyle;
                block.alignment = contentBlock.alignment;
            }
        }
    }
}