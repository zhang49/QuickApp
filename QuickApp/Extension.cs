using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractGenerateTools
{
    public static class Extension
    {

        /// <summary>
        /// 检查val 是否位于[baseVal - distance,baseVal + distance]区间内
        /// </summary>
        /// <param name="val"></param>
        /// <param name="baseVal"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool IsInDeviation(this int val, int baseVal, int distance)
        {
            if (val >= baseVal - distance && val <= baseVal + distance)
            {
                return true;
            }
            return false;
        }
        public static bool IsInRange(this int val, int low, int up)
        {
            if (val >= low && val <= up)
            {
                return true;
            }
            return false;
        }
        public static void Loop(this int targetVal, Action<int> action)
        {
            for (int i = 0; i < targetVal; i++)
            {
                action(i);
            }
        }
    }
}
