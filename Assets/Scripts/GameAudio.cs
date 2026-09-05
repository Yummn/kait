using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public sealed class GameAudio : MonoBehaviour
{
    private const string MusicPath = "Audio/BackgroundMusic";
    private const string MergePath = "Audio/UI/SelectedModel/Merge_B";
    private const string CombatPath = "Audio/Combat/";
    private const string SelectedWeaponPath = CombatPath + "SelectedModel/";
    private const string SelectedCardPath = "Audio/UI/SelectedCards/";
    private const string SelectedRangedPath = "Audio/Ranged/SelectedModel/";
    private const string SelectedWorldPath = "Audio/World/SelectedModel/";
    private const string SelectedUiPath = "Audio/UI/SelectedModel/";
    private const string SelectedSkillPath = "Audio/Skills/SelectedModel/";
    private const string KaitVoicePath = "Audio/Voice/Gloria/";
    private const string EnemyCharacterVoicePath = "Audio/Voice/Enemies/";
    private const float VoiceChannelVolume = 0.5f;
    private const float DuckedEnemyVoiceMultiplier = 0.32f;
    private const float EnemyVoiceFadeSeconds = 0.08f;

    private sealed class EnemyCharacterVoiceBank
    {
        public AudioClip[] shortAttackClips;
        public AudioClip[] rareAttackClips;
        public AudioClip[] hurtClips;
        public AudioClip spawnClip;
        public AudioClip prepareClip;
        public AudioClip pushHurtClip;
        public AudioClip heavyHurtClip;
        public AudioClip deathClip;
        public AudioClip defeatKaitClip;
        public float nextHurtAt;
        public bool spawnPlayed;
        public readonly HashSet<int> preparePlayed = new HashSet<int>();
        public readonly HashSet<int> heavyHurtPlayed = new HashSet<int>();

        public void ResetState()
        {
            nextHurtAt = 0f;
            spawnPlayed = false;
            preparePlayed.Clear();
            heavyHurtPlayed.Clear();
        }
    }

    private static GameAudio instance;

    private AudioSource musicSource;
    private AudioSource effectSource;
    private AudioSource drawSwordSource;
    private AudioSource swingSource;
    private AudioSource impactSource;
    private AudioSource killSource;
    private AudioSource kaitVoiceSource;
    private readonly Dictionary<KaitEnemyType, AudioSource> enemyVoiceSources =
        new Dictionary<KaitEnemyType, AudioSource>();
    private AudioSource rangedSource;
    private AudioSource magicSource;
    private AudioSource magicActionSource;
    private AudioSource skillChargeSource;
    private readonly Dictionary<AudioClip, float> actionCueTimes = new Dictionary<AudioClip, float>();
    private AudioSource worldSource;
    private AudioSource uiSource;
    private AudioSource cardSource;
    private AudioClip cardPickUpClip, cardSnapClip, cardPlayClip, passiveConfirmClip;
    private AudioClip[] drawSwordClips;
    private AudioClip swordSwingClip;
    private AudioClip normalHitClip;
    private AudioClip blockClip;
    private AudioClip[] kaitNormalAttackVoiceClips;
    private AudioClip[] kaitHurtVoiceClips;
    private AudioClip kaitKillVoiceClip;
    private AudioClip kaitChainVoiceClip;
    private AudioClip kaitSmallAttackSkillVoiceClip;
    private AudioClip kaitLargeAttackSkillVoiceClip;
    private AudioClip kaitUltimateVoiceClip;
    private AudioClip kaitHeavyHurtVoiceClip;
    private AudioClip kaitStartVoiceClip;
    private AudioClip kaitWinVoiceClip;
    private AudioClip kaitFailVoiceClip;
    private AudioClip kaitDeathVoiceClip;
    private readonly Dictionary<KaitEnemyType, EnemyCharacterVoiceBank> enemyCharacterVoiceBanks =
        new Dictionary<KaitEnemyType, EnemyCharacterVoiceBank>();
    private AudioClip enemyHurtClip;
    private AudioClip magicImpactClip;
    private AudioClip riftWarningClip;
    private AudioClip landingClip;
    private AudioClip wallStopClip;
    private AudioClip pushClip;
    private AudioClip enemyDeathClip;
    private AudioClip arrowFlightClip;
    private AudioClip arrowImpactClip;
    private AudioClip magicChargeClip;
    private AudioClip magicCastClip;
    private AudioClip bossRoarClip;
    private AudioClip clickClip;
    private AudioClip invalidClip;
    private AudioClip skillReadyClip;
    private AudioClip dreadSlashClip;
    private AudioClip bodyHurtClip;
    private AudioClip winClip;
    private AudioClip loseClip;
    private int enemyHurtFrame = -1;
    private int enemyHurtVoicesThisFrame;
    private float kaitVoiceEndsAt;
    private Coroutine queuedKaitVoiceRoutine;
    private readonly Dictionary<KaitEnemyType, float> enemyCharacterVoiceEndsAt =
        new Dictionary<KaitEnemyType, float>();
    private readonly Dictionary<KaitEnemyType, float> enemyDeathVoiceEndsAt =
        new Dictionary<KaitEnemyType, float>();
    private readonly Dictionary<KaitEnemyType, int> enemyVoiceOrder =
        new Dictionary<KaitEnemyType, int>();
    private int enemyVoiceSequence;
    private readonly Dictionary<KaitEnemyType, Coroutine> queuedEnemyVoiceRoutines =
        new Dictionary<KaitEnemyType, Coroutine>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;

        GameObject audioObject = new GameObject("Game Audio");
        instance = audioObject.AddComponent<GameAudio>();
        DontDestroyOnLoad(audioObject);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = CreateSource("Music", Resources.Load<AudioClip>(MusicPath), true, 0.2f);
        effectSource = CreateSource("Effects", null, false, 0.55f);
        drawSwordSource = CreateSource("Draw Sword", null, false, 0.58f);
        swingSource = CreateSource("Sword Swing", null, false, 0.62f);
        impactSource = CreateSource("Combat Impact", null, false, 0.72f);
        killSource = CreateSource("Kill Impact", null, false, 0.78f);
        kaitVoiceSource = CreateSource("Kait Voice", null, false, VoiceChannelVolume);
        foreach (KaitEnemyType type in System.Enum.GetValues(typeof(KaitEnemyType)))
            enemyVoiceSources[type] = CreateSource($"Enemy Voice - {type}", null, false, VoiceChannelVolume);
        rangedSource = CreateSource("Ranged Effects", null, false, 0.58f);
        magicSource = CreateSource("Magic Effects", null, false, 0.56f);
        magicActionSource = CreateSource("Magic Actions", null, false, 0.56f);
        skillChargeSource = CreateSource("Skill Charge", null, false, 0.42f);
        worldSource = CreateSource("World Effects", null, false, 0.5f);
        uiSource = CreateSource("UI Effects", null, false, 0.42f);
        cardSource = CreateSource("Card Effects", null, false, 0.72f);
        cardPickUpClip = Resources.Load<AudioClip>(SelectedCardPath + "CardPickUp_A");
        cardSnapClip = Resources.Load<AudioClip>(SelectedCardPath + "CardSnap_A");
        cardPlayClip = Resources.Load<AudioClip>(SelectedCardPath + "CardPlay_A");
        passiveConfirmClip = Resources.Load<AudioClip>(SelectedCardPath + "PassiveConfirm_B");
        drawSwordClips = LoadClipGroup("DrawSword", 10);
        swordSwingClip = Resources.Load<AudioClip>(SelectedWeaponPath + "SwordSwing_A_OriginalLevel");
        normalHitClip = Resources.Load<AudioClip>(SelectedWeaponPath + "Hit_A");
        blockClip = Resources.Load<AudioClip>(SelectedWeaponPath + "Block_A_OriginalLevel");
        kaitNormalAttackVoiceClips = LoadClips(
            "Gloria_Battle_N_2", "Gloria_Battle_N_3", "Gloria_Battle_N_4", "Gloria_Battle_N_5");
        kaitKillVoiceClip = LoadKaitVoice("Gloria_Battle_N_1");
        kaitChainVoiceClip = LoadKaitVoice("Gloria_Battle_N_6");
        kaitSmallAttackSkillVoiceClip = LoadKaitVoice("Gloria_Battle_H_1");
        kaitLargeAttackSkillVoiceClip = LoadKaitVoice("Gloria_Battle_H_2");
        kaitUltimateVoiceClip = LoadKaitVoice("Gloria_Battle_C_2");
        kaitHurtVoiceClips = LoadClips(
            "Gloria_Battle_Hit_1", "Gloria_Battle_Hit_3", "Gloria_Battle_Hit_5");
        kaitHeavyHurtVoiceClip = LoadKaitVoice("Gloria_Battle_Hit_6");
        kaitStartVoiceClip = LoadKaitVoice("Gloria_Go_1");
        kaitWinVoiceClip = LoadKaitVoice("Gloria_Win_1");
        kaitFailVoiceClip = LoadKaitVoice("Gloria_Fail_1");
        kaitDeathVoiceClip = LoadKaitVoice("Gloria_Battle_Die_1");
        enemyCharacterVoiceBanks[KaitEnemyType.Grunt] = LoadEnemyCharacterVoiceBank("April");
        enemyCharacterVoiceBanks[KaitEnemyType.Swordsman] = LoadEnemyCharacterVoiceBank("Olivia");
        enemyCharacterVoiceBanks[KaitEnemyType.Archer] = LoadEnemyCharacterVoiceBank("Monica");
        enemyCharacterVoiceBanks[KaitEnemyType.Guard] = LoadEnemyCharacterVoiceBank("Bridget");
        enemyCharacterVoiceBanks[KaitEnemyType.Warlock] = LoadEnemyCharacterVoiceBank("Aloe");
        enemyCharacterVoiceBanks[KaitEnemyType.ShieldKnight] = LoadEnemyCharacterVoiceBank("Ursula");
        enemyHurtClip = Resources.Load<AudioClip>("Audio/Combat/KaitHurt_03");
        riftWarningClip = Resources.Load<AudioClip>(SelectedWorldPath + "RiftOpen_B2");
        magicImpactClip = Resources.Load<AudioClip>(SelectedRangedPath + "MagicImpact_B2");
        landingClip = Resources.Load<AudioClip>(SelectedWorldPath + "SpawnLanding_A");
        wallStopClip = Resources.Load<AudioClip>(SelectedWorldPath + "WallStop_A2");
        pushClip = Resources.Load<AudioClip>(SelectedWeaponPath + "Push_A_OriginalLevel");
        enemyDeathClip = Resources.Load<AudioClip>("Audio/Combat/KaitHurt_01");
        arrowFlightClip = Resources.Load<AudioClip>(SelectedRangedPath + "ArrowFlight_A2");
        arrowImpactClip = Resources.Load<AudioClip>(SelectedRangedPath + "ArrowImpact_A");
        magicChargeClip = Resources.Load<AudioClip>(SelectedSkillPath + "MagicCharge_A");
        magicCastClip = Resources.Load<AudioClip>(SelectedRangedPath + "MagicCast_B2");
        bossRoarClip = Resources.Load<AudioClip>("Audio/World/BossRoar_01");
        clickClip = Resources.Load<AudioClip>(SelectedUiPath + "ButtonClick_A");
        invalidClip = Resources.Load<AudioClip>(SelectedUiPath + "InvalidAction_Defeat_A");
        skillReadyClip = Resources.Load<AudioClip>(SelectedWorldPath + "SkillReady_A2");
        dreadSlashClip = Resources.Load<AudioClip>(SelectedSkillPath + "DreadSlash_B");
        bodyHurtClip = Resources.Load<AudioClip>(SelectedSkillPath + "BodyHurt_B");
        winClip = Resources.Load<AudioClip>(SelectedUiPath + "Victory_B3");
        loseClip = Resources.Load<AudioClip>(SelectedUiPath + "Defeat_B3");
        if (musicSource.clip != null) musicSource.Play();
    }

    private void Update()
    {
        KaitEnemyType latestType = KaitEnemyType.Grunt;
        int latestOrder = int.MinValue;
        bool hasPlayingVoice = false;

        foreach (KeyValuePair<KaitEnemyType, AudioSource> pair in enemyVoiceSources)
        {
            AudioSource source = pair.Value;
            if (source == null || !source.isPlaying) continue;
            int order;
            if (!enemyVoiceOrder.TryGetValue(pair.Key, out order)) order = 0;
            if (!hasPlayingVoice || order > latestOrder)
            {
                latestType = pair.Key;
                latestOrder = order;
                hasPlayingVoice = true;
            }
        }

        float fadeStep = VoiceChannelVolume / EnemyVoiceFadeSeconds * Time.unscaledDeltaTime;
        foreach (KeyValuePair<KaitEnemyType, AudioSource> pair in enemyVoiceSources)
        {
            AudioSource source = pair.Value;
            if (source == null) continue;
            bool shouldDuck = hasPlayingVoice && source.isPlaying && pair.Key != latestType;
            float target = shouldDuck
                ? VoiceChannelVolume * DuckedEnemyVoiceMultiplier
                : VoiceChannelVolume;
            source.volume = Mathf.MoveTowards(source.volume, target, fadeStep);
        }
    }

    public static void PlayMerge(int tileNumber)
    {
        if (instance == null || instance.effectSource == null) return;

        AudioClip clip = Resources.Load<AudioClip>(MergePath);
        if (clip == null) return;

        int level = Mathf.Max(0, Mathf.RoundToInt(Mathf.Log(Mathf.Max(2, tileNumber), 2f)) - 2);
        instance.effectSource.pitch = RisingPitch(level);
        instance.effectSource.PlayOneShot(clip, 0.72f);
    }

    public static void PlayKaitKill(int chainKills)
    {
        if (instance == null || instance.killSource == null) return;

        chainKills = Mathf.Clamp(chainKills, 1, 10);
        AudioClip clip = SelectedKillClip(chainKills);
        if (clip == null) return;
        instance.killSource.pitch = Mathf.Clamp(1f + (chainKills - 1) * 0.035f, 1f, 1.25f);
        instance.killSource.PlayOneShot(clip, 1f);

        if (chainKills <= 1)
            instance.PlayKaitVoice(instance.kaitKillVoiceClip, true);
        else if (chainKills == 2)
            instance.PlayKaitVoice(instance.kaitChainVoiceClip, true);
        else
            instance.PlayKaitVoice(RandomClip(instance.kaitNormalAttackVoiceClips), false);
    }

    public static void PlaySwordSwing()
    {
        PlayActionCue(instance?.swingSource, instance?.swordSwingClip, 1f);
    }

    public static void PlayDrawSword()
    {
        PlayRandom(instance?.drawSwordSource, instance?.drawSwordClips, 0.97f, 1.03f, 0.76f);
    }

    public static void PlayNormalHit()
    {
        PlayOneShot(instance?.impactSource, instance?.normalHitClip, 1f);
    }

    public static void PlayBlock()
    {
        PlayOneShot(instance?.impactSource, instance?.blockClip, 1f);
    }

    public static void PlayKaitNormalAttackVoice()
    {
        if (instance == null) return;
        instance.PlayKaitVoice(RandomClip(instance.kaitNormalAttackVoiceClips), false);
    }

    public static void PlayKaitSmallAttackSkillVoice() =>
        instance?.PlayKaitVoice(instance.kaitSmallAttackSkillVoiceClip, true);

    public static void PlayKaitLargeAttackSkillVoice() =>
        instance?.PlayKaitVoice(instance.kaitLargeAttackSkillVoiceClip, true);

    public static void PlayKaitUltimateVoice() =>
        instance?.PlayKaitVoice(instance.kaitUltimateVoiceClip, true);

    public static void PlayKaitDamageVoice(int currentHp, int maximumHp)
    {
        if (instance == null) return;
        if (currentHp <= 0)
        {
            instance.PlayKaitVoice(instance.kaitDeathVoiceClip, true);
            return;
        }

        bool heavilyWounded = currentHp <= Mathf.Max(1, maximumHp / 3);
        AudioClip clip = heavilyWounded
            ? instance.kaitHeavyHurtVoiceClip
            : RandomClip(instance.kaitHurtVoiceClips);
        instance.PlayKaitVoice(clip, true);
    }

    public static void PlayKaitGameStart()
    {
        if (instance == null) return;
        instance.CancelQueuedKaitVoice();
        instance.ResetEnemyCharacterVoiceState();
        instance.PlayKaitVoice(instance.kaitStartVoiceClip, true);
    }

    public static void PlayKaitVictory() =>
        instance?.PlayKaitVoice(instance.kaitWinVoiceClip, true);

    public static void PlayKaitFailure()
    {
        if (instance == null) return;
        instance.QueueKaitVoice(instance.kaitFailVoiceClip);
    }

    public static void PlayEnemyHurt()
    {
        PlayEnemyHurt(KaitEnemyType.Grunt);
    }

    private static void PlayEnemyHurt(KaitEnemyType type)
    {
        if (instance == null) return;
        if (instance.enemyHurtFrame != Time.frameCount)
        {
            instance.enemyHurtFrame = Time.frameCount;
            instance.enemyHurtVoicesThisFrame = 0;
        }
        if (instance.enemyHurtVoicesThisFrame >= 2) return;
        instance.enemyHurtVoicesThisFrame++;
        instance.PlayEnemyVoiceOneShot(type, instance.enemyHurtClip, 0.72f, 0.97f, 1.03f);
    }

    public static void PlayEnemySpawnVoice(KaitEnemyType type, int enemyId)
    {
        if (instance == null) return;
        EnemyCharacterVoiceBank bank;
        if (!instance.enemyCharacterVoiceBanks.TryGetValue(type, out bank) || bank.spawnPlayed) return;
        if (instance.TryPlayEnemyCharacterVoice(type, bank.spawnClip, false)) bank.spawnPlayed = true;
    }

    public static void PlayEnemyPrepareVoice(KaitEnemyType type, int enemyId)
    {
        if (instance == null) return;
        EnemyCharacterVoiceBank bank;
        if (!instance.enemyCharacterVoiceBanks.TryGetValue(type, out bank) || bank.preparePlayed.Contains(enemyId)) return;
        if (instance.TryPlayEnemyCharacterVoice(type, bank.prepareClip, false)) bank.preparePlayed.Add(enemyId);
    }

    public static void PlayEnemyAttackVoice(KaitEnemyType type, int enemyId)
    {
        if (instance == null || Random.value > 0.5f) return;
        EnemyCharacterVoiceBank bank;
        if (!instance.enemyCharacterVoiceBanks.TryGetValue(type, out bank)) return;
        AudioClip[] pool = Random.value < 0.2f ? bank.rareAttackClips : bank.shortAttackClips;
        instance.TryPlayEnemyCharacterVoice(type, RandomClip(pool), false);
    }

    public static void PlayEnemyHurt(KaitEnemyType type, int enemyId, int currentHp, bool pushed)
    {
        if (instance == null) return;
        EnemyCharacterVoiceBank bank;
        if (!instance.enemyCharacterVoiceBanks.TryGetValue(type, out bank))
        {
            PlayEnemyHurt(type);
            return;
        }

        if (Time.realtimeSinceStartup < bank.nextHurtAt) return;

        AudioClip clip;
        bool heavy = currentHp == 1 && !bank.heavyHurtPlayed.Contains(enemyId);
        if (heavy)
            clip = bank.heavyHurtClip;
        else
        {
            if (Random.value > 0.65f) return;
            clip = pushed ? bank.pushHurtClip : RandomClip(bank.hurtClips);
        }

        if (!instance.TryPlayEnemyCharacterVoice(type, clip, false)) return;
        bank.nextHurtAt = Time.realtimeSinceStartup + 1f;
        if (heavy) bank.heavyHurtPlayed.Add(enemyId);
    }

    public static void PlayEnemyDeath(KaitEnemyType type, int enemyId)
    {
        if (instance == null) return;
        EnemyCharacterVoiceBank bank;
        if (!instance.enemyCharacterVoiceBanks.TryGetValue(type, out bank))
        {
            instance.PlayEnemyVoiceOneShot(type, instance.enemyDeathClip, 0.76f, 0.96f, 1.04f);
            return;
        }
        if (Time.realtimeSinceStartup < instance.EnemyDeathVoiceEnd(type)) return;
        AudioClip deathClip = bank.deathClip;
        if (instance.TryPlayEnemyCharacterVoice(type, deathClip, true))
            instance.enemyDeathVoiceEndsAt[type] = instance.EnemyCharacterVoiceEnd(type);
        else if (!instance.queuedEnemyVoiceRoutines.ContainsKey(type) &&
                 Time.realtimeSinceStartup < instance.kaitVoiceEndsAt)
        {
            // Kait's line has priority, but the first defeated voiced enemy still
            // gets its death line once the player voice channel becomes free.
            instance.enemyDeathVoiceEndsAt[type] = float.PositiveInfinity;
            instance.queuedEnemyVoiceRoutines[type] = instance.StartCoroutine(
                instance.PlayEnemyCharacterVoiceAfterKait(type, deathClip, true));
        }
    }

    public static void PlayEnemyDefeatedKaitVoice(KaitEnemyType type, int enemyId)
    {
        if (instance == null) return;
        EnemyCharacterVoiceBank bank;
        if (!instance.enemyCharacterVoiceBanks.TryGetValue(type, out bank)) return;
        AudioClip defeatClip = bank.defeatKaitClip;
        instance.CancelQueuedEnemyCharacterVoice(type);
        instance.queuedEnemyVoiceRoutines[type] = instance.StartCoroutine(
            instance.PlayEnemyCharacterVoiceAfterKait(type, defeatClip, false));
    }

    public static void PlayEnemyDeath() =>
        instance?.PlayEnemyVoiceOneShot(KaitEnemyType.Grunt, instance.enemyDeathClip, 0.76f, 0.96f, 1.04f);
    public static void PlayArrowFlight() => PlayOneShot(instance?.rangedSource, instance?.arrowFlightClip, 0.58f, 0.98f, 1.03f);
    public static void PlayArrowImpact() => PlayOneShot(instance?.impactSource, instance?.arrowImpactClip, 0.86f, 0.97f, 1.04f);
    public static void PlayMagicCharge() => PlayActionCue(instance?.magicActionSource, instance?.magicChargeClip, 2f, 0.96f, 1.02f);
    public static void PlayRiftWarning() => PlayOneShot(instance?.magicSource, instance?.riftWarningClip, 0.62f, 0.96f, 1.02f);
    public static void PlayMagicCast() => PlayActionCue(instance?.magicActionSource, instance?.magicCastClip, 0.66f, 0.98f, 1.03f);
    public static void PlayMagicImpact() => PlayOneShot(instance?.magicSource, instance?.magicImpactClip, 0.78f, 0.96f, 1.04f);
    public static void PlayLanding() => PlayOneShot(instance?.worldSource, instance?.landingClip, 0.66f, 0.94f, 1.02f);
    public static void PlayPush() => PlayOneShot(instance?.impactSource, instance?.pushClip, 1f);
    public static void PlayWallStop() => PlayOneShot(instance?.worldSource, instance?.wallStopClip, 0.62f, 0.88f, 0.96f);
    public static void PlayBossRoar()
    {
        if (instance == null || instance.enemyCharacterVoiceBanks.ContainsKey(KaitEnemyType.ShieldKnight)) return;
        instance.PlayEnemyVoiceOneShot(KaitEnemyType.ShieldKnight, instance.bossRoarClip, 0.86f, 0.92f, 0.98f);
    }
    public static void PlayClick() => PlayOneShot(instance?.uiSource, instance?.clickClip, 0.72f, 0.98f, 1.02f);
    public static void PlayCardPickUp() => PlayOneShot(instance?.cardSource, instance?.cardPickUpClip, 1f);
    public static void PlayCardSnap() => PlayOneShot(instance?.cardSource, instance?.cardSnapClip, 1f);
    public static void PlayCardPlay() => PlayOneShot(instance?.cardSource, instance?.cardPlayClip, 1f);
    public static void PlayPassiveConfirm() => PlayOneShot(instance?.cardSource, instance?.passiveConfirmClip, 1f);
    public static void PlayInvalid() => PlayOneShot(instance?.uiSource, instance?.invalidClip, 0.72f);
    public static void PlaySkillReady() => PlayOneShot(instance?.uiSource, instance?.skillReadyClip, 0.7f);
    public static void PlayDreadSlash() => PlayOneShot(instance?.swingSource, instance?.dreadSlashClip, 1f);
    public static void PlayBodyHurt() => PlayOneShot(instance?.impactSource, instance?.bodyHurtClip, 1f);
    public static void PlaySkillUse(KaitSkill skill = KaitSkill.None)
    {
        if (instance == null) return;
        PlayOneShot(skill == KaitSkill.DreadSlash ? instance.skillChargeSource : instance.uiSource,
            SelectedSkillClip(skill), 0.76f);
    }

    private static AudioClip SelectedSkillClip(KaitSkill skill)
    {
        switch (skill)
        {
            case KaitSkill.SwiftBoots:
            case KaitSkill.CatAgility:
                return Resources.Load<AudioClip>(SelectedSkillPath + "SpeedBuff_A");
            case KaitSkill.IceTomb:
                return Resources.Load<AudioClip>(SelectedSkillPath + "FrostBind_B");
            case KaitSkill.ShadowStep:
                return Resources.Load<AudioClip>(SelectedSkillPath + "ShadowStep_B");
            case KaitSkill.LesserPhantom:
                return Resources.Load<AudioClip>(SelectedSkillPath + "Phantom_B");
            case KaitSkill.DreadSlash:
                return Resources.Load<AudioClip>(SelectedSkillPath + "DreadCharge_B");
            default:
                return Resources.Load<AudioClip>("Audio/UI/SkillUse_01");
        }
    }
    public static void PlayWin() => PlayOneShot(instance?.uiSource, instance?.winClip, 0.82f);
    public static void PlayLose() => PlayOneShot(instance?.uiSource, instance?.loseClip, 0.82f);

    private static float RisingPitch(int step)
    {
        return Mathf.Clamp(0.94f + Mathf.Max(0, step) * 0.035f, 0.94f, 1.25f);
    }

    private static AudioClip[] LoadClipGroup(string groupName, int count, string root = CombatPath)
    {
        var clips = new AudioClip[count];
        for (int i = 0; i < count; i++)
            clips[i] = Resources.Load<AudioClip>($"{root}{groupName}_{i + 1:00}");
        return clips;
    }

    private static AudioClip LoadKaitVoice(string clipName)
    {
        return Resources.Load<AudioClip>(KaitVoicePath + clipName);
    }

    private static AudioClip[] LoadClips(params string[] clipNames)
    {
        var clips = new AudioClip[clipNames.Length];
        for (int i = 0; i < clipNames.Length; i++) clips[i] = LoadKaitVoice(clipNames[i]);
        return clips;
    }

    private static EnemyCharacterVoiceBank LoadEnemyCharacterVoiceBank(string characterName)
    {
        string root = EnemyCharacterVoicePath + characterName + "/";
        string prefix = characterName + "_";
        return new EnemyCharacterVoiceBank
        {
            shortAttackClips = LoadEnemyCharacterVoiceClips(root,
                prefix + "Battle_N_2", prefix + "Battle_N_3", prefix + "Battle_N_4"),
            rareAttackClips = LoadEnemyCharacterVoiceClips(root,
                prefix + "Battle_N_1", prefix + "Battle_N_5"),
            hurtClips = LoadEnemyCharacterVoiceClips(root,
                prefix + "Battle_Hit_1", prefix + "Battle_Hit_2"),
            spawnClip = Resources.Load<AudioClip>(root + prefix + "Go_1"),
            prepareClip = Resources.Load<AudioClip>(root + prefix + "Battle_H_2"),
            pushHurtClip = Resources.Load<AudioClip>(root + prefix + "Battle_Hit_3"),
            heavyHurtClip = Resources.Load<AudioClip>(root + prefix + "Battle_Hit_4"),
            deathClip = Resources.Load<AudioClip>(root + prefix + "Battle_Die_1"),
            defeatKaitClip = Resources.Load<AudioClip>(root + prefix + "Battle_C_1")
        };
    }

    private static AudioClip[] LoadEnemyCharacterVoiceClips(string root, params string[] clipNames)
    {
        var clips = new AudioClip[clipNames.Length];
        for (int i = 0; i < clipNames.Length; i++)
            clips[i] = Resources.Load<AudioClip>(root + clipNames[i]);
        return clips;
    }

    private static AudioClip SelectedKillClip(int chainKills)
    {
        // The second consecutive kill already uses the selected chain sound.
        return Resources.Load<AudioClip>(SelectedWeaponPath + (chainKills >= 2 ? "Chain_A" : "Kill_A"));
    }

    private static AudioClip RandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        int start = Random.Range(0, clips.Length);
        for (int offset = 0; offset < clips.Length; offset++)
        {
            AudioClip clip = clips[(start + offset) % clips.Length];
            if (clip != null) return clip;
        }
        return null;
    }

    public static void InterruptActionSounds()
    {
        if (instance == null) return;
        instance.drawSwordSource?.Stop();
        instance.swingSource?.Stop();
        instance.rangedSource?.Stop();
        instance.magicActionSource?.Stop();
        instance.skillChargeSource?.Stop();
        instance.actionCueTimes.Clear();
        // Preserve confirmed impacts, kills, voices, cards and music tails.
    }

    private static bool AcceptActionCue(float now, float previous) => now - previous >= 0.04f;

    private static void PlayActionCue(AudioSource source, AudioClip clip, float volume,
        float minimumPitch = 1f, float maximumPitch = 1f)
    {
        if (instance == null || source == null || clip == null) return;
        float now = Time.realtimeSinceStartup;
        if (instance.actionCueTimes.TryGetValue(clip, out float previous) && !AcceptActionCue(now, previous)) return;
        instance.actionCueTimes[clip] = now;
        PlayOneShot(source, clip, volume, minimumPitch, maximumPitch);
    }

    private static void PlayRandom(AudioSource source, AudioClip[] clips, float minimumPitch, float maximumPitch, float volumeScale)
    {
        if (source == null) return;
        AudioClip clip = RandomClip(clips);
        if (clip == null) return;
        source.pitch = Random.Range(minimumPitch, maximumPitch);
        source.PlayOneShot(clip, volumeScale);
    }

    private static void PlayOneShot(AudioSource source, AudioClip clip, float volumeScale, float minimumPitch = 1f, float maximumPitch = 1f)
    {
        if (source == null || clip == null) return;
        source.pitch = Random.Range(minimumPitch, maximumPitch);
        source.PlayOneShot(clip, volumeScale);
    }

    private void PlayEnemyVoiceOneShot(
        KaitEnemyType type, AudioClip clip, float volumeScale,
        float minimumPitch = 1f, float maximumPitch = 1f)
    {
        AudioSource source = EnemyVoiceSource(type);
        if (source == null || clip == null) return;
        BeginEnemyVoice(type, source);
        source.pitch = Random.Range(minimumPitch, maximumPitch);
        source.PlayOneShot(clip, volumeScale);
    }

    private void BeginEnemyVoice(KaitEnemyType type, AudioSource source)
    {
        enemyVoiceSequence++;
        enemyVoiceOrder[type] = enemyVoiceSequence;
        source.volume = VoiceChannelVolume;
    }

    private void PlayKaitVoice(AudioClip clip, bool interruptCurrent)
    {
        if (kaitVoiceSource == null || clip == null) return;
        if (!interruptCurrent && Time.realtimeSinceStartup < kaitVoiceEndsAt) return;

        CancelQueuedKaitVoice();
        if (interruptCurrent) kaitVoiceSource.Stop();
        kaitVoiceSource.pitch = 1f;
        kaitVoiceSource.PlayOneShot(clip, 1f);
        kaitVoiceEndsAt = Time.realtimeSinceStartup + clip.length;
    }

    private void QueueKaitVoice(AudioClip clip)
    {
        if (clip == null) return;
        CancelQueuedKaitVoice();
        float delay = Mathf.Max(0f, kaitVoiceEndsAt - Time.realtimeSinceStartup);
        if (delay <= 0f)
        {
            PlayKaitVoice(clip, true);
            return;
        }
        queuedKaitVoiceRoutine = StartCoroutine(PlayQueuedKaitVoice(clip, delay));
    }

    private IEnumerator PlayQueuedKaitVoice(AudioClip clip, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        queuedKaitVoiceRoutine = null;
        PlayKaitVoice(clip, true);
    }

    private void CancelQueuedKaitVoice()
    {
        if (queuedKaitVoiceRoutine == null) return;
        StopCoroutine(queuedKaitVoiceRoutine);
        queuedKaitVoiceRoutine = null;
    }

    private AudioSource EnemyVoiceSource(KaitEnemyType type)
    {
        AudioSource source;
        return enemyVoiceSources.TryGetValue(type, out source) ? source : null;
    }

    private float EnemyCharacterVoiceEnd(KaitEnemyType type)
    {
        float value;
        return enemyCharacterVoiceEndsAt.TryGetValue(type, out value) ? value : 0f;
    }

    private float EnemyDeathVoiceEnd(KaitEnemyType type)
    {
        float value;
        return enemyDeathVoiceEndsAt.TryGetValue(type, out value) ? value : 0f;
    }

    private bool TryPlayEnemyCharacterVoice(KaitEnemyType type, AudioClip clip, bool interruptCurrent)
    {
        AudioSource source = EnemyVoiceSource(type);
        if (source == null || clip == null) return false;
        if (Time.realtimeSinceStartup < kaitVoiceEndsAt) return false;
        if (!interruptCurrent && Time.realtimeSinceStartup < EnemyCharacterVoiceEnd(type)) return false;

        if (interruptCurrent) source.Stop();
        BeginEnemyVoice(type, source);
        source.pitch = 1f;
        source.PlayOneShot(clip, 1f);
        enemyCharacterVoiceEndsAt[type] = Time.realtimeSinceStartup + clip.length;
        return true;
    }

    private IEnumerator PlayEnemyCharacterVoiceAfterKait(
        KaitEnemyType type, AudioClip clip, bool deathVoice)
    {
        while (queuedKaitVoiceRoutine != null || Time.realtimeSinceStartup < kaitVoiceEndsAt ||
               Time.realtimeSinceStartup < EnemyCharacterVoiceEnd(type))
            yield return null;
        queuedEnemyVoiceRoutines.Remove(type);
        if (TryPlayEnemyCharacterVoice(type, clip, false) && deathVoice)
            enemyDeathVoiceEndsAt[type] = EnemyCharacterVoiceEnd(type);
    }

    private void ResetEnemyCharacterVoiceState()
    {
        CancelAllQueuedEnemyCharacterVoices();
        foreach (AudioSource source in enemyVoiceSources.Values) source?.Stop();
        enemyCharacterVoiceEndsAt.Clear();
        enemyDeathVoiceEndsAt.Clear();
        enemyVoiceOrder.Clear();
        enemyVoiceSequence = 0;
        foreach (AudioSource source in enemyVoiceSources.Values)
            if (source != null) source.volume = VoiceChannelVolume;
        foreach (EnemyCharacterVoiceBank bank in enemyCharacterVoiceBanks.Values) bank.ResetState();
    }

    private void CancelQueuedEnemyCharacterVoice(KaitEnemyType type)
    {
        Coroutine routine;
        if (!queuedEnemyVoiceRoutines.TryGetValue(type, out routine)) return;
        if (routine != null) StopCoroutine(routine);
        queuedEnemyVoiceRoutines.Remove(type);
    }

    private void CancelAllQueuedEnemyCharacterVoices()
    {
        foreach (Coroutine routine in queuedEnemyVoiceRoutines.Values)
            if (routine != null) StopCoroutine(routine);
        queuedEnemyVoiceRoutines.Clear();
    }

    private AudioSource CreateSource(string sourceName, AudioClip clip, bool loop, float volume)
    {
        GameObject sourceObject = new GameObject(sourceName);
        sourceObject.transform.SetParent(transform, false);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.playOnAwake = false;
        source.volume = volume;
        source.spatialBlend = 0f;
        return source;
    }
}
