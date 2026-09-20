using System;
using UnityEngine;
[Serializable] public sealed class ReynardRun
{
 public bool hasLastDirection,mirrorWard;
 public KaitDirection lastDirection;
 public Vector2Int mirrorFoxCell=new Vector2Int(-1,-1),focusedCell;
 public bool[,] spellReady;
 public KaitSkill focusedSkill=KaitSkill.None;
 public int actionId,directionSpells,throughFires,turnFires,backfires,casts,summons,enemyPhases,devours;
 public void Reset(int size){spellReady=new bool[size,size];mirrorFoxCell=new Vector2Int(-1,-1);focusedCell=Vector2Int.zero;focusedSkill=KaitSkill.None;hasLastDirection=mirrorWard=false;lastDirection=KaitDirection.Up;actionId=directionSpells=throughFires=turnFires=backfires=casts=summons=enemyPhases=devours=0;}
}
public enum ReynardDamageKind { DirectionSpell,SpellDirect,SpellSplash }
public enum ReynardDirectionSpell { Through,Turn,Backfire }
[Serializable] public sealed class ReynardVisualEvent
{ public string kind; public Vector2Int from,to; public int amount; }
