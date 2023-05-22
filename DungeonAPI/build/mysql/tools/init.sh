#!/bin/bash
echo "CREATE USER 'com2us'@'%' IDENTIFIED BY '123123';
GRANT ALL PRIVILEGES ON *.* TO 'com2us'@'%' WITH GRANT OPTION;
FLUSH PRIVILEGES;" > mysql_init.sql;\
chmod 777 ./mysql_init.sql;\
service mysql start && \
mysql < mysql_init.sql;\
mysql < mysql_account.sql;\
mysql < mysql_master.sql;\
mysql < mysql_master_data.sql;\
mysql < mysql_game.sql;\
service mysql stop 
# exec mysqld --bind-address=0.0.0.0
