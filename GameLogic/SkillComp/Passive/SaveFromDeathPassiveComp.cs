using System;
using Corgi.GameData;
using IdleCs.GameLog;


namespace IdleCs.GameLogic
{
	public class SaveFromDeathPassiveComp : PassiveSkillComp
    {
	    /// <summary>
	    /// Setting
	    /// </summary>
	     
	    private float _HPfactor;

		public SaveFromDeathPassiveComp()
			: base()
		{
	        PassiveType = PassiveSkillCompType.SaveFromDeath;
			EventManager.Register(CombatEventType.OnDead, OnDead);
		}

		protected override bool LoadInternal(ulong uid)
	    {
			if (base.LoadInternal(uid) == false)
			{
				return false;
			}

			var spec = GetSpec();
			if (spec == null)
			{
				return false;
			}

		    _HPfactor = spec.BasePercent;

		    return true;
	    }

		bool OnDead(EventParam eventParam, CombatLogNode logNode)
		{
			if (Parent == null || Parent.Target == null)
			{
				return false;
			}

			var target = Parent.Target;
			
			if(target.CurHP == 0)
			{
				long healValue = 0;
				if (_HPfactor > 1)
				{
					healValue = target.MaxHP;
				}
				else
				{
					 healValue = (int) (_HPfactor * (float) target.MaxHP);
				}

                target.ResetHP(healValue);

				var passiveLogNode = new SaveFromDeathPassiveSkillCompLogNode(target, target, this);
                logNode.AddChild(passiveLogNode);

				return true;
			}

			return false;
		}

	}
}