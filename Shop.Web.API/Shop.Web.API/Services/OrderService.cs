using Microsoft.Extensions.Options;
using Shop.Web.API.Exceptions;
using Shop.Web.API.Models.Domain;
using Shop.Web.API.Models.Requests;
using Shop.Web.API.Models.Responses;
using Shop.Web.API.Repositories;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace Shop.Web.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;
        private readonly ILogger<OrderService> _logger;


        private static readonly JsonSerializerOptions _jsonOpts = new()
        { PropertyNameCaseInsensitive = true };

        public OrderService(IOrderRepository repo, ILogger<OrderService> logger)
        {
            _repo = repo;
            _logger = logger;
        }
        //public async Task<List<OrderDto>> GetAllAsync(
        //    int businessAccountId, DateTime? from, DateTime? to)
        //{
        //    var records = await _repo.GetAllAsync(businessAccountId, from, to);
        //    return records.Select(MapToDto).ToList();
        //}

        public async Task<PagedResponse<OrderDto>> GetAllAsync(
            int businessAccountId, DateTime? from, DateTime? to, int page, int pageSize, string? status = null, string? search = null)
        {
            var (items, totalCount) = await _repo.GetAllAsync(
                businessAccountId, from, to, page, pageSize, status, search);

            return new PagedResponse<OrderDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        public async Task<OrderDto> GetByIdAsync(int orderId, int businessAccountId)
        {
            var record = await _repo.GetByIdAsync(orderId, businessAccountId);
            if (record is null) throw new NotFoundException($"Order #{orderId} not found.");
            return MapToDto(record);
        }

        //public async Task<List<OrderDto>> GetByCustomerAsync(int customerId, int businessAccountId)
        //{
        //    var records = await _repo.GetByCustomerAsync(customerId, businessAccountId);
        //    return records.Select(MapToDto).ToList();
        //}

        public async Task<PagedResponse<OrderDto>> GetByCustomerAsync(
            int customerId, int businessAccountId, int page, int pageSize)
        {
            var (items, totalCount) = await _repo.GetByCustomerAsync(
                customerId, businessAccountId, page, pageSize);

            return new PagedResponse<OrderDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        public async Task<TodaySummaryDto> GetTodaySummaryAsync(int businessAccountId)
        {
            var r = await _repo.GetTodaySummaryAsync(businessAccountId);
            return new TodaySummaryDto
            {
                TodaySales = r.TodaySales,
                TodayOrders = r.TodayOrders,
                CashAmount = r.CashAmount,
                UpiAmount = r.UpiAmount,
                CreditAmount = r.CreditAmount,
                TotalCustomers = 0,  // set by caller if needed
                CreditPending = 0,
            };
        }

        public async Task<OrderDto> CreateAsync(
            int businessAccountId, int requestingUserId,
            CreateOrderRequest req, string ip)
        {
            if (req.Items.Count == 0)
                throw new AppException("At least one item is required.", "ORDER_NO_ITEMS");

            var expectedTotal = req.Items.Sum(i => Math.Round(i.PricePerUnit * i.Quantity, 2));
            
            //if (Math.Abs(expectedTotal - req.GrandTotal) > 0.01m)
            if (Math.Abs(expectedTotal - req.SubTotal) > 0.01m)
                throw new AppException("Grand total does not match item totals.", "ORDER_TOTAL_MISMATCH");

            if (req.CustomerId == 0 && (req.PaymentType.ToUpper() == "CREDIT" || req.Status.ToUpper() == "CREDIT" || req.Status.ToUpper() == "PARTIAL"))
                throw new AppException("Walk-in customers must pay in full — Cash or UPI only.", "PAYMENT_TYPE_MISMATCH");

            expectedTotal += req.TotalGst;
            if (req.PaidAmount > expectedTotal)
                throw new AppException("Paid amount cannot exceed grand total.", "EXTRA_PAID_AMOUNT");

            var record = await _repo.CreateAsync(businessAccountId, requestingUserId, req, ip);
            if (record is null)
                throw new AppException("Failed to save order. Please try again.", "ORDER_UNEXPECTED");

            _logger.LogInformation(
                "Order created: #{Id} Customer={Customer} Total=₹{Total} Status={Status} BusinessAccountId={BizId} by UserId={UserId}",
                record.Id, req.CustomerName, req.GrandTotal, req.Status, businessAccountId, requestingUserId);

            return MapToDto(record);
        }

        public async Task<OrderDto> UpdateAsync(
            int orderId, int businessAccountId, int requestingUserId,
            UpdateOrderRequest req, string ip)
        {
            if (req.Items.Count == 0)
                throw new AppException("At least one item is required.", "ORDER_NO_ITEMS");

            var expectedTotal = req.Items.Sum(i => Math.Round(i.PricePerUnit * i.Quantity, 2));
            
            if (Math.Abs(expectedTotal - req.SubTotal) > 0.01m)
                throw new AppException("Grand total does not match item totals.", "ORDER_TOTAL_MISMATCH");

            expectedTotal += req.TotalGst;
            if (req.PaidAmount > expectedTotal)
                throw new AppException("Paid amount cannot exceed grand total.", "EXTRA_PAID_AMOUNT");

            if (req.CustomerId == 0 && (req.PaymentType.ToUpper() == "CREDIT" || req.Status.ToUpper() == "CREDIT" || req.Status.ToUpper() == "PARTIAL"))
                throw new AppException("Walk-in customers must pay in full — Cash or UPI only.", "PAYMENT_TYPE_MISMATCH");

            var record = await _repo.UpdateAsync(orderId, businessAccountId, requestingUserId, req, ip);
            if (record is null) throw new NotFoundException($"Order #{orderId} not found.");

            _logger.LogInformation(
                "Order updated: #{Id} by UserId={UserId}", orderId, requestingUserId);

            return MapToDto(record);
        }

        //public async Task<List<PaymentDto>> GetPaymentsByCustomerAsync(int customerId, int businessAccountId)
        //{
        //    var records = await _repo.GetPaymentsByCustomerAsync(customerId, businessAccountId);
        //    return records.Select(r => new PaymentDto
        //    {
        //        Id = r.Id,
        //        CustomerId = r.CustomerId,
        //        OrderId = r.OrderId,
        //        Amount = r.Amount,
        //        PaymentType = r.PaymentType,
        //        Note = r.Note,
        //        Date = r.Date,
        //    }).ToList();
        //}

        public async Task<PagedResponse<PaymentDto>> GetPaymentsByCustomerAsync(
            int customerId, int businessAccountId, int page, int pageSize)
        {
            var (items, totalCount) = await _repo.GetPaymentsByCustomerAsync(
                customerId, businessAccountId, page, pageSize);

            return new PagedResponse<PaymentDto>
            {
                Items = items.Select(r => new PaymentDto
                {
                    Id = r.Id,
                    CustomerId = r.CustomerId,
                    OrderId = r.OrderId,
                    Amount = r.Amount,
                    PaymentType = r.PaymentType,
                    Note = r.Note,
                    Date = r.Date,
                }).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        public async Task<int> RecordPaymentAsync(
            int businessAccountId, int requestingUserId,
            int customerId, RecordPaymentRequest req, string ip)
        {
            var paymentId = await _repo.RecordPaymentAsync(
                businessAccountId, requestingUserId, customerId, req, ip);

            _logger.LogInformation(
                "Payment recorded: #{Id} CustomerId={CId} Amount=₹{Amt} by UserId={UserId}",
                paymentId, customerId, req.Amount, requestingUserId);

            return paymentId;
        }

        public async Task SoftDeleteAsync(
            int orderId, int businessAccountId, int requestingUserId, string ip)
        {
            var affected = await _repo.SoftDeleteAsync(
                orderId, businessAccountId, requestingUserId, ip);
            if (affected == 0)
                throw new NotFoundException($"Order #{orderId} not found.");
            _logger.LogInformation(
                "Order soft-deleted: #{Id} by UserId={UserId}", orderId, requestingUserId);
        }

        public async Task<List<PaymentDto>> GetPaymentsByOrderAsync(
            int orderId, int businessAccountId)
        {
            var records = await _repo.GetPaymentsByOrderAsync(orderId, businessAccountId);
            return records.Select(r => new PaymentDto
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                OrderId = r.OrderId,
                Amount = r.Amount,
                PaymentType = r.PaymentType,
                Note = r.Note,
                Date = r.Date,
            }).ToList();
        }

        public async Task<OrderDashboardSummaryDto> GetDashboardSummaryAsync(
            int businessAccountId, DateTime? from)
            => await _repo.GetDashboardSummaryAsync(businessAccountId, from);

        public async Task<PagedOrderHistoryResponse> GetByCustomerFilteredAsync(
            int customerId,
            int businessAccountId,
            int page,
            int pageSize,
            string? dateFrom = null,
            string? dateTo = null,
            string? search = null,
            string? status = null)
        {
            var (items, totalCount, totalSales, totalPaid, totalDue) =
                await _repo.GetByCustomerFilteredAsync(
                    customerId, businessAccountId,
                    page, pageSize,
                    dateFrom, dateTo, search, status);

            return new PagedOrderHistoryResponse
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Summary = new OrderHistorySummaryDto
                {
                    TotalOrders = totalCount,
                    TotalSales = totalSales,
                    TotalPaid = totalPaid,
                    TotalDue = totalDue,
                },
            };
        }

        // ── Mapper ────────────────────────────────────────────────

        private static OrderDto MapToDto(OrderRecord r)
        {
            var items = new List<OrderItemDto>();
            if (!string.IsNullOrWhiteSpace(r.ItemsJson))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<List<OrderItemDto>>(r.ItemsJson, _jsonOpts);
                    if (parsed is not null) items = parsed;
                }
                catch { /* non-fatal */ }
            }

            return new OrderDto
            {
                Id = r.Id,
                BusinessAccountId = r.BusinessAccountId,
                CustomerId = r.CustomerId,
                CustomerName = r.CustomerName,
                Items = items,
                GrandTotal = r.GrandTotal,
                PaidAmount = r.PaidAmount,
                Balance = r.Balance,
                PaymentType = r.PaymentType,
                Status = r.Status,
                DeliveryNote = r.DeliveryNote,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                //RowVersion = HexToBase64(r.RowVersion),
                RowVersion = r.RowVersion is not null
                                ? Convert.ToBase64String(r.RowVersion)
                                : string.Empty,
                SubTotal = r.SubTotal,
                TaxableAmount = r.TaxableAmount,
                GstType = r.GstType,
                CgstRate = r.CgstRate,
                SgstRate = r.SgstRate,
                IgstRate = r.IgstRate,
                CgstAmount = r.CgstAmount,
                SgstAmount = r.SgstAmount,
                IgstAmount = r.IgstAmount,
                TotalGst = r.TotalGst,
            };
        }

        private static string HexToBase64(string? hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return string.Empty;
            try
            {
                var h = hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? hex[2..] : hex;
                return Convert.ToBase64String(Convert.FromHexString(h));
            }
            catch { return string.Empty; }
        }
    }
}
