
using System.Collections;
using System.Collections.Generic;
using Corgi.GameData;


namespace IdleCs.GameLogic
{
	/// <summary>
	/// skillComponent를 실행하는 parent
	/// skill / skillEffect / skillcomp 
	/// </summary>
	public interface ISkillActor
	{
		ulong SkillBaseUid { get; }
		ulong SkillUid { get; }
		
		SkillType SkillType { get; }
		SkillActorType SkillActorType { get; }
		SkillAttributeType AttributeType { get; }
		
		uint Level { get; }
		uint StackCount { get; }
		uint Mastery { get; }
	}
}