
# Combat Logic
## 개요
크로매틱소울 AFK Raid의 전투 로직의 일부를 발췌
코드 스타일을 보여주는 용도

## 스킬 시스템 구조 (Skill System Architecture)

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
