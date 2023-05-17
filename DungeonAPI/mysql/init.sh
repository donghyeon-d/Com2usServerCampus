sudo apt install -y mariadb-server redis 

sudo service mysql start
sudo service redis start

mysql < mysql_account.sql;
mysql < mysql_master.sql;
mysql < mysql_master_data.sql;
mysql < mysql_game.sql;
