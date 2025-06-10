#!/bin/bash

echo "Testing Level 1 & 2 Integration with Level 3 Autocomplete"
echo "=========================================================="

BASE_URL="http://localhost:5249"

echo ""
echo "🌟 INTEGRATION DEMO: Autocomplete enhances existing endpoints"
echo ""

echo "1. Level 1 - Regular endpoint with invalid city:"
curl -s "$BASE_URL/level1/weatherforecast?city=Londo" | python3 -m json.tool

echo ""
echo ""
echo "2. Level 1 SMART - Same invalid city with autocomplete suggestions:"
curl -s "$BASE_URL/level1/weatherforecast/smart?city=Londo" | python3 -m json.tool

echo ""
echo ""
echo "3. Level 1 SMART - Valid city (no suggestions needed):"
curl -s "$BASE_URL/level1/weatherforecast/smart?city=London" | python3 -m json.tool

echo ""
echo ""
echo "4. Level 2 - Regular history endpoint with non-existent city:"
curl -s "$BASE_URL/level2/weatherforecast/history/NonExistentCity" | python3 -m json.tool

echo ""
echo ""
echo "5. Level 2 SMART - Same city with intelligent suggestions:"
curl -s "$BASE_URL/level2/weatherforecast/history/smart/Berli" | python3 -m json.tool

echo ""
echo ""
echo "6. Level 2 BULK - Multiple cities with autocomplete fallback:"
curl -s -X POST "$BASE_URL/level2/weatherforecast/bulk" \
  -H "Content-Type: application/json" \
  -d '{
    "cities": ["London", "Pari", "Berli", "TokyoXYZ", "NewYork"]
  }' | python3 -m json.tool

echo ""
echo ""
echo "7. Pure Level 3 - City autocomplete for comparison:"
curl -s "$BASE_URL/level3/cities/autocomplete?query=Pari&limit=3" | python3 -m json.tool

echo ""
echo "=========================================================="
echo "🎯 INTEGRATION BENEFITS:"
echo "✅ Existing endpoints work unchanged (backward compatibility)"
echo "✅ New 'smart' endpoints provide enhanced user experience"  
echo "✅ Autocomplete helps users find correct city names"
echo "✅ Bulk operations leverage autocomplete for error recovery"
echo "✅ Same autocomplete service used across all levels"
echo "==========================================================" 