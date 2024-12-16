namespace Gateway.Models
{
    public class AddBook
    {
        //nogle bøger har ét ISBN nummer, nogle har både ISBN-10 og ISBN-13
        public required string ISBN { get; set; }
        // ikke required, fordi nogle bøger har kun ét ISBN nummer
        public string? ISBN_secondary { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        //User / Seller... noget i den stil
        public required string Seller_Name { get; set; }

        // uint fordi der ikke er negative priser
        public required uint Price { get; set; }
    }
}
