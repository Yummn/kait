from pathlib import Path
import re
p=Path('Assets/Scripts/ReynardCatalog.cs');s=p.read_text(encoding='utf-8-sig')
texts=['四邻敌人受2伤，存活者向外推1格','传送至三格内的一个空格','三格内选一格，对该格及四邻造成2伤','三格内选一格，对该格及八邻造成2伤','朝指定方向贯穿射击，每个敌人受3伤','清除全场敌人的当前攻击预告','选三格内敌人，依次跳向两格内近敌；至多5名，各3伤','每次方向咒后，向最近敌人发射1伤飞弹','在三格内一格及四邻维持阻挡敌我射线的雾','在三格内一格及四邻维持蛛网，禁止敌人自主移动','每次方向咒后，对四邻敌人造成1伤','即时法术后，沿上次方向追加贯火','折火额外向当前方向的反面发射一道射线','回火范围扩展至八邻','回火命中后，将四邻存活敌人向外推1格','折火的所有射线贯穿敌人','等待时额外释放一次回火','镜狐四邻真实合并时，恢复镜狐法术位','镜狐合并后，连续吞并最近的同值数字','即时攻击法术命中后，追加一次等量伤害','法术直接伤害扩散至四邻其他敌人，各1伤','主动法术的目标选取距离增加2格','施法后，抵挡随后敌方阶段的首次伤害','即时法术后，追加一次当前持续法术的自动攻击']
for i,t in enumerate(texts,1):
 pattern=r'(id="reynard.R%02d"[^\n]*?cardText=")[^"]*(")'%i
 s,n=re.subn(pattern,lambda m:m[1]+t+m[2],s);assert n==1,(i,n)
p.write_text(s,encoding='utf-8-sig')
p=Path('Assets/Scripts/GameAudio.cs');s=p.read_text(encoding='utf-8-sig').replace('public static bool YummnMode;','public static bool YummnMode;\n    public static bool ReynardMode;');needle='private void PlayKaitVoice(AudioClip clip, bool interruptCurrent)';s=s.replace(needle+'\n    {',needle+'\n    {\n        if(ReynardMode)return;');p.write_text(s,encoding='utf-8-sig')
p=Path('Assets/Scripts/KaitGame.Yummn.cs');s=p.read_text(encoding='utf-8-sig').replace('GameAudio.YummnMode=run.IsYummn;','GameAudio.YummnMode=run.IsYummn;GameAudio.ReynardMode=run.IsReynard;');p.write_text(s,encoding='utf-8-sig')
