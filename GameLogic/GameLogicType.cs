

using System;
using Corgi.GameData;

namespace IdleCs.GameLogic
{
	public static class GlobalSetting
	{
		public static readonly uint ServerIndex = 100;

		public static readonly uint CombatTick = 1500;
	}
	
	public static class GameLogicSetting
	{
		public static SkillAttributeType[,] AttributeMap = new SkillAttributeType[7, 2]
		{
			{SkillAttributeType.SatNone, SkillAttributeType.SatNone}
			, {SkillAttributeType.SatLightning, SkillAttributeType.SatIce}
			, {SkillAttributeType.SatFire, SkillAttributeType.SatEarth}
			, {SkillAttributeType.SatIce, SkillAttributeType.SatLightning}
			, {SkillAttributeType.SatEarth, SkillAttributeType.SatFire}
			, {SkillAttributeType.SatShadow, SkillAttributeType.SatNone}
			, {SkillAttributeType.SatHoly, SkillAttributeType.SatNone}
		};

		public static long GameLogicMaxValue = 9999999999999999; // 1경
	}

	public enum RoomState
	{
		Created
		, Loading
		, Running
		, WillBeDestroy
		, NoConnectionHunting
	}

    public enum UserState
    {
	    Connected
	    , Joined
	    , Active
    }

    public enum CombatLogCategory
    {
	    None
	    , System
	    , User
	    , Dungeon
	    , DungeonStackOverFlow
	    , Unit
	    , Character
	    , Monster
	    , NPC
	    , Skill
	    , Relic
	    , Equip
	    , Tutorial
	    , Chatting
	    , Party
	    , Rift
    }

    public enum SharedInstanceCategory
    {
	    None	= 0,
	    Dungeon	= 1,
	    Arena	= 2
    }

    public enum UserAction
    {
	    None
	    , SkillLab
	    , EquipRefinery
	    , AncientTower
	    , BlackSmith
	    , CharacterManaging
	    , DungeonChallenge
	    , AutoHunting
	    , AdventureChallenge
	    , HallOfBinding
    }
	
	public enum DungeonType
	{
		None = 0
		, Adventure
		, Instance
		, Tutorial
		, WorldBoss
		, Rift
		, Arena
		, Dummy = 98
		, Test			= 99
	}

	public enum SkillActionType
	{
		None = 0
		, Attack
		, Skill
	}
	
    public enum SkillType
    {
	    // for unit
		None                = 0
	    , Attack			= 1
        , Active			= 10
        , Passive			= 11
        , Casting 			= 12
        , Channeling		= 13
        , Relic				= 14
        , External			= 20
    }

	public enum SkillActorType
	{
		None				= 0
		, Skill				= 10
		//, SkillPassive		= 11
		, SkillContinuous	= 20
		, SkillComp			= 30
		, Dungeon			= 40
	}

	public enum ActiveSkillCompType
	{
		DamageAtkFactor			= 1
		, DamageDefFactor		= 2
		
		, DamageMaxHPPercent	= 5
		, DamageCurHPPercent	= 6
		, DamageReducedHPPercent	= 7
		, DamageMaxHPPercentWorldBoss	= 8
		
		, DamageSkillOutput     = 10
		
		, DamageTransfered	    = 20
		, DamageTransferedDOT     = 21
		
		, DrainDamageHPPercent = 30
		
		// heal
		, HealAtkFactor			= 100
		, HealMaxHPPercent		= 101
		
		// continuous
		, DispelBuff  		    = 400
		, DispelDebuff  		    = 410
		, DispelSelfContinuous  	= 420
		
		, AbsorbBuff			= 450
		, AbsorbDebuff			= 451
		
		, ChangeContinuousDuration = 460
		
		// mana
		, RestoreMana = 500
		, ConsumeMana = 501
		, AbsorbMana = 502
		, RestoreNextMana = 510
		, DecreaseManaCostSelf = 511
		
		// etc
		, Interrupt				= 900
		
		, ChangeCurHPPercentSelf = 910
		, Summon = 920
		
		, None = 999
	}

	public enum SkillEffectInstType
	{
		Standard			= 1
		, Relic				= 2
		, Aura			= 20
	}

	public enum PassiveSkillCompType
	{
		None						= 0
		, Stat						= 10
		, Enhance					= 20	
		, Mez						= 30
		, Immune					= 50
		
