using System;

namespace EnvueClustering.ClusteringBase
{
    public interface IIdentifiable
    {
        Guid Id { get; set; }
    }
}