using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FindContainer
{
    /// <summary>
    /// The container packing service.
    /// </summary>
    public static class PackingService
    {
        /// <summary>
        /// Attempts to pack the specified containers with the specified items using the specified algorithms.
        /// </summary>
        /// <param name="containers">The list of containers to pack.</param>
        /// <param name="itemsToPack">The items to pack.</param>
        /// <returns>A container packing result with lists of the packed and unpacked items.</returns>
        public static List<ContainerPackingResult> Pack(List<Container> containers, List<Item> itemsToPack)
        {
            Object sync = new Object();
            List<ContainerPackingResult> result = new List<ContainerPackingResult>();

            Parallel.ForEach(containers, container =>
            {
                ContainerPackingResult containerPackingResult = new ContainerPackingResult();
                containerPackingResult.ContainerId = container.Id;

                // Clone the item list so the parallel updates don't interfere with each other.
                List<Item> items = new List<Item>();
                itemsToPack.ForEach(itp =>
                {
                    items.Add(new Item(itp.Id, itp.Dim1, itp.Dim2, itp.Dim3, itp.Quantity));
                });

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                EbAfit ebAfit = new EbAfit();
                AlgorithmPackingResult algorithmResult = ebAfit.RunPackingAlgorithm(container, items);
                stopwatch.Stop();

                algorithmResult.PackTimeInMilliseconds = stopwatch.ElapsedMilliseconds;

                decimal containerVolume = container.Length * container.Width * container.Height;
                decimal volumePacked = 0;

                algorithmResult.PackedItems.ForEach(pi =>
                {
                    volumePacked += pi.Volume;
                });

                algorithmResult.PercentContainerVolumePacked = (int)Math.Ceiling(volumePacked / containerVolume * 100);

                containerPackingResult.AlgorithmPackingResults.Add(algorithmResult);

                lock (sync)
                {
                    result.Add(containerPackingResult);
                }
            });

            return result;
        }
    }
}
