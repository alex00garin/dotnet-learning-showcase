# Data Directory

## cities.json

**Source:** https://github.com/lutangar/cities.json  
**License:** CC BY 4.0  
**Size:** ~21MB  
**Records:** 154,694+ cities worldwide  

This file contains comprehensive city data including:
- City names
- Countries  
- Geographic coordinates (latitude/longitude)
- Administrative regions (admin1, admin2)

### Usage

The file is automatically loaded by the `CitiesDataSource` service for autocomplete functionality. No internet connection required after initial download.

### Update Instructions

To update to the latest city data:

```bash
curl -o Data/cities.json https://raw.githubusercontent.com/lutangar/cities.json/master/cities.json
```

The application will automatically use the updated file on next restart. 