#!/bin/bash
echo "CREATE USER 'dong'@'%' IDENTIFIED BY '123123';
GRANT ALL PRIVILEGES ON *.* TO 'dong'@'%' WITH GRANT OPTION;
FLUSH PRIVILEGES;" > ./tools/query/mysql_init.sql;\
chmod 777 ./tools/query/mysql_init.sql;\
service mysql start && \
mysql < ./tools/query/mysql_init.sql;\
mysql < ./tools/query/mysql_account.sql;\
mysql < ./tools/query/mysql_master.sql;\
mysql < ./tools/query/mysql_master_data.sql;\
mysql < ./tools/query/mysql_game.sql;\
service mysql stop 
# exec mysqld --bind-address=0.0.0.0
