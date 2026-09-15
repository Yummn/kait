using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    private IEnumerator VerifyStorybook0915()
    {
        foreach(var character in new[]{KaitCharacter.Yummn,KaitCharacter.Kait})
        {
            run.SelectCharacter(character,9915);ConfigureCharacterVisuals();run.StateCommitted=null;EnsureKaitSpine();
            run.skills.Clear();run.passives.Clear();run.rewardQueue.Clear();
            if(run.IsYummn){run.Yummn.profile.maxKi=7;run.Yummn.ki=5;}
            int[,] numbers={{0,2,0,16,0},{4,8,2,0,0},{0,0,4,0,2},{8,4,0,32,0},{0,0,0,0,0}};
            for(int y=0;y<5;y++)for(int x=0;x<5;x++)run.threat[x,y]=numbers[y,x];
            RefreshAll();
            foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(2400,1080)})
            {
                yield return SetMobileSize0912(size.x,size.y);
                var layout=gameContent.GetComponent<KaitStorybookLayout>();
                if(layout.HudPosition.x!=-497)Debug.LogError("STORYBOOK0915_QA: HUD not moved");
                CheckMobile0912(storybookPortrait.rectTransform,"portrait");
                foreach(var heart in storybookHearts)if(heart.gameObject.activeSelf)AssertInHud0914(heart.rectTransform,layout.Hud);
                if(run.IsYummn)foreach(var pip in actionPips){AssertInHud0914(pip.rectTransform,layout.Hud);if(!pip.preserveAspect)Debug.LogError("STORYBOOK0915_QA: qi aspect");}
                if(storybookGroundEdge.sprite==null||!storybookGroundEdge.gameObject.activeInHierarchy)Debug.LogError("STORYBOOK0915_QA: missing ground edge");
                if(storybookForegroundBough.raycastTarget||storybookForegroundBough.transform.GetSiblingIndex()<2)Debug.LogError("STORYBOOK0915_QA: foreground layer/input");
                if(storybookForestDetail.mainTexture.width<1250)Debug.LogError("STORYBOOK0915_QA: forest detail downscaled");
                foreach(var tile in battleCellTiles)if(tile!=null&&tile.rectTransform.rect.width<119)Debug.LogError("STORYBOOK0915_QA: large tile gap");
                Debug.Log($"STORYBOOK0915 textures {character}: left {storybookForestDetail.mainTexture.width}x{storybookForestDetail.mainTexture.height}, tiles {KaitStorybookArt.Floor(run.IsYummn,1,1).texture.width}; tree anchored {storybookForegroundBough.rectTransform.anchoredPosition}");
                yield return CaptureMobile0912("0915-"+character+"-"+size.x);
            }
        }
        Debug.Log("STORYBOOK0915_QA_COMPLETE");
    }
}
