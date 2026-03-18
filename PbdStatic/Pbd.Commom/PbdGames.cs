using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Pbd.Commom
{
    /// <summary>
    /// 游戏参数
    /// </summary>
    public class PbdCustomParams
    {
        public virtual string Title { get; } = "默认";
        public virtual bool NoCheck { get; } = false;
        public virtual byte[] CustomIV { get; } = Array.Empty<byte>();

        public virtual string GetParamsString()
        {
            StringBuilder sb = new(1024);
            sb.Append($"[{nameof(NoCheck)}] {this.NoCheck}\r\n");
            sb.Append($"[{nameof(CustomIV)}] {string.Join(' ', this.CustomIV.ToList().ConvertAll(b => b.ToString("X2")))}\r\n");
            return sb.ToString();
        }
    }

    public class DataManager
    {
        private readonly static List<PbdCustomParams> smTitles;

        public static ReadOnlyCollection<PbdCustomParams> Titles => smTitles.AsReadOnly();

        static DataManager()
        {
            List<PbdCustomParams> games = new(32)
            {
                new PbdCustomParams(),
            };
            smTitles = games;
        }
    }
}
