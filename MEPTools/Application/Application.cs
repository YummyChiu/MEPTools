using Autodesk.Revit.UI;
using MEPTools.Bend;
using MEPTools.Elbow;
using MEPTools.FireHyrantLink;
using MEPTools.Link;
using MEPTools.SuperLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPTools.Application
{
    class Application : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel ribbonPanel = application.GetRibbonPanels().FirstOrDefault(RP => RP.Name == "管综");
            if (ribbonPanel == null)
                ribbonPanel = application.CreateRibbonPanel("管综");
            ribbonPanel.AddItem(new PushButtonData("OneStepBend", "\n\n一键\n翻弯", typeof(Application).Assembly.Location, typeof(BendCommand).FullName));
            ribbonPanel.AddItem(new PushButtonData("OneStepLink", "\n\n一键\n连接", typeof(Application).Assembly.Location, typeof(LinkCommand).FullName));
            ribbonPanel.AddItem(new PushButtonData("OneStepFireHydrantLink", "\n\n连接\n消火栓", typeof(Application).Assembly.Location, typeof(FireHyrantLinkCommand).FullName));
            ribbonPanel.AddItem(new PushButtonData("SmartLift", "\n\n智能\n提拉", typeof(Application).Assembly.Location, typeof(SuperLinkCommand).FullName));
            ribbonPanel.AddItem(new PushButtonData("OnestepElbow", "\n\n排水\n倒角", typeof(Application).Assembly.Location, typeof(ElbowCommand).FullName));

            return Result.Succeeded;
        }
    }
}
