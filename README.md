# EQKillboard

Everquest killboard

## .NET Core
```
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.1
```

## Postgres Installation
```
sudo apt-get install postgresql postgresql-contrib
```


## Sqitch

```
sudo apt-get install sqitch

sqitch config --user user.name 'Marge N. O’Vera'
sqitch config --user user.email 'marge@example.com'

sqitch deploy db:pg:database_name
```

## PostGraphile

```
sudo apt-get install nodejs
sudo npm install postgraphile -g
sudo npm install graphql -g
sudo npm install pg -g
sudo npm install postgraphile-plugin-connection-filter -g

postgraphile --append-plugins postgraphile-plugin-connection-filter -c postgres://user:password@localhost/database_name \
--schema public
```
