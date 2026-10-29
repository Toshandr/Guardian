# Guardian — Акт II. Хранитель

ASP.NET Core Web API, PostgreSQL, EF Core, Redis, Serilog, Docker и xUnit.

## Запуск в VS Code

Нужны .NET 10 SDK и Docker Desktop.

```bash
docker compose up -d postgres redis
dotnet tool install --global dotnet-ef
dotnet restore
dotnet ef migrations add InitialCreate --project src/Guardian.Api --startup-project src/Guardian.Api
dotnet ef database update --project src/Guardian.Api --startup-project src/Guardian.Api
dotnet run --project src/Guardian.Api
```

Swagger: http://localhost:5000/swagger или адрес, который напечатает приложение.

После создания миграции приложение применяет её при старте. Для запуска всего в Docker:

```bash
docker compose up --build
```

Swagger будет доступен на http://localhost:8080/swagger.

## Тесты

```bash
dotnet test
```

## Кеш

Для `GET /api/visitors/{id}`, списка и проверки доступа сначала проверяется Redis. При промахе данные читаются из PostgreSQL и кладутся в Redis. При изменении посетителя сначала сохраняется PostgreSQL, затем обновляется кеш посетителя и удаляются зависимые ключи.


## Kubernetes (локально через Docker Desktop Kubernetes или Minikube)

В каталоге `k8s/` находятся манифесты Namespace, Secret, PostgreSQL с PVC, Redis и API с двумя репликами. Секрет в `secrets.example.yaml` содержит только демонстрационный пароль для локального запуска; для реального окружения замените его и не храните реальные секреты в Git.

### 1. Подготовить проект и миграцию

Перед сборкой образа один раз создайте миграцию, если её ещё нет, и проверьте, что она находится в `src/Guardian.Api/Migrations`:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src/Guardian.Api --startup-project src/Guardian.Api
```

Если миграция уже существует, повторно создавать её не нужно.

### 2. Собрать образ API

Команды выполняются из корня репозитория. Для Docker Desktop Kubernetes локальный образ будет доступен кластеру:

```bash
docker build -t guardian-api:local -f src/Guardian.Api/Dockerfile .
```

Для Minikube перед сборкой выполните `minikube image load guardian-api:local` после сборки либо используйте `eval $(minikube docker-env)` и пересоберите образ внутри окружения Minikube.

### 3. Развернуть кубер

```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/secrets.example.yaml
kubectl apply -f k8s/postgres.yaml
kubectl apply -f k8s/redis.yaml
kubectl apply -f k8s/api.yaml
kubectl -n guardian get pods
```


### 4. Открыть Swagger

```bash
kubectl -n guardian port-forward service/guardian-api 8080:8080
```