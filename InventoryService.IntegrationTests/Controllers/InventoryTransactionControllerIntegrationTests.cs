using System.Net;
using FluentAssertions;
using InventoryService.Application.DTOs;
using InventoryService.Domain.Entities;
using InventoryService.IntegrationTests.Infrastructure;

namespace InventoryService.IntegrationTests.Controllers
{
    public class InventoryTransactionControllerIntegrationTests : IntegrationTestBase
    {
        public InventoryTransactionControllerIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Create_StockInTransaction_IncreasesInventory()
        {
            // Arrange
            var initialInventory = await GetAsync<InventoryDto>("/api/v1/inventory/1");
            var initialQuantity = initialInventory!.Quantity;

            var transactionDto = new CreateInventoryTransactionDto
            {
                InventoryId = 1,
                Type = TransactionType.StockIn,
                Quantity = 30,
                Reference = "PO-INT-001",
                Notes = "Integration test stock in"
            };

            // Act
            var response = await PostAsync("/api/v1/inventorytransaction", transactionDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var transaction = await DeserializeResponse<InventoryTransactionDto>(response);
            transaction.Should().NotBeNull();
            transaction!.Type.Should().Be(TransactionType.StockIn);
            transaction.Quantity.Should().Be(30);

            // Verify inventory was updated
            var updatedInventory = await GetAsync<InventoryDto>("/api/v1/inventory/1");
            updatedInventory!.Quantity.Should().Be(initialQuantity + 30);
        }

        [Fact]
        public async Task Create_StockOutTransaction_DecreasesInventory()
        {
            // Arrange
            var initialInventory = await GetAsync<InventoryDto>("/api/v1/inventory/1");
            var initialQuantity = initialInventory!.Quantity;

            var transactionDto = new CreateInventoryTransactionDto
            {
                InventoryId = 1,
                Type = TransactionType.StockOut,
                Quantity = 15,
                Reference = "SO-INT-001",
                Notes = "Integration test stock out"
            };

            // Act
            var response = await PostAsync("/api/v1/inventorytransaction", transactionDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Verify inventory was updated
            var updatedInventory = await GetAsync<InventoryDto>("/api/v1/inventory/1");
            updatedInventory!.Quantity.Should().Be(initialQuantity - 15);
        }

        [Fact]
        public async Task Create_AdjustmentTransaction_SetsNewQuantity()
        {
            // Arrange
            var transactionDto = new CreateInventoryTransactionDto
            {
                InventoryId = 1,
                Type = TransactionType.Adjustment,
                Quantity = 200,
                Reference = "ADJ-INT-001",
                Notes = "Inventory count adjustment"
            };

            // Act
            var response = await PostAsync("/api/v1/inventorytransaction", transactionDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Verify inventory was adjusted
            var updatedInventory = await GetAsync<InventoryDto>("/api/v1/inventory/1");
            updatedInventory!.Quantity.Should().Be(200);
        }

        [Fact]
        public async Task GetAll_ReturnsAllTransactions()
        {
            // Arrange - Create some transactions first
            await PostAsync("/api/v1/inventorytransaction", new CreateInventoryTransactionDto
            {
                InventoryId = 1,
                Type = TransactionType.StockIn,
                Quantity = 10,
                Reference = "TEST-001",
                Notes = "Test"
            });

            // Act
            var transactions = await GetAsync<List<InventoryTransactionDto>>("/api/v1/inventorytransaction");

            // Assert
            transactions.Should().NotBeNull();
            transactions.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetByInventoryId_ReturnsTransactionsForInventory()
        {
            // Arrange - Create transactions for specific inventory
            await PostAsync("/api/v1/inventorytransaction", new CreateInventoryTransactionDto
            {
                InventoryId = 2,
                Type = TransactionType.StockIn,
                Quantity = 20,
                Reference = "TEST-002",
                Notes = "Test for inventory 2"
            });

            // Act
            var transactions = await GetAsync<List<InventoryTransactionDto>>("/api/v1/inventorytransaction/by-inventory/2");

            // Assert
            transactions.Should().NotBeNull();
            transactions.Should().AllSatisfy(t => t.InventoryId.Should().Be(2));
        }

        [Fact]
        public async Task V2_Search_WithFilters_ReturnsFilteredTransactions()
        {
            // Arrange - Create various transactions
            var stockIn = new CreateInventoryTransactionDto
            {
                InventoryId = 1,
                Type = TransactionType.StockIn,
                Quantity = 50,
                Reference = "FILTER-001",
                Notes = "Filter test"
            };
            await PostAsync("/api/v1/inventorytransaction", stockIn);

            // Act
            var response = await Client.GetAsync("/api/v2/inventorytransaction/search?type=StockIn");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var transactions = await DeserializeResponse<List<InventoryTransactionDto>>(response);
            transactions.Should().NotBeNull();
            transactions.Should().AllSatisfy(t => t.Type.Should().Be(TransactionType.StockIn));
        }

        [Fact]
        public async Task V2_GetSummary_ReturnsTransactionSummary()
        {
            // Arrange - Create various transactions
            await PostAsync("/api/v1/inventorytransaction", new CreateInventoryTransactionDto
            {
                InventoryId = 1,
                Type = TransactionType.StockIn,
                Quantity = 100,
                Reference = "SUM-001",
                Notes = "Summary test"
            });

            await PostAsync("/api/v1/inventorytransaction", new CreateInventoryTransactionDto
            {
                InventoryId = 1,
                Type = TransactionType.StockOut,
                Quantity = 30,
                Reference = "SUM-002",
                Notes = "Summary test"
            });

            // Act
            var response = await Client.GetAsync("/api/v2/inventorytransaction/summary");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var summary = await DeserializeResponse<TransactionSummaryDto>(response);
            summary.Should().NotBeNull();
            summary!.TotalTransactions.Should().BeGreaterThan(0);
            summary.StockInTotal.Should().BeGreaterThanOrEqualTo(100);
            summary.StockOutTotal.Should().BeGreaterThanOrEqualTo(30);
            summary.TransactionsByType.Should().ContainKey("StockIn");
            summary.TransactionsByType.Should().ContainKey("StockOut");
        }
    }
}