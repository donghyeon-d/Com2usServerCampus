all :
	cp -r APIServer  ./build/apiserver/tools
	sudo docker compose -f ./build/docker-compose.yml up --build -d

up : 
	sudo docker compose -f ./build/docker-compose.yml up --build -d

down :
	sudo docker compose -f ./build/docker-compose.yml down

rm :
	rm -rf $(APISERVER_OUTPUT_PATH)/*

re :
	@$(MAKE) down
	@$(MAKE) rm
	@$(MAKE) all

.PHONY : up down rm re