//using System;
//
//using IdleCs.GameData;
//
//
//
//namespace IdleCs.GameLogic
//{
//    [System.Serializable]
//	public class ForceElementalRelationPassiveComp : PassiveSkillComp
//    {
//	    /// <summary>
//	    /// Setting
//	    /// </summary>
//	    private int _forceRelation = 0;
//
//	    public int ForceRelation
//	    {
//		    get { return _forceRelation; }
//	    }
//
//		public ForceElementalRelationPassiveComp()
//			: base()
//		{
//		}
//
//	    protected override bool Load(GameAsset asset)
//	    {
//		    var thisAsset = asset as ForceElementalRelationSkillCompAsset;
//
//		    if (thisAsset == null)
//		    {
//			    return false;
//		    }
//
//		    _forceRelation = thisAsset.ForceRelation;
//
//		    return true;
//	    }
//	}
//}
