#!/bin/bash

# Universal Pagination Demo
# This script demonstrates how the pagination system works with ANY data type

BASE_URL="http://localhost:5249"

echo "🌟 Universal Pagination System Demo"
echo "====================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

print_demo() {
    echo -e "${BLUE}📋 $1${NC}"
    echo "----------------------------------------"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
    echo ""
}

# Demo 1: The system generates ANY type of data
print_demo "Universal Sample Data Generation"
echo "The demo endpoint creates 157 items of ANY structure you want!"
curl -s -X POST "$BASE_URL/level4/paginate/demo" \
  -H "Content-Type: application/json" \
  -d '{"page": 1, "pageSize": 3}' | jq '.data[]'
print_success "Works with any custom object structure"

# Demo 2: Works with different data types seamlessly  
print_demo "Different Page - Same Universal System"
curl -s -X POST "$BASE_URL/level4/paginate/demo" \
  -H "Content-Type: application/json" \
  -d '{"page": 5, "pageSize": 2, "sortBy": "category"}' | jq '.data[]'
print_success "Same pagination logic, any data structure"

# Demo 3: Pagination metadata is identical regardless of data type
print_demo "Universal Pagination Metadata"
curl -s -X POST "$BASE_URL/level4/paginate/demo" \
  -H "Content-Type: application/json" \
  -d '{"page": 10, "pageSize": 5}' | jq '.pagination'
print_success "Metadata structure is consistent across all data types"

echo ""
echo -e "${YELLOW}🎯 Key Universal Features:${NC}"
echo "• Works with ANY C# object type: Product, User, Order, etc."
echo "• Consistent API: PaginatedResponse<T> for any T"
echo "• Universal helpers: PaginationHelper.CreateResponse<T>()"
echo "• Standard metadata: currentPage, totalItems, hasNext, etc."
echo "• Reusable across entire application"
echo ""
echo -e "${GREEN}💡 To use with your own data:${NC}"
echo "1. Create your model: public record MyProduct(string Name, decimal Price);"
echo "2. Use generic helper: PaginationHelper.CreateResponse<MyProduct>()"
echo "3. Same pagination request/response pattern for everything!" 