# GameLogic Architecture Diagram

## 1. 클래스 계층 구조 (Class Hierarchy)

```mermaid
classDiagram
    class CorgiObject {
        +string ObjectId
        +ulong Uid
        +bool IsLoaded
        +Load(ulong)
        +Load(CorgiSharedObject)
        +OnDestroy()
        +OnEvent(CombatEventType)
        +Serialize(IPacketWriter)
    }
    
    class ITickable {
        <<interface>>
        +TickInCombat(ulong, LogNode)
    }
    
    class CorgiCombatObject {
        +TickInCombat(ulong, LogNode)*
        #Tick(ulong, TickLogNode)*
    }
    
    class Unit {
        +Dungeon Dungeon
        +List~SkillActive~ ActiveSkills
        +List~SkillPassive~ PassiveSkills
        +SkillAttack Attack
        +UnitStat Stats
        +UnitState UnitState
        +CorgiFSM FSM
        +DoAction(Unit)
        +ApplyDamage(long)
        +ApplyHeal(long)
        +OnEnterCombat()
        +OnUpdateStat()
        +OnUpdateEffect()
    }
    
    class Character {
        +CharacterInfoSpec Spec
        +List~Equip~ Equips
        +List~SkillPassive~ TalentSkills
        +List~SkillActive~ RelicSkills
        +LoadCombatSetting(Deck)
        +OnEnterCombat()
    }
    
    class Monster {
        +MonsterInfoSpec Spec
        +MonsterGrade Grade
        +List~SkillActive~ InstantSkills
        +SetLevel(uint)
    }
    
    class Skill {
        +Unit Owner
        +SkillInfoSpec Spec
        +uint Level
        +SkillType SkillType
        +ulong SkillUid
        +DoSkill(Unit)
        +OnEvent(CombatEventType)
    }
    
    class SkillActive {
        +bool IsCasting
        +bool IsChanneling
        +ulong CastingTime
        +DoSkill(Unit)
    }
    
    class SkillPassive {
        +OnEvent(CombatEventType)
    }
    
    class SkillAttack {
        +DoSkill(Unit)
    }
    
    class SkillComp {
        <<abstract>>
        +Unit Owner
        +ISkillActor ParentActor
        +GetName()*
        +Apply(Unit, Unit)*
    }
    
    class ActiveSkillComp {
        +ApplyDamage(Unit, Unit)
        +ApplyHeal(Unit, Unit)
    }
    
    class PassiveSkillComp {
        +GetPassiveComp(PassiveSkillCompType)
    }
    
    class Dungeon {
        +DungeonType DungeonType
        +DungeonState State
        +List~Unit~ CharList
        +List~Unit~ MonsterList
        +Stage CurStage
        +TickInCombat(ulong, LogNode)
        +OnAction(CombatLogNode)
        +OnDeadCompletely(Unit)
    }
    
    class Stage {
        +List~Unit~ MonsterList
        +OnEnterCombat()
        +OnFinishCombat()
    }
    
    class CorgiFSM {
        +UnitState CurState
        +RegisterState(UnitState, FSMUnitState)
        +RegisterTrigger(UnitTrigger, UnitState)
        +Trigger(UnitTrigger, CombatLogNode)
        +TickInCombat(ulong, LogNode)
    }
    
    class EventManager {
        +Register(CombatEventType, OnEventDelegate)
        +OnEvent(CombatEventType, EventParam, CombatLogNode)
    }
    
    class ObjectManager {
        +GetId(object) string
    }
    
    CorgiObject <|-- CorgiCombatObject
    ITickable <|.. CorgiCombatObject
    CorgiCombatObject <|-- Unit
    CorgiCombatObject <|-- Skill
    CorgiCombatObject <|-- SkillComp
    Unit <|-- Character
    Unit <|-- Monster
    Skill <|-- SkillActive
    Skill <|-- SkillPassive
    Skill <|-- SkillAttack
    SkillComp <|-- ActiveSkillComp
    SkillComp <|-- PassiveSkillComp
    CorgiObject <|-- Dungeon
    ITickable <|.. Dungeon
    CorgiObject <|-- Stage
    Unit --> CorgiFSM
    CorgiObject --> EventManager
    Unit --> Skill : owns
    Skill --> SkillComp : contains
    Dungeon --> Stage : contains
    Dungeon --> Unit : manages
    Stage --> Unit : contains
```

