using ShipmentDeliveryAPI.DTOs;
using ShipmentDeliveryAPI.Models;
using ShipmentDeliveryAPI.Repositories.Interfaces;

namespace ShipmentDeliveryAPI.Services
{
    public class ShipmentDeliveryService : IShipmentDeliveryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ShipmentDeliveryService> _logger;

        public ShipmentDeliveryService(IUnitOfWork unitOfWork, ILogger<ShipmentDeliveryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> CreateShipmentDeliveryAsync(CreateShipmentDeliveryRequestDto request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Check if shipment exists, if not create it
                var shipment = await _unitOfWork.Shipments.GetByShipmentNumberAsync(request.ShipmentNumber);

                if (shipment == null)
                {
                    shipment = new Shipment
                    {
                        ShipmentNumber = request.ShipmentNumber,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Shipments.AddAsync(shipment);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Check if delivery already exists
                var existingDelivery = await _unitOfWork.Deliveries.GetByDeliveryNumberAsync(request.DeliveryNumber);

                if (existingDelivery != null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogWarning("Delivery number {DeliveryNumber} already exists", request.DeliveryNumber);
                    return false;
                }

                // Create delivery
                var delivery = new Delivery
                {
                    DeliveryNumber = request.DeliveryNumber,
                    DeliveryType = request.DeliveryType,
                    ShipmentId = shipment.ShipmentId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Deliveries.AddAsync(delivery);
                await _unitOfWork.SaveChangesAsync();

                // Add items based on delivery type
                if (request.DeliveryType == DeliveryType.Container && request.ContainerItems != null)
                {
                    var containerItems = request.ContainerItems.Select(item => new ContainerItem
                    {
                        MaterialNumber = item.MaterialNumber,
                        SerialNumber = item.SerialNumber,
                        DeliveryId = delivery.DeliveryId,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await _unitOfWork.ContainerItems.AddRangeAsync(containerItems);
                }
                else if (request.DeliveryType == DeliveryType.Bulk && request.BulkItems != null)
                {
                    var bulkItems = request.BulkItems.Select(item => new BulkItem
                    {
                        MaterialNumber = item.MaterialNumber,
                        EvdSealNumber = item.EvdSealNumber,
                        DeliveryId = delivery.DeliveryId,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await _unitOfWork.BulkItems.AddRangeAsync(bulkItems);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully created shipment delivery for shipment {ShipmentNumber}, delivery {DeliveryNumber}",
                    request.ShipmentNumber, request.DeliveryNumber);

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating shipment delivery for shipment {ShipmentNumber}, delivery {DeliveryNumber}",
                    request.ShipmentNumber, request.DeliveryNumber);
                return false;
            }
        }

        public async Task<ShipmentDeliveryResponseDto?> GetByShipmentNumberAsync(string shipmentNumber)
        {
            try
            {
                var shipment = await _unitOfWork.Shipments.GetShipmentWithDeliveriesAsync(shipmentNumber);

                if (shipment == null)
                {
                    _logger.LogInformation("Shipment with number {ShipmentNumber} not found", shipmentNumber);
                    return null;
                }

                return MapToResponseDto(shipment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shipment by shipment number {ShipmentNumber}", shipmentNumber);
                throw;
            }
        }

        public async Task<ShipmentDeliveryResponseDto?> GetByDeliveryNumberAsync(string deliveryNumber)
        {
            try
            {
                var delivery = await _unitOfWork.Deliveries.GetDeliveryWithShipmentAsync(deliveryNumber);

                if (delivery?.Shipment == null)
                {
                    _logger.LogInformation("Delivery with number {DeliveryNumber} not found", deliveryNumber);
                    return null;
                }

                return MapToResponseDto(delivery.Shipment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shipment by delivery number {DeliveryNumber}", deliveryNumber);
                throw;
            }
        }

        public async Task<List<ShipmentDeliveryResponseDto>> GetAllShipmentsAsync()
        {
            try
            {
                var shipments = await _unitOfWork.Shipments.GetAllAsync();
                var result = new List<ShipmentDeliveryResponseDto>();

                foreach (var shipment in shipments)
                {
                    var shipmentWithDeliveries = await _unitOfWork.Shipments.GetShipmentWithDeliveriesAsync(shipment.ShipmentNumber);
                    if (shipmentWithDeliveries != null)
                    {
                        result.Add(MapToResponseDto(shipmentWithDeliveries));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all shipments");
                throw;
            }
        }

        public async Task<bool> UpdateShipmentAsync(string currentShipmentNumber, UpdateShipmentRequestDto request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Get the existing shipment with deliveries
                var shipment = await _unitOfWork.Shipments.GetShipmentWithDeliveriesAsync(currentShipmentNumber);

                if (shipment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogWarning("Shipment with number {ShipmentNumber} not found for update", currentShipmentNumber);
                    return false;
                }

                // Check if new shipment number already exists (if it's different)
                if (currentShipmentNumber != request.ShipmentNumber)
                {
                    var existingShipment = await _unitOfWork.Shipments.GetByShipmentNumberAsync(request.ShipmentNumber);
                    if (existingShipment != null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogWarning("Shipment number {ShipmentNumber} already exists", request.ShipmentNumber);
                        return false;
                    }
                }

                // Update shipment basic info
                shipment.ShipmentNumber = request.ShipmentNumber;
                shipment.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Shipments.Update(shipment);

                // Update deliveries if provided
                if (request.Deliveries != null && request.Deliveries.Any())
                {
                    // Remove all existing deliveries and their items first
                    foreach (var existingDelivery in shipment.Deliveries)
                    {
                        // Remove container items
                        var existingContainerItems = await _unitOfWork.ContainerItems.FindAsync(ci => ci.DeliveryId == existingDelivery.DeliveryId);
                        if (existingContainerItems.Any())
                        {
                            _unitOfWork.ContainerItems.DeleteRange(existingContainerItems);
                        }

                        // Remove bulk items
                        var existingBulkItems = await _unitOfWork.BulkItems.FindAsync(bi => bi.DeliveryId == existingDelivery.DeliveryId);
                        if (existingBulkItems.Any())
                        {
                            _unitOfWork.BulkItems.DeleteRange(existingBulkItems);
                        }
                    }

                    // Remove existing deliveries
                    if (shipment.Deliveries.Any())
                    {
                        _unitOfWork.Deliveries.DeleteRange(shipment.Deliveries);
                    }

                    await _unitOfWork.SaveChangesAsync();

                    // Add new deliveries
                    foreach (var deliveryDto in request.Deliveries)
                    {
                        // Validate delivery data
                        if (string.IsNullOrWhiteSpace(deliveryDto.DeliveryNumber))
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            _logger.LogWarning("Delivery number is required for update");
                            return false;
                        }

                        // Validate items based on delivery type
                        if (deliveryDto.DeliveryType == DeliveryType.Container)
                        {
                            if (deliveryDto.ContainerItems == null || !deliveryDto.ContainerItems.Any())
                            {
                                await _unitOfWork.RollbackTransactionAsync();
                                _logger.LogWarning("Container items are required for container delivery type");
                                return false;
                            }

                            var invalidContainerItems = deliveryDto.ContainerItems.Where(ci =>
                                string.IsNullOrWhiteSpace(ci.MaterialNumber) || string.IsNullOrWhiteSpace(ci.SerialNumber));

                            if (invalidContainerItems.Any())
                            {
                                await _unitOfWork.RollbackTransactionAsync();
                                _logger.LogWarning("All container items must have material number and serial number");
                                return false;
                            }
                        }
                        else if (deliveryDto.DeliveryType == DeliveryType.Bulk)
                        {
                            if (deliveryDto.BulkItems == null || !deliveryDto.BulkItems.Any())
                            {
                                await _unitOfWork.RollbackTransactionAsync();
                                _logger.LogWarning("Bulk items are required for bulk delivery type");
                                return false;
                            }

                            var invalidBulkItems = deliveryDto.BulkItems.Where(bi =>
                                string.IsNullOrWhiteSpace(bi.MaterialNumber) || string.IsNullOrWhiteSpace(bi.EvdSealNumber));

                            if (invalidBulkItems.Any())
                            {
                                await _unitOfWork.RollbackTransactionAsync();
                                _logger.LogWarning("All bulk items must have material number and EVD seal number");
                                return false;
                            }
                        }

                        // Create new delivery
                        var delivery = new Delivery
                        {
                            DeliveryNumber = deliveryDto.DeliveryNumber,
                            DeliveryType = deliveryDto.DeliveryType,
                            ShipmentId = shipment.ShipmentId,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.Deliveries.AddAsync(delivery);
                        await _unitOfWork.SaveChangesAsync();

                        // Add items based on delivery type
                        if (deliveryDto.DeliveryType == DeliveryType.Container && deliveryDto.ContainerItems != null)
                        {
                            var containerItems = deliveryDto.ContainerItems.Select(item => new ContainerItem
                            {
                                MaterialNumber = item.MaterialNumber,
                                SerialNumber = item.SerialNumber,
                                DeliveryId = delivery.DeliveryId,
                                CreatedAt = DateTime.UtcNow
                            }).ToList();

                            await _unitOfWork.ContainerItems.AddRangeAsync(containerItems);
                        }
                        else if (deliveryDto.DeliveryType == DeliveryType.Bulk && deliveryDto.BulkItems != null)
                        {
                            var bulkItems = deliveryDto.BulkItems.Select(item => new BulkItem
                            {
                                MaterialNumber = item.MaterialNumber,
                                EvdSealNumber = item.EvdSealNumber,
                                DeliveryId = delivery.DeliveryId,
                                CreatedAt = DateTime.UtcNow
                            }).ToList();

                            await _unitOfWork.BulkItems.AddRangeAsync(bulkItems);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully updated shipment {OldShipmentNumber} to {NewShipmentNumber}",
                    currentShipmentNumber, request.ShipmentNumber);

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating shipment {ShipmentNumber}", currentShipmentNumber);
                return false;
            }
        }

        public async Task<bool> DeleteShipmentAsync(string shipmentNumber)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Get the shipment with all its deliveries and items
                var shipment = await _unitOfWork.Shipments.GetShipmentWithDeliveriesAsync(shipmentNumber);

                if (shipment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogWarning("Shipment with number {ShipmentNumber} not found for deletion", shipmentNumber);
                    return false;
                }

                // Delete all container items and bulk items for each delivery
                foreach (var delivery in shipment.Deliveries)
                {
                    // Get and delete container items
                    var containerItems = await _unitOfWork.ContainerItems.FindAsync(ci => ci.DeliveryId == delivery.DeliveryId);
                    if (containerItems.Any())
                    {
                        _unitOfWork.ContainerItems.DeleteRange(containerItems);
                    }

                    // Get and delete bulk items
                    var bulkItems = await _unitOfWork.BulkItems.FindAsync(bi => bi.DeliveryId == delivery.DeliveryId);
                    if (bulkItems.Any())
                    {
                        _unitOfWork.BulkItems.DeleteRange(bulkItems);
                    }
                }

                // Delete all deliveries
                if (shipment.Deliveries.Any())
                {
                    _unitOfWork.Deliveries.DeleteRange(shipment.Deliveries);
                }

                // Delete the shipment
                _unitOfWork.Shipments.Delete(shipment);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully deleted shipment {ShipmentNumber} and all associated data", shipmentNumber);

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error deleting shipment {ShipmentNumber}", shipmentNumber);
                return false;
            }
        }

        private ShipmentDeliveryResponseDto MapToResponseDto(Shipment shipment)
        {
            return new ShipmentDeliveryResponseDto
            {
                ShipmentNumber = shipment.ShipmentNumber,
                ShipmentCreatedAt = shipment.CreatedAt,
                Deliveries = shipment.Deliveries.Select(d => new DeliveryResponseDto
                {
                    DeliveryNumber = d.DeliveryNumber,
                    DeliveryType = d.DeliveryType,
                    CreatedAt = d.CreatedAt,
                    ContainerItems = d.DeliveryType == DeliveryType.Container
                        ? d.ContainerItems.Select(ci => new ContainerItemResponseDto
                        {
                            MaterialNumber = ci.MaterialNumber,
                            SerialNumber = ci.SerialNumber,
                            CreatedAt = ci.CreatedAt
                        }).ToList()
                        : null,
                    BulkItems = d.DeliveryType == DeliveryType.Bulk
                        ? d.BulkItems.Select(bi => new BulkItemResponseDto
                        {
                            MaterialNumber = bi.MaterialNumber,
                            EvdSealNumber = bi.EvdSealNumber,
                            CreatedAt = bi.CreatedAt
                        }).ToList()
                        : null
                }).ToList()
            };
        }
    }
}
