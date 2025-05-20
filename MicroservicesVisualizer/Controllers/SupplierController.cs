using MicroservicesVisualizer.Models.Supplier;
using MicroservicesVisualizer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MicroservicesVisualizer.Controllers
{
    public class SupplierController : Controller
    {
        private readonly ISupplierService _supplierService;
        private readonly ILogger<SupplierController> _logger;

        public SupplierController(
            ISupplierService supplierService,
            ILogger<SupplierController> logger)
        {
            _supplierService = supplierService;
            _logger = logger;
        }

        // Supplier listing
        public async Task<IActionResult> Index()
        {
            try
            {
                var suppliers = await _supplierService.GetAllSuppliersAsync();
                return View(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving suppliers");
                return View(Enumerable.Empty<SupplierDto>());
            }
        }

        // Supplier details
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var supplierWithOrders = await _supplierService.GetSupplierWithOrdersAsync(id);

                if (supplierWithOrders == null || supplierWithOrders.Id == 0)
                {
                    return NotFound();
                }

                return View(supplierWithOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier details for ID: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // Search suppliers
        public async Task<IActionResult> Search(string? name, string? email, bool? isActive)
        {
            try
            {
                var suppliers = await _supplierService.SearchSuppliersAsync(name, email, isActive);
                ViewBag.Name = name;
                ViewBag.Email = email;
                ViewBag.IsActive = isActive;

                return View("Index", suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching suppliers");
                return RedirectToAction(nameof(Index));
            }
        }

        // Create supplier form
        public IActionResult Create()
        {
            return View(new CreateSupplierDto());
        }

        // Create supplier submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSupplierDto model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var supplier = await _supplierService.CreateSupplierAsync(model);
                    return RedirectToAction(nameof(Details), new { id = supplier.Id });
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier");
                ModelState.AddModelError("", "An error occurred while creating the supplier. Please try again.");
                return View(model);
            }
        }

        // Edit supplier form
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id);

                if (supplier == null || supplier.Id == 0)
                {
                    return NotFound();
                }

                var model = new UpdateSupplierDto
                {
                    Name = supplier.Name,
                    ContactName = supplier.ContactName,
                    Phone = supplier.Phone,
                    Address = supplier.Address,
                    Website = supplier.Website,
                    Notes = supplier.Notes
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit supplier form for ID: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // Edit supplier submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateSupplierDto model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _supplierService.UpdateSupplierAsync(id, model);
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier with ID: {Id}", id);
                ModelState.AddModelError("", "An error occurred while updating the supplier. Please try again.");
                return View(model);
            }
        }

        // Activate supplier
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                await _supplierService.ActivateSupplierAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating supplier with ID: {Id}", id);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // Deactivate supplier
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _supplierService.DeactivateSupplierAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating supplier with ID: {Id}", id);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // Purchase orders listing
        public async Task<IActionResult> PurchaseOrders()
        {
            try
            {
                var purchaseOrders = await _supplierService.GetAllPurchaseOrdersAsync();
                return View(purchaseOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase orders");
                return View(Enumerable.Empty<PurchaseOrderDto>());
            }
        }

        // Purchase order details
        public async Task<IActionResult> PurchaseOrderDetails(int id)
        {
            try
            {
                var purchaseOrder = await _supplierService.GetPurchaseOrderByIdAsync(id);

                if (purchaseOrder == null || purchaseOrder.Id == 0)
                {
                    return NotFound();
                }

                return View(purchaseOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase order details for ID: {Id}", id);
                return RedirectToAction(nameof(PurchaseOrders));
            }
        }

        // Filter purchase orders by status
        public async Task<IActionResult> PurchaseOrdersByStatus(PurchaseOrderStatus status)
        {
            try
            {
                var purchaseOrders = await _supplierService.GetPurchaseOrdersByStatusAsync(status);
                ViewBag.CurrentStatus = status;
                return View("PurchaseOrders", purchaseOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase orders by status: {Status}", status);
                return RedirectToAction(nameof(PurchaseOrders));
            }
        }

        // Search purchase orders
        public async Task<IActionResult> SearchPurchaseOrders(
            int? supplierId, PurchaseOrderStatus? status,
            DateTime? startDate, DateTime? endDate,
            decimal? minAmount, decimal? maxAmount)
        {
            try
            {
                var purchaseOrders = await _supplierService.SearchPurchaseOrdersAsync(
                    supplierId, status, startDate, endDate, minAmount, maxAmount);

                ViewBag.SupplierId = supplierId;
                ViewBag.Status = status;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.MinAmount = minAmount;
                ViewBag.MaxAmount = maxAmount;

                // Get suppliers for the filter dropdown
                var suppliers = await _supplierService.GetAllSuppliersAsync();
                ViewBag.Suppliers = suppliers;

                return View("PurchaseOrders", purchaseOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching purchase orders");
                return RedirectToAction(nameof(PurchaseOrders));
            }
        }

        // Purchase order summary
        public async Task<IActionResult> PurchaseOrderSummary(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var summary = await _supplierService.GetPurchaseOrderSummaryAsync(startDate, endDate);
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;

                return View(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase order summary");
                return View(new PurchaseOrderSummaryDto());
            }
        }

        // Update purchase order status form
        public async Task<IActionResult> UpdatePurchaseOrderStatus(int id)
        {
            try
            {
                var purchaseOrder = await _supplierService.GetPurchaseOrderByIdAsync(id);

                if (purchaseOrder == null || purchaseOrder.Id == 0)
                {
                    return NotFound();
                }

                ViewBag.PurchaseOrder = purchaseOrder;
                var model = new UpdatePurchaseOrderStatusDto { Status = purchaseOrder.Status };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading update purchase order status form for ID: {Id}", id);
                return RedirectToAction(nameof(PurchaseOrders));
            }
        }

        // Update purchase order status submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePurchaseOrderStatus(int id, UpdatePurchaseOrderStatusDto model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _supplierService.UpdatePurchaseOrderStatusAsync(id, model);
                    return RedirectToAction(nameof(PurchaseOrderDetails), new { id });
                }

                var purchaseOrder = await _supplierService.GetPurchaseOrderByIdAsync(id);
                ViewBag.PurchaseOrder = purchaseOrder;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchase order status for ID: {Id}", id);
                return RedirectToAction(nameof(PurchaseOrderDetails), new { id });
            }
        }

        // Receive purchase order item form
        public async Task<IActionResult> ReceivePurchaseOrderItem(int id, int itemId)
        {
            try
            {
                var purchaseOrder = await _supplierService.GetPurchaseOrderByIdAsync(id);

                if (purchaseOrder == null || purchaseOrder.Id == 0)
                {
                    return NotFound();
                }

                var item = purchaseOrder.Items.FirstOrDefault(i => i.Id == itemId);

                if (item == null)
                {
                    return NotFound();
                }

                ViewBag.PurchaseOrder = purchaseOrder;
                ViewBag.Item = item;

                var model = new ReceivePurchaseOrderItemDto
                {
                    ReceivedQuantity = 0
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading receive purchase order item form for order ID: {Id}, item ID: {ItemId}", id, itemId);
                return RedirectToAction(nameof(PurchaseOrderDetails), new { id });
            }
        }

        // Receive purchase order item submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReceivePurchaseOrderItem(int id, int itemId, ReceivePurchaseOrderItemDto model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _supplierService.ReceivePurchaseOrderItemAsync(id, itemId, model);
                    return RedirectToAction(nameof(PurchaseOrderDetails), new { id });
                }

                var purchaseOrder = await _supplierService.GetPurchaseOrderByIdAsync(id);
                var item = purchaseOrder.Items.FirstOrDefault(i => i.Id == itemId);

                ViewBag.PurchaseOrder = purchaseOrder;
                ViewBag.Item = item;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving purchase order item for order ID: {Id}, item ID: {ItemId}", id, itemId);
                return RedirectToAction(nameof(PurchaseOrderDetails), new { id });
            }
        }

        // Cancel purchase order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelPurchaseOrder(int id)
        {
            try
            {
                await _supplierService.CancelPurchaseOrderAsync(id);
                return RedirectToAction(nameof(PurchaseOrderDetails), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling purchase order with ID: {Id}", id);
                return RedirectToAction(nameof(PurchaseOrderDetails), new { id });
            }
        }
    }
}