## 2. 시스템 모듈 구조 (System Module Architecture)

```mermaid
graph TB
    subgraph "Core Layer"
        CO[CorgiObject<br/>기본 객체]
        CCO[CorgiCombatObject<br/>전투 객체]
        IT[ITickable<br/>틱 처리 인터페이스]
    end
    
    subgraph "Instance Layer"
        D[Dungeon<br/>던전 관리]
        S[Stage<br/>스테이지]
        U[Unit<br/>유닛 기본]
        CH[Character<br/>캐릭터]
        M[Monster<br/>몬스터]
        EQ[Equip<br/>장비]
        NPC[NPC]
    end
    
    subgraph "Action Layer"
        SK[Skill<br/>스킬 기본]
        SA[SkillActive<br/>액티브 스킬]
        SP[SkillPassive<br/>패시브 스킬]
        SKA[SkillAttack<br/>공격 스킬]
        SKR[SkillRelic<br/>유물 스킬]
    end
    
    subgraph "SkillComp Layer"
        SC[SkillComp<br/>스킬 컴포넌트]
        ASC[ActiveSkillComp<br/>액티브 컴포넌트]
        PSC[PassiveSkillComp<br/>패시브 컴포넌트]
        CC[ConditionComp<br/>조건 컴포넌트]
        EC[EffectComp<br/>효과 컴포넌트]
        TC[TargetComp<br/>타겟 컴포넌트]
    end
    
    subgraph "FSM Layer"
        FSM[CorgiFSM<br/>상태 머신]
        FSMU[FSMUnitState<br/>상태 구현]
    end
    
    subgraph "Manager Layer"
        OM[ObjectManager<br/>객체 ID 관리]
        EM[EventManager<br/>이벤트 관리]
        SM[StageManager<br/>스테이지 관리]
    end
    
    subgraph "Log Layer"
        LN[LogNode<br/>로그 노드]
        CLN[CombatLogNode<br/>전투 로그]
        TLN[TickLogNode<br/>틱 로그]
    end
    
    subgraph "SharedInstance Layer"
        CSO[CorgiSharedObject<br/>공유 객체]
        SD[SharedDungeon<br/>공유 던전]
        SC[SharedCharacter<br/>공유 캐릭터]
    end
    
    CO --> CCO
    IT -.-> CCO
    IT -.-> D
    
    CCO --> U
    CCO --> SK
    CCO --> SC
    
    U --> CH
    U --> M
    U --> NPC
    U --> FSM
    
    SK --> SA
    SK --> SP
    SK --> SKA
    SK --> SKR
    
    SC --> ASC
    SC --> PSC
    SC --> CC
    SC --> EC
    SC --> TC
    
    D --> S
    D --> U
    S --> U
    
    CH --> EQ
    U --> SK
    
    FSM --> FSMU
    
    CO --> EM
    CO --> OM
    
    D --> LN
    U --> CLN
    CLN --> TLN
    
    CO --> CSO
    CSO --> SD
    CSO --> SC
```

## 3. 전투 시스템 플로우 (Combat System Flow)

```mermaid
sequenceDiagram
    participant D as Dungeon
    participant S as Stage
    participant U as Unit
    participant FSM as CorgiFSM
    participant SK as Skill
    participant SC as SkillComp
    participant EM as EventManager
    participant LN as LogNode
    
    D->>S: OnEnterStage()
    S->>U: OnEnterCombat()
    U->>FSM: Trigger(OnEnterCombat)
    FSM->>U: ChangeState(Action)
    
    loop Combat Loop
        D->>D: TickInCombat(deltaTime)
        D->>S: TickInCombat(deltaTime)
        S->>U: TickInCombat(deltaTime)
        U->>FSM: TickInCombat(deltaTime)
        
        FSM->>U: DoAction(target)
        U->>SK: DoSkill(target)
        SK->>SC: Apply(caster, target)
        SC->>U: ApplyDamage(damage)
        SC->>EM: OnEvent(CombatEventType)
        EM->>U: OnEvent(CombatEventType)
        SC->>LN: CreateLogNode()
        LN->>D: AddChild(logNode)
        
        alt Unit Dead
            U->>EM: OnEvent(OnDead)
            EM->>D: OnDeadCompletely(unit)
        end
    end
```

