using Microsoft.AspNetCore.Mvc;
using ShipmentDeliveryAPI.DTOs;
using ShipmentDeliveryAPI.Services;

namespace ShipmentDeliveryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentDeliveryController : ControllerBase
    {
        private readonly IShipmentDeliveryService _shipmentDeliveryService;
        private readonly ILogger<ShipmentDeliveryController> _logger;

        public ShipmentDeliveryController(
            IShipmentDeliveryService shipmentDeliveryService,
            ILogger<ShipmentDeliveryController> logger)
        {
            _shipmentDeliveryService = shipmentDeliveryService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new shipment delivery entry
        /// </summary>
        /// <param name="request">Shipment delivery data</param>
        /// <returns>Success status</returns>
        [HttpPost]
        public async Task<IActionResult> CreateShipmentDelivery([FromBody] CreateShipmentDeliveryRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate request data
                if (string.IsNullOrWhiteSpace(request.ShipmentNumber) || string.IsNullOrWhiteSpace(request.DeliveryNumber))
                {
                    return BadRequest("Shipment number and delivery number are required.");
                }

                // Validate items based on delivery type
                if (request.DeliveryType == Models.DeliveryType.Container)
                {
                    if (request.ContainerItems == null || !request.ContainerItems.Any())
                    {
                        return BadRequest("Container items are required for container delivery type.");
                    }

                    var invalidContainerItems = request.ContainerItems.Where(ci =>
                        string.IsNullOrWhiteSpace(ci.MaterialNumber) || string.IsNullOrWhiteSpace(ci.SerialNumber));

                    if (invalidContainerItems.Any())
                    {
                        return BadRequest("All container items must have material number and serial number.");
                    }
                }
                else if (request.DeliveryType == Models.DeliveryType.Bulk)
                {
                    if (request.BulkItems == null || !request.BulkItems.Any())
                    {
                        return BadRequest("Bulk items are required for bulk delivery type.");
                    }

                    var invalidBulkItems = request.BulkItems.Where(bi =>
                        string.IsNullOrWhiteSpace(bi.MaterialNumber) || string.IsNullOrWhiteSpace(bi.EvdSealNumber));

                    if (invalidBulkItems.Any())
                    {
                        return BadRequest("All bulk items must have material number and EVD seal number.");
                    }
                }

                var result = await _shipmentDeliveryService.CreateShipmentDeliveryAsync(request);

                if (result)
                {
                    return CreatedAtAction(nameof(GetByDeliveryNumber), new { deliveryNumber = request.DeliveryNumber }, request);
                }
                else
                {
                    return Conflict("Failed to create shipment delivery. Delivery number might already exist.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shipment delivery");
                return StatusCode(500, "Internal server error occurred while creating shipment delivery.");
            }
        }

        /// <summary>
        /// Get shipment details by shipment number
        /// </summary>
        /// <param name="shipmentNumber">Shipment number</param>
        /// <returns>Shipment details with all deliveries</returns>
        [HttpGet("shipment/{shipmentNumber}")]
        public async Task<IActionResult> GetByShipmentNumber(string shipmentNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(shipmentNumber))
                {
                    return BadRequest("Shipment number is required.");
                }

                var result = await _shipmentDeliveryService.GetByShipmentNumberAsync(shipmentNumber);

                if (result == null)
                {
                    return NotFound($"Shipment with number {shipmentNumber} not found.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shipment by shipment number: {ShipmentNumber}", shipmentNumber);
                return StatusCode(500, "Internal server error occurred while retrieving shipment.");
            }
        }

        /// <summary>
        /// Get shipment details by delivery number
        /// </summary>
        /// <param name="deliveryNumber">Delivery number</param>
        /// <returns>Shipment details with all deliveries</returns>
        [HttpGet("delivery/{deliveryNumber}")]
        public async Task<IActionResult> GetByDeliveryNumber(string deliveryNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(deliveryNumber))
                {
                    return BadRequest("Delivery number is required.");
                }

                var result = await _shipmentDeliveryService.GetByDeliveryNumberAsync(deliveryNumber);

                if (result == null)
                {
                    return NotFound($"Delivery with number {deliveryNumber} not found.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shipment by delivery number: {DeliveryNumber}", deliveryNumber);
                return StatusCode(500, "Internal server error occurred while retrieving shipment.");
            }
        }

        /// <summary>
        /// Get all shipments
        /// </summary>
        /// <returns>List of all shipments with their deliveries</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllShipments()
        {
            try
            {
                var result = await _shipmentDeliveryService.GetAllShipmentsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all shipments");
                return StatusCode(500, "Internal server error occurred while retrieving shipments.");
            }
        }
    }
}
