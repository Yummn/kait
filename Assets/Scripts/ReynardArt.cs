using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public static class ReynardArt
{
 static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
 static readonly Dictionary<string,Sprite[]> sequences=new Dictionary<string,Sprite[]>();
 static readonly Sprite[] impact=new Sprite[8];
 public static float EnemyVisualScale(KaitEnemyType type)=>type==KaitEnemyType.Swordsman||type==KaitEnemyType.Guard?1.18f:type==KaitEnemyType.Archer?1.16f:1f;
 public static Sprite ImpactFrame(int index){index=Mathf.Clamp(index,0,7);if(impact[index]!=null)return impact[index];var t=Resources.Load<Texture2D>("Reynard/FoxfireImpact");if(t==null)return null;float w=t.width/4f,h=t.height/2f;return impact[index]=Sprite.Create(t,new Rect(index%4*w,t.height-(index/4+1)*h,w,h),Vector2.one*.5f);}
 public static Sprite SequenceFrame(string path,int index){index=Mathf.Clamp(index,0,7);if(!sequences.TryGetValue(path,out var frames)){frames=new Sprite[8];sequences[path]=frames;}if(frames[index]!=null)return frames[index];var t=Resources.Load<Texture2D>("Reynard/"+path);if(t==null)return null;float w=t.width/4f,h=t.height/2f;return frames[index]=Sprite.Create(t,new Rect(index%4*w,t.height-(index/4+1)*h,w,h),Vector2.one*.5f);}
 public static string EnemyId(KaitEnemyType t)=>new[]{"","11300041","11300006","11201007","11202007","11300040","11101005"}[(int)t];
 public static Sprite Load(string path){if(cache.TryGetValue(path,out var s))return s;var t=Resources.Load<Texture2D>("Reynard/"+path);if(t==null)return null;s=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f);cache[path]=s;return s;}
}
public sealed class ReynardMenuHover:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
 public RectTransform Portrait;public int Index;public KaitMainMenu Menu;bool over;
 public void OnPointerEnter(PointerEventData e){over=true;}public void OnPointerExit(PointerEventData e){over=false;}
 void Update(){if(Portrait==null)return;float scale=over||Menu.Selected==(KaitCharacter)Index?1.035f:1f;Portrait.localScale=Vector3.Lerp(Portrait.localScale,Vector3.one*scale,Time.unscaledDeltaTime*9);Portrait.anchoredPosition=new Vector2(0,75+Mathf.Sin(Time.unscaledTime*1.2f+Index)*5);}
}
