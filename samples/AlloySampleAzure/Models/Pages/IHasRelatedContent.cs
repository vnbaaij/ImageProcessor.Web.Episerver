using EPiServer.Core;

namespace AlloySampleAzure.Models.Pages
{
    public interface IHasRelatedContent
    {
        ContentArea RelatedContentArea { get; }
    }
}
