﻿namespace MicroservicesVisualizer.Models.Order
{
    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
