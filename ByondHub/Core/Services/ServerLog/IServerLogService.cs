using System;
using System.IO;

namespace ByondHub.Core.Services.ServerLog
{
    public interface IServerLogService
    {
        FileStream GetSaylog(DateTime date);
        FileStream GetRuntimeLog(DateTime? date = null);
    }
}