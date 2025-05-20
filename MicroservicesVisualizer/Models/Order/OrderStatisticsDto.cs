namespace MicroservicesVisualizer.Models.Order
{
    public class OrderStatisticsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public decimal AverageOrderAmount { get; set; }
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
    }
}
