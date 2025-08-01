﻿namespace Dashboard.Models
{
    public class DashboardViewModel
    {
        public int TotalDispensers { get; set; }
        public int TotalLogs { get; set; }
        public int TotalRefills { get; set; }
        public int TotalChecks { get; set; }
        public int TotalReplacements { get; set; }
        public List<DispenserLog> RecentLogs { get; set; }
    }

}
