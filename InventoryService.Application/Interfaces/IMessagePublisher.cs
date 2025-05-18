﻿namespace InventoryService.Application.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default) where T : class;
    }
}
