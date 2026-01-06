#!/bin/bash
set -e

# Paths
APPSETTINGS=backend/src/Carmasters.Http.Api/appsettings.Secrets.json
ENVFILE=frontend/.env

# Secrets
JWT_SECRET=$(openssl rand -hex 64)
CONSUMER_SECRET=$(openssl rand -base64 32)
SESSION_SECRET=$(openssl rand -base64 32)

# appsettings.Secrets.json
cp ${APPSETTINGS}.example $APPSETTINGS
sed -i.bak "s|\"Secret\": \".*\"|\"Secret\": \"$JWT_SECRET\"|" $APPSETTINGS
sed -i.bak "s|\"ConsumerSecret\": \".*\"|\"ConsumerSecret\": \"$CONSUMER_SECRET\"|" $APPSETTINGS
rm $APPSETTINGS.bak

# .env
cp ${ENVFILE}.example $ENVFILE
sed -i.bak "s|SERVER_SECRET=.*|SERVER_SECRET=$CONSUMER_SECRET|" $ENVFILE
sed -i.bak "s|SESSION_SECRET=.*|SESSION_SECRET=$SESSION_SECRET|" $ENVFILE
rm $ENVFILE.bak

echo "✅ Secrets initialized"
