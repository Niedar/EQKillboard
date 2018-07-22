# EQKillboard

Everquest killboard

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
sudo npm install postgraphile -g

postgraphile -c postgres://user:password@localhost/database_name \
--schema public
```