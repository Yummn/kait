from pathlib import Path
P=Path('Assets/Scripts')
def edit(f,a,b):
 p=P/f;s=p.read_text(encoding='utf-8-sig');assert a in s,(f,a);p.write_text(s.replace(a,b),encoding='utf-8')
edit('KaitCore.cs','public sealed class KaitTurnResult\n{','public sealed class KaitTurnResult\n{\n    public readonly List<ReynardVisualEvent> reynardEvents=new List<ReynardVisualEvent>();')
edit('KaitCore.cs','if (!Inside(p) || walls[p.x, p.y]) break;','if (!Inside(p) || walls[p.x, p.y] || ReynardFogAt(p)) break;')
edit('YummnRepool.cs','            if(IsYummn)Yummn.prepared.Remove(old.id);','            if(IsYummn)Yummn.prepared.Remove(old.id);\n            if(IsReynard&&old.skill==Reynard.focusedSkill)Reynard.focusedSkill=KaitSkill.None;')
# Display integration must use the complete result snapshots, never the Kait instant-skill shortcut.
edit('KaitGame.cs','        if(run.IsYummn)\n        {\n            targetingSkill=KaitSkill.None;','        if(run.IsYummn||run.IsReynard)\n        {\n            targetingSkill=KaitSkill.None;')
edit('KaitGame.Wait.cs','if(!run.IsYummn||busy','if((!run.IsYummn&&!run.IsReynard)||busy')
edit('KaitGame.Yummn.cs','run.IsYummn?"Characters/Yummn/108231_SkeletonData"','run.IsReynard?"Characters/Reynard/11202003/11202003_SkeletonData":run.IsYummn?"Characters/Yummn/108231_SkeletonData"')
edit('KaitGame.Yummn.cs','string key=run.IsYummn?','string key=run.IsReynard?KaitVersion.ReynardSaveKey:run.IsYummn?')
