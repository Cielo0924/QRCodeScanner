namespace Dashboard.Models
{
    public class Dispenser
    {
        public string DispenserID { get; set; }
        public string Location { get; set; }
        public int UnitID { get; set; }
        public string? QRCodeURL { get; set; } 
        public bool IsActive { get; set; }
        public DateTime DateAdded { get; set; }

        public Unit Unit { get; set; }
    }

}
