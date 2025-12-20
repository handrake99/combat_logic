using System.Collections.Generic;
using System.Diagnostics;
using Google;
using IdleCs.Utils;

namespace IdleCs.GameLogic
{
    public static class CorgiCombatLog
    {
        private static List<CombatLogCategory> _logTypes = new List<CombatLogCategory>();
        //-don't need call when operating client
        public static bool Initialize(CombatLogCategory[] logTypes)
        {
            _logTypes.AddRange(logTypes);
            return true;
        }

        [Conditional("CORGI_LOG")]
        public static void Log(CombatLogCategory logCategory, object msg)
        {
            if (_logTypes.Contains(logCategory) == false)
            {
                return;
            }

            CorgiLog.Log(CorgiLogType.Debug, CorgiString.Format("[{0}]{1}", logCategory, msg));
        }
        
        [Conditional("CORGI_LOG")]
        public static void Log(CombatLogCategory logCategory, string format, params object[] args)
        {
            if (_logTypes.Contains(logCategory) == false)
            {
                return;
            }

            var msg = CorgiString.Format(format, args);
            CorgiLog.Log(CorgiLogType.Debug, CorgiString.Format("[{0}]{1}", logCategory, msg));
        }
        
        public static void LogError(CombatLogCategory logCategory, object msg)
        {
            CorgiLog.Log(CorgiLogType.Error, CorgiString.Format("[{0}]{1}", logCategory, msg));
        }
        
        public static void LogError(CombatLogCategory logCategory, string format, params object[] args)
        {
            var msg = CorgiString.Format(format, args);
            CorgiLog.Log(CorgiLogType.Error, CorgiString.Format("[{0}]{1}", logCategory, msg));
        }
        
        public static void LogFatal(CombatLogCategory logCategory, object msg)
        {
            CorgiLog.Log(CorgiLogType.Fatal, CorgiString.Format("[{0}]{1}", logCategory, msg));
        }
        
        public static void LogFatal(CombatLogCategory logCategory, string format, params object[] args)
        {
            var msg = CorgiString.Format(format, args);
            CorgiLog.Log(CorgiLogType.Fatal, CorgiString.Format("[{0}]{1}", logCategory, msg));
        }
    }
}
