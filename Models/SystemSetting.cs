namespace MuseumSystem.Models
{
    public class SystemSetting
    {
        public int Id { get; set; }
        public string Key { get; set; } = "TicketPrice";
        public decimal Value { get; set; } = 25.00m;
    }
}