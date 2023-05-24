using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiceImplementation.FBInstant.Sharing
{
    public class FBInstantSharing
    {
        public static void shareAsync(Dictionary<string, object> p, Action cb)
        {
            FBInstant.shareAsync(p, cb);
        }
    }
}