		// Barrier
		, BarrierDamage				= 100
		, BarrierDamageByDamagedHP 	= 101
		, BarrierDamageByTransferDamage = 102
		
		, Active					= 200
		
		//Custom
		, Custom					= 300
		
		, Dot 						= 400
		
		// etc
		, ReflectDamage				= 500
		, TransferDamage 			= 600
		, TransferDamageToBarrier 	= 610
		
		, Taunt						= 700
		, SaveFromDeath				= 800
		
		, ForceElementalRelation	= 900
		
		, ConvertToTrueDamage		= 1000
		, Vampiric					= 1010
	}

	public enum ImmuneType
	{
		None        = 0
		, All		= 1
		, Damage	= 10
		, Debuff	= 20
		, Mez 		= 30
		, Interrupt = 40
	}

	public enum ConditionCompType
	{
		None = 0
		// Common
		, DungeonType = 10
		, DungeonTime = 11
		
		// Target
		, RoleType = 500
		, ClassType = 501
		
		/// target continuous
		, CurHPPercent 		= 1000
//		, PerDamagedHp 		= 1010
//		, PerDamagedHpRate  = 1011
//		
//		, SkillEffect 		= 1100
//		, HaveSkillEffect	= 1110
//		
		, Immune 			= 1110
//		, Stat 				= 1120
		, Mez 				= 1130
		, Barrier			= 1140
//		, ElementalRelation 			= 1141
//		
//		, CheckUnitAlive    = 1150
		, CheckContinuous= 1500
		, CheckContinuousSelf = 1510
		
		
		// Skill
		, SkillBaseUid = 2000
		, SkillUid = 2001
		
		, SkillType = 2010
		, SkillAttributeType = 2011
		, SkillActorType = 2012
		, SkillAreaType  = 2013
		
		, SkillHitCount = 2020
		, SkillResultType = 2021
		
		, CheckAttributePower = 2050
		
		, CheckCritical		= 2060
		
		
		// Output
		, IsCritical 		= 3000
		, DispelCount		= 3010
		
	}

	public enum CompareCriteriaType
	{
		None = 0
		, Under = 1
		, Over = 2
	}

	public enum SelectCriteriaType
	{
		None = 0
		, Highest
		, Lowest
	}
	

	public enum SkillFeatureType
	{
		None = 0
		// change active comp calc
		, AmplifyingOutput = 10
		, Enhance		 = 20
		
		// change output option
		, Ignore 		= 50
		, Force 		= 60
		
		, Drain		= 110
	}

	public enum SkillFeatureForceType
	{
		None = 0
		, Critical = 10
		, Hit = 20
	}
	
	public enum SkillFeatureIgnoreType
	{
		None = 0
		, Critical = 10
		, Event = 20
		, Barrier = 30
		, Immune = 40
		, Enhance = 50
		, Defence = 60
	}

	public enum EnhanceType
	{
		/// for Damage
		Damage						= 1000
		, DamageToBarrier			= 1030
		, IncomingDamage			= 1100
		, DamageByDefence 			= 1200
		, DamageByEnemyHp			= 1300
		, AttackDamage				= 1500
		, IncomingAttackDamage				= 1510
		, SkillDamage               = 1600
		, IncomingSkillDamage       = 1610
		, SkillEffectDamage			= 1700
		, IncomingSkillEffectDamage			= 1710

		/// for Heal
		, Heal						= 2000
		, IncomingHeal				= 2100
		
		//, DisableIncomingHeal

		/// for 2nd Stat
		, HitProb   				= 3000
		, EvasionProb 				= 3010
		
		, CritProb					= 3100
		, ResilienceProb			= 3110
		
		, CritDamage				= 3200
		, IncomingCritDamage		= 3210
		
		// etc
		, CastingSpeed				= 4000
		
		// for Barrier
		, Barrier					= 4100
		
		// for 2nd Stat
		, DefencePanetration        = 5010
		, Synastry					= 5020
		
		
		, None 						= 9999
	}

    public enum MezType
    {
		None 				= 0
        , Silence			= 5
	    , Sleep				= 20
        , Stun				= 30
        
        , Fortify			= 50 // for world boss
        , Exausted			= 51 // for world boss
        
        , All				= 100
    }
    
