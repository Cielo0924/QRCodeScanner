namespace Dashboard.Models
{
    public class DashboardViewModel
    {
        public int TotalDispensers { get; set; }
        public int TotalLogs { get; set; }
        public int TotalRefills { get; set; }
        public int TotalChecks { get; set; }
        public int TotalReplacements { get; set; }

        public int ActiveDispensers { get; set; }
        public int MaintenanceDispensers { get; set; }
        public int TodaysLogs { get; set; }
        public int ThisWeeksLogs { get; set; }
        public string Status { get; set; } // "Active", "Maintenance", etc.

        public List<DispenserLog> RecentLogs { get; set; }

        // ✅ Add this line
        public List<Unit> Units { get; set; }

        public List<Dispenser> Dispensers { get; set; }
    }


}
