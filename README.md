# Com2us ServerCampus

## 소개
> Com2us ServerCampus 1기 본과제 입니다. 
> 서머너즈워 게임의 컨텐츠의 일부를 모작하며, WebAPI 서버로 구현할 수 있는 내용으로 구성되어있습니다. 게임의 근간은 스테이지 클리어 형식이고, 스테이지를 클리어하면 보상을 받는 구조입니다.

과제의 내용과 설계, 구현에 대해서는 아래 Gitbook에 메모하며 작업하고 있습니다. 자세한 내용은 아래 링크에서 확인할 수 있습니다.

[https://dong-d.gitbook.io/main-subject/](https://dong-d.gitbook.io/main-subject/)

<br><br>
## 빌드 및 배포
* linux(Debian11) 환경에서 docker compose로 빌드 및 배포할 수 있습니다.
* 방법
    ```
    cd ./DungeonAPI
    make all
    ```
* api서버, redis, mariadb 서버가 각각의 컨테이너로 생성되고 실행됩니다.
* api서버는 5182 포트로 열리게 됩니다.
* docker 설치가 필요하면 `./Com2usServerCampus.docker_install.sh` 를 사용하거나 [docker docs](https://docs.docker.com/engine/install/)를 통해 설치할 수 있습니다.

### 포트설정
* 아래 파일에서 설정 변경
* `Com2usServerCampus/DungeonAPI/DungeonAPI/Properties/launchSettings.json`
* "applicationUrl": "http://localhost:5182" 이 부분의 IP와 포트를 설정해주면 됩니다.

