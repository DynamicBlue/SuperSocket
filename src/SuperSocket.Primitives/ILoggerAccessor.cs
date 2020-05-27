using Dynamic.Core.Log;
using System;
using System.Threading.Tasks;


namespace SuperSocket
{
    public interface ILoggerAccessor
    {
        ILogger Logger { get; }
    }
}