using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public sealed partial class KaitGame
{
    private IEnumerator VerifyHomeTriptych(string path)
    {
        foreach(var character in new[]{KaitCharacter.Kait,KaitCharacter.Yummn})
            foreach(string key in KaitCharacterSelection.SaveKeys(character))PlayerPrefs.DeleteKey(key);
        for(int i=0;i<2;i++)
        {
            ShowMainMenu();yield return new WaitForEndOfFrame();
            var hitPoint=mainMenu.Layout.TransformPoint(new Vector3(i==0?-650:650,120,0));
            var pick=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,hitPoint)};
            var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(pick,hits);
            if(hits.Count==0||hits[0].gameObject.GetComponent<Button>()!=mainMenu.CharacterButtons[i])Debug.LogError("HOME_QA: character raycast");
            ExecuteEvents.Execute(mainMenu.CharacterButtons[i].gameObject,pick,ExecuteEvents.pointerClickHandler);
            yield return new WaitForSecondsRealtime(.7f);
            if(!MainMenuVisible||gameplayRoot.activeSelf||mainMenu.Selected!=(KaitCharacter)i)Debug.LogError("HOME_QA: selection started game");
            if(mainMenu.ContinueButton.gameObject.activeSelf)Debug.LogError("HOME_QA: continue without save");
            CaptureCanvasToPng(path+".home-"+i+".png");
            foreach(var label in mainMenu.GetComponentsInChildren<Text>())
                if(label.preferredHeight>label.rectTransform.rect.height+2||label.preferredWidth>label.rectTransform.rect.width+2)Debug.LogError("HOME_QA: overflow "+label.text);
            var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};
            ExecuteEvents.Execute(mainMenu.CharacterButtons[i].gameObject,pointer,ExecuteEvents.pointerEnterHandler);
            yield return new WaitForSecondsRealtime(.45f);
            CaptureCanvasToPng(path+".hover-"+i+".png");
            ExecuteEvents.Execute(mainMenu.CharacterButtons[i].gameObject,pointer,ExecuteEvents.pointerExitHandler);
            MenuQAClick(mainMenu.TutorialButton);yield return null;
            var book=tutorialOverlay.GetComponent<KaitTutorialBook>();
            if(book.YummnMode!=(i==1)||book.PageCount!=3)Debug.LogError("HOME_QA: wrong tutorial");
            CaptureCanvasToPng(path+".tutorial-"+i+".png");
            book.ShowPage(2);book.Next();yield return null;
            if(!MainMenuVisible||gameplayRoot.activeSelf||tutorialOverlay.activeSelf)Debug.LogError("HOME_QA: completion entered game");
            yield return new WaitForEndOfFrame();
            MenuQAClick(mainMenu.SettingsButton);yield return null;
            if(!characterSettingsTitle.text.StartsWith(((KaitCharacter)i).ToString()))Debug.LogError("HOME_QA: wrong settings");
            CaptureCanvasToPng(path+".settings-"+i+".png");settingsOverlay.SetActive(false);
            yield return null;MenuQAClick(mainMenu.StartButton);yield return new WaitForSecondsRealtime(.3f);
            if(MainMenuVisible||!gameplayRoot.activeSelf||run.Character!=(KaitCharacter)i)Debug.LogError("HOME_QA: wrong start");
            SaveCharacterRun();int seedTurn=run.turn;
            ShowMainMenu();yield return new WaitForEndOfFrame();
            if(!mainMenu.ContinueButton.gameObject.activeSelf)Debug.LogError("HOME_QA: missing continue");
            CaptureCanvasToPng(path+".saved-"+i+".png");
            MenuQAClick(mainMenu.ContinueButton);yield return new WaitForSecondsRealtime(.3f);
            if(MainMenuVisible||!gameplayRoot.activeSelf||run.Character!=(KaitCharacter)i||run.turn!=seedTurn)Debug.LogError("HOME_QA: wrong resume");
        }
        Debug.Log("HOME_QA_COMPLETE");Application.Quit();
    }
}
