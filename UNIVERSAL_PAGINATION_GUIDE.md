# 🌟 Universal Pagination System Guide

## Yes! The pagination system works with **ANY** data type!

The Level 4 pagination system is designed to be completely universal and reusable across your entire application. Here's how it works with any data type:

## 🔧 Core Universal Components

### 1. Generic Response Wrapper
```csharp
// Works with ANY type T
public record PaginatedResponse<T>(
    IEnumerable<T> Data,           // Your data of any type
    PaginationMetadata Pagination   // Always the same metadata
);
```

### 2. Universal Helper Methods
```csharp
// Use with any collection
public static PaginatedResponse<T> CreateResponse<T>(
    IEnumerable<T> data,
    int totalItems,
    PaginationRequest request)

// Extension methods work with any IEnumerable or IQueryable
items.ApplyPagination(request)
query.ApplyPagination(request)
```

### 3. Consistent Metadata Structure
```csharp
// Same for ALL data types
public record PaginationMetadata(
    int CurrentPage,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage,
    int FirstItemIndex,
    int LastItemIndex
);
```

## 💡 How to Use with Your Own Data

### Example 1: E-commerce Products
```csharp
// Your custom model
public record Product(
    int Id,
    string Name,
    decimal Price,
    string Category,
    DateTime CreatedDate
);

// Your controller method
app.MapPost("/products/paginated", (
    IProductService productService,
    PaginationRequest request) =>
{
    // Get your data from anywhere
    var allProducts = productService.GetAllProducts();
    
    // Apply pagination universally
    var paginatedData = allProducts.ApplyPagination(request);
    var totalCount = allProducts.Count();
    
    // Create response with the universal helper
    var response = PaginationHelper.CreateResponse(
        paginatedData, 
        totalCount, 
        request);
    
    return Results.Ok(response);
});
```

### Example 2: User Management
```csharp
// Your user model
public record User(
    Guid Id,
    string Email,
    string FullName,
    DateTime RegisteredDate,
    string Role
);

// Service method
public async Task<PaginatedResponse<User>> GetPaginatedUsersAsync(
    PaginationRequest request)
{
    var allUsers = await _userRepository.GetAllAsync();
    
    // Apply sorting if requested
    if (!string.IsNullOrEmpty(request.SortBy))
    {
        allUsers = request.SortBy.ToLower() switch
        {
            "name" => request.SortDirection == SortDirection.Ascending
                ? allUsers.OrderBy(u => u.FullName)
                : allUsers.OrderByDescending(u => u.FullName),
            "date" => request.SortDirection == SortDirection.Ascending
                ? allUsers.OrderBy(u => u.RegisteredDate)
                : allUsers.OrderByDescending(u => u.RegisteredDate),
            _ => allUsers
        };
    }
    
    // Universal pagination
    var paginatedUsers = allUsers.ApplyPagination(request);
    
    return PaginationHelper.CreateResponse(
        paginatedUsers,
        allUsers.Count(),
        request);
}
```

### Example 3: Order History
```csharp
// Complex nested model
public record Order(
    int OrderId,
    string CustomerName,
    List<OrderItem> Items,
    decimal TotalAmount,
    OrderStatus Status,
    DateTime OrderDate
);

public record OrderItem(string ProductName, int Quantity, decimal Price);

// Controller endpoint
app.MapPost("/orders/paginated", async (
    IOrderService orderService,
    OrderPaginationRequest request) =>  // Extend PaginationRequest if needed
{
    var orders = await orderService.GetOrdersAsync();
    
    // Apply filters (your business logic)
    if (request.StatusFilter != null)
        orders = orders.Where(o => o.Status == request.StatusFilter);
    
    if (request.MinAmount.HasValue)
        orders = orders.Where(o => o.TotalAmount >= request.MinAmount);
    
    // Universal pagination works with complex objects too!
    var totalCount = orders.Count();
    var paginatedOrders = orders.ApplyPagination(request);
    
    return PaginationHelper.CreateResponse(paginatedOrders, totalCount, request);
});
```

## 🚀 Advanced Universal Usage

### With Entity Framework
```csharp
public async Task<PaginatedResponse<BlogPost>> GetPaginatedPostsAsync(
    PaginationRequest request)
{
    var query = _context.BlogPosts
        .Include(p => p.Author)
        .Include(p => p.Tags);
    
    var totalCount = await query.CountAsync();
    
    // Apply pagination to IQueryable (more efficient)
    var posts = await query
        .ApplyPagination(request)
        .ToListAsync();
    
    return PaginationHelper.CreateResponse(posts, totalCount, request);
}
```

### With External APIs
```csharp
public async Task<PaginatedResponse<ExternalApiResult>> GetPaginatedApiDataAsync(
    PaginationRequest request)
{
    // Get data from external API
    var apiData = await _httpClient.GetFromJsonAsync<List<ExternalApiResult>>(
        "https://api.example.com/data");
    
    // Apply universal pagination to external data
    var totalCount = apiData.Count;
    var paginatedData = apiData.ApplyPagination(request);
    
    return PaginationHelper.CreateResponse(paginatedData, totalCount, request);
}
```

### With Custom Sorting
```csharp
public static IEnumerable<T> ApplyCustomSorting<T>(
    this IEnumerable<T> items,
    string? sortBy,
    SortDirection direction) where T : class
{
    if (string.IsNullOrEmpty(sortBy)) return items;
    
    var property = typeof(T).GetProperty(sortBy, 
        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
    
    if (property == null) return items;
    
    return direction == SortDirection.Ascending
        ? items.OrderBy(x => property.GetValue(x))
        : items.OrderByDescending(x => property.GetValue(x));
}
```

## 📊 Response Structure (Always the Same!)

No matter what data type you use, the response structure is **always identical**:

```json
{
  "data": [/* Your objects of ANY type */],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalItems": 157,
    "totalPages": 16,
    "hasNextPage": true,
    "hasPreviousPage": false,
    "firstItemIndex": 1,
    "lastItemIndex": 10
  }
}
```

## ✅ Benefits of Universal Design

1. **🔄 Consistency** - Same pagination behavior across your entire app
2. **⚡ Reusability** - Write pagination logic once, use everywhere
3. **🛡️ Type Safety** - Full C# generic type safety with `PaginatedResponse<T>`
4. **📱 Frontend Friendly** - Same JSON structure for all paginated endpoints
5. **🧪 Testability** - Same testing patterns for all paginated data
6. **📚 Maintainability** - Single source of truth for pagination logic

## 🎯 Quick Start Template

```csharp
// 1. Your model (any type!)
public record YourModel(/* your properties */);

// 2. Your endpoint
app.MapPost("/your-endpoint/paginated", (
    PaginationRequest request) =>
{
    // 3. Get your data (from anywhere)
    var yourData = GetYourDataFromSomewhere();
    
    // 4. Apply universal pagination
    var totalCount = yourData.Count();
    var paginatedData = yourData.ApplyPagination(request);
    
    // 5. Create universal response
    return PaginationHelper.CreateResponse(
        paginatedData, 
        totalCount, 
        request);
});
```

## 🧪 Test It Yourself

```bash
# Make the demo script executable
chmod +x test-universal-pagination.sh

# Run the universal demo
./test-universal-pagination.sh

# Try with curl (works with any data type!)
curl -s -X POST http://localhost:5249/level4/paginate/demo \
  -H "Content-Type: application/json" \
  -d '{"page": 1, "pageSize": 5}' | jq '.'
```

**The pagination system is truly universal - it works with products, users, orders, blog posts, API data, or ANY object type you can imagine!** 🌟 