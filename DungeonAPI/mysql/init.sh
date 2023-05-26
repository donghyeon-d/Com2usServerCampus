sudo apt install -y mariadb-server redis 

sudo service mysql start
sudo service redis start

sudo mysql < mysql_account.sql;
sudo mysql < mysql_master.sql;
sudo mysql < mysql_master_data.sql;
sudo mysql < mysql_game.sql;
