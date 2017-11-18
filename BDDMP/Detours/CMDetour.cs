using BDArmory.CounterMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDDMP.Detours
{
    static class CMFlareExtensions
    {
        private static Dictionary<int, bool> isRMFlare = new Dictionary<int, bool>();

        public static void setRMState(this CMFlare thi, bool isRM = true)
        {
            if (isRMFlare.ContainsKey(thi.gameObject.GetInstanceID()))
            {
                isRMFlare.Remove(thi.gameObject.GetInstanceID());
            }
            isRMFlare.Add(thi.gameObject.GetInstanceID(), isRM);
        }

        public static bool getRMState(this CMFlare thi)
        {
            if (!isRMFlare.ContainsKey(thi.gameObject.GetInstanceID())) {
                return false;
            } else
            {
                bool ret;
                isRMFlare.TryGetValue(thi.gameObject.GetInstanceID(), out ret);
                return ret;
            }
        }
    }

    class CMDetour
    {
    }
}
