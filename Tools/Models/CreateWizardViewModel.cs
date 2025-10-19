using System.Collections.Generic;
using Tools.Models.Core;

namespace Tools.Models;

public class CreateWizardViewModel
{
    public List<WebsiteFeature> AllFeatures { get; set; } = new();
}

public record FeatureTreeViewModel(WebsiteFeature Feature, IReadOnlyList<WebsiteFeature> AllFeatures, int Depth);
