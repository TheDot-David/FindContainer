using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FindContainer
{
    public class AlgorithmPackingResult
    {
        public AlgorithmPackingResult()
        {
            this.PackedItems = new List<Item>();
            this.UnpackedItems = new List<Item>();
        }

        public List<Item> PackedItems { get; set; }

        public int PackedItemCount { get; set; }

        public List<Item> UnpackedItems { get; set; }

        public long PackTimeInMilliseconds { get; set; }

        public decimal PercentContainerVolumePacked { get; set; }

        public bool IsCompletePack { get; set; }
    }
}
