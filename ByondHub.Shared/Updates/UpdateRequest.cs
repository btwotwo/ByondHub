using System;
using System.Collections.Generic;
using System.Text;
using ByondHub.Shared.Core;

namespace ByondHub.Shared.Updates
{
    public class UpdateRequest : AuthenticatedRequest
    {
        public string Branch { get; set; }
        public string CommitHash { get; set; }
    }
}
