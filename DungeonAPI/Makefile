APISERVER_PROJECT_PATH := ./DungeonAPI
APISERVER_OUTPUT_PATH := ./build/apiserver/tools


up :
	dotnet publish $(APISERVER_PROJECT_PATH) --output $(APISERVER_OUTPUT_PATH)
	sudo docker compose -f ./build/docker-compose.yml up --build -d

down :
	sudo docker compose -f ./build/docker-compose.yml down

re :
	@$(MAKE) down
	@$(MAKE) rm
	@$(MAKE) up

.PHONY : up down rm re