using UnityEngine;

namespace Renkai.Kurokage
{
    internal static class KurokageUiFont
    {
        private static Font cached;

        public static Font Default
        {
            get
            {
                if (cached != null) return cached;

#if UNITY_6000_0_OR_NEWER
                cached = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
                cached = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
                return cached;
            }
        }
    }
}
