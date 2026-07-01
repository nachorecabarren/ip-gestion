namespace IpGestion.Domain.Enums;

public enum TenantPlan { STARTER, ADVANCED, PRO }
public enum UserRole { OWNER, ADMIN, OPERATOR, VIEWER, EMPLOYEE }
public enum InvitationStatus { PENDING, ACCEPTED, EXPIRED }
public enum EntityType { CLIENT, PROVIDER, EMPLOYEE, TECHNICIAN, COURIER }
public enum StockCondition { NEW, USED, REFURBISHED, A_PLUS, A, B }
public enum StockStatus { AVAILABLE, RESERVED, SOLD, IN_SERVICE, IN_REPAIR, RETURNED, VOIDED }
public enum SaleCategory { RETAIL, WHOLESALE }
public enum SaleOrigin { DIRECT, RESERVATION }
public enum SaleStatus { COMPLETED, VOIDED, CANCELLED, PENDING }
public enum PurchaseStatus { PENDING, ACTIVE, PARTIAL, DELIVERED, CANCELLED }
public enum PurchaseType { DEVICE, ACCESSORY }
public enum ReservationStatus { ACTIVE, SOLD, CANCELLED }
public enum ServiceJobStatus { OPEN, IN_REPAIR, READY_FOR_DELIVERY, DELIVERED, CLOSED, CANCELLED }
public enum PaymentMethod { USD_CASH, USDT, ARS_CASH, ARS_TR, MERCADOPAGO }
public enum Currency { USD, ARS }
public enum CashMovementType { INCOME, EXPENSE, SALE, PURCHASE }
public enum CommissionType { FIXED, PERCENTAGE, RANGE }
public enum CommissionStatus { PENDING, PAID }
public enum SubscriptionStatus { ACTIVE, SUSPENDED, INCOMPLETE, CANCELLED }
public enum ImportOrderStatus { PENDING, IN_TRANSIT, RECEIVED, CANCELLED }
public enum ItemKind { EQUIPMENT, ACCESSORY }
public enum StockItemConditionDetail { SELLADO, USADO }
