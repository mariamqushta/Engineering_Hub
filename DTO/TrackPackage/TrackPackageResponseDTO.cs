namespace Engineering_Hub.DTO.TrackPackage
{
    public class TrackPackageResponseDTO
    {
        public int Id { get; set; }

        public int TrackId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }
    }
}
