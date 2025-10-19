using System.Collections.Generic;

namespace Tools.Models.Core;

public class WebsiteFeature
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? ParentFeatureId { get; set; }

    public virtual WebsiteFeature? ParentFeature { get; set; }

    public virtual ICollection<WebsiteFeature> ChildFeatures { get; set; } = new List<WebsiteFeature>();
}
