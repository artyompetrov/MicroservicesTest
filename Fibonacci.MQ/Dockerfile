FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR "/app/"
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR "/src/"
# --- dependencies ---
COPY ["Fibonacci.Common/Fibonacci.Common.csproj", "Fibonacci.Common/"]
RUN dotnet restore "Fibonacci.Common/Fibonacci.Common.csproj"
COPY ["Fibonacci.Common/", "Fibonacci.Common/"]

# --- target ---
COPY ["Fibonacci.MQ/Fibonacci.MQ.csproj", "Fibonacci.MQ/"]
RUN dotnet restore "Fibonacci.MQ/Fibonacci.MQ.csproj"

COPY ["Fibonacci.MQ/", "Fibonacci.MQ/"]
WORKDIR "/src/Fibonacci.MQ/"
#RUN dotnet build "Fibonacci.MQ.csproj" -c Release -o "/app/build/"

FROM build AS publish
RUN dotnet publish "Fibonacci.MQ.csproj" -c Release -o "/app/publish/"

FROM base AS final
WORKDIR "/app/"
COPY --from=publish "/app/publish/" .
ENTRYPOINT ["dotnet", "Fibonacci.MQ.dll"]