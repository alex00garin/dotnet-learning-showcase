#!/bin/bash

echo "Testing Level 3 - Autocomplete Endpoints"
echo "========================================="

BASE_URL="http://localhost:5249"

echo ""
echo "1. Get available data sources:"
curl -s "$BASE_URL/level3/datasources" | python3 -m json.tool

echo ""
echo ""
echo "2. Test city autocomplete for 'Lon':"
curl -s "$BASE_URL/level3/cities/autocomplete?query=Lon&limit=5" | python3 -m json.tool

echo ""
echo ""
echo "3. Test country autocomplete using generic endpoint:"
curl -s -X POST "$BASE_URL/level3/autocomplete" \
  -H "Content-Type: application/json" \
  -d '{"query": "United", "limit": 5, "dataSource": "countries"}' | python3 -m json.tool

echo ""
echo ""
echo "4. Test data source health check:"
curl -s "$BASE_URL/level3/datasources/cities/health" | python3 -m json.tool

echo ""
echo ""
echo "5. Test cities autocomplete for 'Ber':"
curl -s "$BASE_URL/level3/cities/autocomplete?query=Ber&limit=10" | python3 -m json.tool

echo ""
echo ""
echo "6. Test generic autocomplete with cities:"
curl -s -X POST "$BASE_URL/level3/autocomplete" \
  -H "Content-Type: application/json" \
  -d '{"query": "Paris", "limit": 3, "dataSource": "cities"}' | python3 -m json.tool

echo ""
echo "========================================="
echo "Level 3 testing complete!" 