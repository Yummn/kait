using System;
using UnityEngine;

[Serializable] public sealed class ReynardRun
{
 public bool hasLastDirection,mirrorWard;
 public KaitDirection lastDirection;
 public Vector2Int mirrorFoxCell=new Vector2Int(-1,-1),focusedCell;
 // Kept for replay migration from 0.4. The 0.5 rules use sigils and tails.
 public bool[,] spellReady;
 public KaitSkill focusedSkill=KaitSkill.None;
 public bool[,] sigils;
 public int[,] sigilCreated;
 public int nextSigilSequence,tails;
 public Vector2Int fogCell=new Vector2Int(-1,-1),webCell=new Vector2Int(-1,-1),stoneWallCell=new Vector2Int(-1,-1);
 public int actionId,directionSpells,throughFires,turnFires,backfires,casts,summons,enemyPhases,devours;
 public void Reset(int size)
 {
  spellReady=new bool[size,size];sigils=new bool[size,size];sigilCreated=new int[size,size];
  mirrorFoxCell=fogCell=webCell=stoneWallCell=new Vector2Int(-1,-1);focusedCell=Vector2Int.zero;focusedSkill=KaitSkill.None;
  hasLastDirection=mirrorWard=false;lastDirection=KaitDirection.Up;nextSigilSequence=tails=0;
  actionId=directionSpells=throughFires=turnFires=backfires=casts=summons=enemyPhases=devours=0;
 }
}
public enum ReynardDamageKind { Orb,OrbSecondary,SpellDirect,SpellSplash,SigilEffect,MergeEffect,TailCounter }
public enum ReynardTailSpendReason { SpellPayment,Defense,CapacityTrim }
public enum ReynardDirectionSpell { Through,Turn,Backfire }
[Serializable] public sealed class ReynardVisualEvent
{ public string kind; public Vector2Int from,to; public int amount; }
