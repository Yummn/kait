using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    const string HoldInputPreference="Kait.Input.HoldRepeat";
    readonly KaitHoldInput heldInput=new KaitHoldInput();
    readonly KaitStationaryHold stationaryHold=new KaitStationaryHold();
    bool holdWaitTriggered;
    bool HoldRepeatEnabled=>PlayerPrefs.GetInt(HoldInputPreference,0)==1;
    bool AutoInputReady
    {
        get {var a=kaitSpine?.CurrentAnimation;return !(run.IsYummn&&targetingSkill!=KaitSkill.None)&&!busy&&(a==null||a.Loop||a.IsComplete);}
    }
    void ClearHeldInput(){heldInput.Clear();stationaryHold.Cancel();}
    void BeginHeldDirection(KaitDirection direction,int source)
    {
        if(TutorialBlocksInput()||run.ended)return;
        stationaryHold.Cancel();heldInput.Begin(direction,source,Time.unscaledTime);HandleDirection(direction);
    }
    void BindHeldButton(Button button,KaitDirection direction)
    {
        int source=10+(int)direction;
        var h=button.gameObject.AddComponent<KaitHoldDirectionButton>();
        h.Press=()=>BeginHeldDirection(direction,source);h.Release=()=>heldInput.End(source);
    }
    static bool DirectionHeld(KaitDirection d)
    {
        switch(d)
        {
            case KaitDirection.Up:return Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow);
            case KaitDirection.Down:return Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow);
            case KaitDirection.Left:return Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow);
            default:return Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow);
        }
    }
    void PollHeldInput()
    {
        if(heldInput.Source==1&&heldInput.Direction.HasValue&&!DirectionHeld(heldInput.Direction.Value))heldInput.End(1);
        if(heldInput.Poll(Time.unscaledTime,HoldRepeatEnabled,AutoInputReady,out var d))HandleDirection(d);
    }
    void AddHoldInputSetting(Transform parent)
    {
        var toggle=MakeFlatToggle(parent,new Vector2(0,219),new Vector2(580,32),"长按连续输入（键盘 / 方向键 / 滑动保持）");
        toggle.SetIsOnWithoutNotify(HoldRepeatEnabled);
        toggle.onValueChanged.AddListener(value=>{SaveBooleanPreference(HoldInputPreference,value);ClearHeldInput();});
        var label=toggle.GetComponentInChildren<Text>();label.font=threatBoardFont;label.fontSize=18;
    }
    void OnApplicationPause(bool paused){if(paused){ClearHeldInput();ResetSwipeTracking();}}
}