## 4. 스킬 시스템 구조 (Skill System Architecture)

```mermaid
graph LR
    subgraph "Skill Types"
        SA[SkillActive<br/>액티브 스킬]
        SP[SkillPassive<br/>패시브 스킬]
        SKA[SkillAttack<br/>공격 스킬]
        SKR[SkillRelic<br/>유물 스킬]
        SKC[SkillCasting<br/>캐스팅 스킬]
        SKCH[SkillChanneling<br/>채널링 스킬]
    end
    
    subgraph "Skill Components"
        ASC[ActiveSkillComp<br/>- Damage<br/>- Heal<br/>- Dispel<br/>- Interrupt]
        PSC[PassiveSkillComp<br/>- Stat<br/>- Enhance<br/>- Barrier<br/>- Immune]
        CC[ConditionComp<br/>- HP Check<br/>- Skill Type<br/>- Buff Check]
        TC[TargetComp<br/>- Self<br/>- Friend<br/>- Enemy]
        EC[EffectComp<br/>- Continuous<br/>- Aura]
        FC[FeatureComp<br/>- Amplify<br/>- Force<br/>- Ignore]
    end
    
    subgraph "Skill Processing"
        ST[Select Target]
        CC --> ST
        TC --> ST
        ST --> ASC
        FC --> ASC
        ASC --> EC
        EC --> PSC
    end
    
    SA --> ASC
    SA --> CC
    SA --> TC
    SP --> PSC
    SKA --> ASC
```

## 5. 데이터 흐름도 (Data Flow Diagram)

```mermaid
flowchart TD
    Start([게임 시작]) --> Load[데이터 로드]
    
    Load --> SharedData[SharedInstance<br/>- SharedCharacter<br/>- SharedDungeon<br/>- SharedStage]
    Load --> GameData[GameDataManager<br/>- CharacterInfoSpec<br/>- SkillInfoSpec<br/>- MonsterInfoSpec]
    
    SharedData --> CreateDungeon[던전 생성]
    GameData --> CreateDungeon
    
    CreateDungeon --> CreateUnit[유닛 생성]
    CreateUnit --> CreateCharacter[캐릭터 생성]
    CreateUnit --> CreateMonster[몬스터 생성]
    
    CreateCharacter --> LoadEquip[장비 로드]
    CreateCharacter --> LoadSkill[스킬 로드]
    CreateMonster --> LoadSkill
    
    LoadSkill --> CreateSkillComp[스킬 컴포넌트 생성]
    CreateSkillComp --> RegisterEvent[이벤트 등록]
    
    CreateDungeon --> StartCombat[전투 시작]
    StartCombat --> CombatLoop[전투 루프]
    
    CombatLoop --> Tick[틱 처리]
    Tick --> UpdateUnit[유닛 업데이트]
    Tick --> UpdateSkill[스킬 업데이트]
    Tick --> UpdateEffect[효과 업데이트]
    
    UpdateUnit --> FSMUpdate[FSM 상태 업데이트]
    UpdateSkill --> ApplySkill[스킬 적용]
    ApplySkill --> GenerateLog[로그 생성]
    
    GenerateLog --> LogNode[LogNode 생성]
    LogNode --> CombatLog[CombatLog 저장]
    
    CombatLoop --> CheckWin{승리 체크}
    CheckWin -->|승리| Win[승리 처리]
    CheckWin -->|패배| Lose[패배 처리]
    CheckWin -->|진행| CombatLoop
    
    Win --> Reward[보상 처리]
    Lose --> End([게임 종료])
    Reward --> End
```

