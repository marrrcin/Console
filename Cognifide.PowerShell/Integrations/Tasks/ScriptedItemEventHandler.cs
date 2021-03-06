﻿using System;
using Cognifide.PowerShell.Core.Host;
using Cognifide.PowerShell.Core.Modules;
using Cognifide.PowerShell.Core.Settings;
using Sitecore.Data.Items;
using Sitecore.Events;

namespace Cognifide.PowerShell.Integrations.Tasks
{
    public class ScriptedItemEventHandler
    {
        public void OnEvent(object sender, EventArgs args)
        {
            var eventArgs = args as SitecoreEventArgs;
            if (eventArgs == null)
            {
                return;
            }
            var item = eventArgs.Parameters[0] as Item;
            var eventName = eventArgs.EventName.Replace(':', '/');

            foreach (var root in ModuleManager.GetFeatureRoots(IntegrationPoints.EventHandlersFeature))
            {
                var libraryItem = root.Paths.GetSubItem(eventName);

                if (libraryItem == null)
                {
                    return;
                }
                using (var session = ScriptSessionManager.NewSession(ApplicationNames.Default, true))
                {
                    foreach (Item scriptItem in libraryItem.Children)
                    {
                        if (item != null)
                        {
                            session.SetItemLocationContext(item);
                        }
                        session.SetExecutedScript(scriptItem);
                        session.SetVariable("eventArgs", eventArgs);
                        var script = scriptItem[ScriptItemFieldNames.Script];
                        if (!String.IsNullOrEmpty(script))
                        {
                            session.ExecuteScriptPart(script);
                        }
                    }
                }
            }
        }
    }
}