    public enum SkillFactorType
    {
    	None				= 0
    	, ManaCost 			= 10
    	, CastingTime		= 20
    }
	
	public enum ActiveFollowingType
	{
		Success				= 1
		, Critical			= 10
		, Immune			= 100
	}

	public enum EffectFollowingType
	{
		Success				= 1
		, Resist			= 10
		, Immune			= 100
	}

	//public enum EquipType
	//{
	//	Weapon = 0
	//	, Helmet 	= 1
	//	, Armor 	= 2
	//	, Gloves	= 3
	//	, Shoes		= 4
	//	, Cloak 	= 5
	//	, Trinket	= 6
	//	, None 		= 99
	//}

	public enum AttributeRelationType
	{
		Neutral			= 1
		, Advantage 	= 2
		, DisAdvantage 	= 3
	}

	public enum ActivateTargetRelation
	{
		Self		= 1
		, Friend	= 10
		, Enemy		= 20
	}

	public enum ActivateTargetState
	{
		Alive		= 1
		, Dead		= 2
	}

	public enum SkillAreaType
	{
		None = 0
		, Single = 1
		, Area   = 2
	}
	
	public enum SkillTargetType
	{
		Self = 1
		, Caster = 2
		, Attacker = 3
		
		, Target = 10
		, TargetNearestSecond = 11
		, TargetNearestThird = 12
		
		, FriendAll		= 20
		, FriendTargetNearest  = 21
		, FriendAllButSelf = 22
		
		, EnemyAll		= 30
		, EnemyAllButTarget = 31
		
		, FriendLowestHP = 40
		, FriendTwoLowestHP = 41
		
		, RandomEnemy = 50
		, Summoner = 60
	}
	
	public enum SkillCompType
	{
		Active			= 1
		, Passive		= 2
		, Continuous    = 3
		, Array			= 4
	}

	public enum SkillActiveType
	{
		None = 0
		, Attack = 100
		, Heal = 200
	}

	public enum SkillContinuousState
	{
		Alive			= 1
		, Dead			= 2
	}

	public enum ContinuousDispelType
	{
		None = 0
		, False = 1
		, True = 2
	}
	
	public enum ContinuousBenefitType
	{
		None			= 0 
		
		, Buff			= 1
		, Debuff		= 2
	}

	public enum ContinuousGroupType
	{
		None			= 0
		, Bleed 		= 1
		, Dot			= 2
	}

	public enum UnitType
	{
		None 				= 0
		, Character			= 10
		, Monster			= 20
	}

	public enum MonsterGrade
	{
		None = 0
		, minion = 1
		, boss = 2
	}

	public enum UnitAliveState
	{
		Alive				= 1
		, Dead				= 2
	}

	public enum CombatSideType
	{
		Player				= 1
		, Enemy				= 2
	}

	
	/// <summary>
	/// Event 발생시키기 위한 Category
	/// </summary>
	public enum CombatEventCategory
	{
		None				= 0
		, Rule				= 10
		, Unit				= 20
		, Action			= 30
		, Effect			= 40
	}

// 	/// <summary>
// 	/// EventManager를 가지고 있는 주체
// 	/// </summary>
// 	public enum CombatEventTargetType
// 	{
// 		Unit				= 10
// //		, Action			= 20
// //		, SkillComp			= 30
// 		, Effect			= 40
// 	}

	public enum CombatEventType
	{
		EventNone = 0
		
		, EventRuleStart 		= 100	// don't use
		, OnEnterDungeon 	= 101
		, OnFinishDungeon 	= 102
		, OnEnterStage		= 103
		, OnFinishStage		= 104
		, OnFinishAction 	= 105
		, OnFinishFriendAction = 106
		, OnFinishEnemyAction = 107
		
		, EventUnitStart 	= 200	// don't use
		
		, OnEnterCombat		= 201
		, OnEnterAction		= 202
		, OnDead			= 210
        , OnDeadSelf		= 211
        , OnDeadSalvation   = 212 // salvation passive dead
		, OnDeadCompletely	= 213
		
		, OnOtherDead		= 220
		, OnFriendDead		= 221
		, OnLinkedDead		= 222
		, OnFriendDeadAll	= 223
		
        , OnNearDeath10		= 230
        , OnNearDeath20		= 232
        , OnNearDeath25		= 233
        , OnNearDeath30		= 234
        , OnNearDeath40		= 236
        , OnNearDeath50		= 237
        