## 6. 이벤트 시스템 구조 (Event System Architecture)

```mermaid
graph TD
    subgraph "Event Categories"
        ER[Rule Events<br/>- OnEnterDungeon<br/>- OnFinishStage]
        EU[Unit Events<br/>- OnDead<br/>- OnEnterCombat<br/>- OnKill]
        EA[Action Events<br/>- OnSkill<br/>- OnAttack<br/>- OnCastingStart]
        EE[Effect Events<br/>- OnSkillEffectTick<br/>- OnSkillEffectEnter]
    end
    
    subgraph "Event Flow"
        ET[Event Trigger]
        EM[EventManager]
        HL[Event Handler List]
        EH[Event Handler]
    end
    
    subgraph "Event Targets"
        D[Dungeon]
        U[Unit]
        SK[Skill]
        SC[SkillComp]
        SE[SkillEffectInst]
    end
    
    ER --> ET
    EU --> ET
    EA --> ET
    EE --> ET
    
    ET --> EM
    EM --> HL
    HL --> EH
    
    EH --> D
    EH --> U
    EH --> SK
    EH --> SC
    EH --> SE
    
    D --> ET
    U --> ET
    SK --> ET
    SC --> ET
    SE --> ET
```

## 7. 패키지/네임스페이스 구조 (Package Structure)

```mermaid
graph TD
    subgraph "IdleCs.GameLogic"
        A[Action<br/>- Skill<br/>- SkillActive<br/>- SkillPassive]
        I[Instance<br/>- Unit<br/>- Character<br/>- Monster<br/>- Dungeon<br/>- Stage]
        SC[SkillComp<br/>- Base<br/>- Active<br/>- Passive<br/>- Condition<br/>- Effect<br/>- Target]
        FSM[FSM<br/>- CorgiFSM<br/>- FSMUnitState]
        MGR[Manager<br/>- ObjectManager<br/>- EventManager<br/>- StageManager]
        LOG[Log<br/>- LogNode<br/>- CombatLogNode<br/>- SkillCompLogNode]
        SHARED[SharedInstance<br/>- CorgiSharedObject<br/>- SharedDungeon<br/>- SharedCharacter]
        COMMON[Common<br/>- EventParam<br/>- PartyBuff]
    end
    
    A --> I
    SC --> A
    FSM --> I
    MGR --> I
    LOG --> I
    LOG --> A
    LOG --> SC
    SHARED --> I
    COMMON --> I
```

## 주요 아키텍처 특징

### 1. 객체 계층 구조
- **CorgiObject**: 모든 게임 객체의 기본 클래스 (ID 관리, 직렬화)
- **CorgiCombatObject**: 전투 관련 객체 (틱 처리)
- **Unit**: 전투 유닛 (캐릭터, 몬스터의 기본 클래스)

### 2. 컴포넌트 시스템
- **SkillComp**: 스킬을 모듈화된 컴포넌트로 구성
  - ActiveSkillComp: 데미지, 힐 등 즉시 효과
  - PassiveSkillComp: 스탯, 방어막 등 지속 효과
  - ConditionComp: 조건 체크
  - TargetComp: 타겟 선택
  - EffectComp: 지속 효과 관리

### 3. 상태 머신 (FSM)
- **CorgiFSM**: 유닛의 상태를 관리 (Idle, Moving, Action, Casting, Dead 등)

### 4. 이벤트 시스템
- **EventManager**: 이벤트 기반 프로그래밍으로 느슨한 결합 구현
- 다양한 CombatEventType으로 전투 이벤트 처리

### 5. 로그 시스템
- 계층적 로그 구조 (LogNode -> CombatLogNode -> SkillCompLogNode)
- 모든 전투 액션을 로그로 기록하여 재현 가능

### 6. 틱 기반 시스템
- **ITickable**: 모든 전투 객체가 틱 단위로 업데이트
- Dungeon -> Stage -> Unit -> Skill 순으로 틱 전파
