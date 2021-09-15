using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unattended.PLC
{
    public class PLCTool : ABPLCTool
    {
        private static readonly Lazy<PLCTool> lazy = new Lazy<PLCTool>(() => new PLCTool());

        /// <summary>
        /// ABPLC工具类单例
        /// </summary>
        public static PLCTool SiemensPLCToolInstance { get { return lazy.Value; } }
    }
}
