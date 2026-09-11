using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    private IEnumerator VerifyCharacterCGRuntime(string path)
    {
        yield return new WaitForSecondsRealtime(.3f);
        ShowMainMenu();ShowCharacterSelection();yield return null;
        var selector=characterSelection.GetComponent<KaitCharacterSelection>();
        selector.Fit();Canvas.ForceUpdateCanvases();
        CaptureCanvasToPng(path+".selection.png");
        foreach(var t in characterSelection.GetComponentsInChildren<Text>())
            if(t.preferredHeight>t.rectTransform.rect.height+2||t.preferredWidth>t.rectTransform.rect.width+2)Debug.LogError("CHARACTER_CG_QA: text overflow "+t.text);
        selector.Back();yield return null;
        if(characterSelection.activeSelf||!mainMenu.gameObject.activeSelf)Debug.LogError("CHARACTER_CG_QA: return failed");
        for(int i=0;i<2;i++)
        {
            ShowCharacterSelection();selector=characterSelection.GetComponent<KaitCharacterSelection>();
            selector.StartButtons[i].onClick.Invoke();yield return new WaitForSecondsRealtime(.25f);
            if(run.IsYummn!=(i==1)||characterSelection.activeSelf||!gameplayRoot.activeSelf)Debug.LogError("CHARACTER_CG_QA: wrong start character "+i);
            CaptureCanvasToPng(path+".start-"+i+".png");
            SaveCharacterRun();string key=i==0?"Kait.Run.Kait":"Kait.Run.Yummn.0.8.2";
            ShowMainMenu();ShowCharacterSelection();selector=characterSelection.GetComponent<KaitCharacterSelection>();
            selector.ContinueCharacter(key);yield return new WaitForSecondsRealtime(.2f);
            if(run.IsYummn!=(i==1)||characterSelection.activeSelf||!gameplayRoot.activeSelf)Debug.LogError("CHARACTER_CG_QA: wrong continuation "+i);
        }
        ShowMainMenu();ShowCharacterSelection();yield return null;
        CaptureCanvasToPng(path+".with-saves.png");
        Debug.Log("CHARACTER_CG_QA_COMPLETE");Application.Quit();
    }
}
