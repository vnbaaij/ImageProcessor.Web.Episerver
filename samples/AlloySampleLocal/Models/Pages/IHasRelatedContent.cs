using EPiServer.Core;

namespace AlloySample.Models.Pages
{
    public interface IHasRelatedContent
    {
        ContentArea RelatedContentArea { get; }
    }
}
