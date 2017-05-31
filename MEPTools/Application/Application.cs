using Autodesk.Revit.UI;
using MEPTools.Link;
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
            ribbonPanel.AddItem(new PushButtonData("OneStepLink", "\n\n一键\n连接", typeof(Application).Assembly.Location, typeof(LinkCommand).FullName));
            return Result.Succeeded;
        }
    }
}
