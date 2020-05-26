using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.ProtoBase.Config
{
    public enum IsolationMode
    {
        /// <summary>
        /// No isolation
        /// </summary>
        None,
        /// <summary>
        /// Isolation by AppDomain
        /// </summary>
        AppDomain,

        /// <summary>
        /// Isolation by process
        /// </summary>
        Process
    }
}
