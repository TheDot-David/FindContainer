namespace FindContainer
{
    /// <summary>
    /// The container to pack items into.
    /// </summary>
    public class Container
    {
        /// <summary>
        /// Initializes a new instance of the Container class.
        /// </summary>
        public Container(int id, decimal length, decimal width, decimal height)
        {
            this.Id = id;
            this.Length = length;
            this.Width = width;
            this.Height = height;
            this.Volume = length * width * height;
        }

        public int Id { get; set; }

        public decimal Length { get; set; }

        public decimal Width { get; set; }

        public decimal Height { get; set; }

        public decimal Volume { get; set; }
    }
}
