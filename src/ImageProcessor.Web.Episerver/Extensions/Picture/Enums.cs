using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO: namespace should be ImageProcessor.Web.Episerver.Picture, but wait for other breaking changes(?)
namespace ImageProcessor.Web.Episerver
{
	public enum LazyLoadType
	{
		None,
		Custom,
        CustomProgressive,
		[Obsolete("Use \"Custom\" instead.")]
		Regular = Custom,
		[Obsolete("Use \"CustomProgressive\" instead.")]
        Progressive = CustomProgressive,
		Native,
		Hybrid
	}
}

namespace ImageProcessor.Web.Episerver.Picture
{
    public enum ImageDecoding
    {
        Async,
        Sync,
        Auto,
        None
    }
}


