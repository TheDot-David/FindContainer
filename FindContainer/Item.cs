using System.Runtime.Serialization;

namespace FindContainer
{
    /// <summary>
    /// An item to be packed. Also used to hold post-packing details for the item.
    /// </summary>
    public class Item
    {
        private readonly decimal _volume;

        /// <summary>
        /// Initializes a new instance of the Item class.
        /// </summary>
        /// <param name="id">The item ID.</param>
        /// <param name="dim1">The length of one of the three item dimensions.</param>
        /// <param name="dim2">The length of another of the three item dimensions.</param>
        /// <param name="dim3">The length of the other of the three item dimensions.</param>
        /// <param name="quantity">The item quantity.</param>
        public Item(int id, decimal dim1, decimal dim2, decimal dim3, int quantity)
        {
            this.Id = id;
            this.Dim1 = dim1;
            this.Dim2 = dim2;
            this.Dim3 = dim3;
            this._volume = dim1 * dim2 * dim3;
            this.Quantity = quantity;
        }

        public int Id { get; set; }

        public bool IsPacked { get; set; }

        public decimal Dim1 { get; set; }   // The item 1 dimension

        public decimal Dim2 { get; set; }   // The item 2 dimension

        public decimal Dim3 { get; set; }   // The item 3 dimension

        // x coordinate of the location of the packed item within the container
        public decimal CoordX { get; set; }

        // y coordinate of the location of the packed item within the container
        public decimal CoordY { get; set; }

        // z coordinate of the location of the packed item within the container
        public decimal CoordZ { get; set; }

        // quantity of the items
        public int Quantity { get; set; }

        // x dimension of the orientation of the item as it has been packed
        public decimal PackDimX { get; set; }

        // y dimension of the orientation of the item as it has been packed
        public decimal PackDimY { get; set; }

        // z dimension of the orientation of the item as it has been packed
        public decimal PackDimZ { get; set; }

        public decimal Volume
        {
            get
            {
                return _volume;
            }
        }
    }
}