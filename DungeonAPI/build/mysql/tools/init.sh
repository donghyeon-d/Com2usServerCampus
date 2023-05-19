service mysql start

GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' WITH GRANT OPTION;

alter user 'com2us'@'%' identified with mysql_native_password by 'new password';