echo "CREATE USER 'com2us'@'%' IDENTIFIED BY '123123';
GRANT ALL PRIVILEGES ON *.* TO 'com2us'@'%' WITH GRANT OPTION;
FLUSH PRIVILEGES;" > /tools/query/mysql_init.sql  && 

service mariadb start && 
mariadb < /tools/query/mysql_init.sql && 
mariadb < /tools/query/mysql_account.sql && 
mariadb < /tools/query/mysql_master.sql && 
mariadb < /tools/query/mysql_master_data.sql && 
mariadb < /tools/query/mysql_game.sql && 
service mariadb stop  && 

exec mysqld --bind-address=0.0.0.0
