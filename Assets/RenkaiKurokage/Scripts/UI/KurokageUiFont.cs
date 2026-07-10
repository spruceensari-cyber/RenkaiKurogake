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
                if (cached == null)
                    cached = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return cached;
            }
        }
    }
}