        , OnDamagedHP50		= 240
        , OnDamagedHP40		= 241
        , OnDamagedHP25		= 242
        , OnDamagedHP20		= 243
        , OnDamagedHP10		= 244
        
        
        , OnKill			= 250
		
		, OnInterrupt		= 260
		, OnDispelBuff	    = 261
		, OnDispelDebuff	= 262
		, OnMez				= 265
		
		
		, EventActionStart 	= 300	// don't use
		// action 
		, OnSkill			= 301
		, OnSkillDamage		= 3010
		, OnSkillTargeted	= 3011
		, OnRevive			= 302
		, OnAttack			= 303
		
        , OnCastingStart 	= 310
		, OnCastingCancel 	= 311
		, OnCastingComplete = 312
		, OnCastingInterrupt = 313
		
        , OnChannelingStart 	= 320
		, OnChannelingCancel 	= 321
		, OnChannelingComplete 	= 322
		, OnChannelingTick		= 323

		// 공격
		, OnActiveHit		= 330
		, OnCriticalHit		= 331
		, OnActivePostHit   = 335

		// 피격
		, OnBeingPreHit		= 350
		, OnBeingHit		= 351
		, OnBeingHitAlways	= 352
		, OnBeingCriticalHit = 353
		, OnBeingPostHit     = 354
		
		// 피힐
		, OnHealed			= 360
		, OnHealedMaxHp		= 361
        , OnHealedSelf		= 362
		, OnHealedOver		= 363
		
		// 힐
		, OnHeal			= 365
		
		// 기타
		, OnImmune			= 370
        , OnResist			= 371
		, OnReflectDamage	= 372
		, OnBarrier			= 375

		// SkillEffect
		, EventEffectStart  			= 400	// don't use
		, OnSkillEffectTick				= 401
		, OnSkillEffectEnter			= 402
		, OnSkillEffectMaxStack			= 403
		, OnSkillEffectApply			= 404
		, OnSkillEffectExit				= 410
		, OnSkillEffectDurationOver		= 411
		, OnSkillEffectDispel			= 412
		, OnSkillEffectBreak			= 413
		, OnSkillEffectAttached			= 420
		, OnSkillEffectDetached			= 421
		, OnSkillEffectBarrierBreak		= 430
		, OnSkillEffectUpdated		    = 440
		, OnSkillEffectBuff		        = 450
		, OnSkillEffectDebuff		    = 451
	}

	public enum DungeonState
	{
		None = 0
		, Waiting = 1
		
		, Exploration = 10
		, Exploring   = 11 // for arena
		// Stage 입장  
		, EnterStage  = 20
		, InBossScene = 21
		, EnterCombat = 22
		
		// 전투 진행중
		, InCombat    = 30
		
		// Stage 끝
		, FinishStage			= 35
		, FinishStageByOther	= 36
		
		// win 결과 처리
		, Win = 40	                // 성공 결과 처리중
		, Lose = 41                 // 실패 결과 처리중
		, RewardCompleted	= 42	// 결과 처리 완료
		, StageCompleted = 43       // Stage 완료 처리 
		
		, FinishDungeon		= 50
		
		, OnChallengeStart = 60     // 도전 시작
		, OnChallengeFinish 		// 도전 시작
		
		// 던전 정리
		, Destroy
	}
	
    public enum RedisRequestType
    {
        None = 0
        , RoomCoordinateInfo
        , CharaterInfo
        , RoomInfo
        , RoomStatus
        , RoomDeckInfo
        , PartyLogAll
        , PartyLogList
        , DungeonAuth
        , WorldBossCurHP
        , WorldBossMaxHP
        , WorldBossDamage
        
        , GetRiftInfo
        
        , EnemyInfo // for arena
    }

	
	public enum TickEventType
	{
		None = 0
		, UnitAction
		, CastingAction
		, EffectTick
		, ChannelingTick
	}

	public enum UnitState
	{
		None
		, Idle
		, Exploration
		, Moving
		, Action
		, Actioning
		, Casting
		, Channeling  
		
		, MezStun
		, MezFortify
		, MezExausted
		, MezSilence
		, MezSleep
		
		, Dead
		, Destroyed
	}

	public enum UnitStateEvent
	{
		None
		, OnEnter
		, OnExit
	}

