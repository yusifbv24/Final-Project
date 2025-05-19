﻿namespace OrderService.Application.DTOs
{
    public record CustomerDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string DefaultShippingAddress { get; init; } = string.Empty;
        public string DefaultBillingAddress { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}