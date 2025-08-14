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
