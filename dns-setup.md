# DNS Setup for Custom Domain

To make your custom domain `api.alexandergarin.com` work with your Fly.io application, follow these steps:

## Option 1: Set up a CNAME Record (Recommended for Subdomains)

In your DNS provider's dashboard (where you registered alexandergarin.com), add:

```
Type: CNAME
Name: api
Value: dotnet-weather-crud.fly.dev
```

This is the simplest approach and will automatically use all the IP addresses of your application.

## Option 2: Set up A and AAAA Records

If CNAME doesn't work for your setup, you'll need to add direct IP mappings:

1. Log into your DNS provider's dashboard
2. Add A record(s) for IPv4:
   ```
   Type: A
   Name: api
   Value: [Your app's IPv4 address]
   ```

3. Add AAAA record(s) for IPv6:
   ```
   Type: AAAA
   Name: api
   Value: [Your app's IPv6 address]
   ```

## Important Notes

1. DNS changes can take up to 24-48 hours to propagate worldwide (though often it's much faster)
2. You can check DNS propagation using online tools like [dnschecker.org](https://dnschecker.org/)
3. After DNS is properly configured, Fly.io should automatically detect it and start serving your app at the custom domain

## Testing Your Custom Domain

After making DNS changes, wait a while and then test:

```
curl https://api.alexandergarin.com/health
```

If it works, you should see "Healthy" as the response. 