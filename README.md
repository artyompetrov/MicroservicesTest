# MicroservicesTest

Tech stack:
* ASP.NET Core 5
* DistributedMemoryCache
* Swagger (OpenAPI)
* RabbitMQ + EasyNetQ
* Docker Compose

Project list:
* Fibonacci.MQ - Microservice 1. Sends FibonacciData message to Fibonacci.Rest via OpenApi. Starts negotiation.
* Fibonacci.Rest - Microservice 2. Sends FibonacciData message to Fibonacci.MQ via RabbitMQ message bus.
* Fibonacci.Common - common classes for Fibonacci.MQ and Fibonacci.Rest projects.


# Building the project:

```bash
git clone https://github.com/artyompetrov/MicroservicesTest.git
cd MicroservicesTest/
docker-compose up -d
```
