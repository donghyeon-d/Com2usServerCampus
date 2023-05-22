FROM debian

RUN apt-get -y update && \
	apt-get -y upgrade && \
	apt-get install -y mariadb-server

COPY ./tools /tools/
RUN	chmod +x ./tools/init.sh

RUN mv ./tools/50-server.cnf /etc/mysql/mariadb.conf.d/

ENTRYPOINT ["sh", "./tools/init.sh"]