	public enum UnitTrigger
	{
		StateDurationOver
		, OnExploration
		, OnEnterCombat
		, OnFinishCombat
		
		, OnCancelCasting
		
		, OnMez
		, OnMezEnd
		
		, OnDead
		, OnRevive
		, ClosedToTarget
	}
	
	[System.Flags]
	public enum UnitCombatState
	{
		Ready = 0x0
//		, Confuse = 0x1
//		, DisableAttack = 0x2
		, Silence = 0x4
//		, Blind = 0x8
		, Stun = 0x10
		, Sleep = 0x20
//		, Banish = 0x40
//		, Disappear = 0x80
		, Provoke = 0x100
		, Dead = 0x8000
	}

	public enum ActorSkillAnimationType
	{
		SingleTargetBasic = 0
		, SingleTargetSpecial
		, MultiTargetBasic
		, MultiTargetSpecial
	}

	public enum ItemType
	{
	}

	public enum TauntType
	{
		Target		= 10
		, Caster	= 20
	}
	
	public enum SctMessageType
	{
		None = 0
		, Info
		, Damage
		, Heal
		, DamageCritical
		, HealCritical
		, Absorb
		, Dead
		, DotDamage
		, Resist
		, Immune
		, Stun
		, Sleep
		, Silence
		, Fortify
		, Exausted
		, Effect
		, Shield
		, Interrupt
		, StunOver
		, SleepOver
		, EffectOver
		, Taunt
		, ActiveSkill
		, Gold
		, Exp
		, Level
		, Miss = 100
	}

	[Flags]
	public enum ChattingType
	{
		None = 0
		, General = 1 << 0// 일반 채널
		, League = 1 << 1 // 리그 채널
		, Party = 1 << 2
	}
	
	public enum ChatMessageType
	{
		None = 0
		, Chat = 1
		, Notice = 10
		, Inspection
		, Alert 
		, Event
		, Caution
	}

	public enum CombatLogNodeType
	{
		None = 0
		
		// Dungeon
		, Dungeon = 10
		, Stage = 11
		, Tick = 12
		, Moving = 13
		, Exploration =14
		, Pause = 15
		
		// UI
		, PartyMemberDungeonEnter = 50
		, CumulativeDamage = 51
		
		// action
		, SkillAction = 100
		, SkillEvent   = 110
		, SkillPassive  = 120
		, SkillInvokeAction  = 130
		, SkillActionCancelCasting = 140
		, SkillActionCompleteCasting = 150
		, SkillActionChannelingTick = 160
		
		, RelicSkillAction = 180
		
		// active skillcomp 
		, Damage = 200
		, Heal   = 210
		, Dispel = 220
		, Interrupt = 230
		, AddMana = 240
		, RestoreMana = 241
		, ConsumeMana = 242
		, AbsorbMana = 243
		, RestoreNextMana = 244
		, DecreaseManaCost = 245
		
		, ChangeCurHPSelf = 250
		
		, Summon = 280
		
		// continuoues
		, AddContinuous = 300
		, RemoveContinuous = 310
		, AbsorbContinuous = 320
		, ChangeContinuousDuration = 350
		
		// passive
		, PassiveSkillComp = 400
		, AbsorbDamage = 410
		, MezPassive = 420
		, SaveFromDeath = 430
		, TransferDamage = 450
		, ConvertToTrueDamage = 451
		, Vampiric			= 452
		, ReflectDamage     = 453
		, BarrierPassive = 460
		
		// Unit
		, Die			= 500
	}

	public enum SkillDescType
	{
		None = 0
		, Stat_AttackPower = 10
		, Stat_Defence = 11
		
		, Skill_CastingTime = 20
		
		, Active_ApFactor = 30
		, Active_BaseAmount = 31
		, Active_SkillAmount = 32 
		
		, Passive_BasePercent = 50
		, Passive_BaseAbsolute = 51
		, Passive_Barrier_Params_AbsorbFactor = 60
		, Passive_Barrier_SkillAmount = 61
		
		, Continuous_CoolTime = 80
		, Continuous_MaxStack = 81
		, Continuous_InitStack = 82
		, Continuous_Duration = 83
		, Continuous_Tick = 84
	}
	
	public enum SkillCompResultType
	{
		None = 0
		, Deal
		, Heal
		, Buff
		, Debuff
		
		, Continuous
	}
}