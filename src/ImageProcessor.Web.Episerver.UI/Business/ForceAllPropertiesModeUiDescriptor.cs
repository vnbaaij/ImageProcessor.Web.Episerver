using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Shell;

namespace ImageProcessor.Web.Episerver.UI.Business
{
    [UIDescriptorRegistration]
    public class ForceAllPropertiesModeUiDescriptor : UIDescriptor<IForceAllPropertiesMode>
{
        public ForceAllPropertiesModeUiDescriptor()
        {
            DefaultView = CmsViewNames.AllPropertiesView;

            EnableStickyView = false;
            DisabledViews = new List<string> { CmsViewNames.OnPageEditView, CmsViewNames.PreviewView };
        }
    }
}
