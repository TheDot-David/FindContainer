using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FindContainer
{
    /// <summary>
    /// The container packing result.
    /// </summary>
    public class ContainerPackingResult
    {
        public ContainerPackingResult()
        {
            this.AlgorithmPackingResults = new List<AlgorithmPackingResult>();
        }

        public int ContainerId { get; set; }

        public List<AlgorithmPackingResult> AlgorithmPackingResults { get; set; }
    }
}
