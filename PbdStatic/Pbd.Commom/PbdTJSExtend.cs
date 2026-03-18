
namespace Pbd.Commom
{
    /// <summary>
    /// TJS扩展
    /// </summary>
    internal static class PbdTJSExtend
    {
        public static long ToInt64(this TJSVariant v)
        {
            return v.Type switch
            {
                TJSVariantType.Void => 0L,
                TJSVariantType.String => long.Parse(v.AsString()),
                TJSVariantType.Integer => v.AsInteger(),
                TJSVariantType.Real => (long)v.AsReal(),
                _ => throw new TJSVariantException(v.Type, TJSVariantType.Integer),
            };
        }
        public static ulong ToUInt64(this TJSVariant v)
        {
            return (ulong)v.ToInt64();
        }
        public static int ToInt32(this TJSVariant v)
        {
            return (int)v.ToInt64();
        }
        public static uint ToUInt32(this TJSVariant v)
        {
            return (uint)v.ToInt64();
        }
    }
}
